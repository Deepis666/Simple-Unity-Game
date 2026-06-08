using System.Collections.Generic;
using UnityEngine;

public class QuestRewardAdapter : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Assign the QuestRewardConfig ScriptableObject here.")]
    public InventoryRewardConfig rewardConfig;

    private bool _subscribed;
    private HashSet<int> _rewardedStages = new HashSet<int>();

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribed && MainQuestManager.Instance != null)
        {
            MainQuestManager.Instance.QuestStageChanged -= OnQuestStageChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (MainQuestManager.Instance == null) return;

        MainQuestManager.Instance.QuestStageChanged += OnQuestStageChanged;
        _subscribed = true;

        int currentStage = MainQuestManager.Instance.QuestStage;
        GrantRewardsForStage(currentStage);
    }

    private void OnQuestStageChanged(int newStage, string questText)
    {
        GrantRewardsForStage(newStage);
    }

    private void GrantRewardsForStage(int stage)
    {
        if (rewardConfig == null) return;
        if (InventoryManager.Instance == null) return;

        for (int i = 0; i < rewardConfig.rewards.Count; i++)
        {
            RewardEntry entry = rewardConfig.rewards[i];
            if (entry.questStage != stage) continue;
            if (_rewardedStages.Contains(stage)) continue;

            InventoryManager.Instance.AddItem(entry.itemId, entry.count);
            _rewardedStages.Add(stage);
            Debug.Log(string.Format("[QuestRewardAdapter] Reward granted: {0} x{1} for stage {2}",
                entry.itemId, entry.count, stage));
        }
    }
}
