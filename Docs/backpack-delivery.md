# 背包模块交付文档

## 一、接口文档要求交付的必填信息

### 背包组回复模板

| 项目 | 回复 |
| --- | --- |
| 背包服务入口 | `InventoryManager.Instance` |
| 添加物品接口 | `AddItem(string itemId, int count)` |
| 是否有容量检查接口 | 否（无限背包，无需容量检查） |
| 主线奖励物品 ID | `quest_stone_heart`、`quest_flame_demon_core`、`quest_skeleton_crown`（由 `InventoryRewardConfig` 配置） |
| 主线奖励数量 | 由 `InventoryRewardConfig` ScriptableObject 配置决定，不硬编码 |
| 背包满时处理方式 | 不适用（背包无限大） |
| 武器系统入口 | `WeaponManager.Instance`（独立于背包，随任务阶段自动升级） |
| 药水使用接口 | `UsePotion(string itemId)` |
| 背包快捷键 | 按 **B 键** 开关背包面板（`InventoryUIOpener` 组件） |
| 测试工具 | `InventoryDebugTester`：OnGUI 调试面板，模拟物品获取/消耗 |

### 资源交付格式 - 必填信息

| 模块 | 必填信息 |
| --- | --- |
| 背包 | **背包服务入口**：`InventoryManager.Instance`<br>**接口**：`AddItem` / `RemoveItem` / `HasItem` / `GetItemCount` / `UsePotion`<br>**物品 ID 表**：6 种背包物品（3 药水 + 3 任务道具）+ 3 种武器（武器不进入背包，由 WeaponManager 管理）<br>**武器服务入口**：`WeaponManager.Instance`（任务阶段驱动自动升级）<br>**容量**：无限背包<br>**快捷键**：B 键开关背包面板<br>**测试工具**：`InventoryDebugTester` 调试面板 |

### 物品定义（9 种）

> 以下物品 ID、名称、来源均依据《陨落王座》游戏策划文档。注意：策划文档中主线任务怪物为**石像小鬼 / 炎魔守卫 / 暗影骷髅王**，与 `quest-integration-contract.md` 中的 `EnemyType.Slime / Elite / Boss` 占位名不一致，需等任务系统同学统一枚举命名。

#### 武器（3种，**不进入背包**，随任务阶段自动升级替换）

| 物品 ID | 名称 | 攻击力 | 触发阶段 | 说明 |
| --- | --- | --- | --- | --- |
| `weapon_iron_blade` | 铁刀 | 较低 | 阶段 0-1 | 初始武器"刀刃略有卷口，但足够应对普通怪物"，主角出生即持有 |
| `weapon_steel_blade` | 刚刀 | 中等 | 阶段 2 | "铁匠锻造的优质单刀"，任务 1 完成后自动升级（商店同步解锁出售） |
| `weapon_legendary_blade` | 传说之刃 | 最高 | 阶段 3-4 | "属于勇者的武器，只有心怀正义之人才能发挥其强大力量"，任务 2 完成后自动升级 |

- 三种武器均为**单刀**（非弓箭），由 `WeaponManager` 管理，根据 `MainQuestManager.QuestStage` 自动切换
- 武器升级路径：铁刀 → 刚刀 → 传说之刃
- 战斗系统通过 `WeaponManager.Instance.OnWeaponChanged` 事件获取当前武器攻击力

#### 药水（3种，背包物品，一次性消耗品）

| 物品 ID | 名称 | 效果类型 | 效果值 | 来源 | 堆叠上限 |
| --- | --- | --- | --- | --- | --- |
| `potion_health` | 治理药水 | `HealthRestore`（回复血量） | +30 | 矮人铁匠商店购买 | 99 |
| `potion_mana` | 魔力药水 | `ManaRestore`（回复魔力） | +20 | 商店购买 / 掉落 | 99 |
| `potion_attack_boost` | 力量药水 | `AttackBoost`（提升攻击） | +5 | 矮人铁匠商店购买 | 99 |

