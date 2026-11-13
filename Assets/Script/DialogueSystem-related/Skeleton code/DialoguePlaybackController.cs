using UnityEngine;
using UnityEngine.UI;

public class DialoguePlaybackController : MonoBehaviour
{
    [Header("⏭️ Skip 設定 (由玩家手動操作)")]
    public bool allowSkip = true;
    public GameObject skipUIButton;

    [Tooltip("Skip 時【一般句子】等待的時間(極快)")]
    public float skipWaitTime = 0.05f;

    [Tooltip("Skip 到【最後一句】後，完整顯示畫面多久才開始退場")]
    public float skipFinalLineWaitTime = 0.3f;

    [Header("▶️ Auto 設定 (由 CSV 驅動)")]
    [Tooltip("Auto 時打字速度(比一般快)")]
    public float autoTypeSpeed = 0.015f;
    [Tooltip("Auto 時整句播完後等待多久進入下一句")]
    public float autoWaitTime = 0.8f;

    public bool IsAuto { get; private set; }
    public bool IsSkip { get; private set; }

    private float waitTimer = 0f;
    private DialogueManager manager;

    void Start()
    {
        manager = GetComponent<DialogueManager>();

        // Skip 按鈕顯示邏輯
        if (skipUIButton != null)
        {
            skipUIButton.SetActive(allowSkip);
            Button btn = skipUIButton.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ToggleSkip);
        }
    }

    // 由玩家按 UI 按鈕觸發的正常 Skip
    public void ToggleSkip()
    {
        if (!allowSkip) return;
        IsSkip = !IsSkip;
        if (IsSkip) IsAuto = false; // Skip 開啟時，強制關閉 Auto
        waitTimer = 0f;
    }

    // 由 DialogueManager 解析 CSV 發現標記時呼叫
    public void SetAuto(bool state)
    {
        // ==========================================
        // ★ 核心修正：Skip 優先權最高！
        // 如果目前正在 Skip，嚴禁 CSV 的 Auto 標記來干擾或關閉 Skip
        // ==========================================
        if (IsSkip) return;

        IsAuto = state;
        if (IsAuto) IsSkip = false;
        waitTimer = 0f;
    }

    public float GetCurrentTypeSpeed(float defaultSpeed)
    {
        if (IsSkip) return 0f;
        if (IsAuto) return autoTypeSpeed;
        return defaultSpeed;
    }

    public bool ShouldMuteAudio()
    {
        return IsAuto || IsSkip;
    }

    void Update()
    {
        // 安全鎖：如果 Manager 已經進入 Ending 退場狀態，立刻停止所有控制
        if (manager == null || manager.CurrentState == DialogueManager.State.Ending)
            return;

        // ==========================================
        // ★ 新增：開發者專屬隱藏 Skip (偷吃步)
        // 預設快捷鍵：按住 左邊或右邊的 Ctrl 不放 + 按下 Tab 鍵
        // 無視 Inspector 上的 Allow Skip 限制，強制切換 Skip 狀態
        // ==========================================
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Tab))
        {
            IsSkip = !IsSkip;
            if (IsSkip) IsAuto = false; // 切換為 Skip 時，立刻中斷 Auto
            waitTimer = 0f;
            Debug.Log("[Developer] 隱藏 Skip 模式已觸發！目前的 Skip 狀態: " + IsSkip);
        }

        // 【Skip 行為】
        if (IsSkip)
        {
            // 打字中 -> 瞬間跳到行尾
            if (manager.CurrentState == DialogueManager.State.Typing ||
                manager.CurrentState == DialogueManager.State.Animating)
            {
                manager.ForceSkipLine();
            }
            // 等待玩家點擊中 -> 根據是否為最後一句，決定等待時間
            else if (manager.CurrentState == DialogueManager.State.WaitingForInput ||
                     manager.CurrentState == DialogueManager.State.WaitingForAfterClick)
            {
                waitTimer += Time.deltaTime;

                // 最後一句用較長的等待時間(保留完整退場)，一般句子用極快等待時間
                float targetWait = manager.IsLastLine ? skipFinalLineWaitTime : skipWaitTime;

                if (waitTimer >= targetWait)
                {
                    waitTimer = 0f;

                    if (manager.IsLastLine)
                        manager.ForceEndDialogue(); // 最後一句直接進入正常退場
                    else
                        manager.ForceNextLine();    // 一般句子找下一行
                }
            }
        }
        // 【Auto 行為】
        else if (IsAuto)
        {
            if (manager.CurrentState == DialogueManager.State.WaitingForInput ||
                manager.CurrentState == DialogueManager.State.WaitingForAfterClick)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= autoWaitTime)
                {
                    waitTimer = 0f;

                    if (manager.IsLastLine)
                        manager.ForceEndDialogue();
                    else
                        manager.ForceNextLine();
                }
            }
            else
            {
                waitTimer = 0f;
            }
        }
    }
}