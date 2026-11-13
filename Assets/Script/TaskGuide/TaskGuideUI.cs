using TMPro;
using UnityEngine;

public class TaskGuideUI : MonoBehaviour
{
    [SerializeField]
    private GameObject root;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    private void Awake()
    {
        EnsureTextReferences();
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
        else
            gameObject.SetActive(visible);
    }

    public void Refresh(TaskGuideData task)
    {
        if (titleText != null)
            titleText.text = task != null ? task.title : "目前沒有任務";

        if (descriptionText != null)
            descriptionText.text = task != null ? task.description : "任務已完成或資料尚未載入";
    }

    private void EnsureTextReferences()
    {
        if (root == null)
            root = gameObject;

        if (titleText == null)
            titleText = transform.Find("TaskTitleText")?.GetComponent<TextMeshProUGUI>();

        if (descriptionText == null)
            descriptionText = transform.Find("TaskDescriptionText")?.GetComponent<TextMeshProUGUI>();

        if (titleText == null)
            titleText = CreateText("TaskTitleText", 20, FontStyles.Bold, new Vector2(0f, 0.58f), new Vector2(1f, 1f));

        if (descriptionText == null)
            descriptionText = CreateText("TaskDescriptionText", 16, FontStyles.Normal, new Vector2(0f, 0f), new Vector2(1f, 0.58f));
    }

    private TextMeshProUGUI CreateText(string objectName, float fontSize, FontStyles fontStyle, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(12f, 4f);
        rect.offsetMax = new Vector2(-12f, -4f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;

        return text;
    }
}
