# 背包模块对外接口文档

本文档描述背包模块向其他模块**提供的接口**，以及背包模块**依赖的外部接口**。供地图、战斗、UI、任务、商店同学对接时参考。

---

## 一、背包模块提供的接口

### 1.1 InventoryManager（背包核心）

**入口**：`InventoryManager.Instance`（`[RuntimeInitializeOnLoadMethod]` 自动创建单例，无需场景手动放置）

#### 1.1.1 任务系统 / 商店系统调用

| 方法 | 参数 | 返回值 | 用途 | 调用场景 |
| --- | --- | --- | --- | --- |
| `AddItem` | `string itemId, int count` | `bool` | 向背包添加物品，返回是否成功（无限背包始终返回 true） | 任务奖励发放、商店购买、怪物掉落 |
| `RemoveItem` | `string itemId, int count` | `bool` | 从背包移除物品，返回是否成功（数量不足时返回 false） | 任务道具交付、商店出售 |
| `HasItem` | `string itemId, int count = 1` | `bool` | 检查背包是否持有指定数量的物品 | 任务交付校验（确认玩家持有任务道具） |
| `GetItemCount` | `string itemId` | `int` | 获取某物品当前持有数量 | 任务进度查询 |

**任务道具交付示例**（任务系统同学接入）：

```csharp
// 精灵图书管理员 NPC 交付任务时校验三个任务道具
InventoryManager inv = InventoryManager.Instance;

bool hasStoneHeart  = inv.HasItem("quest_stone_heart");
bool hasFlameCore   = inv.HasItem("quest_flame_demon_core");
bool hasSkeletonCrown = inv.HasItem("quest_skeleton_crown");

if (hasStoneHeart && hasFlameCore && hasSkeletonCrown)
{
    inv.RemoveItem("quest_stone_heart", 1);
    inv.RemoveItem("quest_flame_demon_core", 1);
    inv.RemoveItem("quest_skeleton_crown", 1);
    // 任务完成，发放最终奖励
}
```

**商店购买示例**（商店系统同学接入）：

```csharp
// 矮人铁匠：玩家购买药水
InventoryManager.Instance.AddItem("potion_health", 1);
InventoryManager.Instance.AddItem("potion_attack_boost", 1);
// 扣除金币逻辑由商店系统自行处理
```

#### 1.1.2 UI 系统调用

> `BackpackUI` 已内部订阅 `OnInventoryChanged` 自动刷新，UI 同学无需手动实现以下逻辑。以下仅供参考。

| 方法 / 事件 | 用途 |
| --- | --- |
| `OnInventoryChanged` 事件 | 背包内容变化时触发，UI 订阅此事件刷新显示 |
| `GetAllSlots()` | 获取所有背包格子（`InventorySlot[]`），每个含 `itemId` + `count` |
| `GetItemDefinition(string itemId)` | 查询物品的显示信息（名称、图标、描述、分类） |
| `UsePotion(string itemId)` | UI 的"使用"按钮或快捷键调用，消耗 1 瓶药水并触发效果事件 |

```csharp
// UI 接入骨架代码
void Start()
{
    InventoryManager.Instance.OnInventoryChanged += RefreshBackpack;
    RefreshBackpack();
}

void RefreshBackpack()
{
    ClearSlots();
    foreach (var slot in InventoryManager.Instance.GetAllSlots())
    {
        if (slot.IsEmpty) continue;
        var def = InventoryManager.Instance.GetItemDefinition(slot.itemId);
        // 创建格子：显示 def.icon(Sprite), def.itemName, slot.count
        // 药水格子添加 [使用] 按钮 → InventoryManager.Instance.UsePotion(slot.itemId)
    }
}

void OnDestroy()
{
    if (InventoryManager.Instance != null)
        InventoryManager.Instance.OnInventoryChanged -= RefreshBackpack;
}
```

#### 1.1.3 战斗系统调用

| 事件 | 参数 | 用途 |
| --- | --- | --- |
| `OnPotionUsed` | `(string itemId, PotionEffectType effectType, int effectValue)` | 玩家使用药水时触发，战斗系统接收后应用效果 |

