using System.Collections.Generic;
using UnityEngine;

public class InventoryTestHarness : MonoBehaviour
{
    private bool _showBackpack;
    private bool _initialized;
    private Vector2 _scrollPos;

    private List<InventoryItemData> _mockDefinitions = new List<InventoryItemData>();

    private void Start()
    {
        SetupTestEnvironment();
    }

    private void SetupTestEnvironment()
    {
        if (_initialized) return;

        if (InventoryManager.Instance == null)
        {
            GameObject go = new GameObject("InventoryManager");
            go.AddComponent<InventoryManager>();
        }

        CreateMockItemDefinitions();
        RegisterMockDefinitions();

        _initialized = true;
        Debug.Log("[InventoryTestHarness] Test environment ready. Press B to open backpack.");
    }

    private void CreateMockItemDefinitions()
    {
        _mockDefinitions.Clear();

        _mockDefinitions.Add(CreateMockItem("potion_health", "治理药水",
            ItemCategory.Potion, PotionEffectType.HealthRestore, 30));
        _mockDefinitions.Add(CreateMockItem("potion_mana", "魔力药水",
            ItemCategory.Potion, PotionEffectType.ManaRestore, 20));
        _mockDefinitions.Add(CreateMockItem("potion_attack_boost", "力量药水",
            ItemCategory.Potion, PotionEffectType.AttackBoost, 5));
        _mockDefinitions.Add(CreateMockItem("quest_stone_heart", "石像心",
            ItemCategory.QuestItem, default, 0));
        _mockDefinitions.Add(CreateMockItem("quest_flame_demon_core", "炎魔之核",
            ItemCategory.QuestItem, default, 0));
        _mockDefinitions.Add(CreateMockItem("quest_skeleton_crown", "骷髅王冠",
            ItemCategory.QuestItem, default, 0));
        _mockDefinitions.Add(CreateMockItem("weapon_iron_blade", "铁刀",
            ItemCategory.Weapon, default, 0, 5));
        _mockDefinitions.Add(CreateMockItem("weapon_steel_blade", "刚刀",
            ItemCategory.Weapon, default, 0, 10));
        _mockDefinitions.Add(CreateMockItem("weapon_legendary_blade", "传说之刃",
            ItemCategory.Weapon, default, 0, 15));
    }

    private InventoryItemData CreateMockItem(string id, string name, ItemCategory cat,
        PotionEffectType effect = default, int effectVal = 0, int atk = 0)
    {
        InventoryItemData data = ScriptableObject.CreateInstance<InventoryItemData>();
        data.itemId = id;
        data.itemName = name;
        data.itemCategory = cat;
        data.maxStack = 99;
        if (cat == ItemCategory.Weapon)
        {
            data.maxStack = 1;
            data.attackPower = atk;
        }
        if (cat == ItemCategory.Potion)
        {
            data.potionEffectType = effect;
            data.effectValue = effectVal;
        }
        return data;
    }

