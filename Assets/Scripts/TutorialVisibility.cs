using UnityEngine;

public class TutorialVisibility : MonoBehaviour
{
    public enum HideMode
    {
        SetGameObjectActive,
        RenderersOnly,
        CanvasGroup
    }

    [Header("Visibility")]
    [SerializeField] private HideMode hideMode = HideMode.RenderersOnly;
    [SerializeField] private bool visibleWhenTutorialTargetHighlighted = true;
    [SerializeField] private bool visibleAfterTutorialEnds = false;

    [Header("Child Objects")]
    [SerializeField] private bool includeChildren = true;
    [SerializeField] private bool includeInactiveChildren = true;

    private Renderer[] renderers;
    private CanvasGroup[] canvasGroups;

    private void Awake()
    {
        CacheTargets();
        SetVisible(false);
    }

    private void OnValidate()
    {
        CacheTargets();
    }

    public void SetTutorialHighlightVisible(bool hasHighlightTarget)
    {
        SetVisible(hasHighlightTarget && visibleWhenTutorialTargetHighlighted);
    }

    public void OnTutorialEnded()
    {
        SetVisible(visibleAfterTutorialEnds);
    }

    public void RefreshTargets()
    {
        CacheTargets();
    }

    private void CacheTargets()
    {
        if (includeChildren)
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
            canvasGroups = GetComponentsInChildren<CanvasGroup>(includeInactiveChildren);
        }
        else
        {
            Renderer renderer = GetComponent<Renderer>();
            renderers = renderer != null ? new[] { renderer } : new Renderer[0];

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroups = canvasGroup != null ? new[] { canvasGroup } : new CanvasGroup[0];
        }
    }

    private void SetVisible(bool visible)
    {
        switch (hideMode)
        {
            case HideMode.SetGameObjectActive:
                gameObject.SetActive(visible);
                break;

            case HideMode.RenderersOnly:
                SetRenderersVisible(visible);
                break;

            case HideMode.CanvasGroup:
                SetCanvasGroupsVisible(visible);
                break;
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        if (renderers == null)
        {
            CacheTargets();
        }

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = visible;
            }
        }
    }

    private void SetCanvasGroupsVisible(bool visible)
    {
        if (canvasGroups == null)
        {
            CacheTargets();
        }

        foreach (CanvasGroup targetCanvasGroup in canvasGroups)
        {
            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.alpha = visible ? 1f : 0f;
                targetCanvasGroup.interactable = visible;
                targetCanvasGroup.blocksRaycasts = visible;
            }
        }
    }
}