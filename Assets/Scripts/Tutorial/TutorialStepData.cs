using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
public class TutorialStepData : ScriptableObject
{
    public Sprite assistantIcon;

    [TextArea(3, 6)]
    public string dialogueText;

    public string targetID;
    public float highlightRadius = 80f;
    public Vector2 highlightPadding = new Vector2(12f, 12f);
    public Color dimColor = new Color(0.08f, 0.08f, 0.08f, 0.78f);
    public Color highlightTintColor = new Color(1f, 1f, 1f, 0.04f);
    public Color highlightBorderColor = new Color(1f, 1f, 1f, 0.45f);
    public float highlightBorderThickness = 4f;

    public Vector2 assistantAnchoredPosition;
    public Vector2 dialogueAnchoredPosition;
    public bool placeDialogueRightOfAssistant;
    public Vector2 dialogueOffsetFromAssistant = new Vector2(24f, 0f);
    public bool pauseGame = true;
}
