using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private Image assistantImage;
    [SerializeField] private Text instructionText;
    [SerializeField] private RectTransform highlightCircle;
    [SerializeField] private TutorialSpotlightOverlay spotlightOverlay;
    [SerializeField] private Button nextButton;
    [SerializeField] private TutorialVisibility[] tutorialVisibilityTargets;

    [Header("Tutorial")]
    [SerializeField] private TutorialSequenceData currentTutorial;

    private int _currentStepIndex;
    private static readonly Dictionary<string, TutorialTarget> AllTargets = new Dictionary<string, TutorialTarget>();

    private void Awake()
    {
        Instance = this;
        EnsureSpotlightOverlay();
        EnsureNextButton();
    }

    private void Start()
    {
        if (currentTutorial != null && currentTutorial.steps.Count > 0)
        {
            StartTutorial();
        }
        else
        {
            EndTutorial();
        }
    }

    public void StartTutorial()
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 1f;
            tutorialCanvasGroup.blocksRaycasts = true;
            tutorialCanvasGroup.interactable = true;
        }

        SetNextButtonVisible(true);

        _currentStepIndex = 0;
        ShowStep(_currentStepIndex);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTimeScale(0f);
        }
    }

    private void ShowStep(int index)
    {
        if (currentTutorial == null || index >= currentTutorial.steps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStepData step = currentTutorial.steps[index];
        if (step == null)
        {
            OnNextStepClicked();
            return;
        }

        RectTransform assistantRect = null;
        if (assistantImage != null)
        {
            assistantImage.sprite = step.assistantIcon;

            assistantRect = assistantImage.rectTransform;
            assistantRect.anchoredPosition = step.assistantAnchoredPosition;
        }

        if (dialogueText != null)
        {
            dialogueText.text = step.dialogueText;
            ApplyDialoguePosition(dialogueText.rectTransform, assistantRect, step);
        }

        if (instructionText != null)
        {
            instructionText.text = step.dialogueText;
            ApplyDialoguePosition(instructionText.rectTransform, assistantRect, step);
        }

        GameObject targetObject = FindTargetObject(step.targetID);
        bool hasHighlightTarget = targetObject != null;

        SetTutorialVisibilityForHighlight(hasHighlightTarget);

        if (hasHighlightTarget)
        {
            if (spotlightOverlay != null)
            {
                if (targetObject.transform is RectTransform targetRect)
                {
                    spotlightOverlay.Show(targetRect, GetHighlightPadding(step), step.dimColor, step.highlightTintColor, step.highlightBorderColor, step.highlightBorderThickness);
                }
                else
                {
                    spotlightOverlay.ShowWorldTarget(targetObject, GetHighlightPadding(step), step.dimColor, step.highlightTintColor, step.highlightBorderColor, step.highlightBorderThickness);
                }
            }

            if (highlightCircle != null)
            {
                highlightCircle.gameObject.SetActive(false);
            }
        }
        else
        {
            if (spotlightOverlay != null)
            {
                spotlightOverlay.Show(null, Vector2.zero, step.dimColor, Color.clear, step.highlightBorderColor, 0f);
            }

            if (highlightCircle != null)
            {
                highlightCircle.gameObject.SetActive(false);
            }
        }
    }

    

    public void OnNextStepClicked()
    {
        _currentStepIndex++;
        ShowStep(_currentStepIndex);
    }

    public void EndTutorial()
    {   
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.blocksRaycasts = false;
            tutorialCanvasGroup.interactable = false;
        }

        if (spotlightOverlay != null)
        {
            spotlightOverlay.Hide();
        }

        SetNextButtonVisible(false);
        SetTutorialVisibilityForEnd();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyGameSpeed();
        }

        foreach (TutorialVisibility visibilityTarget in tutorialVisibilityTargets)
        {
            if (visibilityTarget != null)
            {
                visibilityTarget.OnTutorialEnded();
            }
        }
    }

    public static void RegisterTarget(TutorialTarget target)
    {
        if (target == null || string.IsNullOrEmpty(target.TargetID))
            return;

        AllTargets[target.TargetID] = target;
    }

    public static void UnregisterTarget(TutorialTarget target)
    {
        if (target == null || string.IsNullOrEmpty(target.TargetID))
            return;

        if (AllTargets.TryGetValue(target.TargetID, out TutorialTarget registeredTarget) && registeredTarget == target)
        {
            AllTargets.Remove(target.TargetID);
        }
    }

    private static Vector2 GetHighlightPadding(TutorialStepData step)
    {
        if (step.highlightPadding != Vector2.zero)
            return step.highlightPadding;

        return new Vector2(step.highlightRadius, step.highlightRadius);
    }

    private static void ApplyDialoguePosition(RectTransform dialogueRect, RectTransform assistantRect, TutorialStepData step)
    {
        if (dialogueRect == null)
            return;

        if (!step.placeDialogueRightOfAssistant || assistantRect == null)
        {
            dialogueRect.anchoredPosition = step.dialogueAnchoredPosition;
            return;
        }

        float assistantRightEdge = assistantRect.anchoredPosition.x + ((1f - assistantRect.pivot.x) * assistantRect.rect.width);
        float dialogueLeftPivotOffset = dialogueRect.pivot.x * dialogueRect.rect.width;
        float dialogueX = assistantRightEdge + dialogueLeftPivotOffset + step.dialogueOffsetFromAssistant.x;
        float dialogueY = assistantRect.anchoredPosition.y + step.dialogueOffsetFromAssistant.y;

        dialogueRect.anchoredPosition = new Vector2(dialogueX, dialogueY);
    }

    private static GameObject FindTargetObject(string targetID)
    {
        if (string.IsNullOrEmpty(targetID))
            return null;

        if (AllTargets.TryGetValue(targetID, out TutorialTarget target))
            return target.TargetRect != null ? target.TargetRect.gameObject : target.gameObject;

        return FindSceneObjectByName(targetID);
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject != null)
            return targetObject;

        Platform[] platforms = Object.FindObjectsByType<Platform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i].name == objectName)
                return platforms[i].gameObject;
        }

        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            GameObject sceneObject = sceneObjects[i];
            if (sceneObject.name == objectName && sceneObject.scene.IsValid())
                return sceneObject;
        }

        return null;
    }

    private void EnsureSpotlightOverlay()
    {
        if (spotlightOverlay == null)
        {
            spotlightOverlay = GetComponentInChildren<TutorialSpotlightOverlay>(true);
        }

        if (spotlightOverlay == null)
        {
            GameObject overlayObject = new GameObject("SpotlightOverlay", typeof(RectTransform), typeof(TutorialSpotlightOverlay));
            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();

            overlayRect.SetParent(transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.SetAsFirstSibling();

            spotlightOverlay = overlayObject.GetComponent<TutorialSpotlightOverlay>();
        }

        spotlightOverlay.Hide();
    }

    private void EnsureNextButton()
    {
        if (nextButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].name == "NextButton")
                {
                    nextButton = buttons[i];
                    break;
                }
            }
        }

        if (nextButton == null)
        {
            nextButton = CreateNextButton();
        }

        nextButton.onClick.RemoveListener(OnNextStepClicked);
        nextButton.onClick.AddListener(OnNextStepClicked);
        nextButton.transform.SetAsLastSibling();
        SetNextButtonVisible(false);
    }

    private Button CreateNextButton()
    {
        GameObject buttonObject = new GameObject("NextButton", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(transform, false);
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-48f, 48f);
        buttonRect.sizeDelta = new Vector2(160f, 56f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.92f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(buttonRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "Next";
        label.fontSize = 26f;
        label.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        return buttonObject.GetComponent<Button>();
    }

    private void SetNextButtonVisible(bool visible)
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(visible);
            nextButton.interactable = visible;
        }
    }

    private void SetTutorialVisibilityForHighlight(bool hasHighlightTarget)
{
    if (tutorialVisibilityTargets == null)
        return;

    foreach (TutorialVisibility target in tutorialVisibilityTargets)
    {
        if (target != null)
        {
            target.SetTutorialHighlightVisible(hasHighlightTarget);
        }
    }
}

    private void SetTutorialVisibilityForEnd()
    {
        if (tutorialVisibilityTargets == null)
            return;

        foreach (TutorialVisibility target in tutorialVisibilityTargets)
        {
            if (target != null)
            {
                target.OnTutorialEnded();
            }
        }
    }
    
}