- 调用 `UsePotion(itemId)` 消耗 1 个，触发 `OnPotionUsed` 事件通知战斗系统
- 玩家可通过快捷键 **1/2/3/4** 使用快捷栏中的药水（需战斗系统配合实现快捷栏绑定）
- 商店随任务 1 完成后解锁（治理药水、力量药水在售）

#### 任务道具（3种，背包物品，击杀任务目标掉落）

| 物品 ID | 名称 | 对应任务阶段 | 掉落来源 | 用途 | 堆叠上限 |
| --- | --- | --- | --- | --- | --- |
| `quest_stone_heart` | 石像心 | 阶段 1（击杀石像小鬼） | 石像小鬼（草原） | 提交给精灵图书管理员，证明第一段讨伐完成 | 99 |
| `quest_flame_demon_core` | 炎魔之核 | 阶段 2（击杀炎魔守卫） | 炎魔守卫（熔岩洞穴） | 提交给精灵图书管理员，"滚烫的熔岩石"，必掉落 | 99 |
| `quest_skeleton_crown` | 骷髅王冠 | 阶段 3（击杀暗影骷髅王） | 暗影骷髅王（骷髅王座） | 提交给精灵图书管理员，"已破裂但仍散发暗影气息" | 99 |

- 任务道具通过 `QuestRewardAdapter` 在任务阶段推进时自动添加到背包
- `InventoryRewardConfig` 中按阶段配置掉落
- 玩家需集齐三件任务道具后找任务 NPC（精灵图书管理员）交付

### 接口说明

`IInventoryService` 完整接口：

```csharp
public interface IInventoryService
{
    // 基础背包操作（无限容量）
    bool AddItem(string itemId, int count);
    int GetItemCount(string itemId);
    bool RemoveItem(string itemId, int count);
    bool HasItem(string itemId, int count = 1);
    InventorySlot[] GetAllSlots();

    // 药水使用
    bool UsePotion(string itemId);

    // 查询
    InventoryItemData GetItemDefinition(string itemId);

    // 事件
    event Action OnInventoryChanged;
    event Action<string, PotionEffectType, int> OnPotionUsed;
}
```

`WeaponManager` 独立接口（非背包）：

```csharp
// 单例入口
WeaponManager.Instance

// 属性
string CurrentWeaponId                    // 当前武器 ID
InventoryItemData CurrentWeaponData       // 当前武器数据（含 attackPower）

// 事件
event Action<string> OnWeaponChanged      // 武器切换通知（参数为 weaponId）
```

---

## 二、完成任务所需但目前缺失的资料

### 2.1 任务系统源码（关键依赖）

`QuestRewardAdapter` 和 `WeaponManager` 依赖 `MainQuestManager.Instance.QuestStageChanged` 事件。以下文件尚未实现：

| 缺失文件 | 影响 | 建议负责人 |
| --- | --- | --- |
| `MainQuestManager.cs` | `QuestRewardAdapter` 和 `WeaponManager` 无法订阅任务进度 | 任务系统同学 |
| `IMainQuestService.cs` | 接口定义未落地，编译无法通过 | 任务系统同学 |
| `EnemyType` 枚举定义 | 编译依赖。注意：策划文档敌人为石像小鬼/炎魔守卫/暗影骷髅王，与 contract 中的 Slime/Elite/Boss 不一致，需统一 | 任务系统同学 |

**当前处理**：`QuestRewardAdapter` 和 `WeaponManager` 均做了 `MainQuestManager.Instance` 的 null 检查，任务系统未就绪时静默跳过。

### 2.2 战斗/玩家系统（药水效果 + 武器攻击力接收方）

玩家属性（生命值、魔力值、攻击力、防御力、暴击率）需要战斗系统实现。药水和武器效果通过事件推送：

