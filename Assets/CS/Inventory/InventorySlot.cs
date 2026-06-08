using System;

[Serializable]
public class InventorySlot
{
    public string itemId;
    public int count;

    public bool IsEmpty
    {
        get { return string.IsNullOrEmpty(itemId) || count <= 0; }
    }

    public void Clear()
    {
        itemId = null;
        count = 0;
    }

    public void Set(string id, int amount)
    {
        itemId = id;
        count = amount;
    }

    public int AddAmount(int amount)
    {
        count += amount;
        return count;
    }

    public int RemoveAmount(int amount)
    {
        count -= amount;
        if (count <= 0)
        {
            Clear();
            return 0;
        }
        return count;
    }
}
