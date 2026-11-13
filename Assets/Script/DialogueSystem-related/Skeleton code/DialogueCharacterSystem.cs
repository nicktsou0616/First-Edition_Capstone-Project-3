using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// ★ 修正 1：明確增加 expressID，並維持原本架構
[System.Serializable]
public class CharacterExpression
{
    public string expressID;
    public Sprite leftSprite;
    public Sprite rightSprite;
}

[System.Serializable]
public class CharacterData
{
    public string id;
    public List<CharacterExpression> expressions = new List<CharacterExpression>();
}

public class DialogueCharacterSystem : MonoBehaviour
{
    [Header("👥 角色資料庫")]
    public List<CharacterData> characters = new List<CharacterData>();

    [Header("🖥️ 角色 UI Image")]
    public Image leftImage;
    public Image rightImage;

    [Header("🎨 顏色設定")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("✨ 動畫參數")]
    public float slideOffset = 300f;
    public float moveDuration = 0.25f;
    public float colorFadeDuration = 0.2f;

    private RectTransform leftRect, rightRect;
    private Vector2 leftOrigin, rightOrigin;

    private Dictionary<string, CharacterData> characterDictionary = new Dictionary<string, CharacterData>();

    private string currentLeftID = "";
    private string currentRightID = "";
    private string currentLeftExpress = "";
    private string currentRightExpress = "";

    public void InitializeSystem()
    {
        leftRect = leftImage.GetComponent<RectTransform>();
        rightRect = rightImage.GetComponent<RectTransform>();
        leftOrigin = leftRect.anchoredPosition;
        rightOrigin = rightRect.anchoredPosition;

        characterDictionary.Clear();
        foreach (var data in characters)
        {
            if (data != null && !string.IsNullOrEmpty(data.id) && !characterDictionary.ContainsKey(data.id))
            {
                characterDictionary.Add(data.id, data);
            }
        }

        SetAlpha(leftImage, 0f);
        SetAlpha(rightImage, 0f);
        leftRect.anchoredPosition = leftOrigin - new Vector2(slideOffset, 0);
        rightRect.anchoredPosition = rightOrigin + new Vector2(slideOffset, 0);
    }

    public IEnumerator UpdateCharactersRoutine(string newLeftID, string newRightID, string leftExpress, string rightExpress, string speaker, bool isNarrator)
    {
        StopAllCoroutines();

        // ★ 修正 2：明確判斷 Express 是否改變，只有改變才做事
        bool leftIDChanged = (newLeftID != currentLeftID);
        bool rightIDChanged = (newRightID != currentRightID);
        bool leftExpressChanged = (leftExpress != currentLeftExpress);
        bool rightExpressChanged = (rightExpress != currentRightExpress);

        // 1. 滑出舊角色 (僅當 ID 改變時)
        Coroutine outL = null, outR = null;
        if (leftIDChanged && !string.IsNullOrEmpty(currentLeftID))
            outL = StartCoroutine(MoveRoutine(leftRect, leftRect.anchoredPosition, leftOrigin - new Vector2(slideOffset, 0), leftImage, false));
        if (rightIDChanged && !string.IsNullOrEmpty(currentRightID))
            outR = StartCoroutine(MoveRoutine(rightRect, rightRect.anchoredPosition, rightOrigin + new Vector2(slideOffset, 0), rightImage, false));

        if (outL != null) yield return outL;
        if (outR != null) yield return outR;

        // 2. 更換圖片 (ID 改變，或 ID 相同但 Express 改變時才更換)
        if (leftIDChanged || leftExpressChanged)
            ChangeSprite(leftImage, newLeftID, leftExpress, true, ref currentLeftID, ref currentLeftExpress);

        if (rightIDChanged || rightExpressChanged)
            ChangeSprite(rightImage, newRightID, rightExpress, false, ref currentRightID, ref currentRightExpress);

        // 3. 滑入新角色 (僅當 ID 改變時)
        Coroutine inL = null, inR = null;
        if (leftIDChanged && !string.IsNullOrEmpty(currentLeftID))
            inL = StartCoroutine(MoveRoutine(leftRect, leftOrigin - new Vector2(slideOffset, 0), leftOrigin, leftImage, true));
        if (rightIDChanged && !string.IsNullOrEmpty(currentRightID))
            inR = StartCoroutine(MoveRoutine(rightRect, rightOrigin + new Vector2(slideOffset, 0), rightOrigin, rightImage, true));

        if (inL != null) yield return inL;
        if (inR != null) yield return inR;

        // 4. 漸變顏色明暗
        yield return FadeColorsRoutine(speaker, isNarrator);
    }

