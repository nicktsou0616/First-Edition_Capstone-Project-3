using UnityEngine;
using System.Collections;

public class AutoStoryPlayer : MonoBehaviour
{
    [Header("玩家筆記本")]
    public GameProgress gameProgress;

    [Header("要播放的劇本")]
    public StoryData storyToPlay;

    // 先定義列舉型別 (不可以加 Header)
    public enum PlayCondition
    {
        Always,             // 每次進這個場景都播
        PlayIfHasFlag,      // 【有】某個 Flag 才播
        PlayIfNotHasFlag    // 【沒有】某個 Flag 才播 (最常用：例如第一次進房間播，拿到 Intro_Done 後就不再播)
    }

    // 再宣告變數 (把 Header 加在這裡)
    [Header("觸發條件")]
    public PlayCondition condition = PlayCondition.Always;

    [Tooltip("如果是 Always 此欄位留空即可")]
    public string checkFlag;

    private void Start()
    {
        // 稍微等 0.1 秒，確保 StoryManager 跟場景已經完全載入初始化
        StartCoroutine(CheckAndPlayRoutine());
    }

    private IEnumerator CheckAndPlayRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        bool canPlay = true;

        if (condition != PlayCondition.Always && gameProgress != null && !string.IsNullOrEmpty(checkFlag))
        {
            bool hasFlag = gameProgress.HasFlag(checkFlag);

            if (condition == PlayCondition.PlayIfHasFlag && !hasFlag) canPlay = false;
            if (condition == PlayCondition.PlayIfNotHasFlag && hasFlag) canPlay = false;
        }

        if (canPlay && storyToPlay != null)
        {
            Debug.Log($"[AutoStoryPlayer] 條件符合，開始自動播放劇情: {storyToPlay.name}");
            StoryManager.Instance.PlayStory(storyToPlay);
        }
        else
        {
            Debug.Log($"[AutoStoryPlayer] 條件不符，跳過自動播放。");
        }
    }
}