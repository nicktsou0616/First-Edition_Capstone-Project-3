using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskGuideCatalog", menuName = "RPG/Task Guide Catalog")]
public class TaskGuideCatalog : ScriptableObject
{
    [Tooltip("依遊戲進程排列；新增任務時在 Inspector 將 TaskGuideData 加入此清單")]
    public List<TaskGuideData> tasks = new List<TaskGuideData>();
}
