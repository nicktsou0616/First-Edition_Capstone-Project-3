using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("核心 Prefab")]
    [Tooltip("請將你的 Player Prefab (從 Project 資料夾) 拖入這裡")]
    public GameObject playerPrefab;

    // ★ 擴充準備：當玩家生成完畢時觸發，可供 Camera 或其他系統重新綁定目標
    public static System.Action<GameObject> OnPlayerSpawned;

    private void Awake()
    {
        // 確保全遊戲只有一個 GameDirector
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("[GameDirector] 尚未設定 Player Prefab！無法動態生成玩家。");
            return;
        }

        // 防呆：如果場景裡「已經」有 Player (例如企劃測試時手動擺的)，就跳過生成，直接用現成的
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            Debug.Log("[GameDirector] 場景中已存在 Player，跳過動態生成。");
            OnPlayerSpawned?.Invoke(existingPlayer);
            return;
        }

        // 尋找場景中所有的出生點
        PlayerSpawnPoint[] allSpawnPoints = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        PlayerSpawnPoint targetPoint = null;
        PlayerSpawnPoint defaultPoint = null;

        foreach (var sp in allSpawnPoints)
        {
            if (!string.IsNullOrEmpty(StoryManager.nextSpawnID) && sp.spawnID == StoryManager.nextSpawnID)
            {
                targetPoint = sp;
            }
            if (sp.isDefault)
            {
                defaultPoint = sp;
            }
        }

        // 決定最終出生座標
        Vector3 spawnPosition = Vector3.zero;
        if (targetPoint != null)
        {
            spawnPosition = targetPoint.transform.position;
            Debug.Log($"[GameDirector] 找到指定出生點 [{targetPoint.spawnID}]，生成玩家。");
        }
        else if (defaultPoint != null)
        {
            spawnPosition = defaultPoint.transform.position;
            Debug.Log($"[GameDirector] 沒有指定 ID，使用預設出生點 [{defaultPoint.spawnID}] 生成玩家。");
        }
        else if (allSpawnPoints.Length > 0)
        {
            spawnPosition = allSpawnPoints[0].transform.position;
            Debug.LogWarning("[GameDirector] 找不到對應 ID 也沒有預設點，隨機使用找到的第一個出生點。");
        }
        else
        {
            Debug.LogWarning("[GameDirector] 場景中沒有任何 PlayerSpawnPoint，玩家將生成於原點 (0,0,0)。");
        }

        // 真正執行生成 (Instantiate)
        GameObject newPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // 生成完畢後，清空 ID，避免下次切換場景時誤用
        StoryManager.nextSpawnID = "";

        // 發送事件，通知攝影機去追蹤這隻新生成的 Player
        OnPlayerSpawned?.Invoke(newPlayer);
    }
}