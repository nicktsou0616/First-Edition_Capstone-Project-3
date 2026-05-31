using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance;

    private const string RuntimePromptName = "RuntimeInteractionPromptCanvas";
    private const string PromptFontName = "GenSekiGothic2-R SDF";

    public TextMeshProUGUI promptText;
    public string promptMessage = "按空白鍵互動";
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    public float smoothTime = 0.06f;

    private Transform target;
    private Vector2 smoothVelocity;
    private bool hasPosition;

    private void Awake()
    {
        if (gameObject.name != RuntimePromptName)
        {
            enabled = false;
            return;
        }

        if (promptText != null || Instance == null)
        {
            Instance = this;
        }

        Hide();
    }

    private void LateUpdate()
    {
        if (target == null || promptText == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + worldOffset);
        promptText.enabled = screenPos.z > 0f;
        if (!promptText.enabled) return;

        RectTransform promptRect = promptText.rectTransform;
        Canvas canvas = promptText.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            promptRect.position = screenPos;
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (canvasCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = mainCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvasCamera, out Vector2 localPoint))
        {
            localPoint.y += 300f;

            if (!hasPosition || smoothTime <= 0f)
            {
                promptRect.anchoredPosition = localPoint;
                smoothVelocity = Vector2.zero;
                hasPosition = true;
            }
            else
            {
                promptRect.anchoredPosition = Vector2.SmoothDamp(
                    promptRect.anchoredPosition,
                    localPoint,
                    ref smoothVelocity,
                    smoothTime
                );
            }
        }
    }

    public void Show(Transform player)
    {
        if (promptText == null) return;

        if (target != player)
        {
            hasPosition = false;
            smoothVelocity = Vector2.zero;
        }

        target = player;
        ActivateHierarchy(promptText.transform);
        promptText.text = promptMessage;
        promptText.enabled = true;
    }

    public void Hide()
    {
        target = null;
        hasPosition = false;
        smoothVelocity = Vector2.zero;
        if (promptText != null)
            promptText.enabled = false;
    }

    public static void ShowFor(Transform player)
    {
        InteractionPromptUI prompt = GetOrCreateInstance();
        if (prompt != null)
        {
            prompt.Show(player);
        }
    }

    public static void HideCurrent()
    {
        if (Instance != null)
        {
            Instance.Hide();
        }
    }

    private static InteractionPromptUI GetOrCreateInstance()
    {
        if (Instance != null && Instance.promptText != null)
        {
            return Instance;
        }

        GameObject existingRuntimePrompt = GameObject.Find(RuntimePromptName);
        if (existingRuntimePrompt != null)
        {
            Instance = existingRuntimePrompt.GetComponent<InteractionPromptUI>();
            if (Instance != null && Instance.promptText != null)
            {
                return Instance;
            }
        }

        GameObject canvasObject = new GameObject(RuntimePromptName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("InteractionPromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = GetPromptFont();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 48f;
        text.color = Color.white;
        text.outlineColor = Color.black;
        text.outlineWidth = 0.2f;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(420f, 80f);

        Instance = canvasObject.AddComponent<InteractionPromptUI>();
        Instance.promptText = text;
        Instance.Hide();
        return Instance;
    }

    private static TMP_FontAsset GetPromptFont()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text != null && text.font != null && text.font.name == PromptFontName)
            {
                return text.font;
            }
        }

        return TMP_Settings.defaultFontAsset;
    }

    private static void ActivateHierarchy(Transform transformToActivate)
    {
        Transform current = transformToActivate;
        while (current != null)
        {
            current.gameObject.SetActive(true);
            current = current.parent;
        }
    }
}
