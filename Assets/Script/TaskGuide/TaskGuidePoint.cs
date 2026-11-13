using UnityEngine;

public class TaskGuidePoint : MonoBehaviour
{
    [Tooltip("任務資料用來指定目標位置的唯一 ID")]
    public string pointId;

    [Tooltip("此任務點所在的線索書地圖區域 ID，需對應 NotebookMapController.MapArea.areaId")]
    public string areaId;

    [Tooltip("方便在 Inspector 辨識的名稱")]
    public string displayName;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(pointId))
            pointId = gameObject.name;
    }
}
