using UnityEngine;
using System.Collections;

public class RallyPointVisual : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.5f;
    [SerializeField] private float displayDuration = 1.0f; // 顯示多久後開始消失
    [SerializeField] private float fadeDuration = 0.5f;    // 消失的過程持續多久

    private SpriteRenderer _sprite;
    private Coroutine _activeFadeRoutine;
    private Vector3 _baseScale = Vector3.one;

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void ShowAt(Vector3 position)
    {
        // 1. 設置位置並顯示
        transform.position = position;
        gameObject.SetActive(true);

        // 2. 重置狀態 (防止連續點擊時透明度卡住)
        if (_sprite != null)
        {
            Color c = _sprite.color;
            c.a = 1f;
            _sprite.color = c;
        }
        transform.localScale = _baseScale;

        // 3. 處理協程：如果上一個淡出還在跑，先停止它
        if (_activeFadeRoutine != null)
        {
            StopCoroutine(_activeFadeRoutine);
        }
        _activeFadeRoutine = StartCoroutine(FadeRoutine());
    }

    void Update()
    {
        // 呼吸燈效果 (只在 Alpha 大於 0 時跑，比較省效能)
        if (gameObject.activeInHierarchy && _sprite != null && _sprite.color.a > 0.1f)
        {
            float s = 1f + Mathf.PingPong(Time.time * pulseSpeed, pulseAmount);
            transform.localScale = _baseScale * s;
        }
    }

    private IEnumerator FadeRoutine()
    {
        // 第一階段：維持完全顯示
        yield return new WaitForSeconds(displayDuration);

        // 第二階段：線性淡出
        float elapsed = 0f;
        Color startColor = _sprite.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;

            // 同時改變透明度
            if (_sprite != null)
            {
                float newAlpha = Mathf.Lerp(1f, 0f, progress);
                _sprite.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            }

            // 選修：讓旗幟縮小一點會更有「消失感」
            transform.localScale = _baseScale * Mathf.Lerp(1f, 0.5f, progress);

            yield return null;
        }

        // 結束後隱藏
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}