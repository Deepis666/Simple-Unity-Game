using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRegistry", menuName = "ARPG/Inventory/Item Registry")]
public class InventoryItemRegistry : ScriptableObject
{
    [Tooltip("Drag all InventoryItemData assets here. InventoryManager auto-loads this at runtime.")]
    public List<InventoryItemData> items = new List<InventoryItemData>();
}
