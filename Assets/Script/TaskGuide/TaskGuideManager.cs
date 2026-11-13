using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskGuideManager : MonoBehaviour
{
    public static TaskGuideManager Instance { get; private set; }
    private const string CurrentPrefix = "TaskGuideCurrent:";
    private const string DonePrefix = "TaskGuideDone:";
    private const string StartedFlag = "TaskGuideStarted";
    private const string TrackingFlag = "TaskGuideTracking";

    [SerializeField] private List<TaskGuideData> tasks = new List<TaskGuideData>();
    [SerializeField] private TaskGuideData currentTask;
    public TaskGuideData CurrentTask => currentTask;
    public bool IsTracked { get; private set; }
    public event Action<TaskGuideData> OnTaskChanged;
    public event Action<bool> OnTrackingChanged;
    private GameProgress progress;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindFirstObjectByType<TaskGuideManager>() != null) return;
        new GameObject("TaskGuideSystem").AddComponent<TaskGuideManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        progress = Resources.Load<GameProgress>("StoryAssets/MainProgress");
        if (tasks.Count == 0)
        {
            TaskGuideCatalog catalog = Resources.Load<TaskGuideCatalog>("TaskGuideAssets/TaskGuideCatalog");
            if (catalog != null)
                foreach (TaskGuideData task in catalog.tasks)
                    if (task != null) tasks.Add(task);
            if (tasks.Count == 0)
                foreach (string assetName in new[] { "Task_FindAuditorium", "Task_FindAtrium", "Task_FindNpc1" })
                {
                    TaskGuideData task = Resources.Load<TaskGuideData>("TaskGuideAssets/" + assetName);
                    if (task != null) tasks.Add(task);
                }
            if (tasks.Count == 0)
                Debug.LogError("TaskGuideManager: no TaskGuideData could be loaded. Check each asset's script reference.", this);
        }
        if (GetComponent<TaskGuidePointRegistry>() == null) gameObject.AddComponent<TaskGuidePointRegistry>();
        if (GetComponent<TaskCompletionWatcher>() == null) gameObject.AddComponent<TaskCompletionWatcher>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        RestoreFromProgress();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (TaskGuidePointRegistry.Instance != null) TaskGuidePointRegistry.Instance.RefreshPoints();
    }

    public void RestoreFromProgress()
    {
        if (progress == null) return;
        IsTracked = progress.HasFlag(TrackingFlag);
        string id = null;
        foreach (string flag in progress.flags)
            if (flag.StartsWith(CurrentPrefix, StringComparison.Ordinal)) { id = flag.Substring(CurrentPrefix.Length); break; }
        currentTask = tasks.Find(t => t != null && t.taskId == id);
        if (currentTask == null && id == null && !progress.HasFlag(StartedFlag) && tasks.Count > 0)
            SetTask(tasks[0]);
        OnTaskChanged?.Invoke(currentTask);
        OnTrackingChanged?.Invoke(IsTracked);
    }

    public void SetTask(TaskGuideData task)
    {
        currentTask = task;
        if (progress != null)
        {
            progress.AddFlag(StartedFlag);
            progress.flags.RemoveAll(f => f.StartsWith(CurrentPrefix, StringComparison.Ordinal));
            if (task != null) progress.AddFlag(CurrentPrefix + task.taskId);
        }
        OnTaskChanged?.Invoke(currentTask);
    }

    public void SetTaskById(string taskId)
    {
        var task = tasks.Find(t => t != null && t.taskId == taskId);
        if (task != null) SetTask(task);
        else Debug.LogWarning("TaskGuideManager: unknown task " + taskId, this);
    }

    public void ToggleTracking()
    {
        if (currentTask == null) return;
        IsTracked = !IsTracked;
        if (progress != null)
        {
            progress.flags.Remove(TrackingFlag);
            if (IsTracked) progress.AddFlag(TrackingFlag);
        }
        OnTrackingChanged?.Invoke(IsTracked);
    }

    public bool CompleteCurrentTask()
    {
        if (currentTask == null) return false;
        int index = tasks.FindIndex(t => t != null && t.taskId == currentTask.taskId);
        if (index < 0) return false;
        if (progress != null) progress.AddFlag(DonePrefix + currentTask.taskId);
        SetTask(index + 1 < tasks.Count ? tasks[index + 1] : null);
        return true;
    }

    public bool ReportCompletion(string targetId, TaskCompletionType type)
    {
        return currentTask != null && currentTask.completionType == type &&
               currentTask.completionTargetId == targetId && CompleteCurrentTask();
    }

    public void ClearTask() => SetTask(null);
}