```csharp
// 战斗系统接入示例
InventoryManager.Instance.OnPotionUsed += (itemId, effect, value) =>
{
    switch (effect)
    {
        case PotionEffectType.HealthRestore: playerHP += value; break;
        case PotionEffectType.ManaRestore:   playerMP += value; break;
        case PotionEffectType.AttackBoost:   playerAtk += value; break;
    }
};
```

### 1.2 WeaponManager（武器系统）

**入口**：`WeaponManager.Instance`（`[RuntimeInitializeOnLoadMethod]` 自动创建单例）

武器**不在背包中**，由 `WeaponManager` 根据任务阶段自动升级。

| 成员 | 类型 | 用途 |
| --- | --- | --- |
| `CurrentWeaponId` | `string` 属性 | 当前装备武器的 ID |
| `CurrentWeaponData` | `InventoryItemData` 属性 | 当前武器完整数据（含 `attackPower`） |
| `OnWeaponChanged` | `event Action<string>` | 武器切换事件，参数为 `weaponId`（null=未装备），战斗系统更新攻击力 |

武器自动升级路径（无需外部调用）：
- 阶段 0-1 → `weapon_iron_blade`（铁刀）
- 阶段 2 → `weapon_steel_blade`（刚刀）
- 阶段 3-4 → `weapon_legendary_blade`（传说之刃）

```csharp
// 战斗系统接入
WeaponManager.Instance.OnWeaponChanged += (weaponId) =>
{
    var data = WeaponManager.Instance.CurrentWeaponData;
    playerAttack = data != null ? data.attackPower : 0;
};
```

### 1.3 QuestRewardAdapter（任务奖励桥接）

**挂载方式**：场景中创建空 GameObject，挂载 `QuestRewardAdapter` 组件，拖入 `InventoryRewardConfig`。

| 功能 | 说明 |
| --- | --- |
| 自动发放任务道具 | 订阅 `QuestStageChanged`，阶段 1→2→3 时自动调用 `AddItem` 发放石像心/炎魔之核/骷髅王冠 |
| 防重复发放 | 内部 `_rewardedStages` 记录已发放的阶段，不会重复发放 |

### 1.4 InventoryUIOpener（快捷键）

**挂载方式**：场景中任意 GameObject，挂载 `InventoryUIOpener`，拖入背包 Panel 引用。

| 配置 | 说明 |
| --- | --- |
| `backpackPanel` | 挂载了 `BackpackUI` 的 Canvas GameObject |
| `toggleKey` | 默认为 `KeyCode.B`，可在 Inspector 修改 |

> `BackpackUI` 负责 Canvas UI 构建：消费/任务双 Tab，槽位列表，使用按钮，武器 HUD。素材从 `Assets/GUI_Parts/` 拖入 Inspector。

### 1.5 配置资产

| ScriptableObject | 菜单路径 | 用途 |
| --- | --- | --- |
| `InventoryItemData` | `ARPG > Inventory > Item Data` | 定义每个物品的 ID、名称、图标、分类、属性 |
| `InventoryItemRegistry` | `ARPG > Inventory > Item Registry` | 汇集所有 `InventoryItemData`，放入 `Assets/Resources/`，`InventoryManager` 启动时自动加载 |
| `InventoryRewardConfig` | `ARPG > Inventory > Quest Reward Config` | 配置任务各阶段的奖励物品及数量 |

### 1.6 枚举定义

```csharp
public enum ItemCategory { Weapon, Potion, QuestItem }
public enum PotionEffectType { AttackBoost, HealthRestore, ManaRestore }
```

---

## 二、背包模块依赖的外部接口（需要其他模块提供）

