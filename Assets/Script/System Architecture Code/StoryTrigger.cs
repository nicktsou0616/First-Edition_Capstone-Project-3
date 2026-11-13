using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StoryTrigger : MonoBehaviour
{
    [Header("📖 要播放的劇本")]
    public StoryData storyToPlay;

    [Header("⚙️ 觸發設定")]
    public bool playOnEnter = false;
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange = false;

    void Start()
    {
        // 確保碰撞體一定是 Trigger 模式
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // 如果玩家在範圍內，按下互動鍵，且目前「沒有」在播劇情，才執行
        if (isPlayerInRange && !playOnEnter && Input.GetKeyDown(interactKey))
        {
            if (!StoryManager.Instance.isPlayingStory)
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        if (storyToPlay != null)
        {
            StoryManager.Instance.PlayStory(storyToPlay);
        }
        else
        {
            Debug.LogWarning($"[StoryTrigger] {gameObject.name} 缺少 StoryData！請在 Inspector 拖入劇本。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ★ 關鍵：必須確保進入的物件 Tag 是 Player
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log($"[StoryTrigger] 玩家進入範圍: {gameObject.name}，可以按 {interactKey} 互動。");

            // 如果設定為碰到就自動播放，且目前沒有其他劇情在播
            if (playOnEnter && !StoryManager.Instance.isPlayingStory)
            {
                Interact();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log($"[StoryTrigger] 玩家離開範圍: {gameObject.name}");
        }
    }
}