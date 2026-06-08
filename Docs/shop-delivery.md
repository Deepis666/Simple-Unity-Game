# 商店模块交付文档

## 一、接口文档要求交付的必填信息

### 商店组回复模板

| 项目 | 回复 |
| --- | --- |
| 货币服务入口 | `CurrencyManager.Instance` |
| 货币类型 | 金币（Gold），单一货币体系 |
| 初始金额 | 100 金币（`CurrencyManager.initialGold` 可配置） |
| 金币来源 | 初始自带 + 击杀敌人掉落（`EnemyDummy.goldDrop`） |
| 购买商品 | 三种药水：治理药水（30G）、魔力药水（20G）、力量药水（25G）（由 `ShopConfig` 配置） |
| 购买接口 | 通过 `ShopUI` 面板点击"购买"按钮，内部调用 `CurrencyManager.SpendGold()` + `InventoryManager.AddItem()` |
| 交互方式 | 靠近商店 NPC → 按 **E 键** 打开商店面板（`ShopNpcInteraction` 自动添加触发碰撞体） |

### 资源交付格式 - 必填信息

| 模块 | 必填信息 |
| --- | --- |
| 商店 | **货币入口**：`CurrencyManager.Instance`<br>**接口**：`Gold` / `SpendGold` / `AddGold` / `HasGold` / `OnGoldChanged`<br>**商品配置**：`ShopConfig` ScriptableObject（物品 ID + 价格）<br>**商店面板**：`ShopUI`（Canvas 面板，与背包面板同风格）<br>**交互方式**：`ShopNpcInteraction`（触发器 + E 键）<br>**打怪掉落**：`EnemyDummy.Die()` → `CurrencyManager.AddGold(goldDrop)` |

---

## 二、架构总览

```
                    ┌─────────────────────────────┐
                    │     CurrencyManager           │
                    │     (Singleton, 跨场景)        │
                    │                               │
                    │  Gold: int                    │
                    │  SpendGold(int) → bool        │
                    │  AddGold(int)                 │
                    │  OnGoldChanged event          │
                    └──────┬──────────────────┬─────┘
                           │                  │
              金币不足 / 购买成功       击杀掉落金币
                           │                  │
              ┌────────────▼─────┐   ┌───────▼────────┐
              │     ShopUI        │   │  EnemyDummy     │
              │  (Canvas 商店面板) │   │  (测试敌人)     │
              │                   │   │                 │
              │  ShopConfig       │   │  maxHealth      │
              │  → 商品列表        │   │  goldDrop       │
              │  购买按钮          │   │  TakeDamage()   │
              │  金币显示          │   │  Die()          │
              └────────┬──────────┘   └────────────────┘
                       │
              E 键交互 (触发器)
                       │
           ┌───────────▼──────────┐
           │ ShopNpcInteraction    │
           │  (挂在商店 NPC 上)    │
           │                       │
           │  OnTriggerEnter → 提示│
           │  E 键 → ShopUI.Toggle│
           └───────────────────────┘

     ┌──────────────────────────────────────────┐
     │            InventoryManager               │
     │  AddItem()  ← ShopUI 购买时调用           │
     └──────────────────────────────────────────┘
```

---

## 三、文件清单

### 正式代码（6 个）

| 文件 | 路径 | 说明 |
| --- | --- | --- |
| `ICurrencyService.cs` | `Assets/CS/Shop/` | 货币服务接口 |
| `CurrencyManager.cs` | `Assets/CS/Shop/` | 货币管理器（单例，初始 100 金币，跨场景持久） |
| `ShopConfig.cs` | `Assets/CS/Shop/` | 商店配置 ScriptableObject（商品 ID + 价格列表） |
| `ShopUI.cs` | `Assets/CS/Shop/` | Canvas 商店面板（商品列表 + 购买按钮 + 金币显示） |
| `ShopNpcInteraction.cs` | `Assets/CS/Shop/` | 商店 NPC 交互（触发器 + E 键打开商店） |

### 测试代码（2 个）

| 文件 | 路径 | 说明 |
| --- | --- | --- |
| `PlayerController.cs` | `Assets/CS/Shop/Test/` | 立方体玩家移动（WASD）+ 左键攻击 + E 交互 |
| `EnemyDummy.cs` | `Assets/CS/Shop/Test/` | 测试敌人（受击扣血 → 死亡掉落金币） |

---

## 四、接口说明

### 4.1 CurrencyManager（货币核心）

**入口**：`CurrencyManager.Instance`（`[RuntimeInitializeOnLoadMethod]` 自动创建）

| 成员 | 类型 | 用途 |
| --- | --- | --- |
| `Gold` | `int` 属性 | 当前金币数量（只读） |
| `SpendGold(int amount)` | `bool` | 扣除金币，返回是否成功（余额不足返回 false） |
| `AddGold(int amount)` | `void` | 增加金币（敌人掉落、任务奖励等） |
| `HasGold(int amount)` | `bool` | 检查是否有足够金币 |
| `OnGoldChanged` | `event Action<int>` | 金币变化事件，参数为当前余额 |

