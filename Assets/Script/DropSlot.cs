using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public string slotID;
    public GameObject currentItem = null;

    private RectTransform rectTransform;
    private Image slotImage;
    private Color originalSlotColor;

    private TextMeshProUGUI mixedLineText;
    private string originalLinkContent = "";

    private TextSlotGenerator slotGenerator;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        slotImage = GetComponent<Image>();

        if (slotImage != null)
        {
            originalSlotColor = slotImage.color;
        }
    }

    void Start()
    {
        GameObject mixedLineObj = GameObject.Find("MixedLine");
        if (mixedLineObj != null)
        {
            mixedLineText = mixedLineObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        slotGenerator = Object.FindFirstObjectByType<TextSlotGenerator>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragItem draggedItem = eventData.pointerDrag.GetComponent<DragItem>();

            if (draggedItem != null)
            {
                if (currentItem != null && currentItem != draggedItem.gameObject)
                {
                    DragItem oldItem = currentItem.GetComponent<DragItem>();
                    if (oldItem != null) oldItem.ReturnToWordBank();
                }

                TextMeshProUGUI draggedTextComp = draggedItem.GetComponentInChildren<TextMeshProUGUI>();
                if (draggedTextComp != null)
                {
                    currentItem = draggedItem.gameObject;
                    StartCoroutine(HandleDropProcess(draggedItem, draggedTextComp));
                }
            }
        }
    }

    private IEnumerator HandleDropProcess(DragItem draggedItem, TextMeshProUGUI draggedTextComp)
    {
        if (mixedLineText == null) yield break;

        string pattern = $"<link=\"{slotID}\">(.*?)</link>";

        if (string.IsNullOrEmpty(originalLinkContent))
        {
            Match match = Regex.Match(mixedLineText.text, pattern);
            if (match.Success) originalLinkContent = match.Groups[1].Value;
        }

        // 1. 計算按鈕需要的完美寬度
        float neededWidth = draggedTextComp.preferredWidth + 20f;

        // 2. ★ 你的客製化要求實現：精準測量單一個底線有多寬！
        float singleUnderscoreWidth = mixedLineText.GetPreferredValues("_").x;
        if (singleUnderscoreWidth <= 0.1f) singleUnderscoreWidth = 15f; // 防呆機制

        // 3. ★ 拿掉死板的長度限制，直接算出完美貼合的底線數量！
        int underscoreCount = Mathf.CeilToInt(neededWidth / singleUnderscoreWidth);

        // 產生完美長度的隱形底線字串
        string unbreakablePlaceholder = new string('_', underscoreCount);

        string replacement = $"<link=\"{slotID}\"><color=#00000000>{unbreakablePlaceholder}</color></link>";
        mixedLineText.text = Regex.Replace(mixedLineText.text, pattern, replacement);

        mixedLineText.ForceMeshUpdate();
        yield return new WaitForEndOfFrame();

        rectTransform.sizeDelta = new Vector2(neededWidth, rectTransform.sizeDelta.y);

        if (slotGenerator != null)
        {
            slotGenerator.UpdateSlotPosition(slotID, gameObject, 0f);
        }

        if (slotImage != null)
        {
            slotImage.color = new Color(originalSlotColor.r, originalSlotColor.g, originalSlotColor.b, 0f);
        }
        draggedItem.OnDroppedInSlot(this);
    }

    public void ResetSlot()
    {
        if (mixedLineText != null && !string.IsNullOrEmpty(originalLinkContent))
        {
            string pattern = $"<link=\"{slotID}\">(.*?)</link>";
            string replacement = $"<link=\"{slotID}\">{originalLinkContent}</link>";
            mixedLineText.text = Regex.Replace(mixedLineText.text, pattern, replacement);

            mixedLineText.ForceMeshUpdate();
            if (slotGenerator != null)
            {
                slotGenerator.UpdateSlotPosition(slotID, gameObject, slotGenerator.widthPadding);
            }
        }

        if (slotImage != null) slotImage.color = originalSlotColor;
        currentItem = null;
    }
}