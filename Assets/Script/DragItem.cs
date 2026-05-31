using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
// ★ 新增 IDropHandler，讓按鈕自己也能接住別人丟過來的東西
public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Canvas parentCanvas;

    private Image buttonImage;
    private Outline buttonOutline;
    private DropSlot currentSlot = null;

    private TextMeshProUGUI textComp;
    private Color originalTextColor;
    private Color originalButtonColor; // ★ 記住黑底的顏色

    [Header("放進格子後的文字顏色")]
    public Color droppedTextColor = new Color(1f, 0.9f, 0.2f);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
        buttonImage = GetComponent<Image>();
        buttonOutline = GetComponent<Outline>();

        if (buttonImage != null)
        {
            originalButtonColor = buttonImage.color;
        }

        textComp = GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            originalTextColor = textComp.color;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(parentCanvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        if (buttonImage != null) buttonImage.color = originalButtonColor;
        if (buttonOutline != null) buttonOutline.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (transform.parent == parentCanvas.transform)
        {
            transform.SetParent(originalParent, false);
        }
    }

    public void OnDroppedInSlot(DropSlot slot)
    {
        currentSlot = slot;
        transform.SetParent(slot.transform, false);

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // ★ 隱身術改良：把透明度設為 0，讓滑鼠依然點得到它
        if (buttonImage != null)
        {
            buttonImage.color = new Color(originalButtonColor.r, originalButtonColor.g, originalButtonColor.b, 0f);
        }
        if (buttonOutline != null) buttonOutline.enabled = false;

        if (textComp != null)
        {
            textComp.color = droppedTextColor;
        }
    }

    public void ReturnToWordBank()
    {
        if (currentSlot != null)
        {
            transform.SetParent(originalParent, false);

            // ★ 恢復不透明的黑底
            if (buttonImage != null) buttonImage.color = originalButtonColor;
            if (buttonOutline != null) buttonOutline.enabled = true;

            if (textComp != null)
            {
                textComp.color = originalTextColor;
            }

            currentSlot.ResetSlot();
            currentSlot = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ReturnToWordBank();
    }

    // ★★★ 終極防線：如果新答案直接砸在我（舊答案）身上
    public void OnDrop(PointerEventData eventData)
    {
        // 轉交給我的格子處理置換！
        if (currentSlot != null)
        {
            currentSlot.OnDrop(eventData);
        }
    }
}