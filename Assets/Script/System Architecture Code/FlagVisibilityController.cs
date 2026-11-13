using UnityEngine;

public class FlagVisibilityController : MonoBehaviour
{
    [Header("玩家筆記本 (資料來源)")]
    public GameProgress gameProgress;

    [Header("顯示條件設定")]
    [Tooltip("要檢查的 Flag 名稱 (例如：Talked_Kyle_1)")]
    public string targetFlag;

    public enum VisibilityMode
    {
        ShowIfHasFlag,  // 有 Flag 才顯示 (例如：拿到鑰匙後，門口出現新NPC)
        HideIfHasFlag   // 有 Flag 就隱藏 (例如：講完話後，擋路的NPC離開)
    }
    public VisibilityMode mode = VisibilityMode.ShowIfHasFlag;

    [Header("要控制的目標")]
    [Tooltip("如果留空，就會預設隱藏/顯示掛著這個腳本的物件本身")]
    public GameObject targetObject;

    private void Awake()
    {
        // 防呆：如果沒拖入目標，就把自己當作目標
        if (targetObject == null)
        {
            targetObject = this.gameObject;
        }
    }

    private void Start()
    {
        CheckVisibility();
    }

    // 獨立拉出方法，未來有需要可以在劇情結束時動態呼叫刷新
    public void CheckVisibility()
    {
        if (gameProgress == null || string.IsNullOrEmpty(targetFlag)) return;

        bool hasFlag = gameProgress.HasFlag(targetFlag);

        // 根據模式決定是否該顯示
        bool shouldBeActive = (mode == VisibilityMode.ShowIfHasFlag) ? hasFlag : !hasFlag;

        targetObject.SetActive(shouldBeActive);

        Debug.Log($"[FlagVisibilityController] 檢查 Flag '{targetFlag}' (擁有:{hasFlag}) -> 設定 {targetObject.name} 顯示狀態為: {shouldBeActive}");
    }
}