| 缺失内容 | 影响 | 接入方式 |
| --- | --- | --- |
| 玩家 HP/MP/攻击力 系统 | 药水效果无法实际生效 | 订阅 `InventoryManager.Instance.OnPotionUsed` |
| 武器攻击力应用 | 装备武器后攻击力不更新 | 订阅 `WeaponManager.Instance.OnWeaponChanged`，读取 `CurrentWeaponData.attackPower` |
| 快捷栏（数字键 1-4） | 快捷键使用药水无法触发 | 战斗系统需监听 1/2/3/4 键，调用 `InventoryManager.Instance.UsePotion()` |

```csharp
// 战斗系统接入示例
void Start()
{
    // 武器切换
    WeaponManager.Instance.OnWeaponChanged += (weaponId) =>
    {
        var data = WeaponManager.Instance.CurrentWeaponData;
        playerAttack = data != null ? data.attackPower : 0;
    };

    // 药水效果
    InventoryManager.Instance.OnPotionUsed += (itemId, effect, value) =>
    {
        switch (effect)
        {
            case PotionEffectType.AttackBoost:  playerAttack += value; break;
            case PotionEffectType.HealthRestore: playerHP += value;    break;
            case PotionEffectType.ManaRestore:   playerMP += value;    break;
        }
    };
}
```

### 2.3 商店系统

策划文档指定矮人铁匠商店出售刚刀、治理药水、力量药水。商店随任务 1 完成后解锁。背包侧已提供 `AddItem` / `RemoveItem` 接口，商店系统直接调用即可。

### 2.4 正式物品图标资源

`InventoryItemData` ScriptableObject 定义了 `Sprite icon` 字段，9 种物品需要美术提供图标。建议路径：

| 物品 ID | 名称 | 建议路径 |
| --- | --- | --- |
| `weapon_iron_blade` | 铁刀 | `Assets/Art/Items/weapon_iron_blade.png` |
| `weapon_steel_blade` | 刚刀 | `Assets/Art/Items/weapon_steel_blade.png` |
| `weapon_legendary_blade` | 传说之刃 | `Assets/Art/Items/weapon_legendary_blade.png` |
| `potion_health` | 治理药水 | `Assets/Art/Items/potion_health.png` |
| `potion_mana` | 魔力药水 | `Assets/Art/Items/potion_mana.png` |
| `potion_attack_boost` | 力量药水 | `Assets/Art/Items/potion_attack_boost.png` |
| `quest_stone_heart` | 石像心 | `Assets/Art/Items/quest_stone_heart.png` |
| `quest_flame_demon_core` | 炎魔之核 | `Assets/Art/Items/quest_flame_demon_core.png` |
| `quest_skeleton_crown` | 骷髅王冠 | `Assets/Art/Items/quest_skeleton_crown.png` |

### 2.5 背包 UI 界面

已提供 `BackpackUI`（Canvas 面板，消耗品/任务道具双 Tab）+ `InventoryUIOpener`（B 键开关）。使用 `Assets/GUI_Parts/` 中的素材。详细配置见 3.3 步骤 3。

待 UI 同学完善：
- 武器 HUD 显示（`BackpackUI.weaponHudIcon/Name/Atk` 已预留）
- 正式物品图标（当前使用 `skill_icon_01` / `stoune_icon` 占位）

### 2.6 Unity 项目工程

代码已放入 `Assets/CS/Inventory/`。需在 Unity Editor 中打开项目后编译，并创建 ScriptableObject 资产（见 3.3 配置步骤）。

---

## 三、实现思路及非代码部分完成说明

### 3.1 架构总览

