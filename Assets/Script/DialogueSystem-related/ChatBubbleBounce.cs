using UnityEngine;

public class ChatBubbleBounce : MonoBehaviour
{
    [Header("UI 參考 (UI References)")]
    [Tooltip("若保持空白，程式啟動時會自動去 Canvas 子物件下尋找 Plus_01~03")]
    public RectTransform[] plusRects = new RectTransform[3];
    public CanvasGroup[] plusCanvasGroups = new CanvasGroup[3];

    [Header("彈跳設定 (Bounce Settings)")]
    [Tooltip("彈跳的最大高度 (Pixel)")]
    public float bounceHeight = 10f;
    [Tooltip("單次彈跳循環所需時間 (秒)")]
    public float bounceDuration = 0.60f;
    [Tooltip("每個加號起跳的延遲時間差 (秒)")]
    public float phaseDelay = 0.17f;

    [Header("透明度設定 (Opacity Settings)")]
    [Range(0f, 1f)] public float minOpacity = 0.30f;
    [Range(0f, 1f)] public float maxOpacity = 1.00f;
    [Tooltip("透明度曲線：X軸為高度(0~1)，Y軸為透明度變化(0~1)")]
    public AnimationCurve opacityCurve;

    [Header("額外設定 (Optional)")]
    public bool enableScaleAnimation = false;

    private Vector2[] initialPositions = new Vector2[3];
    private float globalTime = 0f;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        if (isInitialized) return;

        // 自動從現有 Hierarchy 結構抓取元件
        Transform canvasTransform = transform.Find("Canvas");
        if (canvasTransform == null)
        {
            Debug.LogError("ChatBubbleBounce: 找不到 Canvas 子物件，請確認 Hierarchy。");
            return;
        }

        string[] plusNames = { "Plus_01", "Plus_02", "Plus_03" };

        for (int i = 0; i < 3; i++)
        {
            if (plusRects[i] == null)
            {
                Transform plusTransform = canvasTransform.Find(plusNames[i]);
                if (plusTransform != null)
                {
                    plusRects[i] = plusTransform.GetComponent<RectTransform>();
                    plusCanvasGroups[i] = plusTransform.GetComponent<CanvasGroup>();

                    // 若尚未手動加入 CanvasGroup，程式在此自動補上以防報錯
                    if (plusCanvasGroups[i] == null)
                    {
                        plusCanvasGroups[i] = plusTransform.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }

            if (plusRects[i] != null)
            {
                // 記錄最原始的 anchoredPosition，確保 X 軸與基準 Y 軸永遠不受污染
                initialPositions[i] = plusRects[i].anchoredPosition;
            }
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        InitializeReferences();

        // 每次重新顯示物件 (SetActive) 時，重置計時器
        globalTime = 0f;

        // 強制將三個點復原至基準位置與初始透明度
        for (int i = 0; i < 3; i++)
        {
            if (plusRects[i] == null || plusCanvasGroups[i] == null) continue;

            plusRects[i].anchoredPosition = initialPositions[i];
            plusCanvasGroups[i].alpha = minOpacity;

            if (enableScaleAnimation)
                plusRects[i].localScale = Vector3.one;
        }
    }

    private void Update()
    {
        // 全域計時器，絕不產生 GC
        globalTime += Time.deltaTime;

        for (int i = 0; i < 3; i++)
        {
            if (plusRects[i] == null || plusCanvasGroups[i] == null) continue;

            // 扣除相位延遲，計算該 Plus 自己的時間
            float localTime = globalTime - (i * phaseDelay);

            float yOffset = 0f;
            float height01 = 0f;

            // 當 localTime >= 0，代表度過了前期的等待，該 Plus 正式開始起跳
            if (localTime >= 0f)
            {
                // 讓時間在 0 ~ bounceDuration 之間無限循環，並標準化為 0.0 ~ 1.0
                float normalizedTime = Mathf.Repeat(localTime, bounceDuration) / bounceDuration;

                // 核心重力數學模型: y = 4 * t * (1 - t)
                height01 = 4f * normalizedTime * (1f - normalizedTime);
                yOffset = bounceHeight * height01;
            }

            // 更新 Y 軸 (加上初始位置的 Y)，絕對不改變 X 軸
            plusRects[i].anchoredPosition = new Vector2(initialPositions[i].x, initialPositions[i].y + yOffset);

            // 根據目前物理高度比例 (0~1)，透過曲線與 Lerp 決定當前透明度
            float curveValue = opacityCurve.Evaluate(height01);
            plusCanvasGroups[i].alpha = Mathf.Lerp(minOpacity, maxOpacity, curveValue);

            // 若啟用 Scale (預設關閉)
            if (enableScaleAnimation)
            {
                float scale = Mathf.Lerp(1f, 1.05f, height01);
                plusRects[i].localScale = new Vector3(scale, scale, 1f);
            }
        }
    }

    private void Reset()
    {
        // 在 Inspector 加入腳本時，自動提供適用的預設透明度對應曲線
        opacityCurve = new AnimationCurve(
            new Keyframe(0f, 0f),       // 基準高度: 對應最小透明度
            new Keyframe(0.5f, 0.65f),  // 一半高度: 對應 65% 進度的透明度
            new Keyframe(1f, 1f)        // 最高點: 對應最大透明度
        );
    }
}