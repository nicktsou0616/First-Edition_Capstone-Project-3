using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeductionCoreSystem_GameManager : MonoBehaviour
{
    [Header("🎯 題目設定區")]
    [Tooltip("請把左邊列表裝著空格的『外框容器 (SlotContainer)』拖進來")]
    public Transform slotContainer; // ★ 改成只要放入容器就好！

    [Tooltip("請依序輸入每個空格對應的『正確文字』(例如: Mistake, Correct)")]
    public string[] correctAnswers;

    [Header("🎮 UI 綁定區")]
    public Button submitButton;
    [Header("📦 結果視窗")]
    public ResultPopupUI resultPopup;

    void Start()
    {
        resultPopup.gameObject.SetActive(false);

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(CheckAnswers);
        }
    }

    public void CheckAnswers()
    {
        if (slotContainer == null)
        {
            Debug.LogError("🚨 找不到容器！請把 SlotContainer 拖進 CoreSystem_GameManager 裡！");
            return;
        }

        // ★ 自動尋找魔法：去容器裡面，把所有生成出來的 DropSlot 抓出來排好！
        DropSlot[] allSlots = slotContainer.GetComponentsInChildren<DropSlot>();

        bool isAllCorrect = true;

        if (allSlots.Length != correctAnswers.Length)
        {
            Debug.LogError($"🚨 數量不對！畫面生出了 {allSlots.Length} 個空格，但你只設定了 {correctAnswers.Length} 個正確答案！");
            return;
        }

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

                if (playerAnswer == expectedAnswer)
                {
                    Debug.Log($"✔️ 第 {i + 1} 格答對了！");
                }
                else
                {
                    Debug.Log($"❌ 第 {i + 1} 格答錯了！玩家選: {playerAnswer}, 正確為: {expectedAnswer}");
                    isAllCorrect = false;
                }
            }
        }

        SimulateLevelTransition(isAllCorrect);
    }

    private void SimulateLevelTransition(bool success)
    {
        if (success)
        {
            resultPopup.gameObject.SetActive(true);
            resultPopup.Show(
                "答對了！只有史萊姆符合「長條拖延痕跡」、「黏液殘留」的要素呢。",
                OnSuccessConfirm
            );
        }
        else
        {
            resultPopup.gameObject.SetActive(true);
            resultPopup.Show(
                "錯了喔！你再想想看吧，有痕跡代表不會飛，而且聚集在水源處而非洞穴也不是哥布林的習性。",
                OnFailConfirm
            );
        }
    }

    private void OnSuccessConfirm()
    {
        Debug.Log("成功確認");
        SceneManager.LoadScene("Game");
    }

    private void OnFailConfirm()
    {
        Debug.Log("失敗確認");
    }
}