```csharp
// 购买物品示例
if (CurrencyManager.Instance.SpendGold(price))
{
    InventoryManager.Instance.AddItem(itemId, 1);
}

// 击杀敌人掉落金币
CurrencyManager.Instance.AddGold(10);
```

### 4.2 ShopConfig（商店配置）

**创建**：右键 → `Create` → `ARPG` → `Shop` → `Shop Config`

| 字段 | 说明 |
| --- | --- |
| `Items` | 商品列表，每条含 `Item Id`（字符串）和 `Price`（价格） |

建议配置：

| Item Id | Price | 说明 |
| --- | --- | --- |
| `potion_health` | 30 | 治理药水 |
| `potion_mana` | 20 | 魔力药水 |
| `potion_attack_boost` | 25 | 力量药水 |

### 4.3 ShopUI（商店面板）

**挂载方式**：与 `BackpackUI` 相同——Canvas 子节点 → 挂载 `ShopUI` → 拖入 Sprite 素材和 `ShopConfig`。

| 字段 | 说明 |
| --- | --- |
| `shopConfig` | 拖入 `ShopConfig` 资产 |
| `Panel Background` 等 Sprite 字段 | 同 `BackpackUI` 素材对照表 |
| `Gold Text` | 金币显示文本（运行时自动创建，无需赋值） |

**API**：

| 方法 | 用途 |
| --- | --- |
| `Toggle()` | 开关面板（`ShopNpcInteraction` 调用） |
| `Open()` | 打开面板并刷新 |
| `Close()` | 关闭面板 |

### 4.4 ShopNpcInteraction（NPC 交互）

**挂载方式**：商店 NPC GameObject 上挂载此组件 + Collider（IsTrigger = true）+ 引用 `ShopUI`。

| 字段 | 说明 |
| --- | --- |
| `shopUI` | 拖入 `ShopUI` 组件所在 GameObject |
| `interactKey` | 交互快捷键，默认 `KeyCode.E` |
| `promptUI` | 交互提示 UI（可选，运行时显示"按 [E] 打开商店"） |

**逻辑**：
- 玩家（Tag = "Player"）进入触发器 → 显示提示
- 按 E 键 → `shopUI.Toggle()`
- 玩家离开触发器 → 自动关闭商店

---

## 五、Unity Editor 配置步骤

### 5.1 正式流程

#### 第一步：创建 ShopConfig

1. 右键 → `Create` → `ARPG` → `Shop` → `Shop Config`，命名为 `ShopConfig`
2. Inspector 中设置 Items 列表：

| Item Id | Price |
| --- | --- |
| `potion_health` | 30 |
| `potion_mana` | 20 |
| `potion_attack_boost` | 25 |

#### 第二步：创建商店 Canvas 面板

1. Hierarchy 右键 → `UI` → `Canvas`（如果已有 Canvas 则复用）
2. 右键 Canvas → `UI` → `Panel`，重命名为 `ShopPanel`
3. 选中 `ShopPanel` → `Add Component` → 搜索 `ShopUI` → 添加
4. 将 `ShopConfig` 资产拖入 `ShopUI` 的 `Shop Config` 字段
5. 拖入 GUI_Parts 素材（同 BackpackUI 素材对照表）：

| ShopUI 字段 | 素材文件 |
| --- | --- |
| `Panel Background` | `GUI_Parts/Gui_parts/big_background` |
| `Title Bar Sprite` | `GUI_Parts/Gui_parts/name_bar` |
| `Close Button Sprite` | `GUI_Parts/Gui_parts/button_cancel` |
| `Slot Frame Sprite` | `GUI_Parts/Gui_parts/Mini_frame0` |
| `Buy Button Sprite` | `GUI_Parts/Gui_parts/button` |
| `Potion Default Icon` | `GUI_Parts/Icons/skill_icon_01` |

#### 第三步：创建商店 NPC

1. Hierarchy 右键 → `3D Object` → `Cube`，重命名为 `ShopNpc`
2. 选中 `ShopNpc` → `Add Component` → `ShopNpcInteraction`
3. 将 Hierarchy 中的 `ShopPanel` 拖入 `ShopUI` 字段
4. **不要勾选** Cube 自带 BoxCollider 的 `Is Trigger`（物理碰撞靠它站立）
5. `ShopNpcInteraction` 启动时自动添加 `SphereCollider`（IsTrigger）用于交互检测

#### 第四步：创建玩家（测试用）

1. Hierarchy 右键 → `3D Object` → `Cube`，重命名为 `Player`
2. 选中 `Player` → `Add Component` → `PlayerController`
3. `Add Component` → `CharacterController`（PlayerController 会自动添加，手动也行）
4. 确认 Tag 为 `Player`（Inspector 顶部 Tag 下拉选择，如无则 Add Tag 新建）
5. 将 Main Camera 设为 Player 子节点或独立（独立时需确保 Camera.main 可见）

#### 第五步：创建敌人（测试用）