```
┌─────────────────────────────────────────────────┐
│                   任务系统 (Quest)                │
│  MainQuestManager                                │
│  └─ QuestStageChanged event                     │
│  阶段 0: 未接任务                                 │
│  阶段 1: 击杀石像小鬼 (草原)                       │
│  阶段 2: 击杀炎魔守卫 (熔岩洞穴)                    │
│  阶段 3: 击杀暗影骷髅王 (骷髅王座)                   │
│  阶段 4: 主线完成                                 │
└──────┬──────────────────────┬───────────────────┘
       │                      │
       │ 订阅                  │ 订阅
       ▼                      ▼
┌──────────────┐    ┌──────────────────────────────┐
│ WeaponManager│    │   QuestRewardAdapter          │
│              │    │   - 监听 QuestStageChanged    │
│ 任务阶段→武器 │    │   - 读取 RewardConfig        │
│ 自动升级替换  │    │   - 调用 AddItem()           │
│              │    │                               │
│ 铁刀(0-1)    │    │   阶段1→石像心               │
│ 刚刀(2)      │    │   阶段2→炎魔之核             │
│ 传说之刃(3-4) │    │   阶段3→骷髅王冠             │
│              │    │                               │
│ OnWeaponChanged   └──────────┬───────────────────┘
│   → 战斗系统   │              │ 调用
└──────────────┘    ┌──────────▼───────────────────┐
                    │  InventoryManager (Singleton) │
                    │  ┌──────────┬──────────┐      │
                    │  │   药水    │ 任务道具  │      │
                    │  │ 治理/魔力 │ 石像心    │      │
                    │  │ /力量     │ 炎魔之核  │      │
                    │  │          │ 骷髅王冠  │      │
                    │  │ UsePotion│ 收集展示  │      │
                    │  │ 一次性    │          │      │
                    │  └──────────┴──────────┘      │
                    │  无限容量                       │
                    │  OnInventoryChanged → UI 刷新  │
                    │  OnPotionUsed → 战斗系统       │
                    └───────────────────────────────┘

游戏流程（策划文档）：
  小镇接任务 → 草原杀石像小鬼(得石像心,武器升刚刀) 
  → 熔岩洞穴杀炎魔守卫(得炎魔之核,武器升传说之刃)
  → 骷髅王座杀暗影骷髅王(得骷髅王冠) → 交任务→通关
```

### 3.2 文件清单

#### 正式代码（10 个）

| 文件 | 路径 | 说明 |
| --- | --- | --- |
| `IInventoryService.cs` | `Assets/CS/Inventory/` | 背包服务接口（药水/任务道具，不含武器） |
| `InventoryManager.cs` | `Assets/CS/Inventory/` | 背包管理器（单例，无限容量，启动时加载 Registry） |
| `InventorySlot.cs` | `Assets/CS/Inventory/` | 背包格子数据结构 |
| `InventoryItemData.cs` | `Assets/CS/Inventory/` | 物品定义 ScriptableObject（支持三类） |
| `InventoryItemRegistry.cs` | `Assets/CS/Inventory/` | 物品注册表 ScriptableObject（Editor 配置入口） |
| `InventoryRewardConfig.cs` | `Assets/CS/Inventory/` | 奖励配置 ScriptableObject |
| `QuestRewardAdapter.cs` | `Assets/CS/Inventory/` | 任务→背包奖励适配器 |
| `WeaponManager.cs` | `Assets/CS/Inventory/` | 武器管理器（任务阶段驱动自动升级） |
| `InventoryUIOpener.cs` | `Assets/CS/Inventory/` | B 键背包开关 |
| `BackpackUI.cs` | `Assets/CS/Inventory/` | Canvas 背包面板（使用 GUI_Parts 素材） |

#### 测试/调试代码（2 个）

| 文件 | 路径 | 说明 |
| --- | --- | --- |
| `InventoryTestHarness.cs` | `Assets/CS/Inventory/Test/` | 独立测试面板（无需 UI 即可测试背包功能） |
| `InventoryDebugTester.cs` | `Assets/CS/Inventory/Test/` | 调试面板（物品获取/消耗测试按钮） |

### 3.3 如何在 Unity Editor 中配置

> `InventoryManager` 和 `WeaponManager` 无需手动挂载——通过 `[RuntimeInitializeOnLoadMethod]` 自动创建，跨场景持久化。

---

#### A. 正式流程（Canvas UI：`BackpackUI` + `InventoryUIOpener`）

##### A-1. 创建物品定义 ScriptableObject（可选，不创建也能用测试面板模拟）

右键 → `Create` → `ARPG` → `Inventory` → `Item Data`，创建 9 份：

