using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class ArticleEntry
{
    public string entryTitle;
    [TextArea(10, 30)]
    public string content;
}

[System.Serializable]
public class BookData
{
    public GameObject bookButton;
    public List<ArticleEntry> articles = new List<ArticleEntry>();
}

public class NotebookManager : MonoBehaviour
{
    [Header("📚 圖鑑資料庫")]
    public List<BookData> database = new List<BookData>();

    [Header("✨ 動畫控制")]
    public GameObject bookTemplateObject;
    public float animDuration = 0.5f;
    public AnimationCurve swingInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve settleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public Vector2 startOffset = new Vector2(-800f, -500f);
    public float startRotationZ = 45f;
    public Vector2 overshootOffset = new Vector2(30f, 30f);
    public float overshootRotationZ = -5f;
    private Vector2 targetPos;

    [Header("🖥️ UI 綁定")]
    public TextMeshProUGUI bookTitleText;
    public GameObject tabButtonPrefab;
    public Transform tabContentContainer;
    public TextMeshProUGUI storyText;
    public ScrollRect storyScrollView;

    // ★ 新增：標籤選取狀態的視覺設定
    [Header("🎨 標籤選取視覺設定")]
    [Tooltip("沒被選到時的顏色 (建議暗一點的灰色)")]
    public Color normalTabColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [Tooltip("被選到時的顏色 (建議純白或亮色)")]
    public Color activeTabColor = Color.white;

    private BookData currentBook;
    private RectTransform bookRect;
    private Coroutine animCoroutine;

    // ★ 用來記住目前畫面上所有生成的「按鈕文字」，方便我們去幫它變色
    private List<TextMeshProUGUI> activeTabTexts = new List<TextMeshProUGUI>();

    void Start()
    {
        if (bookTemplateObject != null)
        {
            bookRect = bookTemplateObject.GetComponent<RectTransform>();
            targetPos = bookRect.anchoredPosition;
            bookTemplateObject.SetActive(false);
        }
    }

    public void OpenBook(int bookIndex)
    {
        if (bookIndex < 0 || bookIndex >= database.Count) return;
        currentBook = database[bookIndex];

        if (currentBook.bookButton != null)
        {
            TextMeshProUGUI btnText = currentBook.bookButton.GetComponentInChildren<TextMeshProUGUI>();
            bookTitleText.text = btnText != null ? btnText.text : "找不到文字";
        }
        else bookTitleText.text = "未綁定按鈕";

        // 清空舊的列表與按鈕
        activeTabTexts.Clear();
        foreach (Transform child in tabContentContainer) Destroy(child.gameObject);

        // 生成新按鈕
        for (int i = 0; i < currentBook.articles.Count; i++)
        {
            int index = i;
            GameObject newTab = Instantiate(tabButtonPrefab, tabContentContainer);

            // 抓出這顆新按鈕身上的文字組件，並存進我們的名單裡
            TextMeshProUGUI tabText = newTab.GetComponentInChildren<TextMeshProUGUI>();
            tabText.text = currentBook.articles[i].entryTitle;
            activeTabTexts.Add(tabText);

            newTab.GetComponent<Button>().onClick.AddListener(() => OpenArticle(index));
        }

        if (currentBook.articles.Count > 0) OpenArticle(0);
        else storyText.text = "尚無內容...";

        if (bookTemplateObject != null)
        {
            bookTemplateObject.SetActive(true);
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(PlayBookSwingAnimation());
        }
    }

    public void OpenArticle(int articleIndex)
    {
        // 1. 更新右邊內文
        storyText.text = currentBook.articles[articleIndex].content;
        if (storyScrollView != null) storyScrollView.verticalNormalizedPosition = 1f;

        // 2. ★ 更新左邊按鈕的視覺狀態 (變色魔法)
        for (int i = 0; i < activeTabTexts.Count; i++)
        {
            if (activeTabTexts[i] == null) continue;

            if (i == articleIndex)
            {
                // 被選中的那個：變成亮色，並且變粗體！
                activeTabTexts[i].color = activeTabColor;
                activeTabTexts[i].fontStyle = FontStyles.Bold;
            }
            else
            {
                // 沒被選中的：變成暗色，恢復一般粗細
                activeTabTexts[i].color = normalTabColor;
                activeTabTexts[i].fontStyle = FontStyles.Normal;
            }
        }
    }

    public void CloseBook()
    {
        if (bookTemplateObject != null) bookTemplateObject.SetActive(false);
    }

    private IEnumerator PlayBookSwingAnimation()
    {
        if (bookRect == null) yield break;
        Vector2 startPos = targetPos + startOffset;
        Vector2 overshootPos = targetPos + overshootOffset;
        bookRect.localScale = Vector3.one;

        float elapsed = 0f;
        float midTime = animDuration * 0.6f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            if (elapsed < midTime)
            {
                float t = elapsed / midTime;
                float curveT = swingInCurve.Evaluate(t);
                bookRect.anchoredPosition = Vector2.Lerp(startPos, overshootPos, curveT);
                bookRect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startRotationZ, overshootRotationZ, curveT));
            }
            else
            {
                float t = (elapsed - midTime) / (animDuration - midTime);
                float curveT = settleCurve.Evaluate(t);
                bookRect.anchoredPosition = Vector2.Lerp(overshootPos, targetPos, curveT);
                bookRect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(overshootRotationZ, 0f, curveT));
            }
            yield return null;
        }

        bookRect.anchoredPosition = targetPos;
        bookRect.localEulerAngles = Vector3.zero;
    }
}