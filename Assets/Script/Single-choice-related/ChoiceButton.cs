using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class ChoiceButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public int choiceIndex;

    [Header("UI 參照")]
    public TMP_Text buttonText;

    [Header("動畫設定")]
    [Range(0.8f, 1f)] public float pressedScale = 0.92f;
    [Range(0f, 1f)] public float pressedAlpha = 0.25f;
    public float pressDuration = 0.1f;
    public float bgFadeOutDuration = 0.2f;

    // 事件回傳：(選擇的按鈕本身, 選擇的 Index)
    public Action<ChoiceButton> OnChoiceMade;
    public Action<int> OnChoiceAnimDone;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isInteractable = true;
    private bool isPressed = false;
    private Coroutine currentAnim;

    // ==========================================
    // ★ 新增：安全防呆機制 (確保元件隨時都有被抓到)
    // ==========================================
    private void EnsureInitialized()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        EnsureInitialized();
    }
    // ==========================================

    public void SetInteractable(bool state)
    {
        isInteractable = state;
    }

    public void ResetState()
    {
        EnsureInitialized(); // ★ 重置前，確保自己已經準備好了

        if (currentAnim != null) StopCoroutine(currentAnim);
        isInteractable = true;
        isPressed = false;

        if (rectTransform != null) rectTransform.localScale = Vector3.one;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (buttonText != null) buttonText.enabled = true; // 恢復文字顯示
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = true;
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(ScaleAndFadeRoutine(pressedScale, pressedAlpha, pressDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 玩家按住後移開滑鼠，取消選取狀態
        if (!isInteractable || !isPressed) return;

        isPressed = false;
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(ScaleAndFadeRoutine(1f, 1f, pressDuration)); // 恢復原狀
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable || !isPressed) return;

        // 確認選擇成立
        isPressed = false;
        if (currentAnim != null) StopCoroutine(currentAnim);

        StartCoroutine(ConfirmSelectionRoutine());
    }

    private IEnumerator ScaleAndFadeRoutine(float targetScale, float targetAlpha, float duration)
    {
        EnsureInitialized(); // 安全起見
        Vector3 startScale = rectTransform.localScale;
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        rectTransform.localScale = Vector3.one * targetScale;
        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator ConfirmSelectionRoutine()
    {
        EnsureInitialized(); // 安全起見
        // 立即通知 Module 鎖定所有按鈕，避免連點
        OnChoiceMade?.Invoke(this);

        // 動畫階段一：文字瞬間消失
        if (buttonText != null) buttonText.enabled = false;

        // 動畫階段二：背景加速淡出 (Ease In)
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < bgFadeOutDuration)
        {
            time += Time.deltaTime;
            float t = time / bgFadeOutDuration;
            t = t * t; // Ease In (加速曲線)

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 動畫完成，通知 Module 開始退場
        OnChoiceAnimDone?.Invoke(choiceIndex);
    }
}