**药水（3份，ItemCategory = Potion）：**

| 文件名 | Item Id | Item Name | Effect Type | Effect Value | Max Stack |
| --- | --- | --- | --- | --- | --- |
| `Potion_Health.asset` | `potion_health` | 治理药水 | HealthRestore | 30 | 99 |
| `Potion_Mana.asset` | `potion_mana` | 魔力药水 | ManaRestore | 20 | 99 |
| `Potion_AttackBoost.asset` | `potion_attack_boost` | 力量药水 | AttackBoost | 5 | 99 |

**任务道具（3份，ItemCategory = QuestItem）：**

| 文件名 | Item Id | Item Name | Quest Stage | Max Stack |
| --- | --- | --- | --- | --- |
| `Quest_StoneHeart.asset` | `quest_stone_heart` | 石像心 | 1 | 99 |
| `Quest_FlameDemonCore.asset` | `quest_flame_demon_core` | 炎魔之核 | 2 | 99 |
| `Quest_SkeletonCrown.asset` | `quest_skeleton_crown` | 骷髅王冠 | 3 | 99 |

**武器（3份，ItemCategory = Weapon）：**

| 文件名 | Item Id | Item Name | Attack Power | Max Stack |
| --- | --- | --- | --- | --- |
| `Weapon_IronBlade.asset` | `weapon_iron_blade` | 铁刀 | 5 | 1 |
| `Weapon_SteelBlade.asset` | `weapon_steel_blade` | 刚刀 | 10 | 1 |
| `Weapon_LegendaryBlade.asset` | `weapon_legendary_blade` | 传说之刃 | 15 | 1 |

##### A-2. 创建奖励配置 ScriptableObject（可选）

右键 → `Create` → `ARPG` → `Inventory` → `Quest Reward Config`，添加奖励条目：

| Quest Stage | Item Id | Count | 说明 |
| --- | --- | --- | --- |
| 1 | `quest_stone_heart` | 1 | 击杀石像小鬼掉落 |
| 2 | `quest_flame_demon_core` | 1 | 击杀炎魔守卫掉落（策划要求必掉落） |
| 3 | `quest_skeleton_crown` | 1 | 击杀暗影骷髅王掉落 |

##### A-3. 注册物品定义（可选）

1. 右键 → `Create` → `ARPG` → `Inventory` → `Item Registry`，创建 `ItemRegistry.asset`
2. 将步骤 A-1 创建的 9 个 `InventoryItemData` 拖入 Registry 的 `Items` 列表
3. 将 `ItemRegistry.asset` 放入 `Assets/Resources/`（若目录不存在则手动创建）
4. 运行时 `InventoryManager` 自动通过 `Resources.Load` 加载

> 不创建 Registry 也没关系——测试流程会自动注入模拟数据，正式流程中调用 `AddItem` / `RemoveItem` 不受影响。

##### A-4. 场景搭建（逐步操作）

**第一步：创建 Canvas**

- Hierarchy 右键 → `UI` → `Canvas`
- 选中 Canvas，Inspector 确认 Render Mode = `Screen Space - Overlay`
- 如果 Hierarchy 中没有 `EventSystem`，Unity 会自动创建（必须有，否则按钮无响应）

**第二步：创建背包面板**

- 右键 Canvas → `UI` → `Panel`，重命名为 `BackpackPanel`
- 选中 `BackpackPanel`，Inspector 中 `Add Component` → 搜索 `BackpackUI` → 添加
- 此时 `BackpackUI` 组件出现在 Inspector 中，有 10 个 Sprite 字段待填充

**第三步：拖入素材（从 GUI_Parts 文件夹）**

在 Project 窗口依次找到以下文件，拖入 `BackpackUI` 对应字段：

