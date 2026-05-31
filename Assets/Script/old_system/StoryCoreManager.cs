using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class StorySegmentDef
{
    [InspectorLabel("章節名稱 (自己看得懂就好)")]
    public string SegmentName;

    [Header("🌍【世界基礎規則 (Base Rule)】")]
    [InspectorLabel("這個章節的預設規則")]
    public GameMode SegmentBaseMode = GameMode.StoryMode; 

    [InspectorLabel("這章要強制開啟的【房間】")]
    public GameObject TargetRoom;

    [InspectorLabel("玩家在這章的【出生點】(選填)")]
    public Transform SpawnPoint;

    [Header("👻【玩家顯示設定】")]
    [InspectorLabel("進入此章節時，要自動顯示玩家嗎？")]
    public bool showPlayerOnStart = true;

    [Header("🧠【事件控制】")]
    [InspectorLabel("進入此章節時是否清除事件記憶（避免卡住互動）")]
    public bool resetSceneEventsOnEnter = false;
}

public class StoryCoreManager : MonoBehaviour
{
    public static StoryCoreManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [Header("🎮【全域玩家綁定】")]
    public GameObject GlobalPlayer;

    [Header("📖【遊戲主線大綱】")]
    public List<StorySegmentDef> storySegments = new List<StorySegmentDef>();

    [Header("🔍【當前遊戲進度】")]
    public int currentSegmentIndex = 0;

    [HideInInspector] public bool isFreeRoamMode = false;

    [Header("🛠️【房間控制】")]
    public List<GameObject> allRoomsInScene = new List<GameObject>();
    public bool autoCorrectRoomOnStart = true;

    [Header("Scene Flow")]
    [SerializeField] private bool loadEndSceneWhenStoryEnds = true;
    [SerializeField] private string endSceneName = "EndScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        int gameResult = GameResultState.GetResult();
        if (gameResult >= 0)
        {
            StartCoroutine(ReturnFromGameRoutine(gameResult));
            return;
        }

