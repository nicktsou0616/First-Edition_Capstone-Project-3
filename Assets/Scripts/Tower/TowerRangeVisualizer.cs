using UnityEngine;

public class TowerRangeVisualizer : MonoBehaviour
{
    private static TowerRangeVisualizer _currentVisible;

    [SerializeField] private int segments = 96;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color color = new Color(0f, 1f, 0f, 0.55f);

    private LineRenderer _lineRenderer;
    private float _range;

    private void Awake()
    {
        CreateLineRenderer();
        Hide();
    }

    public void SetRange(float range)
    {
        _range = range;
        DrawCircle();
    }

    public void Toggle()
    {
        if (_currentVisible == this)
        {
            Hide();
            _currentVisible = null;
            return;
        }

        if (_currentVisible != null)
        {
            _currentVisible.Hide();
        }

        Show();
        _currentVisible = this;
    }

    private void Show()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
        }
    }

    private void Hide()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    private void CreateLineRenderer()
    {
        GameObject rangeObject = new GameObject("RangeVisualizer");
        rangeObject.transform.SetParent(transform);
        rangeObject.transform.localPosition = Vector3.zero;
        rangeObject.transform.localRotation = Quaternion.identity;
        rangeObject.transform.localScale = Vector3.one;

        _lineRenderer = rangeObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.loop = true;
        _lineRenderer.positionCount = segments;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
        _lineRenderer.sortingOrder = 20;

        Material material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material = material;
    }

    private void DrawCircle()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * _range,
                Mathf.Sin(angle) * _range,
                0f
            );

            _lineRenderer.SetPosition(i, point);
        }
    }

    private void OnDisable()
    {
        if (_currentVisible == this)
        {
            _currentVisible = null;
        }
    }
}