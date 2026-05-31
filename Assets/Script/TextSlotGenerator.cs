using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextSlotGenerator : MonoBehaviour
{
    [Header("1. 基本設定")]
    public TMP_Text questionText;
    public GameObject slotPrefab;
    public Transform slotContainer;

    [Header("2. 黃金比例設定 (數值已鎖定)")]
    public bool autoHeight = false;

    public float manualHeight = 50f;
    public float widthPadding = -20f;
    public float yOffset = 22.5f;

    public Dictionary<string, DropSlot> generatedSlots = new Dictionary<string, DropSlot>();

    void OnValidate()
    {
        manualHeight = 50f;
        // ★ 維持你原本的 -20f，讓初始空框框不會擋到字
        widthPadding = -20f;
        yOffset = 22.5f;
    }

    void Start()
    {
        Invoke("GenerateSlots", 0.1f);
    }

    [ContextMenu("手動測試生成")]
    public void GenerateSlots()
    {
        if (questionText == null || slotPrefab == null || slotContainer == null) return;

        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        generatedSlots.Clear();

        questionText.ForceMeshUpdate();

        float finalHeight = manualHeight;
        if (autoHeight)
        {
            TMP_FontAsset fontAsset = questionText.font;
            float fontSize = questionText.fontSize;
            float ascender = fontAsset.faceInfo.ascentLine * (fontSize / fontAsset.faceInfo.pointSize);
            float descender = fontAsset.faceInfo.descentLine * (fontSize / fontAsset.faceInfo.pointSize);
            finalHeight = ascender - descender;
        }

        foreach (var linkInfo in questionText.textInfo.linkInfo)
        {
            string linkID = linkInfo.GetLinkID();

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float sumY = 0;
            int count = 0;

            for (int i = 0; i < linkInfo.linkTextLength; i++)
            {
                int charIndex = linkInfo.linkTextfirstCharacterIndex + i;
                TMP_CharacterInfo charInfo = questionText.textInfo.characterInfo[charIndex];

                if (!charInfo.isVisible) continue;

                Vector3 bl = questionText.transform.TransformPoint(charInfo.bottomLeft);
                Vector3 tr = questionText.transform.TransformPoint(charInfo.topRight);

                if (bl.x < minX) minX = bl.x;
                if (tr.x > maxX) maxX = tr.x;

                sumY += (bl.y + tr.y) / 2f;
                count++;
            }

            if (count == 0) continue;

            float avgY = sumY / count;
            Vector3 centerPos = new Vector3((minX + maxX) / 2f, avgY + yOffset, 0);

            float totalWidth = (maxX - minX) + widthPadding;
            if (totalWidth < 10) totalWidth = 50;

            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.name = linkID;

            RectTransform slotRect = newSlot.GetComponent<RectTransform>();
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            newSlot.transform.position = centerPos;
            slotRect.sizeDelta = new Vector2(totalWidth, finalHeight);

            Image img = newSlot.GetComponent<Image>();
            if (img != null) img.color = Color.white;

            DropSlot dropScript = newSlot.GetComponent<DropSlot>();
            if (dropScript != null)
            {
                dropScript.slotID = linkID;
                generatedSlots.Add(linkID, dropScript);
            }
        }
    }

    // ★ 多加了一個 currentPadding 變數，讓大腦可以決定這次要多寬！
    public void UpdateSlotPosition(string linkID, GameObject slotObj, float currentPadding)
    {
        if (questionText == null || slotObj == null) return;

        questionText.ForceMeshUpdate();

        int linkIndex = -1;
        for (int i = 0; i < questionText.textInfo.linkInfo.Length; i++)
        {
            if (questionText.textInfo.linkInfo[i].GetLinkID() == linkID)
            {
                linkIndex = i;
                break;
            }
        }

        if (linkIndex == -1) return;

        TMP_LinkInfo linkInfo = questionText.textInfo.linkInfo[linkIndex];

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float sumY = 0;
        int count = 0;

        for (int i = 0; i < linkInfo.linkTextLength; i++)
        {
            int charIndex = linkInfo.linkTextfirstCharacterIndex + i;
            TMP_CharacterInfo charInfo = questionText.textInfo.characterInfo[charIndex];

            if (!charInfo.isVisible) continue;

            Vector3 bl = questionText.transform.TransformPoint(charInfo.bottomLeft);
            Vector3 tr = questionText.transform.TransformPoint(charInfo.topRight);

            if (bl.x < minX) minX = bl.x;
            if (tr.x > maxX) maxX = tr.x;

            sumY += (bl.y + tr.y) / 2f;
            count++;
        }

        if (count == 0) return;

        float avgY = sumY / count;
        Vector3 centerPos = new Vector3((minX + maxX) / 2f, avgY + yOffset, 0);

        slotObj.transform.position = centerPos;

        // ★★★ 使用傳進來的寬裕空間
        float totalWidth = (maxX - minX) + currentPadding;
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            slotRect.sizeDelta = new Vector2(totalWidth, slotRect.sizeDelta.y);
        }
    }
}