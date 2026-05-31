using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TutorialSpotlightOverlay : Graphic, ICanvasRaycastFilter
{
    [SerializeField] private Color dimColor = new Color(0.08f, 0.08f, 0.08f, 0.78f);
    [SerializeField] private Color highlightTintColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color borderColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private float borderThickness = 4f;

    private readonly Vector3[] _targetCorners = new Vector3[4];
    private RectTransform _target;
    private Renderer _worldRendererTarget;
    private Collider2D _worldCollider2DTarget;
    private Collider _worldColliderTarget;
    private Transform _worldTransformTarget;
    private Rect _holeRect;
    private Vector2 _padding;
    private Canvas _canvas;
    private bool _hasTarget;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = true;
        _canvas = GetComponentInParent<Canvas>();
    }

    private void LateUpdate()
    {
        if (_target != null)
        {
            UpdateHoleRect();
            SetVerticesDirty();
            return;
        }

        if (_worldCollider2DTarget != null)
        {
            UpdateHoleRect(_worldCollider2DTarget.bounds);
            SetVerticesDirty();
            return;
        }

        if (_worldColliderTarget != null)
        {
            UpdateHoleRect(_worldColliderTarget.bounds);
            SetVerticesDirty();
            return;
        }

        if (_worldRendererTarget != null)
        {
            UpdateHoleRect(_worldRendererTarget.bounds);
            SetVerticesDirty();
            return;
        }

        if (_worldTransformTarget != null)
        {
            UpdateHoleRect(new Bounds(_worldTransformTarget.position, Vector3.one));
            SetVerticesDirty();
        }
    }

    public void Show(RectTransform target, Vector2 padding, Color stepDimColor, Color stepHighlightTintColor, Color stepBorderColor, float stepBorderThickness)
    {
        ClearWorldTargets();
        _target = target;
        _padding = padding;
        dimColor = stepDimColor;
        highlightTintColor = stepHighlightTintColor;
        borderColor = stepBorderColor;
        borderThickness = Mathf.Max(0f, stepBorderThickness);
        _hasTarget = target != null;

        if (_hasTarget)
        {
            UpdateHoleRect();
        }

        gameObject.SetActive(true);
        SetVerticesDirty();
    }

    public void ShowWorldTarget(GameObject targetObject, Vector2 padding, Color stepDimColor, Color stepHighlightTintColor, Color stepBorderColor, float stepBorderThickness)
    {
        ClearWorldTargets();
        _target = null;
        _padding = padding;
        dimColor = stepDimColor;
        highlightTintColor = stepHighlightTintColor;
        borderColor = stepBorderColor;
        borderThickness = Mathf.Max(0f, stepBorderThickness);
        _worldCollider2DTarget = targetObject != null ? targetObject.GetComponentInChildren<Collider2D>() : null;
        _worldColliderTarget = targetObject != null ? targetObject.GetComponentInChildren<Collider>() : null;
        _worldRendererTarget = targetObject != null ? targetObject.GetComponentInChildren<Renderer>() : null;
        _worldTransformTarget = targetObject != null ? targetObject.transform : null;
        _hasTarget = targetObject != null;

        if (_worldCollider2DTarget != null)
        {
            UpdateHoleRect(_worldCollider2DTarget.bounds);
        }
        else if (_worldColliderTarget != null)
        {
            UpdateHoleRect(_worldColliderTarget.bounds);
        }
        else if (_worldRendererTarget != null)
        {
            UpdateHoleRect(_worldRendererTarget.bounds);
        }
        else if (_worldTransformTarget != null)
        {
            UpdateHoleRect(new Bounds(_worldTransformTarget.position, Vector3.one));
        }

        gameObject.SetActive(true);
        SetVerticesDirty();
    }

    public void Hide()
    {
        _target = null;
        ClearWorldTargets();
        _hasTarget = false;
        gameObject.SetActive(false);
        SetVerticesDirty();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!_hasTarget)
            return true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint);
        return !_holeRect.Contains(localPoint);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect fullRect = GetPixelAdjustedRect();
        if (!_hasTarget)
        {
            AddRect(vh, fullRect, dimColor);
            return;
        }

        Rect hole = ClampRect(_holeRect, fullRect);

        AddRect(vh, new Rect(fullRect.xMin, hole.yMax, fullRect.width, fullRect.yMax - hole.yMax), dimColor);
        AddRect(vh, new Rect(fullRect.xMin, fullRect.yMin, fullRect.width, hole.yMin - fullRect.yMin), dimColor);
        AddRect(vh, new Rect(fullRect.xMin, hole.yMin, hole.xMin - fullRect.xMin, hole.height), dimColor);
        AddRect(vh, new Rect(hole.xMax, hole.yMin, fullRect.xMax - hole.xMax, hole.height), dimColor);

        if (highlightTintColor.a > 0f)
        {
            AddRect(vh, hole, highlightTintColor);
        }

        if (borderThickness > 0f && borderColor.a > 0f)
        {
            float thickness = borderThickness;
            AddRect(vh, new Rect(hole.xMin, hole.yMax - thickness, hole.width, thickness), borderColor);
            AddRect(vh, new Rect(hole.xMin, hole.yMin, hole.width, thickness), borderColor);
            AddRect(vh, new Rect(hole.xMin, hole.yMin, thickness, hole.height), borderColor);
            AddRect(vh, new Rect(hole.xMax - thickness, hole.yMin, thickness, hole.height), borderColor);
        }
    }

    private void UpdateHoleRect()
    {
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        Camera canvasCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
        _target.GetWorldCorners(_targetCorners);

        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        for (int i = 0; i < _targetCorners.Length; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, _targetCorners[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvasCamera, out Vector2 localPoint);
            min = Vector2.Min(min, localPoint);
            max = Vector2.Max(max, localPoint);
        }

        min -= _padding;
        max += _padding;
        _holeRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void UpdateHoleRect(Bounds worldBounds)
    {
        if (Camera.main == null)
        {
            _hasTarget = false;
            return;
        }

        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;
        Vector3[] worldCorners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z),
            new Vector3(max.x, min.y, max.z)
        };

        UpdateHoleRectFromWorldCorners(worldCorners, Camera.main);
    }

    private void UpdateHoleRectFromWorldCorners(Vector3[] worldCorners, Camera worldCamera)
    {
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        Camera canvasCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        for (int i = 0; i < worldCorners.Length; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldCorners[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvasCamera, out Vector2 localPoint);
            min = Vector2.Min(min, localPoint);
            max = Vector2.Max(max, localPoint);
        }

        min -= _padding;
        max += _padding;
        _holeRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void ClearWorldTargets()
    {
        _worldRendererTarget = null;
        _worldCollider2DTarget = null;
        _worldColliderTarget = null;
        _worldTransformTarget = null;
    }

    private static Rect ClampRect(Rect rect, Rect bounds)
    {
        float xMin = Mathf.Clamp(rect.xMin, bounds.xMin, bounds.xMax);
        float xMax = Mathf.Clamp(rect.xMax, bounds.xMin, bounds.xMax);
        float yMin = Mathf.Clamp(rect.yMin, bounds.yMin, bounds.yMax);
        float yMax = Mathf.Clamp(rect.yMax, bounds.yMin, bounds.yMax);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static void AddRect(VertexHelper vh, Rect rect, Color color)
    {
        if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f)
            return;

        int startIndex = vh.currentVertCount;
        vh.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMax), color, Vector2.zero);
        vh.AddVert(new Vector3(rect.xMax, rect.yMin), color, Vector2.zero);
        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
}
