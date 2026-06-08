using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    void Update()
    {
        // 当玩家按下 F 键时显示对话
        if (Input.GetKeyDown(KeyCode.F) && MainQuestManager.Instance != null)
        {
            if (dialoguePanel.activeSelf)
            {
                dialoguePanel.SetActive(false); // 再次按 F 关闭
            }
            else
            {
                dialogueText.text = MainQuestManager.Instance.GetNpcDialogueText();
                dialoguePanel.SetActive(true);
            }
        }
    }
}