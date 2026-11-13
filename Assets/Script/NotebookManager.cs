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
    public Sprite image;
    [Min(1)] public int databasePage = 1;
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
    public GameObject fullMapRoot;
    public GameObject tabButtonPrefab;
    public Transform tabContentContainer;
    public TextMeshProUGUI storyText;
    public ScrollRect storyScrollView;
    public GameObject databaseContentRoot;
    public GameObject inventoryContentRoot;

    [Header("物品欄卡片清單")]
    public ScrollRect inventoryCatalogScrollView;
    public RectTransform inventoryCatalogContent;

    
    public RectTransform leftMenuScrollRect;

    [Header("資料庫分頁")]
    [Min(1)] public int databasePageCount = 3;
    public ScrollRect databaseCatalogScrollView;
    public RectTransform databaseCatalogContent;
    public Button databaseBackButton;

    [Header("物品欄卡片與詳細視窗")]
    [Range(0f, 1f)] public float inventoryBackdropOpacity = 0.25f;
    [Min(0.01f)] public float inventoryZoomDuration = 0.45f;

    [Header("資料庫左側頁碼清單")]
    public Vector2 databasePageButtonSize = new Vector2(430f, 90f);
    [Min(1f)] public float databasePageFontSize = 42f;
    [Min(0f)] public float databasePageSpacing = 8f;
    [Min(0f)] public float databaseListTopExpansion = 120f;

    // ★ 新增：標籤選取狀態的視覺設定
    [Header("🎨 標籤選取視覺設定")]
    [Tooltip("沒被選到時的顏色 (建議暗一點的灰色)")]
    public Color normalTabColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [Tooltip("被選到時的顏色 (建議純白或亮色)")]
    public Color activeTabColor = Color.white;

    private BookData currentBook;
    private RectTransform bookRect;
    private RectTransform rightContentRect;
    private float originalRightContentWidth;
    private Coroutine animCoroutine;
    private Vector2 originalLeftMenuPosition;
    private Vector2 originalLeftMenuSize;
    private bool isInventoryOpen;
    private bool isDatabaseOpen;
    private int currentDatabasePage = 1;
    private GameObject databaseDetailOverlay;
    private TextMeshProUGUI databaseDetailTitle;
    private Image databaseDetailImage;
    private TextMeshProUGUI databaseDetailMissingImage;
    private TextMeshProUGUI databaseDetailDescription;
    private ScrollRect databaseDetailScrollView;
    private GameObject inventoryDetailOverlay;
    private RectTransform inventoryDetailPanel;
    private CanvasGroup inventoryDetailCanvasGroup;
    private TextMeshProUGUI inventoryDetailTitle;
    private Image inventoryDetailImage;
    private TextMeshProUGUI inventoryDetailMissingImage;
    private TextMeshProUGUI inventoryDetailDescription;
    private ScrollRect inventoryDetailScrollView;
    private Coroutine inventoryZoomCoroutine;

    private VerticalLayoutGroup leftMenuLayoutGroup;
    private readonly List<TextMeshProUGUI> databasePageTexts =
        new List<TextMeshProUGUI>();

    void Start()
    {
        if (fullMapRoot != null) fullMapRoot.SetActive(false);
        if (databaseBackButton != null) databaseBackButton.onClick.AddListener(ShowDatabaseCatalog);

        if (leftMenuScrollRect != null)
        {
            originalLeftMenuPosition = leftMenuScrollRect.anchoredPosition;
            originalLeftMenuSize = leftMenuScrollRect.sizeDelta;
        }
        if (tabContentContainer != null)
            leftMenuLayoutGroup =
                tabContentContainer.GetComponent<VerticalLayoutGroup>();
        if (bookTemplateObject != null)
        {
            bookRect = bookTemplateObject.GetComponent<RectTransform>();
            targetPos = bookRect.anchoredPosition;
            if (inventoryContentRoot != null)
            {
                rightContentRect = inventoryContentRoot.transform.parent as RectTransform;
                if (rightContentRect != null)
                    originalRightContentWidth = rightContentRect.rect.width;
            }
            CreateDatabaseDetailOverlay();
            CreateInventoryCatalog();
            CreateInventoryDetailOverlay();
            bookTemplateObject.SetActive(false);
        }
    }

    public void OpenBook(int bookIndex)
    {
        if (bookIndex < 0 || bookIndex >= database.Count) return;
        CloseDatabaseDetail();
        CloseInventoryDetail();
        if (fullMapRoot != null) fullMapRoot.SetActive(false);
        currentBook = database[bookIndex];

        bool isInventory = bookIndex == 0;
        isInventoryOpen = isInventory;
        isDatabaseOpen = bookIndex == 1;
        if (isDatabaseOpen) currentDatabasePage = 1;
        if (rightContentRect != null)
            rightContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                isInventory ? bookRect.rect.width : originalRightContentWidth);
        inventoryContentRoot.SetActive(isInventory);
        databaseContentRoot.SetActive(!isInventory);
        bookTitleText.gameObject.SetActive(!isInventory && !isDatabaseOpen);

        if (leftMenuScrollRect != null)
        {
            float topExpansion = isDatabaseOpen ? databaseListTopExpansion : 0f;
            leftMenuScrollRect.anchoredPosition = originalLeftMenuPosition + Vector2.up * (topExpansion * 0.5f);
            leftMenuScrollRect.sizeDelta = originalLeftMenuSize + Vector2.up * topExpansion;
            leftMenuScrollRect.gameObject.SetActive(isDatabaseOpen);
        }

        if (bookIndex == 0) // 物品欄
        {
            bookTitleText.text = "";
        }
        else if (currentBook.bookButton != null)
        {
            TextMeshProUGUI btnText =
                currentBook.bookButton.GetComponentInChildren<TextMeshProUGUI>();

            bookTitleText.text = btnText != null ? btnText.text : "找不到文字";
        }
        else
        {
            bookTitleText.text = "未綁定按鈕";
        }

        if (isDatabaseOpen) RebuildDatabasePageButtons();
        else ClearArticleButtons();
        if (isDatabaseOpen) ShowDatabaseCatalog();
        if (isInventory) ShowInventoryCatalog();

        if (bookTemplateObject != null)
        {
            bookTemplateObject.SetActive(true);
            if (isDatabaseOpen) ResetLeftMenuScroll();
            if (isInventory && inventoryCatalogScrollView != null && inventoryCatalogContent != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryCatalogContent);
                inventoryCatalogScrollView.verticalNormalizedPosition = 1f;
            }
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(PlayBookSwingAnimation());
        }
    }

    public void OpenMap()
    {
        CloseDatabaseDetail();
        CloseInventoryDetail();
        if (bookTemplateObject != null) bookTemplateObject.SetActive(false);
        if (fullMapRoot != null)
        {
            fullMapRoot.SetActive(true);
            NotebookMapController map = fullMapRoot.GetComponent<NotebookMapController>();
            if (map != null) map.ShowMap();
        }
    }

    public void OpenArticle(int articleIndex)
    {
        if (currentBook == null || articleIndex < 0 || articleIndex >= currentBook.articles.Count) return;
        if (isDatabaseOpen && currentBook.articles[articleIndex].databasePage != currentDatabasePage) return;

        ArticleEntry article = currentBook.articles[articleIndex];
        if (isDatabaseOpen)
        {
            ShowDatabaseDetail(article);
            return;
        }
        if (isInventoryOpen)
        {
            ShowInventoryDetail(article);
            return;
        }
        if (storyText != null) storyText.text = article.content;
        if (storyScrollView != null) storyScrollView.verticalNormalizedPosition = 1f;
    }

    private void SelectDatabasePage(int page)
    {
        if (!isDatabaseOpen) return;
        currentDatabasePage = Mathf.Clamp(page, 1, Mathf.Max(1, databasePageCount));
        UpdateDatabasePageSelection();
        ShowDatabaseCatalog();
    }

    private void ClearArticleButtons()
    {
        databasePageTexts.Clear();
        foreach (Transform child in tabContentContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void RebuildDatabasePageButtons()
    {
        ClearArticleButtons();
        if (leftMenuLayoutGroup != null)
            leftMenuLayoutGroup.spacing = databasePageSpacing;

        int pageCount = Mathf.Max(1, databasePageCount);
        for (int page = 1; page <= pageCount; page++)
        {
            int selectedPage = page;
            GameObject newTab = Instantiate(tabButtonPrefab, tabContentContainer);
            RectTransform buttonRect = newTab.GetComponent<RectTransform>();
            float buttonWidth = Mathf.Max(1f, databasePageButtonSize.x);
            float buttonHeight = Mathf.Max(1f, databasePageButtonSize.y);
            buttonRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, buttonWidth);
            buttonRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, buttonHeight);

            RectTransform underline = newTab.transform.Find("Underline") as RectTransform;
            if (underline != null)
                underline.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal, Mathf.Max(0f, buttonWidth - 100f));

            TextMeshProUGUI tabText = newTab.GetComponentInChildren<TextMeshProUGUI>();
            tabText.text = selectedPage.ToString();
            tabText.fontSize = databasePageFontSize;
            databasePageTexts.Add(tabText);
            newTab.GetComponent<Button>().onClick.AddListener(
                () => SelectDatabasePage(selectedPage));
        }

        UpdateDatabasePageSelection();
    }

    private void UpdateDatabasePageSelection()
    {
        for (int i = 0; i < databasePageTexts.Count; i++)
        {
            TextMeshProUGUI text = databasePageTexts[i];
            if (text == null) continue;

            bool selected = i + 1 == currentDatabasePage;
            text.color = selected ? activeTabColor : normalTabColor;
            text.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private void ShowDatabaseCatalog()
    {
        if (!isDatabaseOpen || databaseCatalogContent == null) return;
        CloseDatabaseDetail();
        if (storyScrollView != null) storyScrollView.gameObject.SetActive(false);
        if (databaseBackButton != null) databaseBackButton.gameObject.SetActive(false);
        if (databaseCatalogScrollView != null) databaseCatalogScrollView.gameObject.SetActive(true);

        foreach (Transform child in databaseCatalogContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        int entryCount = 0;
        for (int i = 0; i < currentBook.articles.Count; i++)
        {
            if (currentBook.articles[i].databasePage != currentDatabasePage) continue;
            CreateCatalogCard(i, databaseCatalogContent);
            entryCount++;
        }
        if (entryCount == 0) CreateCatalogMessage("本頁尚無詞條", databaseCatalogContent);

        LayoutRebuilder.ForceRebuildLayoutImmediate(databaseCatalogContent);
        if (databaseCatalogScrollView != null) databaseCatalogScrollView.verticalNormalizedPosition = 1f;
    }

    private void CreateInventoryCatalog()
    {
        if (inventoryContentRoot == null) return;
        if (inventoryCatalogScrollView == null)
        {
            Transform catalog = inventoryContentRoot.transform.Find("Inventory_Catalog_ScrollView");
            if (catalog != null) inventoryCatalogScrollView = catalog.GetComponent<ScrollRect>();
        }
        if (inventoryCatalogScrollView == null)
        {
            Debug.LogError("[NotebookManager] Inventory_Catalog_ScrollView is missing.");
            return;
        }
        if (inventoryCatalogContent == null)
            inventoryCatalogContent = inventoryCatalogScrollView.content;

        inventoryCatalogScrollView.horizontal = false;
        inventoryCatalogScrollView.vertical = true;
        inventoryCatalogScrollView.movementType = ScrollRect.MovementType.Clamped;
    }

    private void ShowInventoryCatalog()
    {
        if (!isInventoryOpen || inventoryCatalogContent == null) return;
        CloseInventoryDetail();
        inventoryCatalogScrollView.gameObject.SetActive(true);

        foreach (Transform child in inventoryCatalogContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentBook.articles.Count; i++)
            CreateCatalogCard(i, inventoryCatalogContent);

        if (currentBook.articles.Count == 0)
            CreateCatalogMessage("尚無物品", inventoryCatalogContent);

        LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryCatalogContent);
        inventoryCatalogScrollView.verticalNormalizedPosition = 1f;
    }

    private void CreateCatalogCard(int articleIndex, RectTransform contentRoot)
    {
        ArticleEntry article = currentBook.articles[articleIndex];
        GameObject card = new GameObject("Notebook_Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(contentRoot, false);
        Image cardBackground = card.GetComponent<Image>();
        cardBackground.color = new Color(1f, 1f, 1f, 0.12f);
        cardBackground.raycastTarget = false;

        GameObject picture = new GameObject("Entry_Image_Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform pictureRect = picture.GetComponent<RectTransform>();
        pictureRect.SetParent(cardRect, false);
        SetCardRect(pictureRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(12f, -12f), new Vector2(-24f, 220f));
        Image pictureImage = picture.GetComponent<Image>();
        pictureImage.sprite = article.image;
        pictureImage.preserveAspect = true;
        pictureImage.color = article.image != null ? Color.white : new Color(0.88f, 0.91f, 0.94f, 1f);
        picture.GetComponent<Button>().onClick.AddListener(() => OpenArticle(articleIndex));
        if (article.image == null)
            CreateCardText("Missing_Image_Text", pictureRect, "尚無圖片", 28f, new Color(0.2f, 0.2f, 0.2f, 1f));

        GameObject name = new GameObject("Entry_Name_Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.SetParent(cardRect, false);
        SetCardRect(nameRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(-24f, 52f));
        name.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
        name.GetComponent<Button>().onClick.AddListener(() => OpenArticle(articleIndex));
        CreateCardText("Entry_Name", nameRect, article.entryTitle, 30f,
            new Color(0.2f, 0.2f, 0.2f, 1f));
    }

    private void CreateDatabaseDetailOverlay()
    {
        GameObject overlay = new GameObject("Database_Detail_Overlay", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.SetParent(bookRect, false);
        SetCardRect(overlayRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        Image backdrop = overlay.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.62f);
        Button backdropButton = overlay.GetComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;
        backdropButton.onClick.AddListener(CloseDatabaseDetail);

        GameObject panel = new GameObject("Database_Detail_Panel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(overlayRect, false);
        SetCardRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(250f, 0f), new Vector2(860f, 730f));
        panel.GetComponent<Image>().color = new Color(0.9f, 0.93f, 0.96f, 1f);
        // A Button with no click action absorbs clicks inside the panel, so only the backdrop closes it.
        panel.GetComponent<Button>().transition = Selectable.Transition.None;

        databaseDetailTitle = CreateCardText("Database_Detail_Title", panelRect, "", 42f,
            new Color(0.15f, 0.19f, 0.24f, 1f));
        SetCardRect(databaseDetailTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(-48f, 64f));

        GameObject imageArea = new GameObject("Database_Detail_Image_Area", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform imageAreaRect = imageArea.GetComponent<RectTransform>();
        imageAreaRect.SetParent(panelRect, false);
        SetCardRect(imageAreaRect, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(-48f, 280f));
        imageArea.GetComponent<Image>().color = new Color(0.98f, 0.99f, 1f, 1f);

        GameObject imageObject = new GameObject("Database_Detail_Image", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(imageAreaRect, false);
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = new Vector2(12f, 12f);
        imageRect.offsetMax = new Vector2(-12f, -12f);
        databaseDetailImage = imageObject.GetComponent<Image>();
        databaseDetailImage.preserveAspect = true;
        databaseDetailImage.raycastTarget = false;
        databaseDetailMissingImage = CreateCardText("Database_Detail_No_Image", imageAreaRect,
            "尚無圖片", 30f, new Color(0.3f, 0.34f, 0.38f, 1f));

        GameObject scrollObject = new GameObject("Database_Detail_Description_ScrollView",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.SetParent(panelRect, false);
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(24f, 24f);
        scrollRect.offsetMax = new Vector2(-24f, -408f);
        scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.55f);

        GameObject textObject = new GameObject("Database_Detail_Description", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(ContentSizeFitter));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(scrollRect, false);
        SetCardRect(textRect, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(-24f, 0f));
        databaseDetailDescription = textObject.GetComponent<TextMeshProUGUI>();
        databaseDetailDescription.font = bookTitleText.font;
        databaseDetailDescription.fontSize = 30f;
        databaseDetailDescription.alignment = TextAlignmentOptions.TopLeft;
        databaseDetailDescription.color = new Color(0.15f, 0.19f, 0.24f, 1f);
        databaseDetailDescription.raycastTarget = false;
        databaseDetailDescription.richText = false;
        textObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        databaseDetailScrollView = scrollObject.GetComponent<ScrollRect>();
        databaseDetailScrollView.content = textRect;
        databaseDetailScrollView.viewport = scrollRect;
        databaseDetailScrollView.horizontal = false;
        databaseDetailScrollView.vertical = true;
        databaseDetailScrollView.movementType = ScrollRect.MovementType.Clamped;
        databaseDetailScrollView.scrollSensitivity = 32f;

        databaseDetailOverlay = overlay;
        databaseDetailOverlay.SetActive(false);
    }

    private void ShowDatabaseDetail(ArticleEntry article)
    {
        if (databaseDetailOverlay == null) return;
        databaseDetailTitle.text = string.IsNullOrWhiteSpace(article.entryTitle) ? "未命名詞條" : article.entryTitle;
        databaseDetailImage.sprite = article.image;
        databaseDetailImage.enabled = article.image != null;
        databaseDetailMissingImage.gameObject.SetActive(article.image == null);
        databaseDetailDescription.text = string.IsNullOrWhiteSpace(article.content) ? "尚無介紹" : article.content;
        databaseDetailOverlay.SetActive(true);
        databaseDetailOverlay.transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(databaseDetailDescription.rectTransform);
        databaseDetailScrollView.verticalNormalizedPosition = 1f;
    }

    public void CloseDatabaseDetail()
    {
        if (databaseDetailOverlay != null) databaseDetailOverlay.SetActive(false);
    }

    private void CreateInventoryDetailOverlay()
    {
        GameObject overlay = new GameObject("Inventory_Detail_Overlay", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.SetParent(bookRect, false);
        SetCardRect(overlayRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, inventoryBackdropOpacity);
        Button backdropButton = overlay.GetComponent<Button>();
        backdropButton.transition = Selectable.Transition.None;
        backdropButton.onClick.AddListener(CloseInventoryDetail);

        GameObject panel = new GameObject("Inventory_Detail_Panel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup));
        inventoryDetailPanel = panel.GetComponent<RectTransform>();
        inventoryDetailPanel.SetParent(overlayRect, false);
        inventoryDetailPanel.anchorMin = Vector2.zero;
        inventoryDetailPanel.anchorMax = Vector2.one;
        inventoryDetailPanel.offsetMin = new Vector2(60f, 40f);
        inventoryDetailPanel.offsetMax = new Vector2(-60f, -40f);
        panel.GetComponent<Image>().color = new Color(0.9f, 0.93f, 0.96f, 0.96f);
        panel.GetComponent<Button>().transition = Selectable.Transition.None;
        inventoryDetailCanvasGroup = panel.GetComponent<CanvasGroup>();

        GameObject back = new GameObject("Inventory_Return_Button", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform backRect = back.GetComponent<RectTransform>();
        backRect.SetParent(inventoryDetailPanel, false);
        SetCardRect(backRect, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(88f, 68f));
        back.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
        back.GetComponent<Button>().onClick.AddListener(CloseInventoryDetail);

        GameObject backIcon = new GameObject("Return_Arrow_Icon", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform backIconRect = backIcon.GetComponent<RectTransform>();
        backIconRect.SetParent(backRect, false);
        backIconRect.anchorMin = Vector2.zero;
        backIconRect.anchorMax = Vector2.one;
        backIconRect.offsetMin = new Vector2(12f, 8f);
        backIconRect.offsetMax = new Vector2(-12f, -8f);
        Image arrowImage = backIcon.GetComponent<Image>();
        arrowImage.sprite = Resources.Load<Sprite>("NotebookIcons/ReturnArrow");
        arrowImage.preserveAspect = true;
        arrowImage.raycastTarget = false;
        if (arrowImage.sprite == null)
        {
            arrowImage.enabled = false;
            CreateCardText("Return_Button_Fallback", backRect, "返回", 28f,
                new Color(0.15f, 0.19f, 0.24f, 1f));
        }

        inventoryDetailTitle = CreateCardText("Inventory_Detail_Title", inventoryDetailPanel,
            "", 46f, new Color(0.15f, 0.19f, 0.24f, 1f));
        SetCardRect(inventoryDetailTitle.rectTransform, new Vector2(0f, 1f),
            new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(30f, -24f),
            new Vector2(-260f, 70f));

        GameObject imageArea = new GameObject("Inventory_Detail_Image_Area",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform imageAreaRect = imageArea.GetComponent<RectTransform>();
        imageAreaRect.SetParent(inventoryDetailPanel, false);
        imageAreaRect.anchorMin = Vector2.zero;
        imageAreaRect.anchorMax = new Vector2(0.5f, 1f);
        imageAreaRect.offsetMin = new Vector2(32f, 32f);
        imageAreaRect.offsetMax = new Vector2(-12f, -112f);
        imageArea.GetComponent<Image>().color = new Color(0.98f, 0.99f, 1f, 1f);

        GameObject imageObject = new GameObject("Inventory_Detail_Image",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(imageAreaRect, false);
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = new Vector2(14f, 14f);
        imageRect.offsetMax = new Vector2(-14f, -14f);
        inventoryDetailImage = imageObject.GetComponent<Image>();
        inventoryDetailImage.preserveAspect = true;
        inventoryDetailImage.raycastTarget = false;
        inventoryDetailMissingImage = CreateCardText("Inventory_Detail_No_Image", imageAreaRect,
            "尚無圖片", 32f, new Color(0.3f, 0.34f, 0.38f, 1f));

        GameObject scrollObject = new GameObject("Inventory_Detail_Description_ScrollView",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.SetParent(inventoryDetailPanel, false);
        scrollRect.anchorMin = new Vector2(0.5f, 0f);
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(12f, 32f);
        scrollRect.offsetMax = new Vector2(-32f, -112f);
        scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.55f);

        GameObject textObject = new GameObject("Inventory_Detail_Description",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(ContentSizeFitter));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(scrollRect, false);
        SetCardRect(textRect, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(-28f, 0f));
        inventoryDetailDescription = textObject.GetComponent<TextMeshProUGUI>();
        inventoryDetailDescription.font = bookTitleText.font;
        inventoryDetailDescription.fontSize = 32f;
        inventoryDetailDescription.alignment = TextAlignmentOptions.TopLeft;
        inventoryDetailDescription.color = new Color(0.15f, 0.19f, 0.24f, 1f);
        inventoryDetailDescription.raycastTarget = false;
        inventoryDetailDescription.richText = false;
        textObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        inventoryDetailScrollView = scrollObject.GetComponent<ScrollRect>();
        inventoryDetailScrollView.content = textRect;
        inventoryDetailScrollView.viewport = scrollRect;
        inventoryDetailScrollView.horizontal = false;
        inventoryDetailScrollView.vertical = true;
        inventoryDetailScrollView.movementType = ScrollRect.MovementType.Clamped;
        inventoryDetailScrollView.scrollSensitivity = 32f;

        inventoryDetailOverlay = overlay;
        inventoryDetailOverlay.SetActive(false);
    }

    private void ShowInventoryDetail(ArticleEntry article)
    {
        if (inventoryDetailOverlay == null) return;
        inventoryDetailTitle.text = string.IsNullOrWhiteSpace(article.entryTitle)
            ? "未命名物品" : article.entryTitle;
        inventoryDetailImage.sprite = article.image;
        inventoryDetailImage.enabled = article.image != null;
        inventoryDetailMissingImage.gameObject.SetActive(article.image == null);
        inventoryDetailDescription.text = string.IsNullOrWhiteSpace(article.content)
            ? "尚無介紹" : article.content;
        inventoryDetailOverlay.SetActive(true);
        inventoryDetailOverlay.transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryDetailDescription.rectTransform);
        inventoryDetailScrollView.verticalNormalizedPosition = 1f;

        if (inventoryZoomCoroutine != null) StopCoroutine(inventoryZoomCoroutine);
        inventoryZoomCoroutine = StartCoroutine(PlayInventoryDetailZoom());
    }

    private IEnumerator PlayInventoryDetailZoom()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, inventoryZoomDuration);
        inventoryDetailPanel.localScale = Vector3.one * 0.82f;
        inventoryDetailCanvasGroup.alpha = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            inventoryDetailPanel.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, eased);
            inventoryDetailCanvasGroup.alpha = eased;
            yield return null;
        }
        inventoryDetailPanel.localScale = Vector3.one;
        inventoryDetailCanvasGroup.alpha = 1f;
        inventoryZoomCoroutine = null;
    }

    public void CloseInventoryDetail()
    {
        if (inventoryZoomCoroutine != null)
        {
            StopCoroutine(inventoryZoomCoroutine);
            inventoryZoomCoroutine = null;
        }
        if (inventoryDetailOverlay != null) inventoryDetailOverlay.SetActive(false);
    }

    private void CreateCatalogMessage(string message, RectTransform contentRoot)
    {
        GameObject holder = new GameObject("Catalog_Empty", typeof(RectTransform));
        RectTransform rect = holder.GetComponent<RectTransform>();
        rect.SetParent(contentRoot, false);
        CreateCardText("Empty_Text", rect, message, 30f, Color.white);
    }

    private TextMeshProUGUI CreateCardText(string name, RectTransform parent, string value, float fontSize, Color color)
    {
        GameObject label = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetCardRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        text.font = bookTitleText.font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        text.text = value;
        return text;
    }

    private static void SetCardRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void ResetLeftMenuScroll()
    {
        if (leftMenuScrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        if (tabContentContainer is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(leftMenuScrollRect);
        ScrollRect scrollView = leftMenuScrollRect.GetComponent<ScrollRect>();
        if (scrollView != null) scrollView.verticalNormalizedPosition = 1f;
    }

    public void CloseBook()
    {
        CloseDatabaseDetail();
        CloseInventoryDetail();
        if (bookTemplateObject != null) bookTemplateObject.SetActive(false);
        if (fullMapRoot != null) fullMapRoot.SetActive(false);
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
