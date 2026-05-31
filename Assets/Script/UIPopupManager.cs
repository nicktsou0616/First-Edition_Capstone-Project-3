using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIPopupManager : MonoBehaviour
{
    public static UIPopupManager Instance;

    // 🐧 Linux 概念：嚴格的系統狀態機 (State Machine)
    public enum PopupState
    {
        Idle,       // 閒置待機
        FadingIn,   // 淡入中
        Showing,    // 穩定顯示中 (計時器運作)
        FadingOut   // 淡出中
    }

    [Header("UI 模組")]
    public CanvasGroup popupCanvasGroup;
    public TextMeshProUGUI popupTextUI;

    [Header("時間設定 (Timers)")]
    public float fadeTime = 0.2f;
    public float holdTime = 2.0f; // 訊息保證停留時間

    [Header("🛠️ Linux Syslog (日誌系統)")]
    public bool enableVerboseLog = true;

    [SerializeField, InspectorLabel("當前系統狀態 (唯讀)")]
    private PopupState currentState = PopupState.Idle;

    // 🐧 Linux 概念：工作佇列 (Workqueue) - 處理多重事件併發
    private Queue<string> messageQueue = new Queue<string>();

    // 內部計時器
    private float stateTimer = 0f;
    private string currentMessage = "";

    // 提供給外部判斷是否還有任務在跑
    public bool IsPlaying => currentState != PopupState.Idle || messageQueue.Count > 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.gameObject.SetActive(true); // 永遠保持 Active，避免 Coroutine 死亡
        }
    }

    // =========================================
    // 🐧 Linux 概念：中斷處理程序 (Interrupt Handler)
    // 外部呼叫 ShowText 就像發出硬體中斷，只做最輕量的紀錄，不阻塞主程式
    // =========================================
    public void ShowText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 如果目前正在顯示一模一樣的字，我們不排隊，直接「重置計時器延壽」
        if (currentState == PopupState.Showing && currentMessage == text)
        {
            stateTimer = holdTime;
            if (enableVerboseLog)
                Debug.Log($"<color=orange>[Popup Syslog]</color> 收到重複訊息 '{text}'，直接重置保留計時器 (Heartbeat)。");
            return;
        }

        // 不同的字，或者還在其他狀態，就乖乖排隊
        messageQueue.Enqueue(text);
        if (enableVerboseLog)
            Debug.Log($"<color=cyan>[Popup Syslog]</color> 訊息加入佇列: '{text}' | 目前排隊數量: {messageQueue.Count}");
    }

    public void Hide()
    {
        if (currentState == PopupState.Showing || currentState == PopupState.FadingIn)
        {
            ChangeState(PopupState.FadingOut);
        }
    }

    // =========================================
    // 🐧 Linux 概念：背景守護行程 (Daemon)
    // 負責消化佇列、推動狀態機與計時器
    // =========================================
    private void Update()
    {
        switch (currentState)
        {
            case PopupState.Idle:
                // 檢查佇列是否有工作
                if (messageQueue.Count > 0)
                {
                    currentMessage = messageQueue.Dequeue();
                    if (popupTextUI != null) popupTextUI.text = currentMessage;
                    ChangeState(PopupState.FadingIn);
                }
                break;

            case PopupState.FadingIn:
                stateTimer += Time.deltaTime;
                float inAlpha = Mathf.Clamp01(stateTimer / fadeTime);
                if (popupCanvasGroup != null) popupCanvasGroup.alpha = inAlpha;

                if (stateTimer >= fadeTime)
                {
                    ChangeState(PopupState.Showing);
                }
                break;

            case PopupState.Showing:
                stateTimer -= Time.deltaTime;

                // 完整的透明 Debug：每 0.5 秒印一次狀態 (避免每幀洗頻，但能看見進度)
                if (enableVerboseLog && Mathf.FloorToInt(stateTimer * 10) % 5 == 0)
                {
                    Debug.Log($"[Popup Syslog] 顯示中: '{currentMessage}' | 剩餘時間: {stateTimer:F2}s | 佇列等待數: {messageQueue.Count}");
                }

                if (stateTimer <= 0)
                {
                    ChangeState(PopupState.FadingOut);
                }
                break;

            case PopupState.FadingOut:
                stateTimer += Time.deltaTime;
                float outAlpha = 1f - Mathf.Clamp01(stateTimer / fadeTime);
                if (popupCanvasGroup != null) popupCanvasGroup.alpha = outAlpha;

                if (stateTimer >= fadeTime)
                {
                    currentMessage = "";
                    ChangeState(PopupState.Idle);
                }
                break;
        }
    }

    // =========================================
    // 狀態切換與初始化
    // =========================================
    private void ChangeState(PopupState newState)
    {
        if (enableVerboseLog)
            Debug.Log($"<color=yellow>[Popup Syslog]</color> 狀態切換: {currentState} ➔ {newState}");

        currentState = newState;
        stateTimer = 0f; // 狀態切換時重置本地計時器

        if (newState == PopupState.Showing)
        {
            stateTimer = holdTime; // 進入 Showing 狀態，賦予完整保留時間
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class UIPopupManager : MonoBehaviour
//{
//    public static UIPopupManager Instance;
//    public Text popupText;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    public void ShowPopup(string text, float duration = 2f)
//    {
//        StartCoroutine(PopupCoroutine(text, duration));
//    }

//    private IEnumerator PopupCoroutine(string text, float duration)
//    {
//        popupText.text = text;
//        popupText.gameObject.SetActive(true);
//        yield return new WaitForSeconds(duration);
//        popupText.gameObject.SetActive(false);
//    }
//}
