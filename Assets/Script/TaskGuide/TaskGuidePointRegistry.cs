using System.Collections.Generic;
using UnityEngine;

public class TaskGuidePointRegistry : MonoBehaviour
{
    public static TaskGuidePointRegistry Instance { get; private set; }

    private readonly Dictionary<string, TaskGuidePoint> points = new Dictionary<string, TaskGuidePoint>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshPoints();
    }

    public void RefreshPoints()
    {
        points.Clear();

        TaskGuidePoint[] found = FindObjectsByType<TaskGuidePoint>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (TaskGuidePoint point in found)
        {
            Register(point);
        }
    }

    public void Register(TaskGuidePoint point)
    {
        if (point == null || string.IsNullOrEmpty(point.pointId)) return;
        points[point.pointId] = point;
    }

    public bool TryGetPoint(string pointId, out TaskGuidePoint point)
    {
        point = null;
        if (string.IsNullOrEmpty(pointId)) return false;

        if (points.TryGetValue(pointId, out point) && point != null)
            return true;

        RefreshPoints();
        return points.TryGetValue(pointId, out point) && point != null;
    }
}
