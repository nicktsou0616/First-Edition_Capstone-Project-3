using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleReportController : MonoBehaviour
{
    private static bool _isShowing;
    private static TMP_FontAsset _battleReportFont;

    public static void ShowReport(int remainingLives, int buildingCount, int remainingResources)
    {
        if (_isShowing) return;

        _isShowing = true;
        Time.timeScale = 0f;

        GameObject root = new GameObject("BattleReportCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject dimObject = new GameObject("DimBackground", typeof(RectTransform), typeof(Image));
        dimObject.transform.SetParent(root.transform, false);
        RectTransform dimRect = dimObject.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        dimObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panelObject = new GameObject("BattleReportPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(root.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 430f);
        panelRect.anchoredPosition = Vector2.zero;
        panelObject.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.11f, 0.96f);

        TextMeshProUGUI titleText = CreateText("Title", panelObject.transform, "戰報", 52, FontStyles.Bold);
        titleText.rectTransform.anchoredPosition = new Vector2(0f, 145f);
        titleText.rectTransform.sizeDelta = new Vector2(520f, 80f);

        TextMeshProUGUI reportText = CreateText(
            "ReportText",
            panelObject.transform,
            $"剩餘生命：{remainingLives}\n建築數：{buildingCount}\n剩餘金錢：{remainingResources}",
            34,
            FontStyles.Normal
        );
        reportText.alignment = TextAlignmentOptions.Left;
        reportText.rectTransform.anchoredPosition = new Vector2(35f, 20f);
        reportText.rectTransform.sizeDelta = new Vector2(460f, 180f);

        Button closeButton = CreateButton(panelObject.transform, "關閉");
        closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -145f);
        closeButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            _isShowing = false;
            SceneManager.LoadScene("SampleScene");
        });
    }

    public static int CountBuildings()
    {
        Platform[] platforms = FindObjectsOfType<Platform>();
        int count = 0;

        foreach (Platform platform in platforms)
        {
            if (platform != null && platform.transform.childCount > 0)
            {
                count++;
            }
        }

        return count;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string content, int fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = GetBattleReportFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        return text;
    }

    private static Button CreateButton(Transform parent, string label)
    {
        GameObject buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 70f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.25f, 0.42f, 0.95f, 1f);

        Button button = buttonObject.GetComponent<Button>();

        TextMeshProUGUI labelText = CreateText("Label", buttonObject.transform, label, 30, FontStyles.Bold);
        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private static TMP_FontAsset GetBattleReportFont()
    {
        if (_battleReportFont != null) return _battleReportFont;

        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null && text.font != null && text.font.name == "GenSekiGothic2-R SDF")
            {
                _battleReportFont = text.font;
                return _battleReportFont;
            }
        }

        return TMP_Settings.defaultFontAsset;
    }
}
