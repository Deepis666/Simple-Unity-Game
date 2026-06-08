using UnityEngine;

public class InventoryUIOpener : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The backpack panel GameObject to toggle. If it has a BackpackUI component, Toggle() is called instead of SetActive.")]
    public GameObject backpackPanel;

    [Tooltip("Key to open/close the backpack. Default: B")]
    public KeyCode toggleKey = KeyCode.B;

    private BackpackUI _backpackUI;

    private void Start()
    {
        if (backpackPanel != null)
        {
            _backpackUI = backpackPanel.GetComponent<BackpackUI>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (_backpackUI != null)
            {
                _backpackUI.Toggle();
            }
            else if (backpackPanel != null)
            {
                bool newState = !backpackPanel.activeSelf;
                backpackPanel.SetActive(newState);
                Debug.Log(string.Format("[InventoryUIOpener] Backpack {0} (key: {1})",
                    newState ? "opened" : "closed", toggleKey));
            }
            else
            {
                Debug.LogWarning("[InventoryUIOpener] Backpack panel reference is not assigned. " +
                    "Create a Canvas GameObject, attach BackpackUI, assign sprites, then drag it here.");
            }
        }
    }
}
