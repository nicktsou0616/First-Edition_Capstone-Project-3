using UnityEngine;

public class SaveLoadUI : MonoBehaviour
{
    [Header("玩家筆記本")]
    public GameProgress gameProgress;

    // 給「存檔按鈕」的 OnClick() 呼叫
    public void OnSaveButtonClicked()
    {
        if (gameProgress != null)
        {
            gameProgress.SaveProgress();
            // 若有 UI 點擊音效，可加在此處
        }
    }

    // 給「讀檔按鈕」的 OnClick() 呼叫
    public void OnLoadButtonClicked()
    {
        if (gameProgress != null)
        {
            gameProgress.LoadProgress();
            if (TaskGuideManager.Instance != null)
                TaskGuideManager.Instance.RestoreFromProgress();
        }
    }
}
