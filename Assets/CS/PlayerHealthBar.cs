using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Target")]
    public PlayerHealth playerHealth;

    [Header("UI")]
    public Image fillImage;
    public Text hpText;

    [Header("Colors")]
    public Color green = new Color(0.3f, 0.8f, 0.3f);
    public Color yellow = new Color(0.9f, 0.85f, 0.2f);
    public Color red = new Color(0.85f, 0.15f, 0.15f);

    private float _displayedRatio = 1f;

    private void Start()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.HealthChanged += OnHealthChanged;
        SetBar(playerHealth.CurrentHealth / playerHealth.MaxHealth);
    }

    private void OnHealthChanged(float current, float max)
    {
        SetBar(current / max);
    }

    private void Update()
    {
        _displayedRatio = Mathf.Lerp(_displayedRatio, _displayedRatio, 0.1f);
    }

    private void SetBar(float ratio)
    {
        if (fillImage != null)
        {
            fillImage.rectTransform.anchorMax = new Vector2(ratio, 1);
            fillImage.color = Color.Lerp(red, Color.Lerp(yellow, green, ratio * 2f - 0.5f), Mathf.Clamp01(ratio * 3f));
        }

        if (hpText != null && playerHealth != null)
            hpText.text = string.Format("{0} / {1}",
                Mathf.CeilToInt(playerHealth.CurrentHealth),
                Mathf.CeilToInt(playerHealth.MaxHealth));
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }
}
