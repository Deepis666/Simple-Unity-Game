using System;

public interface IInventoryService
{
    bool AddItem(string itemId, int count);
    int GetItemCount(string itemId);
    bool RemoveItem(string itemId, int count);
    bool HasItem(string itemId, int count = 1);

    InventorySlot[] GetAllSlots();

    bool UsePotion(string itemId);

    InventoryItemData GetItemDefinition(string itemId);

    event Action OnInventoryChanged;
    event Action<string, PotionEffectType, int> OnPotionUsed;
}