| BackpackUI 字段 | 素材文件 | 在 Project 中的位置 |
| --- | --- | --- |
| `Panel Background` | `big_background` | `Assets/GUI_Parts/Gui_parts/big_background` |
| `Title Bar Sprite` | `name_bar` | `Assets/GUI_Parts/Gui_parts/name_bar` |
| `Close Button Sprite` | `button_cancel` | `Assets/GUI_Parts/Gui_parts/button_cancel` |
| `Slot Frame Sprite` | `Mini_frame0` | `Assets/GUI_Parts/Gui_parts/Mini_frame0` |
| `Tab Active Sprite` | `button_ready_on` | `Assets/GUI_Parts/Gui_parts/button_ready_on` |
| `Tab Inactive Sprite` | `button_ready_off` | `Assets/GUI_Parts/Gui_parts/button_ready_off` |
| `Use Button Sprite` | `button` | `Assets/GUI_Parts/Gui_parts/button` |
| `Potion Default Icon` | `skill_icon_01` | `Assets/GUI_Parts/Icons/skill_icon_01` |
| `Quest Default Icon` | `stoune_icon` | `Assets/GUI_Parts/Icons/stoune_icon` |

> 字段名以 Inspector 中实际显示为准。至少填入 `Panel Background` 和 `Slot Frame Sprite`，其余字段未填时使用灰色色块代替。

**第四步：创建背包开关**

- Hierarchy 右键 → `Create Empty`，重命名为 `BackpackOpener`
- 选中 `BackpackOpener` → `Add Component` → 搜索 `InventoryUIOpener` → 添加
- 将 Hierarchy 中的 `BackpackPanel` 拖入 `InventoryUIOpener` 的 `Backpack Panel` 字段
- 确认 `Toggle Key` 为 `B`

**第五步：运行验证**

1. 点击 Play 进入 Game 视图
2. 按 **B 键** → 屏幕中央弹出背包面板（标题栏 + 消耗品/任务道具标签 + 内容区）
3. 面板右上角 **X 按钮** 关闭，或再按 B 键关闭
4. Console 中应无报错

##### A-5. 场景结构总览

```
Scene
├── Canvas
│   └── BackpackPanel          ← BackpackUI 组件 + Image (big_background)
│       ├── TitleBar           ← [运行时自动创建]
│       │   ├── TitleText
│       │   └── CloseButton
│       ├── TabBar             ← [运行时自动创建]
│       │   ├── Tab_消耗品     ← 点击切换
│       │   └── Tab_任务道具   ← 点击切换
│       └── ScrollView         ← [运行时自动创建]
│           └── Viewport
│               └── Content    ← 槽位列表
├── BackpackOpener             ← InventoryUIOpener 组件
└── EventSystem                ← Unity 自动创建
```

---

#### B. 测试流程（OnGUI 测试面板：`InventoryDebugTester`）

> 测试流程与正式流程**互斥**——场景中只能保留一种背包 UI。测试完成后删除 `InventoryDebugTester`，切换至正式流程。

##### B-1. 场景搭建

- Hierarchy 右键 → `Create Empty`，重命名为 `InventoryDebug`
- 选中 `InventoryDebug` → `Add Component` → 搜索 `InventoryDebugTester` → 添加
- **无需**拖入任何素材（纯文本 OnGUI 面板，自动注入 9 种模拟数据）

##### B-2. 运行验证

1. 点击 Play
2. 右侧出现 `物品测试面板`（240px 宽，始终可见）：
   - **物品增减**：每行 `+1 物品名` + `(数量) -1` 双按钮，共 9 行
   - **药水使用**：治理药水 / 魔力药水 / 力量药水的使用按钮
   - **批量**：药水各+3 / 任务道具各+1 / 药水各+10 / 清空全部
   - **武器**：显示当前装备武器 ID 和 ATK
3. 点击 `+1 治理药水` → 按钮上数量从 0 变为 1
4. Console 输出 `[InventoryDebugTester] Mock item definitions injected (9 items).`

##### B-3. 与正式流程配合使用

如果希望同时使用测试面板（加物品）和正式 Canvas UI（看效果）：

