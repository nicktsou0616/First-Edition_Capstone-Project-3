using UnityEngine;
using System.Collections.Generic;

public class CoreSystem_GameManager : MonoBehaviour
{
    public static CoreSystem_GameManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private PlayerController currentPlayer;
    private HashSet<FlagData> activeFlags = new HashSet<FlagData>();
    private HashSet<string> completedEvents = new HashSet<string>();

    [Header("📚【系統存檔字典】")]
    public List<FlagData> allAvailableFlags = new List<FlagData>();

    // ==========================================
    // 🐧 控制權系統 (Control Authority System) - 三層時間軸
    // 嚴格公式： PlayerCanMove = !Locked && (PermissionSnapshot ?? BaseRule)
    // ==========================================
    public bool BaseRuleAllowsMovement { get; private set; } = false;
    public bool? PermissionAllowsMovement { get; private set; } = null;
    private int controlLockCount = 0;

    // 玩家物理移動的唯一真理
    public bool PlayerCanMove => (controlLockCount <= 0) && (PermissionAllowsMovement ?? BaseRuleAllowsMovement);

    // 門/機關 判定世界規則的依據 (無視對話鎖)
    public bool IsFreeRoamRuleActive => (PermissionAllowsMovement ?? BaseRuleAllowsMovement);

    [System.Serializable]
    private class FlagSaveData
    {
        public List<string> savedFlagNames = new List<string>();
        public List<string> completedEventSignatures = new List<string>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadFlags();
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (currentPlayer == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                currentPlayer = p.GetComponent<PlayerController>();
                EvaluatePlayerState();
            }
        }
    }

    public void SetBaseRule(bool allowsMovement)
    {
        BaseRuleAllowsMovement = allowsMovement;
        PermissionAllowsMovement = null;
        EvaluatePlayerState();
    }

    public void SetMovementPermission(bool? allowsMovement)
    {
        PermissionAllowsMovement = allowsMovement;
        EvaluatePlayerState();
    }

    public void PushControlLock(string reason = "Unknown")
    {
        controlLockCount++;
        EvaluatePlayerState();
    }

    public void PopControlLock(string reason = "Unknown")
    {
        controlLockCount--;
        if (controlLockCount < 0) controlLockCount = 0;
        EvaluatePlayerState();
    }

    public void ForcePlayableControl()//跳過功能
    {
        controlLockCount = 0;
        PermissionAllowsMovement = true;
        EvaluatePlayerState();
    }

    private void EvaluatePlayerState()
    {
        if (currentPlayer == null) return;

        bool canMove = PlayerCanMove;
        currentPlayer.canMove = canMove;

        if (!canMove)
        {
            Rigidbody2D rb = currentPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void AddFlag(FlagData newFlag)
    {
        if (newFlag == null) return;
        if (!activeFlags.Contains(newFlag)) activeFlags.Add(newFlag);
    }

    public void AddFlagByName(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;

        FlagData flag = allAvailableFlags.Find(x => x != null && x.name == flagName);
        if (flag == null)
        {
            foreach (FlagData loadedFlag in Resources.FindObjectsOfTypeAll<FlagData>())
            {
                if (loadedFlag != null && loadedFlag.name == flagName)
                {
                    flag = loadedFlag;
                    break;
                }
            }
        }

        AddFlag(flag);
    }

    public bool CheckFlags(List<FlagData> flags, ConditionType condition)
    {
        if (flags == null || flags.Count == 0) return true;
        if (condition == ConditionType.All)
        {
            foreach (var f in flags) if (f != null && !activeFlags.Contains(f)) return false;
            return true;
        }
        else
        {
            foreach (var f in flags) if (f != null && activeFlags.Contains(f)) return true;
            return false;
        }
    }

    public bool HasAnyFlag(List<FlagData> flags)
    {
        if (flags == null || flags.Count == 0) return false;
        foreach (var f in flags) if (activeFlags.Contains(f)) return true;
        return false;
    }

    public void MarkEventCompleted(string eventSignature)
    {
        if (!completedEvents.Contains(eventSignature)) completedEvents.Add(eventSignature);
    }

    public bool IsEventCompleted(string eventSignature) => completedEvents.Contains(eventSignature);

    public void SaveFlags()
    {
        FlagSaveData data = new FlagSaveData();
        foreach (var flag in activeFlags) data.savedFlagNames.Add(flag.name);
        foreach (var evtSig in completedEvents) data.completedEventSignatures.Add(evtSig);
        PlayerPrefs.SetString("GameProgress_Flags", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void LoadFlags()
    {
        if (!PlayerPrefs.HasKey("GameProgress_Flags")) return;
        FlagSaveData data = JsonUtility.FromJson<FlagSaveData>(PlayerPrefs.GetString("GameProgress_Flags"));
        activeFlags.Clear();
        completedEvents.Clear();
        foreach (string flagName in data.savedFlagNames)
        {
            FlagData f = allAvailableFlags.Find(x => x != null && x.name == flagName);
            if (f != null) activeFlags.Add(f);
        }
        foreach (string sig in data.completedEventSignatures) completedEvents.Add(sig);
        SaveFlags();
    }
}
