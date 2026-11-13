using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InvestigationModule : MonoBehaviour
{
    [Header("🎯 題目設定區 (此題專屬)")]
    [Tooltip("請把左邊列表裝著空格的『外框容器 (SlotContainer)』拖進來")]
    public Transform slotContainer;

    [Tooltip("請依序輸入每個空格對應的『正確文字』(例如: Mistake, Correct)")]
    public string[] correctAnswers;

    [Header("🎮 UI 綁定區")]
    public Button submitButton;

    // 紀錄 StoryManager 派來的任務結束通知
    private Action<bool> onCompleteCallback;

    /// <summary>
    /// 由 StoryManager 呼叫，準備接收結果
    /// </summary>
    public void StartDeduction(Action<bool> callback)
    {
        onCompleteCallback = callback;

        // 綁定送出按鈕
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckAnswers);
        }
    }

    /// <summary>
    /// 玩家按下送出時檢查答案 (與你原本的邏輯完全相同！)
    /// </summary>
    public void CheckAnswers()
    {
        if (slotContainer == null)
        {
            Debug.LogError("🚨 找不到容器！請把 SlotContainer 拖進 InvestigationModule 裡！");
            return;
        }

        // 自動尋找魔法：去容器裡面，把所有生成出來的 DropSlot 抓出來排好！
        DropSlot[] allSlots = slotContainer.GetComponentsInChildren<DropSlot>();

        if (allSlots.Length != correctAnswers.Length)
        {
            Debug.LogWarning($"🚨 數量不對！畫面生出了 {allSlots.Length} 個空格，但你只設定了 {correctAnswers.Length} 個正確答案！");
            return;
        }

        bool isAllCorrect = true;

        for (int i = 0; i < allSlots.Length; i++)
        {
            DropSlot slot = allSlots[i];

            if (slot.currentItem == null)
            {
                Debug.LogWarning($"⚠️ 第 {i + 1} 個空格還沒填喔！");
                return; // 有空格沒填，拒絕結算
            }

            TextMeshProUGUI itemText = slot.currentItem.GetComponentInChildren<TextMeshProUGUI>();
            if (itemText != null)
            {
                string playerAnswer = itemText.text.Trim();
                string expectedAnswer = correctAnswers[i].Trim();

                if (playerAnswer != expectedAnswer)
                {
                    Debug.Log($"❌ 第 {i + 1} 格答錯了！玩家選: {playerAnswer}, 正確為: {expectedAnswer}");
                    isAllCorrect = false;
                }
                else
                {
                    Debug.Log($"✔️ 第 {i + 1} 格答對了！");
                }
            }
        }

        // 將結果 (true/false) 回傳給 StoryManager，由導演決定下一步！
        onCompleteCallback?.Invoke(isAllCorrect);
    }
}