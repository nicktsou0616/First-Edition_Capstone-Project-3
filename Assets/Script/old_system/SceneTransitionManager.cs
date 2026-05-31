using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [Header("轉場 UI 設定")]
    public Image fadeOverlay;

    public bool isTransitioning { get; private set; }
    public bool roomReady { get; private set; } = true; // ★新增：房間解鎖狀態

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (fadeOverlay != null)
            {
                Color color = fadeOverlay.color;
                color.a = 0f;
                fadeOverlay.color = color;
                fadeOverlay.gameObject.SetActive(false);
                fadeOverlay.raycastTarget = false;
            }
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public IEnumerator ExecuteRoomTransition(
        GameObject oldRoom,
        GameObject newRoom,
        Transform targetPos,
        SceneTransitionType type,
        float duration)
    {
        isTransitioning = true;
        roomReady = false; // ★新增：開始轉場立即上鎖

        Color fadeColor = (type == SceneTransitionType.BlackScreen) ? Color.black : Color.white;
        float halfDuration = duration / 2f;

        // 1. 畫面漸暗
        if (type != SceneTransitionType.Cut)
            yield return StartCoroutine(FadeRoutine(0f, 1f, fadeColor, halfDuration));

        // 2. 切換房間 + 傳送玩家
        if (oldRoom != null) oldRoom.SetActive(false);
        if (newRoom != null) newRoom.SetActive(true);

        if (targetPos != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = targetPos.position;
        }

        // 3. 畫面漸亮
        if (type != SceneTransitionType.Cut)
            yield return StartCoroutine(FadeRoutine(1f, 0f, fadeColor, halfDuration));

        isTransitioning = false;

        // ★新增：延遲2幀後才真正解鎖 roomReady
        yield return null;
        yield return null;
        roomReady = true;
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, Color color, float duration)
    {
        if (fadeOverlay == null) yield break;

        Canvas parentCanvas = fadeOverlay.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.gameObject.SetActive(true);
            parentCanvas.enabled = true;
            parentCanvas.sortingOrder = 32767;
        }

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.raycastTarget = true;

        float elapsed = 0f;
        color.a = startAlpha;
        fadeOverlay.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeOverlay.color = color;

        if (endAlpha <= 0f)
        {
            fadeOverlay.gameObject.SetActive(false);
            fadeOverlay.raycastTarget = false;
        }
    }
}
