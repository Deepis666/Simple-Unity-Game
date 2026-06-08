using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RewardEntry
{
    [Tooltip("Quest stage that triggers this reward.")]
    public int questStage;
    public string itemId;
    public int count;
}

[CreateAssetMenu(fileName = "QuestRewardConfig", menuName = "ARPG/Inventory/Quest Reward Config")]
public class InventoryRewardConfig : ScriptableObject
{
    [Tooltip("Rewards triggered when reaching specific quest stages. " +
             "Recommended: stage 1-3 = quest items, stage 4 = final reward.")]
    public List<RewardEntry> rewards = new List<RewardEntry>();
}