| 依赖项 | 来源模块 | 状态 | 说明 |
| --- | --- | --- | --- |
| `MainQuestManager.Instance` | 任务系统 | **未实现** | `QuestRewardAdapter` 和 `WeaponManager` 需要订阅 `QuestStageChanged` 事件 |
| `MainQuestManager.QuestStage` | 任务系统 | **未实现** | `WeaponManager` 启动时需要读取当前阶段以确定初始武器 |
| `MainQuestManager.QuestStageChanged` 事件 | 任务系统 | **未实现** | `(int newStage, string questText)` |
| 战斗系统订阅 `OnPotionUsed` | 战斗系统 | **未实现** | 药水效果需要战斗系统接收并应用（HP/MP/ATK 变更） |
| 战斗系统订阅 `OnWeaponChanged` | 战斗系统 | **未实现** | 武器攻击力变化需要战斗系统接收并更新玩家属性 |
| 背包 UI 面板（GameObject） | UI 系统 | **已实现** | `BackpackUI` 提供 Canvas 面板，素材使用 `GUI_Parts/`。`InventoryUIOpener` 挂载后拖入引用即可。 |
| 物品 Sprite 图标 | 美术 | **未到位** | `InventoryItemData.icon` 字段需要美术资源 |
| 商店系统 | 商店系统 | **未实现** | 商店购买时调用 `InventoryManager.AddItem()`，反之调用 `RemoveItem()` |

### 2.1 关键外部依赖说明

#### MainQuestManager（最优先依赖）

背包模块中两个组件依赖任务系统：

| 组件 | 缺失时的行为 | 影响 |
| --- | --- | --- |
| `QuestRewardAdapter` | null 检查后静默跳过，不发奖励 | 任务道具无法自动添加到背包 |
| `WeaponManager` | null 检查后默认使用铁刀，不响应阶段变化 | 武器始终为铁刀，不会自动升级 |

**建议**：任务系统完成后，`QuestRewardAdapter` 和 `WeaponManager` 即自动生效（它们均在 `Start()` 中主动订阅）。

#### 战斗系统（PotionUsed / WeaponChanged 接收方）

这两个事件**只负责通知**，背包模块不关心接收方是否存在。战斗系统完成后订阅即可。

---

## 三、物品 ID 速查表

### 3.1 背包物品

| 分类 | 物品 ID | 名称 | 效果 |
| --- | --- | --- | --- |
| 药水 | `potion_health` | 治理药水 | 回血 +30 |
| 药水 | `potion_mana` | 魔力药水 | 回蓝 +20 |
| 药水 | `potion_attack_boost` | 力量药水 | 加攻 +5 |
| 任务 | `quest_stone_heart` | 石像心 | 草原→石像小鬼掉落 |
| 任务 | `quest_flame_demon_core` | 炎魔之核 | 熔岩洞穴→炎魔守卫掉落 |
| 任务 | `quest_skeleton_crown` | 骷髅王冠 | 骷髅王座→暗影骷髅王掉落 |

### 3.2 武器（不在背包，WeaponManager 管理）

| 物品 ID | 名称 | 攻击力 | 触发阶段 |
| --- | --- | --- | --- |
| `weapon_iron_blade` | 铁刀 | 5 | 阶段 0-1 |
| `weapon_steel_blade` | 刚刀 | 10 | 阶段 2 |
| `weapon_legendary_blade` | 传说之刃 | 15 | 阶段 3-4 |

---

## 四、各模块接入清单

| 模块 | 需要做的事 | 调用的接口 |
| --- | --- | --- |
| **任务系统** | 任务阶段变化时触发 `QuestStageChanged` 事件（背包侧自动响应） | 无需主动调用背包接口 |
| **任务系统** | 任务交付时校验 `HasItem` + 扣除 `RemoveItem` | `HasItem` / `RemoveItem` |
| **商店系统** | 购买时 `AddItem`，出售时 `RemoveItem` | `AddItem` / `RemoveItem` / `HasItem` |
| **战斗系统** | 订阅 `OnPotionUsed` 应用药水效果 | 订阅事件 |
| **战斗系统** | 订阅 `OnWeaponChanged` 更新攻击力 | 订阅事件 |
| **UI 系统** | 创建背包面板，订阅 `OnInventoryChanged`，B 键打开 | `OnInventoryChanged` / `GetAllSlots` / `GetItemDefinition` / `UsePotion` |
| **UI 系统** | HUD 显示当前武器 | 订阅 `WeaponManager.OnWeaponChanged` |
| **地图组** | 无需接入背包模块 | — |
