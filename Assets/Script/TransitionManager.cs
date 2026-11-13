using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    [Header("轉場元件")]
    [Tooltip("黑色或白色遮罩的 CanvasGroup")]
    public CanvasGroup canvasGroup;

    [Header("轉場設定")]
    [Tooltip("單次淡入或淡出的時間 (秒)")]
    public float fadeDuration = 1f;

    [Tooltip("強烈蓄力 -> 爆發加速 -> 高速維持 -> 強烈減速 -> 緩停")]
    public AnimationCurve fadeCurve = new AnimationCurve(
        // 0%：完全透明
        new Keyframe(0.00f, 0.00f, 0f, 0f),

        // 15%：幾乎不動，蓄力
        new Keyframe(0.15f, 0.01f, 0f, 0.05f),

        // 30%：非常猛烈的爆發加速
        new Keyframe(0.30f, 0.75f, 10f, 1f),

        // 55%：高速區間
        new Keyframe(0.55f, 0.92f, 0.5f, 0.5f),

        // 70%：接近完全遮住，開始強烈煞車
        new Keyframe(0.70f, 0.99f, 1f, 0.05f),

        // 85%：慢慢貼近終點
        new Keyframe(0.85f, 0.995f, 0.02f, 0.01f),

        // 100%：完全遮住
        new Keyframe(1.00f, 1.00f, 0f, 0f)
    );

    // 防止同時觸發多個轉場
    private static bool isTransitioning = false;

    /// <summary>
    /// 使用指定的轉場 Prefab 切換 Scene。
    /// </summary>
    public static void LoadScene(string sceneName, GameObject transitionPrefab)
    {
        // 如果正在轉場，或沒有指定 Prefab，就不執行
        if (isTransitioning || transitionPrefab == null)
            return;

        isTransitioning = true;

        // 動態生成一次性的轉場窗簾
        GameObject transitionInstance = Instantiate(transitionPrefab);

        // 確保換 Scene 時窗簾不會被舊 Scene 一起刪掉
        DontDestroyOnLoad(transitionInstance);

        // 取得 Prefab 上的 TransitionManager
        TransitionManager manager =
            transitionInstance.GetComponent<TransitionManager>();

        // 開始轉場
        manager.StartCoroutine(
            manager.TransitionFlow(sceneName)
        );
    }

    private IEnumerator TransitionFlow(string sceneName)
    {
        // 開始時擋住 UI
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;

        // ==========================================
        // A. 淡入：透明 -> 完全遮住
        // ==========================================

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / fadeDuration
            );

            // 使用強烈速度曲線
            canvasGroup.alpha =
                fadeCurve.Evaluate(progress);

            yield return null;
        }

        // 確保完全不透明
        canvasGroup.alpha = 1f;

        // ==========================================
        // B. 完全黑 / 白時切換 Scene
        // ==========================================

        AsyncOperation asyncLoad =
            SceneManager.LoadSceneAsync(sceneName);

        // 等待 Scene 完全載入
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // ==========================================
        // C. 淡出：完全遮住 -> 透明
        // ==========================================

        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                timer / fadeDuration
            );

            // 反向使用同一條曲線
            canvasGroup.alpha =
                fadeCurve.Evaluate(1f - progress);

            yield return null;
        }

        // 確保完全透明
        canvasGroup.alpha = 0f;

        // ==========================================
        // D. 轉場完成
        // ==========================================

        isTransitioning = false;

        // 收起這次使用的窗簾
        Destroy(gameObject);
    }
}