    private void RegisterMockDefinitions()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.itemDefinitions.Clear();
        for (int i = 0; i < _mockDefinitions.Count; i++)
        {
            InventoryManager.Instance.itemDefinitions.Add(_mockDefinitions[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            _showBackpack = !_showBackpack;
            Debug.Log(string.Format("[InventoryTestHarness] Backpack {0}",
                _showBackpack ? "opened" : "closed"));
        }
    }

    private void OnGUI()
    {
        if (!_initialized) return;

        DrawToolbar();

        if (_showBackpack)
        {
            DrawBackpackPanel();
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 30));
        GUILayout.BeginHorizontal();
        GUI.color = _showBackpack ? Color.green : Color.white;
        if (GUILayout.Button(_showBackpack ? "[B] Close Backpack" : "[B] Open Backpack",
            GUILayout.Height(25)))
        {
            _showBackpack = !_showBackpack;
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawBackpackPanel()
    {
        int panelW = Mathf.Min(900, Screen.width - 40);
        int panelH = Mathf.Min(620, Screen.height - 40);
        int panelX = (Screen.width - panelW) / 2;
        int panelY = (Screen.height - panelH) / 2;

        Rect panelRect = new Rect(panelX, panelY, panelW, panelH);
        GUI.Box(panelRect, "Backpack Test Panel");
        GUI.BeginGroup(panelRect);

        int leftW = panelW * 3 / 5;
        DrawInventorySlots(10, 25, leftW - 20, panelH - 35);
        DrawTestButtons(leftW + 5, 25, panelW - leftW - 15, panelH - 35);

        GUI.EndGroup();
    }

    private void DrawInventorySlots(int x, int y, int w, int h)
    {
        GUI.Box(new Rect(x, y, w, h), "");

        if (InventoryManager.Instance == null)
        {
            GUI.Label(new Rect(x + 10, y + 10, w - 20, 25), "InventoryManager not found.");
            return;
        }

        InventorySlot[] slots = InventoryManager.Instance.GetAllSlots();

        GUI.Label(new Rect(x + 10, y + 5, 200, 20),
            string.Format("Backpack ({0} items)", slots.Length));

        int listY = y + 28;
        int listH = h - 36;
        int contentH = Mathf.Max(listH, slots.Length * 28 + 10);

        _scrollPos = GUI.BeginScrollView(
            new Rect(x + 5, listY, w - 10, listH),
            _scrollPos,
            new Rect(0, 0, w - 25, contentH));

        if (slots.Length == 0)
        {
            GUI.Label(new Rect(10, 10, w - 30, 25), "(empty)");
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty) continue;

            InventoryItemData def = InventoryManager.Instance.GetItemDefinition(slots[i].itemId);
            string displayName = def != null ? def.itemName : slots[i].itemId;
            string categoryLabel = def != null ? def.itemCategory.ToString() : "?";

            int rowY = 5 + i * 28;

            Color rowColor = Color.white;
            if (def != null && def.itemCategory == ItemCategory.Potion)
                rowColor = new Color(0.7f, 1f, 0.7f);
            else if (def != null && def.itemCategory == ItemCategory.QuestItem)
                rowColor = new Color(0.7f, 0.7f, 1f);

            GUI.color = rowColor;
            GUI.Box(new Rect(5, rowY, w - 30, 25), "");
            GUI.color = Color.white;

            GUI.Label(new Rect(10, rowY + 3, w - 300, 20),
                string.Format("[{0}] {1}", categoryLabel, displayName));
            GUI.Label(new Rect(w - 280, rowY + 3, 60, 20),
                string.Format("x{0}", slots[i].count), GetRightAlignStyle());

            InventoryItemData slotDef = InventoryManager.Instance.GetItemDefinition(slots[i].itemId);
            if (slotDef != null && slotDef.itemCategory == ItemCategory.Potion && slots[i].count > 0)
            {
                if (GUI.Button(new Rect(w - 130, rowY + 1, 55, 22), "Use"))
                {
                    InventoryManager.Instance.UsePotion(slots[i].itemId);
                }
            }

            if (slots[i].count > 1)
            {
                if (GUI.Button(new Rect(w - 70, rowY + 1, 55, 22), "-1"))
                {
                    InventoryManager.Instance.RemoveItem(slots[i].itemId, 1);
                }
            }
        }

        GUI.EndScrollView();
    }

    private void DrawTestButtons(int x, int y, int w, int h)
    {
        GUI.Box(new Rect(x, y, w, h), "Test Controls");

        int btnH = 28;
        int pad = 8;
        int curY = y + 5;
        int innerW = w - 16;

        GUI.Label(new Rect(x + pad, curY, innerW, 20), "--- Potions ---");
        curY += 22;

        for (int i = 0; i < _mockDefinitions.Count; i++)
        {
            InventoryItemData def = _mockDefinitions[i];
            if (def.itemCategory != ItemCategory.Potion) continue;

            int count = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemCount(def.itemId) : 0;

            if (GUI.Button(new Rect(x + pad, curY, innerW / 2 - 4, btnH),
                string.Format("+1 {0}", def.itemName)))
            {
                InventoryManager.Instance.AddItem(def.itemId, 1);
            }
            if (count > 0 && GUI.Button(new Rect(x + pad + innerW / 2 + 4, curY, innerW / 2 - 4, btnH),
                string.Format("Use ({0})", count)))
            {
                InventoryManager.Instance.UsePotion(def.itemId);
            }
            curY += btnH + 3;
        }

        curY += 8;
        GUI.Label(new Rect(x + pad, curY, innerW, 20), "--- Quest Items ---");
        curY += 22;

        for (int i = 0; i < _mockDefinitions.Count; i++)
        {
            InventoryItemData def = _mockDefinitions[i];
            if (def.itemCategory != ItemCategory.QuestItem) continue;

            int count = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemCount(def.itemId) : 0;

            if (GUI.Button(new Rect(x + pad, curY, innerW, btnH),
                string.Format("+1 {0} ({1})", def.itemName, count)))
            {
                InventoryManager.Instance.AddItem(def.itemId, 1);
            }
            curY += btnH + 3;
        }

        curY += 8;
        GUI.Label(new Rect(x + pad, curY, innerW, 20), "--- Bulk Add ---");
        curY += 22;

        if (GUI.Button(new Rect(x + pad, curY, innerW, btnH), "Add All Potions x3"))
        {
            InventoryManager.Instance.AddItem("potion_health", 3);
            InventoryManager.Instance.AddItem("potion_mana", 3);
            InventoryManager.Instance.AddItem("potion_attack_boost", 3);
        }
        curY += btnH + 3;

        if (GUI.Button(new Rect(x + pad, curY, innerW, btnH), "Add All Quest Items x1"))
        {
            InventoryManager.Instance.AddItem("quest_stone_heart", 1);
            InventoryManager.Instance.AddItem("quest_flame_demon_core", 1);
            InventoryManager.Instance.AddItem("quest_skeleton_crown", 1);
        }
        curY += btnH + 3;

        if (GUI.Button(new Rect(x + pad, curY, innerW / 2 - 4, btnH), "+10 Potions each"))
        {
            InventoryManager.Instance.AddItem("potion_health", 10);
            InventoryManager.Instance.AddItem("potion_mana", 10);
            InventoryManager.Instance.AddItem("potion_attack_boost", 10);
        }
        if (GUI.Button(new Rect(x + pad + innerW / 2 + 4, curY, innerW / 2 - 4, btnH), "Clear All"))
        {
            ClearAll();
        }
        curY += btnH + 3;

        curY += 8;
        GUI.Label(new Rect(x + pad, curY, innerW, 20), "--- Weapon Info ---");
        curY += 22;

        string weaponStatus;
        if (WeaponManager.Instance == null)
        {
            weaponStatus = "WeaponManager not available";
        }
        else
        {
            string wid = WeaponManager.Instance.CurrentWeaponId;
            weaponStatus = string.IsNullOrEmpty(wid) ? "No weapon" : wid;
        }
        GUI.Label(new Rect(x + pad, curY, innerW, 20),
            string.Format("Equipped: {0}", weaponStatus));
        curY += 20;

        if (WeaponManager.Instance != null && WeaponManager.Instance.CurrentWeaponData != null)
        {
            GUI.Label(new Rect(x + pad, curY, innerW, 20),
                string.Format("ATK: {0}", WeaponManager.Instance.CurrentWeaponData.attackPower));
        }
    }

    private void ClearAll()
    {
        if (InventoryManager.Instance == null) return;

        InventorySlot[] slots = InventoryManager.Instance.GetAllSlots();
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (!slots[i].IsEmpty)
            {
                InventoryManager.Instance.RemoveItem(slots[i].itemId, slots[i].count);
            }
        }
    }

    private static GUIStyle _rightAlignStyle;
    private static GUIStyle GetRightAlignStyle()
    {
        if (_rightAlignStyle == null)
        {
            _rightAlignStyle = new GUIStyle(GUI.skin.label);
            _rightAlignStyle.alignment = TextAnchor.MiddleRight;
        }
        return _rightAlignStyle;
    }
}
