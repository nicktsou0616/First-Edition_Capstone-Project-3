using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Image))]
public class TaskGuideTrackingUI : MonoBehaviour
{
    public TMP_Text statusText;
    public Color trackedColor = new Color(0.4f, 1f, 0f, 0.85f);
    public float messageSeconds = 1.5f;
    public float fadeSeconds = 0.2f;
    private Image background;
    private Button button;
    private Color originalColor;
    private float messageStartedAt;
    private bool messageVisible;

    public bool IsTracking(TaskGuideData task) =>
        task != null && TaskGuideManager.Instance != null &&
        TaskGuideManager.Instance.CurrentTask == task && TaskGuideManager.Instance.IsTracked;

    private void Awake()
    {
        background = GetComponent<Image>();
        button = GetComponent<Button>();
        originalColor = background.color;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(ToggleTracking);
        HideMessage();
    }

    private void OnEnable() { RefreshColor(); }

    private void Update()
    {
        button.interactable = TaskGuideManager.Instance != null && TaskGuideManager.Instance.CurrentTask != null;
        RefreshColor();
        if (!messageVisible || statusText == null) return;
        float elapsed = Time.unscaledTime - messageStartedAt;
        float fade = Mathf.Max(0.01f, fadeSeconds);
        float hold = Mathf.Max(0f, messageSeconds);
        if (elapsed < fade) statusText.alpha = elapsed / fade;
        else if (elapsed < fade + hold) statusText.alpha = 1f;
        else if (elapsed < fade * 2f + hold) statusText.alpha = 1f - (elapsed - fade - hold) / fade;
        else HideMessage();
    }

    private void RefreshColor()
    {
        if (background == null) background = GetComponent<Image>();
        background.color = TaskGuideManager.Instance != null && TaskGuideManager.Instance.IsTracked
            ? trackedColor : originalColor;
    }

    private void ToggleTracking()
    {
        var manager = TaskGuideManager.Instance;
        if (manager == null || manager.CurrentTask == null) return;
        manager.ToggleTracking();
        RefreshColor();
        if (statusText == null) return;
        statusText.text = manager.IsTracked ? "已追蹤任務" : "已取消追蹤任務";
        messageStartedAt = Time.unscaledTime;
        messageVisible = true;
        statusText.gameObject.SetActive(true);
        statusText.alpha = 0f;
    }

    private void HideMessage()
    {
        messageVisible = false;
        if (statusText == null) return;
        statusText.alpha = 0f;
        statusText.gameObject.SetActive(false);
    }

    private void OnDisable() { HideMessage(); }
}