1. Hierarchy 右键 → `3D Object` → `Cube`，重命名为 `Enemy`
2. 放在玩家可达区域
3. 选中 `Enemy` → `Add Component` → `EnemyDummy`
4. 设置 `Max Health = 3`，`Gold Drop = 10`

#### 第六步：运行验收

1. 点击 Play
2. WASD 移动玩家靠近商店 NPC（黄色 Gizmo 圈为交互范围）
3. Console 输出 `[ShopNpc] Player entered shop range. Press E to open.`
4. 按 **E 键** → 商店面板弹出，显示三种药水及价格、当前金币（100）
5. 点击"购买" → 金币减少、物品加入背包
6. 金币不足时按钮变为灰色"金币不足"
7. 按 **B 键** 打开背包确认物品已添加
8. 找到敌人 → 鼠标左键点击攻击 3 次 → 敌人死亡 → Console 输出 `Dropped 10 gold.` → 金币增加

---

### 5.2 场景结构总览

```
Scene
├── Canvas
│   ├── BackpackPanel          ← BackpackUI（背包）
│   └── ShopPanel              ← ShopUI（商店）
├── ShopNpc (Cube)             ← ShopNpcInteraction + BoxCollider(IsTrigger)
├── Player (Cube)              ← PlayerController + CharacterController + Tag=Player
├── Enemy (Cube)               ← EnemyDummy
├── BackpackOpener             ← InventoryUIOpener（B 键开关背包）
├── InventoryDebug             ← InventoryDebugTester（右侧测试面板）
└── EventSystem                ← Unity 自动创建
```

---

## 六、交互流程

```
玩家靠近商店 NPC
    │
    ▼ (OnTriggerEnter)
显示"按 [E] 打开商店"
    │
    ▼ (按 E)
ShopUI 弹出
    │
    ├─ 显示商品列表（名称 + 价格 + 购买按钮）
    ├─ 显示当前金币
    ├─ 金币足够：按钮绿色"购买"
    └─ 金币不足：按钮灰色"金币不足"，不可点击
         │
         ▼ (点击购买)
    CurrencyManager.SpendGold(price)
         │
         ├─ 成功 → InventoryManager.AddItem(itemId, 1)
         └─ 失败 → 不扣金币，不添加物品
              │
              ▼
         刷新 UI（金币 + 购买按钮状态）
    │
    ▼ (玩家离开触发器 或 按 X 关闭)
ShopUI 关闭
```

---

## 七、与其他模块的接口关系

| 调用方 | 调用的接口 | 场景 |
| --- | --- | --- |
| `ShopUI` | `CurrencyManager.SpendGold()` | 购买商品时扣除金币 |
| `ShopUI` | `InventoryManager.AddItem()` | 购买成功后添加物品到背包 |
| `ShopUI` | `InventoryManager.OnInventoryChanged` | 订阅以刷新面板 |
| `ShopUI` | `CurrencyManager.OnGoldChanged` | 订阅以刷新金币显示 |
| `EnemyDummy` | `CurrencyManager.AddGold()` | 敌人死亡时掉落金币 |
| `PlayerController` | `EnemyDummy.TakeDamage()` | 玩家攻击敌人 |
| `ShopNpcInteraction` | `ShopUI.Toggle()` | E 键开关商店面板 |

---

## 八、测试方案

### 商店交互测试

1. Play → WASD 移动玩家到商店 NPC 旁 → Console 输出 `Player entered shop range`
2. 按 E → 商店面板弹出，显示三种商品和金币 100
3. 点击"治理药水"的购买按钮 → 金币变为 70 → 背包增加 1 瓶治理药水
4. 连续购买 3 次魔力药水（60G）→ 金币变为 10 → 所有"购买"按钮变为"金币不足"
5. 按 B → 背包面板应显示 1 瓶治理药水 + 3 瓶魔力药水
6. 按 E 或离开 NPC → 商店关闭

### 战斗掉落测试

1. 玩家移动到敌人旁
2. 鼠标左键点击敌人 3 次 → Console 每次输出剩余 HP → 第 3 次输出 `Dropped 10 gold.`
3. 金币变为 110 → 商店面板按钮恢复可购买状态

### 边界测试

- 金币正好等于价格时能否购买
- 金币为 0 时显示"金币不足"
- 玩家远离 NPC 时商店自动关闭
- 商店面板 X 按钮正常关闭

---

## 九、接入清单

| 模块 | 需要做的事 | 调用的接口 |
| --- | --- | --- |
| **战斗系统** | 敌人死亡时调用 `CurrencyManager.AddGold(goldAmount)` | `AddGold` |
| **背包系统** | 已提供 `AddItem` —— 商店购买时自动调用 | 无需额外操作 |
| **任务系统** | 可将金币作为任务奖励发放 | `AddGold` |
| **地图组** | 提供商店 NPC 点位 | 挂载 `ShopNpcInteraction` |
| **UI 组** | 正式商店面板可使用 `ShopUI` 或替换 | — |
