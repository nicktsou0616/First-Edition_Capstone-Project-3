using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TeleportDoor : MonoBehaviour
{
    [Header("🚪【單一場景：這扇門通往哪裡？】")]
    [InspectorLabel("【關閉】要隱藏的舊房間")]
    public GameObject RoomToDisable;

    [InspectorLabel("【開啟】要顯示的新房間")]
    public GameObject RoomToEnable;

    [InspectorLabel("【傳送】玩家要瞬移到哪個位置？")]
    public Transform TeleportTarget;

    [InspectorLabel("過場畫面特效")]
    public SceneTransitionType transitionType = SceneTransitionType.BlackScreen;

    [InspectorLabel("特效要播幾秒？")]
    public float transitionDuration = 0.5f;

    [InspectorLabel("碰到門就自動傳送嗎？")]
    public bool triggerOnEnter = true;

    [Header("🔓 門解鎖設定")]
    [InspectorLabel("這扇門一開始就能使用")]
    public bool unlockedFromStart = false;

    // 給事件系統解鎖用的隱藏變數
    [HideInInspector]
    public bool isIndependentlyUnlocked = false;

    private bool isPlayerNearby = false;

    // 【防呆修復】全域防護：防止玩家傳送後瞬間踩到另一個門又被傳送
    private static float _lastTeleportTime = 0f;
    private const float TELEPORT_COOLDOWN = 1.0f; // 傳送冷卻時間

    private void Start()
    {
        // 確保至少掛了一個 Collider2D，但不強制改成 isTrigger
        // 讓物理 Collider 自然阻擋沒解鎖的玩家
    }

    private void Update()
    {
        if (!triggerOnEnter && isPlayerNearby)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TryTeleport();
            }
        }
    }

    // 不論是實體碰撞還是 Trigger 碰撞，都進行判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (triggerOnEnter) TryTeleport();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (triggerOnEnter) TryTeleport();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) isPlayerNearby = false;
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) isPlayerNearby = false;
    }

    private void TryTeleport()
    {
        // 【防呆修復】剛傳送完不能立刻再傳送
        if (Time.time - _lastTeleportTime < TELEPORT_COOLDOWN) return;

        if (RoomToEnable == null || TeleportTarget == null) return;

        // =========================================
        // ⭐ 核心修復：門禁權限與世界模式徹底脫鉤！
        // 只看這扇門自己有沒有被解鎖 (完全不鳥 FreeRoamMode)
        // =========================================
        bool canPass = unlockedFromStart || isIndependentlyUnlocked;

        if (!canPass) return; // 沒解鎖，直接安靜退下，實體 Collider 會擋住玩家

        // 通過驗證，開始傳送
        _lastTeleportTime = Time.time;
        Debug.Log($"🚪 Door: [{gameObject.name}] 觸發傳送，目標房間: {(RoomToEnable != null ? RoomToEnable.name : "Null")}");
        if (SceneTransitionManager.Instance != null)
        {
            StartCoroutine(
                SceneTransitionManager.Instance.ExecuteRoomTransition(
                    RoomToDisable,
                    RoomToEnable,
                    TeleportTarget,
                    transitionType,
                    transitionDuration
                )
            );
        }
    }
}