using System.Collections.Generic;
using UnityEngine;

public class InventoryDebugTester : MonoBehaviour
{
    private bool _showPanel = true;
    private List<InventoryItemData> _mockDefinitions = new List<InventoryItemData>();

    private void Start()
    {
        CreateMockItemDefinitions();
        RegisterMockDefinitions();
        Debug.Log("[InventoryDebugTester] Mock item definitions injected (9 items).");
    }

    #region Mock Data

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
            ItemCategory.QuestItem));
        _mockDefinitions.Add(CreateMockItem("quest_flame_demon_core", "炎魔之核",
            ItemCategory.QuestItem));
        _mockDefinitions.Add(CreateMockItem("quest_skeleton_crown", "骷髅王冠",
            ItemCategory.QuestItem));
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
        if (cat == ItemCategory.Weapon) { data.maxStack = 1; data.attackPower = atk; }
        if (cat == ItemCategory.Potion) { data.potionEffectType = effect; data.effectValue = effectVal; }
        return data;
    }

    private void RegisterMockDefinitions()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.itemDefinitions.Clear();
        for (int i = 0; i < _mockDefinitions.Count; i++)
            InventoryManager.Instance.itemDefinitions.Add(_mockDefinitions[i]);
        InventoryManager.Instance.ForceRefreshUI();
    }

    #endregion

    private void OnGUI()
    {
        if (!_showPanel) return;

        int panelW = 240;
        int panelH = Screen.height - 20;
        int panelX = Screen.width - panelW - 10;
        int panelY = 10;

        Rect panelRect = new Rect(panelX, panelY, panelW, panelH);
        GUI.Box(panelRect, "物品测试面板");
        GUI.BeginGroup(panelRect);

        int curY = 25;
        int pad = 8;
        int innerW = panelW - pad * 2;
        int btnH = 24;

        curY = DrawItemButtons(curY, pad, innerW, btnH);
        curY += 4;
        curY = DrawPotionQuickUse(curY, pad, innerW, btnH);
        curY += 4;
        curY = DrawBulkButtons(curY, pad, innerW, btnH);
        curY += 4;
        DrawWeaponInfo(curY, pad, innerW);

        GUI.EndGroup();
    }

    private int DrawItemButtons(int curY, int pad, int innerW, int btnH)
    {
        GUI.Label(new Rect(pad, curY, innerW, 20), "--- 物品增减 ---");
        curY += 20;

        if (InventoryManager.Instance == null) return curY;

        for (int i = 0; i < _mockDefinitions.Count; i++)
        {
            InventoryItemData def = _mockDefinitions[i];
            int count = InventoryManager.Instance.GetItemCount(def.itemId);

            float halfW = (innerW - 4) / 2f;

            if (GUI.Button(new Rect(pad, curY, halfW, btnH),
                string.Format("+1 {0}", def.itemName)))
                InventoryManager.Instance.AddItem(def.itemId, 1);

            GUI.enabled = count > 0;
            if (GUI.Button(new Rect(pad + halfW + 4, curY, halfW, btnH),
                string.Format("({0}) -1", count)))
                InventoryManager.Instance.RemoveItem(def.itemId, 1);
            GUI.enabled = true;

            curY += btnH + 2;
        }
        return curY;
    }

    private int DrawPotionQuickUse(int curY, int pad, int innerW, int btnH)
    {
        GUI.Label(new Rect(pad, curY, innerW, 20), "--- 药水使用 ---");
        curY += 20;

        if (InventoryManager.Instance == null) return curY;

        for (int i = 0; i < _mockDefinitions.Count; i++)
        {
            InventoryItemData def = _mockDefinitions[i];
            if (def.itemCategory != ItemCategory.Potion) continue;

            int count = InventoryManager.Instance.GetItemCount(def.itemId);
            GUI.enabled = count > 0;
            if (GUI.Button(new Rect(pad, curY, innerW, btnH),
                string.Format("使用 {0} ({1})", def.itemName, count)))
                InventoryManager.Instance.UsePotion(def.itemId);
            GUI.enabled = true;
            curY += btnH + 2;
        }
        return curY;
    }

    private int DrawBulkButtons(int curY, int pad, int innerW, int btnH)
    {
        GUI.Label(new Rect(pad, curY, innerW, 20), "--- 批量 ---");
        curY += 20;

        if (InventoryManager.Instance == null) return curY;

        float halfW = (innerW - 4) / 2f;

        if (GUI.Button(new Rect(pad, curY, halfW, btnH), "药水各+3"))
        {
            InventoryManager.Instance.AddItem("potion_health", 3);
            InventoryManager.Instance.AddItem("potion_mana", 3);
            InventoryManager.Instance.AddItem("potion_attack_boost", 3);
        }
        if (GUI.Button(new Rect(pad + halfW + 4, curY, halfW, btnH), "任务道具各+1"))
        {
            InventoryManager.Instance.AddItem("quest_stone_heart", 1);
            InventoryManager.Instance.AddItem("quest_flame_demon_core", 1);
            InventoryManager.Instance.AddItem("quest_skeleton_crown", 1);
        }
        curY += btnH + 2;

        if (GUI.Button(new Rect(pad, curY, halfW, btnH), "药水各+10"))
        {
            InventoryManager.Instance.AddItem("potion_health", 10);
            InventoryManager.Instance.AddItem("potion_mana", 10);
            InventoryManager.Instance.AddItem("potion_attack_boost", 10);
        }
        if (GUI.Button(new Rect(pad + halfW + 4, curY, halfW, btnH), "清空全部"))
        {
            InventorySlot[] slots = InventoryManager.Instance.GetAllSlots();
            for (int j = slots.Length - 1; j >= 0; j--)
                if (!slots[j].IsEmpty)
                    InventoryManager.Instance.RemoveItem(slots[j].itemId, slots[j].count);
        }
        return curY + btnH + 2;
    }

    private void DrawWeaponInfo(int curY, int pad, int innerW)
    {
        GUI.Label(new Rect(pad, curY, innerW, 20), "--- 武器 ---");
        curY += 20;

        if (WeaponManager.Instance == null)
        {
            GUI.Label(new Rect(pad, curY, innerW, 20), "未就绪");
            return;
        }

        string wid = WeaponManager.Instance.CurrentWeaponId;
        GUI.Label(new Rect(pad, curY, innerW, 20),
            string.Format("装备: {0}", string.IsNullOrEmpty(wid) ? "无" : wid));
        curY += 18;

        if (WeaponManager.Instance.CurrentWeaponData != null)
            GUI.Label(new Rect(pad, curY, innerW, 20),
                string.Format("ATK: {0}", WeaponManager.Instance.CurrentWeaponData.attackPower));
    }
}
