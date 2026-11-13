using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimationController : MonoBehaviour
{
    [Header("UI 綁定 (請拖入指定的 GameObject / Button)")]
    [Tooltip("請拖入 OPT Btn (Button 元件)")]
    [SerializeField] private Button optBtn;

    [Tooltip("請拖入 OPT_Menu (空的父 GameObject)")]
    [SerializeField] private GameObject optMenu;

    [Tooltip("請拖入 OPT_Menu 裡面的 continue (Button 元件)")]
    [SerializeField] private Button continueBtn;

    // NEW: Button SFX - 新增音效欄位供 Inspector 指定
    [Header("按鈕音效 (Button SFX)")]
    [Tooltip("OPT Btn 的點擊音效")]
    [SerializeField] private AudioClip optBtnSound;
    [Tooltip("Continue Btn 的點擊音效")]
    [SerializeField] private AudioClip continueBtnSound;
    // ----------------------------------------

    [Header("動畫時間與交疊設定")]
    [Tooltip("單一元件完整淡入或淡出的時間 (秒)。目前固定測試基準為 0.42 秒。")]
    [SerializeField] private float animDuration = 0.42f;

    [Tooltip("按鈕淡出到什麼比例時，選單開始淡入 (預設 0.4 = 按鈕淡出到 40% 進度時 Menu 出現)")]
    [SerializeField, Range(0f, 1f)] private float menuFadeInStartNormalized = 0.4f;

    [Header("動畫曲線 (多段精準控制：梯形速度平台，無 Overshoot)")]

    // Keyframe(time, value, inTangent, outTangent)
    // 嚴格鎖定 0.25 ~ 0.70 的 Tangent 為 1.244 (或 -1.244)，創造完美的等速高速平台

    [Tooltip("OPT Btn 淡出：0.25~0.70 為穩定高速平台")]
    [SerializeField]
    private AnimationCurve optBtnFadeOutCurve = new AnimationCurve(
        new Keyframe(0.00f, 1.00f, 0.000f, -0.500f),
        new Keyframe(0.15f, 0.90f, -0.900f, -0.900f),
        new Keyframe(0.25f, 0.78f, -1.244f, -1.244f), // 平台開始
        new Keyframe(0.70f, 0.22f, -1.244f, -1.244f), // 平台結束
        new Keyframe(0.85f, 0.08f, -0.900f, -0.900f),
        new Keyframe(1.00f, 0.00f, 0.000f, 0.000f)
    );

    [Tooltip("OPT Menu 淡入：0.25~0.70 為穩定高速平台")]
    [SerializeField]
    private AnimationCurve optMenuFadeInCurve = new AnimationCurve(
        new Keyframe(0.00f, 0.00f, 0.000f, 0.500f),
        new Keyframe(0.15f, 0.10f, 0.900f, 0.900f),
        new Keyframe(0.25f, 0.22f, 1.244f, 1.244f), // 平台開始
        new Keyframe(0.70f, 0.78f, 1.244f, 1.244f), // 平台結束
        new Keyframe(0.85f, 0.92f, 0.900f, 0.900f),
        new Keyframe(1.00f, 1.00f, 0.000f, 0.000f)
    );

    [Tooltip("OPT Menu 淡出：0.25~0.70 為穩定高速平台")]
    [SerializeField]
    private AnimationCurve optMenuFadeOutCurve = new AnimationCurve(
        new Keyframe(0.00f, 1.00f, 0.000f, -0.500f),
        new Keyframe(0.15f, 0.90f, -0.900f, -0.900f),
        new Keyframe(0.25f, 0.78f, -1.244f, -1.244f),
        new Keyframe(0.70f, 0.22f, -1.244f, -1.244f),
        new Keyframe(0.85f, 0.08f, -0.900f, -0.900f),
        new Keyframe(1.00f, 0.00f, 0.000f, 0.000f)
    );

    [Tooltip("OPT Btn 淡入：0.25~0.70 為穩定高速平台")]
    [SerializeField]
    private AnimationCurve optBtnFadeInCurve = new AnimationCurve(
        new Keyframe(0.00f, 0.00f, 0.000f, 0.500f),
        new Keyframe(0.15f, 0.10f, 0.900f, 0.900f),
        new Keyframe(0.25f, 0.22f, 1.244f, 1.244f),
        new Keyframe(0.70f, 0.78f, 1.244f, 1.244f),
        new Keyframe(0.85f, 0.92f, 0.900f, 0.900f),
        new Keyframe(1.00f, 1.00f, 0.000f, 0.000f)
    );

    // 內部使用的 CanvasGroup
    private CanvasGroup optBtnCanvasGroup;
    private CanvasGroup optMenuCanvasGroup;

    // 動畫鎖
    private bool isAnimating = false;

    private void Awake()
    {
        if (!CheckReferences()) return;

        optBtnCanvasGroup = GetOrAddCanvasGroup(optBtn.gameObject);
        optMenuCanvasGroup = GetOrAddCanvasGroup(optMenu);
    }

    private void Start()
    {
        if (optBtnCanvasGroup == null || optMenuCanvasGroup == null) return;

        SetCanvasGroupState(optBtnCanvasGroup, alpha: 1f, interactable: true);
        SetCanvasGroupState(optMenuCanvasGroup, alpha: 0f, interactable: false);

        optBtn.onClick.AddListener(OnOptBtnClicked);
        continueBtn.onClick.AddListener(OnContinueBtnClicked);
    }

    private void OnDestroy()
    {
        if (optBtn != null) optBtn.onClick.RemoveListener(OnOptBtnClicked);
        if (continueBtn != null) continueBtn.onClick.RemoveListener(OnContinueBtnClicked);
    }

    private bool CheckReferences()
    {
        bool isAllAssigned = true;
        if (optBtn == null)
        {
            Debug.LogError("[MenuAnimationController] 錯誤：'OPT Btn' 沒有指定！");
            isAllAssigned = false;
        }
        if (optMenu == null)
        {
            Debug.LogError("[MenuAnimationController] 錯誤：'OPT_Menu' 沒有指定！");
            isAllAssigned = false;
        }
        if (continueBtn == null)
        {
            Debug.LogError("[MenuAnimationController] 錯誤：'Continue Btn' 沒有指定！");
            isAllAssigned = false;
        }
        return isAllAssigned;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        return cg;
    }

    private void SetCanvasGroupState(CanvasGroup cg, float alpha, bool interactable)
    {
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }

    private void OnOptBtnClicked()
    {
        if (isAnimating) return;

        // NEW: Button SFX - 播放 OPT Btn 音效
        if (AudioManager.Instance != null && optBtnSound != null)
        {
            AudioManager.Instance.PlaySFX(optBtnSound);
        }

        StartCoroutine(OpenMenuRoutine());
    }

    private void OnContinueBtnClicked()
    {
        if (isAnimating) return;

        // NEW: Button SFX - 播放 Continue Btn 音效
        if (AudioManager.Instance != null && continueBtnSound != null)
        {
            AudioManager.Instance.PlaySFX(continueBtnSound);
        }

        StartCoroutine(CloseMenuRoutine());
    }

    private IEnumerator OpenMenuRoutine()
    {
        isAnimating = true;

        SetCanvasGroupState(optBtnCanvasGroup, optBtnCanvasGroup.alpha, false);

        float timer = 0f;
        float menuStartTime = animDuration * menuFadeInStartNormalized;
        float totalDuration = menuStartTime + animDuration;

        while (timer < totalDuration)
        {
            timer += Time.deltaTime;

            float btnProgress = Mathf.Clamp01(timer / animDuration);
            optBtnCanvasGroup.alpha = optBtnFadeOutCurve.Evaluate(btnProgress);

            float menuProgress = Mathf.Clamp01((timer - menuStartTime) / animDuration);
            optMenuCanvasGroup.alpha = optMenuFadeInCurve.Evaluate(menuProgress);

            yield return null;
        }

        SetCanvasGroupState(optBtnCanvasGroup, alpha: 0f, interactable: false);
        SetCanvasGroupState(optMenuCanvasGroup, alpha: 1f, interactable: true);

        isAnimating = false;
    }

    private IEnumerator CloseMenuRoutine()
    {
        isAnimating = true;

        SetCanvasGroupState(optMenuCanvasGroup, optMenuCanvasGroup.alpha, false);

        float timer = 0f;
        float totalDuration = animDuration * 2f;

        while (timer < totalDuration)
        {
            timer += Time.deltaTime;

            float menuProgress = Mathf.Clamp01(timer / animDuration);
            optMenuCanvasGroup.alpha = optMenuFadeOutCurve.Evaluate(menuProgress);

            float btnProgress = Mathf.Clamp01((timer - animDuration) / animDuration);
            optBtnCanvasGroup.alpha = optBtnFadeInCurve.Evaluate(btnProgress);

            yield return null;
        }

        SetCanvasGroupState(optMenuCanvasGroup, alpha: 0f, interactable: false);
        SetCanvasGroupState(optBtnCanvasGroup, alpha: 1f, interactable: true);

        isAnimating = false;
    }
}