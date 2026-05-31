using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum GameMode
{
    [InspectorName("🎬 劇情章節 (預設禁止移動)")] StoryMode,
    [InspectorName("🏃 探索章節 (預設允許移動)")] FreeRoamMode
}

public enum ExecutionBehavior
{
    [InspectorName("🔥 只能觸發一次 (用過就作廢)")] RunOnce,
    [InspectorName("🔁 可以無限次重複觸發")] Repeatable
}

public enum TriggerType
{
    [InspectorName("純呼叫 (被其他程式啟動)")] FromFlow,
    [InspectorName("一進入這個房間就自動發生")] OnEnterScene,
    [InspectorName("玩家踩到/碰到就自動發生")] OnEnter,
    [InspectorName("玩家靠近並按下互動鍵 (E/空白鍵)")] OnInteract
}

public enum ActionType
{
    [InspectorName("💬 啟動【對話或事件模組】(自動 Temporary Lock)")] Dialogue,
    [InspectorName("🔓 開門或解鎖機關")] Unlock,
    [InspectorName("🚩 給予玩家一張【紀錄貼紙】")] SetFlag,
    [InspectorName("⏳ 停住！等玩家拿到特定貼紙才繼續")] WaitFlags,
    [InspectorName("📖 推進【主線劇情進度】")] AdvanceSegment,
    [InspectorName("🎬 播放【過場動畫Timeline】(自動 Temporary Lock)")] PlayCutscene,
    [InspectorName("🏃 覆蓋移動權限 (Permission Override)")] ChangeMode,
    [InspectorName("👻 顯示 或 隱藏玩家模型")] TogglePlayer,
    [InspectorName("💡 顯示提示文字 (漂浮提示)")] ShowPopupText,
    [InspectorName("🔀 結束舊系統並切換新模組")] SwitchToNewModule
}

public enum ConditionType
{
    [InspectorName("必須【全部都有】才算數")] All,
    [InspectorName("只要有【其中一個】就算數")] Any
}

public enum SceneTransitionType
{
    [InspectorName("黑畫面漸變")] BlackScreen,
    [InspectorName("白光漸變")] WhiteFlash,
    [InspectorName("瞬間切換 (沒特效)")] Cut
}

[CreateAssetMenu(fileName = "New Flag", menuName = "遊戲設計工具/新增一張【紀錄貼紙】(Flag)")]
public class FlagData : ScriptableObject
{
    [TextArea(2, 4), InspectorLabel("這張貼紙的用途筆記")] public string description;
}

[System.Serializable]
public class StepData
{
    [InspectorLabel("1. 要做什麼動作？")]
    public ActionType Action;

    [Header("2. 要用到什麼東西？")]
    [InspectorLabel("【模組引用區】")]
    public GameObject TargetModule;

    [InspectorLabel("【動畫引用區】")]
    public PlayableAsset CutsceneAsset;

    [Header("(選填) 貼紙設定")]
    public List<FlagData> TargetFlags = new List<FlagData>();
    public ConditionType FlagCondition;

    [Header("(選填) 權限與玩家顯示")]
    [InspectorLabel("強制允許玩家移動？(配合 🏃 覆蓋移動權限 使用)")]
    public bool grantMovementPermission = true;

    [HideInInspector] public GameMode TargetMode;

    [Tooltip("如果動作選了【顯示/隱藏玩家】，請打勾決定要顯示還是隱藏")]
    [InspectorLabel("要顯示玩家嗎？(打勾=顯示，不勾=隱藏)")]
    public bool isPlayerVisible = true;

    [Header("(選填) 提示文字設定")]
    [InspectorLabel("要顯示什麼文字？(配合💡動作使用)")]
    [TextArea(2, 3)]
    public string popupTextContent;
}

[System.Serializable]
public class EventBlock
{
    [Header("📝【事件基本資料】")]
    public string EventName = "新事件";
    public int BelongSegmentIndex = -1;

    [Header("📌【章節判定設定】")]
    public bool isRequiredForSegment = true;

    [Header("🎯【怎麼觸發？】")]
    public ExecutionBehavior ExecutionBehavior = ExecutionBehavior.RunOnce;
    public TriggerType TriggerType = TriggerType.OnInteract;
    public List<GameObject> TriggerTargets = new List<GameObject>();
    public ConditionType TargetCondition = ConditionType.Any;
    public float InteractDistance = 2.0f;

    [Header("🔒【條件檢查】")]
    public List<FlagData> RequireFlags = new List<FlagData>();
    public List<FlagData> ExcludeFlags = new List<FlagData>();

    [Header("🎬【流程步驟】")]
    public List<StepData> Steps = new List<StepData>();

    [Header("👻【玩家狀態】")]
    public bool showPlayerAfterEvent = true;

    [Header("🚪【事件結束後的收尾 (單一場景傳送)】")]
    [InspectorLabel("事件跑完後，要自動切換房間嗎？")] public bool teleportAfterEvent = false;
    [InspectorLabel("轉場畫面特效")] public SceneTransitionType transitionType = SceneTransitionType.BlackScreen;
    [InspectorLabel("特效要播幾秒？")] public float transitionDuration = 1.0f;
    [InspectorLabel("【關閉】要隱藏的舊房間")] public GameObject RoomToDisable;
    [InspectorLabel("【開啟】要顯示的下一個房間")] public GameObject RoomToEnable;
    [InspectorLabel("【傳送】玩家要瞬移到哪個位置？")] public Transform TeleportTarget;

    [HideInInspector] public bool hasExecuted = false;
    [HideInInspector] public GameMode EventGameMode = GameMode.StoryMode;

    public string GetEventSignature()
    {
        string sig = EventName;
        foreach (var f in RequireFlags)
        {
            if (f != null) sig += "_" + f.name;
        }
        return sig;
    }
}