1. 场景中同时保留 `InventoryDebug`（右侧测试面板）和正式流程的 Canvas + BackpackOpener
2. 点击 Play → 右侧面板点 `+1` 加物品 → 按 **B 键** 打开正式 Canvas 背包 → 消耗品/任务道具切换查看

> 注意：`InventoryDebugTester` 已不再监听 B 键，不会与正式面板冲突。

### 3.4 武器自动升级流程

```
任务推进 QuestStage 变化
    │
    ▼
WeaponManager.OnQuestStageChanged()
    │
    ├─ 遍历 weaponProgression[]
    ├─ 找 questStage <= currentStage 的最大条目
    │   Stage 0-1 → 铁刀 ("刀刃略有卷口")
    │   Stage 2   → 刚刀 ("铁匠锻造的优质单刀")
    │   Stage 3-4 → 传说之刃 ("心怀正义之人才能发挥力量")
    │
    ├─ 与当前武器比较，相同则跳过
    ├─ 更新 _currentWeaponId
    ├─ 触发 OnWeaponChanged(newWeaponId)
    │       │
    │       └─ 战斗系统读取 CurrentWeaponData.attackPower 更新玩家攻击力
    └─ 输出日志
```

### 3.5 药水使用流程

```
玩家点击药水 [使用] 或按快捷键 1/2/3/4
    │
    ▼
InventoryManager.UsePotion("potion_health")
    ├─ 验证物品存在且 itemCategory == Potion
    ├─ RemoveItem("potion_health", 1)
    ├─ 触发 OnPotionUsed(itemId, HealthRestore, 30)
    │       │
    │       └─ 战斗系统收到事件 → 玩家 HP += 30
    └─ 触发 OnInventoryChanged() → UI 刷新
```

### 3.6 与设计文档中交互逻辑的对应

| 策划描述 | 背包系统实现 |
| --- | --- |
| 精灵图书管理员 NPC 发任务 | 任务系统 (`QuestNpcInteraction`)，背包侧不涉及 |
| 击杀石像小鬼→得石像心 | `QuestRewardAdapter` 在阶段 1 时 `AddItem("quest_stone_heart", 1)` |
| 击杀炎魔守卫→得炎魔之核（必掉落） | `QuestRewardAdapter` 在阶段 2 时 `AddItem("quest_flame_demon_core", 1)` |
| 击杀暗影骷髅王→得骷髅王冠 | `QuestRewardAdapter` 在阶段 3 时 `AddItem("quest_skeleton_crown", 1)` |
| 找精灵图书管理员交付任务道具 | 任务系统负责（校验 `HasItem`），背包侧提供查询接口 |
| 矮人铁匠商店买药水 | 商店系统调用 `InventoryManager.Instance.AddItem(itemId, count)` |
| 铁刀自动升级刚刀 | `WeaponManager` 在阶段 2 时自动切换 |
| 刚刀自动升级传说之刃 | `WeaponManager` 在阶段 3 时自动切换 |
| B 键开背包 | `InventoryDebugTester`（测试期）/ `InventoryUIOpener`（正式 Canvas UI） |

### 3.7 背包 UI 使用

#### 正式 UI（BackpackUI + InventoryUIOpener）

基于 Unity Canvas 的正式背包面板。必须完成 3.3-A 的场景搭建才能使用。

**操作方式：**
- 按 **B 键** 开关背包面板
- 点击标题栏下方 **消耗品 / 任务道具** 标签切换分类
- 药水行右侧 **使用** 按钮消耗 1 瓶
- 右上角 **X** 关闭面板

**Tab 过滤逻辑：**

| 标签 | 显示 |
| --- | --- |
| 消耗品 | `ItemCategory == Potion` 的物品 |
| 任务道具 | `ItemCategory == QuestItem` 的物品 |

**无物品时**：内容区显示灰色 `暂无消耗品` 或 `暂无任务道具` 提示。

#### 测试面板（InventoryDebugTester）

OnGUI 渲染的纯文本测试面板。仅需在场景中挂载一个组件即可使用（见 3.3-B）。

