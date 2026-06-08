using UnityEngine;
using UnityEngine.UI;

public class ShopNpcInteraction : MonoBehaviour
{
    [Header("Shop Reference")]
    public ShopUI shopUI;

    [Tooltip("Key to interact with the NPC. Default: E")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Trigger Settings")]
    [Tooltip("Radius of the interaction trigger. Auto-created at runtime.")]
    public float triggerRadius = 2f;

    [Header("Prompt")]
    public GameObject promptUI;
    public Text promptText;

    private bool _playerInRange;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        EnsureTriggerCollider();
    }

    private void EnsureTriggerCollider()
    {
        SphereCollider[] existing = GetComponents<SphereCollider>();
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i].isTrigger) return;
        }

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = triggerRadius;
        Debug.Log(string.Format("[ShopNpc] Auto-added trigger SphereCollider (radius={0}). " +
            "Keep the main BoxCollider as non-trigger for physics.", triggerRadius));
    }

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            if (shopUI != null)
                shopUI.Toggle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            ShowPrompt(true);
            Debug.Log("[ShopNpc] Player entered shop range. Press E to open.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            ShowPrompt(false);
            if (shopUI != null)
                shopUI.Close();
            Debug.Log("[ShopNpc] Player left shop range.");
        }
    }

    private void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
        if (show && promptText != null)
            promptText.text = string.Format("按 [{0}] 打开商店", interactKey);
    }
}
