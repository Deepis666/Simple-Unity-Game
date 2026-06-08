using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopEntry
{
    public string itemId;
    public int price;
}

[CreateAssetMenu(fileName = "ShopConfig", menuName = "ARPG/Shop/Shop Config")]
public class ShopConfig : ScriptableObject
{
    [Tooltip("Items sold by the shop, with their prices.")]
    public List<ShopEntry> items = new List<ShopEntry>();
}
