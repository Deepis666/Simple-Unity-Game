using System;
using UnityEngine;

[Serializable]
public class WeaponStageEntry
{
    [Tooltip("When quest stage reaches this value, this weapon auto-equips.")]
    public int questStage;
    public string weaponId;
}

public class WeaponManager : MonoBehaviour
{
    private static WeaponManager _instance;

    public static WeaponManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WeaponManager>();
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;

        GameObject go = new GameObject("WeaponManager");
        _instance = go.AddComponent<WeaponManager>();
        DontDestroyOnLoad(go);
    }

    [Header("Weapon Stage Progression")]
    [Tooltip("Weapon auto-equips when quest stage >= the configured threshold. Sorted low to high.")]
    public WeaponStageEntry[] weaponProgression = new WeaponStageEntry[]
    {
        new WeaponStageEntry { questStage = 0, weaponId = "weapon_iron_blade" },
        new WeaponStageEntry { questStage = 2, weaponId = "weapon_steel_blade" },
        new WeaponStageEntry { questStage = 3, weaponId = "weapon_legendary_blade" }
    };

    private string _currentWeaponId;

    public string CurrentWeaponId
    {
        get { return _currentWeaponId; }
    }

    public InventoryItemData CurrentWeaponData
    {
        get { return GetWeaponDefinition(_currentWeaponId); }
    }

    public event Action<string> OnWeaponChanged;

    private bool _subscribed;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        TrySubscribe();
        RefreshWeapon();
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
    }

    private void OnQuestStageChanged(int newStage, string questText)
    {
        RefreshWeapon();
    }

    private void RefreshWeapon()
    {
        string newWeaponId = GetWeaponForCurrentStage();

        if (newWeaponId == _currentWeaponId) return;

        string oldWeapon = _currentWeaponId;
        _currentWeaponId = newWeaponId;

        Debug.Log(string.Format("[WeaponManager] Weapon changed: {0} -> {1}",
            oldWeapon ?? "null", newWeaponId ?? "null"));

        if (OnWeaponChanged != null)
        {
            OnWeaponChanged(newWeaponId);
        }
    }

    private string GetWeaponForCurrentStage()
    {
        if (MainQuestManager.Instance == null) return weaponProgression[0].weaponId;

        int currentStage = MainQuestManager.Instance.QuestStage;
        string bestWeapon = null;

        for (int i = 0; i < weaponProgression.Length; i++)
        {
            if (currentStage >= weaponProgression[i].questStage)
            {
                bestWeapon = weaponProgression[i].weaponId;
            }
        }

        return bestWeapon;
    }

    private InventoryItemData GetWeaponDefinition(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return null;
        if (InventoryManager.Instance == null) return null;
        return InventoryManager.Instance.GetItemDefinition(weaponId);
    }
}
