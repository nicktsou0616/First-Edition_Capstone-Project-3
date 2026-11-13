using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene HUD task panel, independent of the notebook's open/closed state.</summary>
public class TaskGuideHUD : MonoBehaviour
{
    private TaskGuideUI taskUI;
    private TaskGuideData displayedTask;

    private void Awake()
    {
        var panel = new GameObject("TaskGuidePanel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -112f);
        rect.sizeDelta = new Vector2(300f, 82f);

        var background = panel.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.58f);
        background.raycastTarget = true;

        taskUI = panel.AddComponent<TaskGuideUI>();
        var map = GetComponentInChildren<NotebookMapController>(true);
        TMP_FontAsset font = map != null ? map.ChineseFont : null;
        if (font != null)
            foreach (var label in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
                label.font = font;

        var statusObject = new GameObject("TrackingStatusText", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        statusObject.transform.SetParent(panel.transform, false);
        var statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.anchorMin = statusRect.anchorMax = new Vector2(0.5f, 0f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -8f);
        statusRect.sizeDelta = new Vector2(300f, 28f);
        var status = statusObject.GetComponent<TextMeshProUGUI>();
        if (font != null) status.font = font;
        status.fontSize = 20f;
        status.color = Color.white;
        status.alignment = TextAlignmentOptions.Center;
        status.raycastTarget = false;
        statusObject.SetActive(false);

        var tracking = panel.AddComponent<TaskGuideTrackingUI>();
        tracking.statusText = status;
        taskUI.Refresh(null);
    }

    private void Update()
    {
        if (taskUI == null) return;
        var manager = TaskGuideManager.Instance;
        var task = manager != null ? manager.CurrentTask : null;
        if (task == displayedTask) return;
        displayedTask = task;
        taskUI.Refresh(task);
    }
}
