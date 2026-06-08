using System;

public interface IMainQuestService
{
    int QuestStage { get; }
    bool IsQuestCompleted { get; }

    event Action<int, string> QuestStageChanged;

    void AcceptMainQuest();
    void OnEnemyKilled(EnemyType enemyType);
    string GetCurrentQuestText();
    string GetNpcDialogueText();
}
