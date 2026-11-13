using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskCompletionWatcher : MonoBehaviour
{
    private void Update()
    {
        var manager = TaskGuideManager.Instance;
        var task = manager != null ? manager.CurrentTask : null;
        if (task == null || task.completionType != TaskCompletionType.ReachLocation) return;
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null || !player.gameObject.activeInHierarchy) return;
        TaskGuidePoint point;
        if (TaskGuidePointRegistry.Instance == null ||
            !TaskGuidePointRegistry.Instance.TryGetPoint(task.completionTargetId, out point) ||
            point == null || !point.gameObject.activeInHierarchy) return;
        if (point.areaId != task.targetAreaId ||
            !string.Equals(SceneManager.GetActiveScene().name, task.targetAreaId,
                System.StringComparison.OrdinalIgnoreCase) ||
            point.gameObject.scene != SceneManager.GetActiveScene()) return;
        if (Mathf.Abs(player.transform.position.x - point.transform.position.x) <=
            Mathf.Max(0f, task.reachDistanceX))
            manager.CompleteCurrentTask();
    }
}