    private IEnumerator MoveRoutine(RectTransform rect, Vector2 start, Vector2 end, Image img, bool isFadingIn)
    {
        float t = 0;
        float startAlpha = img.color.a;
        float targetAlpha = isFadingIn ? 1f : 0f;
        Color c = img.color;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            rect.anchoredPosition = Vector2.Lerp(start, end, smoothT);

            c.a = Mathf.Lerp(startAlpha, targetAlpha, smoothT);
            img.color = c;
            yield return null;
        }
        rect.anchoredPosition = end;
        c.a = targetAlpha;
        img.color = c;
    }

    private IEnumerator FadeColorsRoutine(string speaker, bool isNarrator)
    {
        Color targetLeft = (speaker == currentLeftID && !isNarrator) ? activeColor : inactiveColor;
        Color targetRight = (speaker == currentRightID && !isNarrator) ? activeColor : inactiveColor;

        Color startLeft = leftImage.color;
        Color startRight = rightImage.color;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / colorFadeDuration;

            leftImage.color = new Color(Mathf.Lerp(startLeft.r, targetLeft.r, t), Mathf.Lerp(startLeft.g, targetLeft.g, t), Mathf.Lerp(startLeft.b, targetLeft.b, t), leftImage.color.a);
            rightImage.color = new Color(Mathf.Lerp(startRight.r, targetRight.r, t), Mathf.Lerp(startRight.g, targetRight.g, t), Mathf.Lerp(startRight.b, targetRight.b, t), rightImage.color.a);
            yield return null;
        }
    }

    private void ChangeSprite(Image img, string id, string expressStr, bool isLeft, ref string cacheID, ref string cacheExpress)
    {
        cacheID = id;
        cacheExpress = expressStr;

        // ★ 防呆 1 & 2：ID 不存在或為空
        if (string.IsNullOrEmpty(id) || !characterDictionary.ContainsKey(id))
        {
            if (!string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[DialogueCharacterSystem] 找不到角色 ID: {id}。");
            }
            if (img != null) { img.sprite = null; SetAlpha(img, 0f); }
            return;
        }

        CharacterData charData = characterDictionary[id];

        // ★ 防呆 3 & 4：Expressions 為 null 或 Count 為 0
        if (charData.expressions == null || charData.expressions.Count == 0)
        {
            Debug.LogWarning($"[DialogueCharacterSystem] 角色 {id} 沒有設定任何 Expression 資料。");
            if (img != null) img.sprite = null;
            return;
        }

        int expressIndex = -1;

        // ★ 修正 1 & 防呆 5 & 6：字串比對 fallback 至數字索引
        if (!string.IsNullOrEmpty(expressStr))
        {
            // 優先嘗試比對字串 ID
            for (int i = 0; i < charData.expressions.Count; i++)
            {
                if (charData.expressions[i].expressID == expressStr)
                {
                    expressIndex = i;
                    break;
                }
            }

            // 找不到字串 ID 時，嘗試當作數字索引 (舊 CSV 相容)
            if (expressIndex == -1)
            {
                if (!int.TryParse(expressStr, out expressIndex))
                {
                    Debug.LogWarning($"[DialogueCharacterSystem] 角色 {id} 的 Express '{expressStr}' 找不到對應的 expressID，也無法解析為數字。將使用預設表情 0。");
                    expressIndex = 0;
                }
            }
        }
        else
        {
            expressIndex = 0; // 空字串自動使用 0
        }

        // ★ 防呆 7 & 8：索引範圍異常
        if (expressIndex < 0 || expressIndex >= charData.expressions.Count)
        {
            Debug.LogWarning($"[DialogueCharacterSystem] 角色 {id} 的 Express 索引 {expressIndex} 超出範圍 (總數: {charData.expressions.Count})，將自動使用預設表情 0。");
            expressIndex = 0;
        }

        CharacterExpression expr = charData.expressions[expressIndex];
        Sprite targetSprite = isLeft ? expr.leftSprite : expr.rightSprite;

        // ★ 修正 3 & 防呆 9 & 10：Sprite 為空的 fallback
        if (targetSprite == null)
        {
            Debug.LogWarning($"[DialogueCharacterSystem] 角色 {id} (Express: {(string.IsNullOrEmpty(expressStr) ? "0" : expressStr)}) 的 {(isLeft ? "Left" : "Right")} Sprite 為空。");

            Sprite fallbackSprite = isLeft ? expr.rightSprite : expr.leftSprite;
            if (fallbackSprite != null)
            {
                Debug.LogWarning($"[DialogueCharacterSystem] -> 已安全 fallback：自動替換為角色 {id} 另一側的 Sprite。");
                targetSprite = fallbackSprite;
            }
        }

        if (img != null) img.sprite = targetSprite;
    }

    public IEnumerator SlideOutAllRoutine()
    {
        StopAllCoroutines();

        Coroutine outL = null, outR = null;
        if (!string.IsNullOrEmpty(currentLeftID))
            outL = StartCoroutine(MoveRoutine(leftRect, leftRect.anchoredPosition, leftOrigin - new Vector2(slideOffset, 0), leftImage, false));
        if (!string.IsNullOrEmpty(currentRightID))
            outR = StartCoroutine(MoveRoutine(rightRect, rightRect.anchoredPosition, rightOrigin + new Vector2(slideOffset, 0), rightImage, false));

        if (outL != null) yield return outL;
        if (outR != null) yield return outR;

        currentLeftID = "";
        currentRightID = "";
        currentLeftExpress = "";
        currentRightExpress = "";
    }

    public void SnapToState(string leftID, string rightID, string leftExpress, string rightExpress, string speaker, bool isNarrator)
    {
        StopAllCoroutines();

        ChangeSprite(leftImage, leftID, leftExpress, true, ref currentLeftID, ref currentLeftExpress);
        ChangeSprite(rightImage, rightID, rightExpress, false, ref currentRightID, ref currentRightExpress);

        leftRect.anchoredPosition = string.IsNullOrEmpty(leftID) ? (leftOrigin - new Vector2(slideOffset, 0)) : leftOrigin;
        rightRect.anchoredPosition = string.IsNullOrEmpty(rightID) ? (rightOrigin + new Vector2(slideOffset, 0)) : rightOrigin;

        SetAlpha(leftImage, string.IsNullOrEmpty(leftID) ? 0f : 1f);
        SetAlpha(rightImage, string.IsNullOrEmpty(rightID) ? 0f : 1f);

        Color targetLeft = (speaker == currentLeftID && !isNarrator) ? activeColor : inactiveColor;
        Color targetRight = (speaker == currentRightID && !isNarrator) ? activeColor : inactiveColor;

        leftImage.color = new Color(targetLeft.r, targetLeft.g, targetLeft.b, leftImage.color.a);
        rightImage.color = new Color(targetRight.r, targetRight.g, targetRight.b, rightImage.color.a);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}