**操作方式：**
- 面板固定在屏幕右侧，始终可见
- 点击 `+1 物品名` 向背包添加 1 个
- 点击 `(数量) -1` 移除 1 个
- 药水区点击 `使用` 消耗药水
- 批量按钮快速填充或清空背包

> 测试面板不显示物品图标，仅用于增减物品和验证数据流。视觉效果需通过正式 Canvas UI 查看。

### 3.8 UI 接入建议

策划文档要求背包 UI 分三大 Tab（武器、消耗品、任务道具），武器在 HUD 显示。`BackpackUI` 已实现消耗品/任务道具双 Tab，武器通过 `weaponHudIcon` / `weaponHudName` / `weaponHudAtk` 字段在 HUD 显示。

```csharp
// 战斗系统接入示例：通过 InventoryUIOpener + BackpackUI 即可使用。
// BackpackUI 已内部订阅 OnInventoryChanged / OnWeaponChanged 自动刷新。
// 外部模块只需调用 InventoryManager API 即可驱动 UI 更新。
```

### 3.9 测试方案

#### 正式 UI 测试

1. 按 A-4 搭建正式场景
2. 启动 Play → 按 B 键 → 背包面板从屏幕中央弹出
3. 确认标题栏显示"背 包"，X 按钮可关闭
4. 确认 TabBar 显示"消耗品"和"任务道具"两个标签（选中高亮、未选中灰色）
5. 通过右侧测试面板点 `+1 治理药水` → 切换到背包面板的"消耗品"标签 → 应看到治理药水条目
6. 切换至"任务道具"标签 → 应显示"暂无任务道具"
7. 点 `任务道具各+1` → 切回"任务道具"标签 → 应看到石像心、炎魔之核、骷髅王冠三条
8. 点药水的"使用"按钮 → 数量减 1 → Console 输出 `[InventoryManager] Used potion: ...`

#### 快速功能测试（仅测数据流，无需 UI 素材）

1. 场景中仅挂载 `InventoryDebugTester`
2. 点 `+1 治理药水` → 按钮数字从 0 变为 1 → Console 无报错
3. 点 `(1) -1` → 数字回到 0
4. 点 `药水各+3` → 三种药水各显示 3
5. 点 `使用 治理药水 (3)` → 数量变为 2 → Console 输出 `OnPotionUsed`
6. 点 `清空全部` → 所有数量归零

#### 集成测试（需任务系统就绪）

- **武器自动升级**：推进阶段 → `CurrentWeaponId` 依次为铁刀→刚刀→传说之刃
- **任务道具获取**：阶段 1/2/3 → 背包依次获得石像心/炎魔之核/骷髅王冠
- **交付校验**：`HasItem` 正确校验三件任务道具

### 3.10 接入顺序

#### 正式接入

| 步骤 | 操作 | 产出 |
| --- | --- | --- |
| 1 | 将所有 `.cs` 放入 `Assets/CS/Inventory/` | 编译通过 |
| 2 | 创建 9 份 `InventoryItemData` (3.3-A-1) | 物品定义资产 |
| 3 | 创建 `ItemRegistry` 并注册，放入 `Resources/` (3.3-A-3) | 自动加载配置 |
| 4 | 搭建 Canvas + `BackpackUI` (3.3-A-4 第一~三步) | 背包面板 |
| 5 | 创建 `BackpackOpener` + `InventoryUIOpener` (3.3-A-4 第四步) | B 键开关 |
| 6 | 运行 → B 键开背包 → 验收 | 功能可用 |

#### 快速测试（跳过 ScriptableObject 创建）

| 步骤 | 操作 | 产出 |
| --- | --- | --- |
| 1 | 场景中创建空 GameObject `InventoryDebug` | — |
| 2 | 挂载 `InventoryDebugTester` 组件 | 右侧测试面板 |
| 3 | （可选）同时搭建正式 UI（见正式接入步骤 4-5） | 配合查看效果 |
| 4 | 运行 → 点 `+1` 加物品 → 按 B 看效果 | 功能验证 |