        if (autoCorrectRoomOnStart) EnforceCurrentSegmentRoom();
    }

    private IEnumerator ReturnFromGameRoutine(int gameResult)
    {
        yield return null;
        yield return null;
        ReturnFromGame(gameResult);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ReturnFromGame(int gameResult)
    {
        int munSegmentIndex = FindSegmentIndexByRoomName("4_MUN");
        if (munSegmentIndex >= 0)
        {
            currentSegmentIndex = munSegmentIndex;
        }

        EnforceCurrentSegmentRoom();

        if (GlobalPlayer != null)
        {
            GlobalPlayer.SetActive(true);
        }

        if (CoreSystem_GameManager.Instance != null)
        {
            CoreSystem_GameManager.Instance.AddFlagByName("4th");
            CoreSystem_GameManager.Instance.SetMovementPermission(true);
        }

        StartCoroutine(TriggerGameEndingBranchNextFrame(gameResult));
    }

    private int FindSegmentIndexByRoomName(string roomName)
    {
        for (int i = 0; i < storySegments.Count; i++)
        {
            if (storySegments[i] != null &&
                storySegments[i].TargetRoom != null &&
                storySegments[i].TargetRoom.name == roomName)
            {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator TriggerGameEndingBranchNextFrame(int gameResult)
    {
        yield return null;
        yield return null;

        SceneEventManager munEventManager = FindRoomEventManager("4_MUN");
        if (munEventManager == null)
        {
            Debug.LogWarning("[StoryCoreManager] Could not find 4_MUN SceneEventManager for game ending branch.");
            yield break;
        }

        string eventName = GetGameEndingEventName(gameResult);
        if (!string.IsNullOrEmpty(eventName))
        {
            bool triggered = munEventManager.TriggerEventByName(eventName);
            if (triggered)
            {
                GameResultState.ClearResult();
                StartCoroutine(LoadEndSceneAfterEndingEvent(munEventManager));
            }
        }
    }

    private IEnumerator LoadEndSceneAfterEndingEvent(SceneEventManager eventManager)
    {
        yield return null;
        yield return new WaitWhile(() => eventManager != null && eventManager.IsEventExecuting);
        yield return null;

        if (loadEndSceneWhenStoryEnds && !string.IsNullOrEmpty(endSceneName))
        {
            Time.timeScale = 1f;
            GameResultState.ClearResult();
            SceneManager.LoadScene(endSceneName);
        }
    }

    private SceneEventManager FindRoomEventManager(string roomName)
    {
        foreach (GameObject room in allRoomsInScene)
        {
            if (room != null && room.name == roomName)
            {
                return room.GetComponentInChildren<SceneEventManager>(true);
            }
        }

        return null;
    }

    private string GetGameEndingEventName(int gameResult)
    {
        switch (gameResult)
        {
            case 0:
                return "GameEnding_Perfect";
            case 1:
                return "GameEnding_Normal";
            case 2:
                return "GameEnding_Failed";
            default:
                return "";
        }
    }

    public void EnforceCurrentSegmentRoom()
    {
        if (storySegments.Count == 0) return;

        if (currentSegmentIndex < 0) currentSegmentIndex = 0;
        if (currentSegmentIndex >= storySegments.Count) currentSegmentIndex = storySegments.Count - 1;

        foreach (var room in allRoomsInScene)
            if (room != null) room.SetActive(false);

        StorySegmentDef seg = storySegments[currentSegmentIndex];

        if (seg.TargetRoom != null) seg.TargetRoom.SetActive(true);

        if (GlobalPlayer != null)
        {
            if (seg.SpawnPoint != null) GlobalPlayer.transform.position = seg.SpawnPoint.position;
            GlobalPlayer.SetActive(seg.showPlayerOnStart);
        }

        if (CoreSystem_GameManager.Instance != null) 
            CoreSystem_GameManager.Instance.SetBaseRule(seg.SegmentBaseMode == GameMode.FreeRoamMode);

        if (seg.resetSceneEventsOnEnter) ResetAllSceneEvents();
    }

    public void ResetAllSceneEvents()
    {
        var managers = FindObjectsByType<SceneEventManager>(FindObjectsSortMode.None);
        foreach (var m in managers) m.ResetEventExecutionState();
    }

    public string GetCurrentSegmentName()
    {
        if (currentSegmentIndex < storySegments.Count) return storySegments[currentSegmentIndex].SegmentName;
        return "END";
    }

    public void AdvanceNextSegment()
    {
        if (storySegments.Count == 0) return;

        if (currentSegmentIndex >= storySegments.Count - 1)
        {
            if (loadEndSceneWhenStoryEnds && !string.IsNullOrEmpty(endSceneName))
            {
                Time.timeScale = 1f;
                GameResultState.ClearResult();
                SceneManager.LoadScene(endSceneName);
            }

            return;
        }

        // 【補回】抓出舊章節的房間，準備進行轉場隱藏
        GameObject oldRoom = storySegments[currentSegmentIndex].TargetRoom;

        currentSegmentIndex++;
        StorySegmentDef nextSeg = storySegments[currentSegmentIndex];

        if (CoreSystem_GameManager.Instance != null)
            CoreSystem_GameManager.Instance.SetBaseRule(nextSeg.SegmentBaseMode == GameMode.FreeRoamMode);

        // 【補回缺失】執行真正的跨章節搬移與黑畫面轉場
        if (SceneTransitionManager.Instance != null && nextSeg.TargetRoom != null)
        {
            SceneTransitionManager.Instance.StartCoroutine(
                SceneTransitionManager.Instance.ExecuteRoomTransition(
                    oldRoom,
                    nextSeg.TargetRoom,
                    nextSeg.SpawnPoint,
                    SceneTransitionType.BlackScreen,
                    1.5f // 跨章節轉場預設 1.5 秒
                )
            );

            // 【補回】同步新章節的玩家顯示狀態
            if (GlobalPlayer != null)
            {
                GlobalPlayer.SetActive(nextSeg.showPlayerOnStart);
            }
        }
        else
        {
            // 如果沒有轉場系統，退回原本的瞬間切換
            EnforceCurrentSegmentRoom();
        }
    }
}
