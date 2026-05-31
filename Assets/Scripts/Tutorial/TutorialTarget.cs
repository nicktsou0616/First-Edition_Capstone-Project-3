using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    [SerializeField] private string targetID;
    [SerializeField] private RectTransform targetRect;

    public string TargetID => targetID;
    public RectTransform TargetRect => targetRect != null ? targetRect : transform as RectTransform;

    private void OnEnable()
    {
        TutorialManager.RegisterTarget(this);
    }

    private void OnDisable()
    {
        TutorialManager.UnregisterTarget(this);
    }
}