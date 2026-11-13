using UnityEngine;

public enum NotebookCategory
{
    Inventory,      // 物品欄
    Glossary,       // 名詞解釋；對應目前書本的「資料庫」分頁
    StoryProgress   // 故事進程
}

public enum NotebookUnlockMode
{
    Locked,         // 尚未設定解鎖條件，保持隱藏
    Flag,           // 取得指定故事 Flag 後顯示
    EvidenceOwned   // 取得指定證物後顯示
}

[CreateAssetMenu(
    fileName = "NewNotebookEntry",
    menuName = "StorySystem/Notebook Entry"
)]
public class NotebookEntryData : ScriptableObject
{
    [Header("條目內容")]
    public string entryID;
    public NotebookCategory category;
    public string title;

    [TextArea(4, 12)]
    public string content;

    public Sprite icon;

    [Header("顯示條件")]
    public NotebookUnlockMode unlockMode = NotebookUnlockMode.Locked;
    public string requiredFlag;
    public EvidenceData requiredEvidence;
}