using UnityEngine;

public enum TaskCompletionType
{
    ReachLocation,
    TalkToNpc,
    InteractObject,
    ClickFeature,
    CompleteEvent,
    Manual
}

[CreateAssetMenu(fileName = "NewTaskGuide", menuName = "RPG/Task Guide")]
public class TaskGuideData : ScriptableObject
{
    [Tooltip("用於內部辨識的唯一任務 ID")]
    public string taskId;

    [Tooltip("顯示在任務指引上的標題")]
    public string title;

    [TextArea(2, 4)]
    [Tooltip("顯示在任務指引上的內容")]
    public string description;

    [Header("Notebook Map Target")]
    [Tooltip("場景中的任務點 ID。線索書地圖會優先使用該 TaskGuidePoint 的位置")]
    public string targetPointId;

    [Tooltip("目標地圖區域 ID，需對應 NotebookMapController.MapArea.areaId")]
    public string targetAreaId;

    [Range(0f, 1f)]
    [Tooltip("找不到場景任務點時，標記在線索書地圖上的水平位置。0 是最左，1 是最右")]
    public float targetX01 = 0.5f;

    [Tooltip("追蹤任務時是否在線索書地圖上顯示標記")]
    public bool showMarker = true;

    [Header("Marker Icon")]
    [Tooltip("對應 TaskMarkerIconSet 內的 iconId，例如 BlueDot")]
    public string markerIconId = "BlueDot";

    [Header("Completion")]
    [Tooltip("任務完成條件類型")]
    public TaskCompletionType completionType;

    [Tooltip("完成條件目標 ID，例如任務點 ID、NPC ID、功能 ID 或事件 ID")]
    public string completionTargetId;


    [Tooltip("ReachLocation 類型使用，只判斷玩家與目標點 X 軸距離")]
    public float reachDistanceX = 0.5f;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(taskId))
            taskId = name;
    }
}
