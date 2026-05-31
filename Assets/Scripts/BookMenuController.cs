using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BookMenuController : MonoBehaviour
{
    private enum BookTab
    {
        Towers,
        Enemies,
        Settings
    }

    [Header("Book Sprites")]
    [SerializeField] private Sprite bookClosedSprite;
    [SerializeField] private Sprite bookOpenSprite;
    [SerializeField] private Sprite towerTabSprite;
    [SerializeField] private Sprite enemyTabSprite;
    [SerializeField] private Sprite settingsTabSprite;
    [SerializeField] private Sprite closeButtonSprite;
    [SerializeField] private Sprite previousPageSprite;
    [SerializeField] private Sprite nextPageSprite;

    [Header("Book Data")]
    [SerializeField] private TowerData[] towers;
    [SerializeField] private EnemyData[] enemies;
    [SerializeField] private GameObject[] enemyPrefabs;


    [Header("Book Layout")]
    [SerializeField] private Vector2 leftGridPosition = new Vector2(-275f, -20f);
    [SerializeField] private Vector2 rightPagePosition = new Vector2(278f, -10f);


    [Header("Left Grid Item Layout")]
    [SerializeField] private Vector2 gridItemTopLeftPosition = new Vector2(-86f, 78f);
    [SerializeField] private Vector2 gridItemSpacing = new Vector2(172f, -156f);
    [SerializeField] private int gridItemColumns = 2;

    [Header("Settings Page Layout")]
    [SerializeField] private Vector2 settingsTitlePosition = new Vector2(0f, 135f);
    [SerializeField] private Vector2 settingsTitleSize = new Vector2(320f, 46f);
    [SerializeField] private Vector2 volumeLabelPosition = new Vector2(-38f, 82f);
    [SerializeField] private Vector2 volumeLabelSize = new Vector2(230f, 32f);
    [SerializeField] private Vector2 volumeValuePosition = new Vector2(126f, 82f);
    [SerializeField] private Vector2 volumeValueSize = new Vector2(80f, 32f);
    [SerializeField] private Vector2 volumeSliderPosition = new Vector2(0f, 40f);
    [SerializeField] private Vector2 volumeSliderSize = new Vector2(270f, 34f);
    [SerializeField] private Vector2 languageLabelPosition = new Vector2(-92f, -35f);
    [SerializeField] private Vector2 languageLabelSize = new Vector2(130f, 38f);
    [SerializeField] private Vector2 languageButtonPosition = new Vector2(70f, -35f);
    [SerializeField] private Vector2 languageButtonSize = new Vector2(190f, 42f);
    [SerializeField] private Vector2 controlLabelPosition = new Vector2(-92f, -102f);
    [SerializeField] private Vector2 controlLabelSize = new Vector2(130f, 38f);
    [SerializeField] private Vector2 controlButtonPosition = new Vector2(70f, -102f);
    [SerializeField] private Vector2 controlButtonSize = new Vector2(190f, 42f);

    [Header("Detail Page Layout")]
    [SerializeField] private Vector2 detailIconPosition = new Vector2(0f, 145f);
    [SerializeField] private Vector2 detailTitlePosition = new Vector2(0f, 65f);
    [SerializeField] private Vector2 detailStatsPosition = new Vector2(0f, -55f);
    [SerializeField] private Vector2 detailDescriptionPosition = new Vector2(0f, -185f);

    [SerializeField] private Vector2 detailIconSize = new Vector2(150f, 120f);
    [SerializeField] private Vector2 detailTitleSize = new Vector2(330f, 45f);
    [SerializeField] private Vector2 detailStatsSize = new Vector2(350f, 170f);
    [SerializeField] private Vector2 detailDescriptionSize = new Vector2(350f, 88f);


    private const int ItemsPerPage = 4;
    private readonly List<Button> _itemButtons = new List<Button>();

    private GameObject _bookPanel;
    private RectTransform _leftGrid;
    private RectTransform _rightPage;
    private TMP_Text _volumeValueText;
    private TMP_Text _pageText;
    private BookTab _currentTab = BookTab.Towers;
    private int _currentPage;
    private int _selectedIndex;
    private float _previousTimeScale = 1f;

    private void Awake()
    {
        EnsureBookObjects();

        if (Application.isPlaying)
        {
            RefreshContent();
            _bookPanel.SetActive(false);
        }
        else
        {
            RefreshContent();
            _bookPanel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            EnsureBookObjects();
            RefreshContent();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && isActiveAndEnabled)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null || Application.isPlaying || !isActiveAndEnabled)
                {
                    return;
                }

                EnsureBookObjects();
                RefreshContent();

                if (_bookPanel != null)
                {
                    _bookPanel.SetActive(true);
                }
            };
#endif
        }
    }

    private void EnsureBookObjects()
    {
        Button bookButton = transform.Find("BookButton")?.GetComponent<Button>();
        if (bookButton == null)
        {
            BuildBookButton();
        }
        else if (Application.isPlaying)
        {
            bookButton.onClick.RemoveListener(OpenBook);
            bookButton.onClick.AddListener(OpenBook);
        }

        Transform panelTransform = transform.Find("BookPanel");
        if (panelTransform == null)
        {
            BuildBookPanel();
        }
        else
        {
            _bookPanel = panelTransform.gameObject;
            _leftGrid = panelTransform.Find("LeftGrid")?.GetComponent<RectTransform>();
            _rightPage = panelTransform.Find("RightPage")?.GetComponent<RectTransform>();
            _pageText = panelTransform.Find("PageText")?.GetComponent<TMP_Text>();

            if (Application.isPlaying)
            {
                RegisterExistingButton(panelTransform, "CloseBookButton", CloseBook);
                RegisterExistingButton(panelTransform, "PrevPageButton", PreviousPage);
                RegisterExistingButton(panelTransform, "NextPageButton", NextPage);
                RegisterExistingButton(panelTransform, "TowerTab", () => SelectTab(BookTab.Towers));
                RegisterExistingButton(panelTransform, "EnemyTab", () => SelectTab(BookTab.Enemies));
                RegisterExistingButton(panelTransform, "SettingsTab", () => SelectTab(BookTab.Settings));
            }
        }
    }

    private void OpenBook()
    {
        _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : GetFallbackTimeScale();
        Time.timeScale = 0f;
        _bookPanel.SetActive(true);
        RefreshContent();
    }

    private void CloseBook()
    {
        _bookPanel.SetActive(false);
        float restoreScale = _previousTimeScale > 0f ? _previousTimeScale : GetFallbackTimeScale();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTimeScale(restoreScale);
        }
        else
        {
            Time.timeScale = restoreScale;
        }
    }

    private void SelectTab(BookTab tab)
    {
        _currentTab = tab;
        _currentPage = 0;
        _selectedIndex = 0;
        RefreshContent();
    }

    private void NextPage()
    {
        if (_currentTab == BookTab.Towers)
        {
            if (_currentPage < 1)
            {
                _currentPage++;
            }
            else
            {
                SelectTab(BookTab.Enemies);
                return;
            }
        }
        else
        {
            _currentPage = Mathf.Min(_currentPage + 1, GetPageCount() - 1);
        }

        _selectedIndex = _currentPage * ItemsPerPage;
        RefreshContent();
    }

    private void PreviousPage()
    {
        if (_currentTab == BookTab.Enemies && _currentPage == 0)
        {
            _currentTab = BookTab.Towers;
            _currentPage = 1;
        }
        else
        {
            _currentPage = Mathf.Max(0, _currentPage - 1);
        }

        _selectedIndex = _currentPage * ItemsPerPage;
        RefreshContent();
    }

    private void BuildBookButton()
    {
        Button button = CreateButton("BookButton", transform, bookClosedSprite, new Vector2(92f, 78f));
        RectTransform rt = button.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-28f, -24f);
        button.onClick.AddListener(OpenBook);
    }

    private void BuildBookPanel()
    {
        _bookPanel = CreateRect("BookPanel", transform, new Vector2(1120f, 660f)).gameObject;
        RectTransform panelRT = _bookPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;

        Image bookImage = _bookPanel.AddComponent<Image>();
        bookImage.sprite = bookOpenSprite;
        bookImage.preserveAspect = true;

        BuildTabs();

        _leftGrid = CreateRect("LeftGrid", _bookPanel.transform, new Vector2(390f, 330f));
        _leftGrid.anchoredPosition = leftGridPosition;

        _rightPage = CreateRect("RightPage", _bookPanel.transform, new Vector2(405f, 440f));
        _rightPage.anchoredPosition = rightPagePosition;

        Button closeButton = CreateButton("CloseBookButton", _bookPanel.transform, closeButtonSprite, new Vector2(56f, 56f));
        closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(495f, 260f);
        closeButton.onClick.AddListener(CloseBook);

        Button prevButton = CreateButton("PrevPageButton", _bookPanel.transform, previousPageSprite, new Vector2(48f, 58f));
        prevButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-420f, -250f);
        prevButton.onClick.AddListener(PreviousPage);

        Button nextButton = CreateButton("NextPageButton", _bookPanel.transform, nextPageSprite, new Vector2(48f, 58f));
        nextButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160f, -250f);
        nextButton.onClick.AddListener(NextPage);

        _pageText = CreateText("PageText", _bookPanel.transform, "1/1", 24, TextAlignmentOptions.Center);
        _pageText.rectTransform.sizeDelta = new Vector2(150f, 40f);
        _pageText.rectTransform.anchoredPosition = new Vector2(-290f, -250f);

        _bookPanel.SetActive(!Application.isPlaying);
    }

    private void BuildTabs()
    {
        Button towerTab = CreateButton("TowerTab", _bookPanel.transform, towerTabSprite, new Vector2(117f, 216f));
        towerTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(-420f, 438f);
        towerTab.onClick.AddListener(() => SelectTab(BookTab.Towers));

        Button enemyTab = CreateButton("EnemyTab", _bookPanel.transform, enemyTabSprite, new Vector2(117f, 216f));
        enemyTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(-295f, 438f);
        enemyTab.onClick.AddListener(() => SelectTab(BookTab.Enemies));

        Button settingsTab = CreateButton("SettingsTab", _bookPanel.transform, settingsTabSprite, new Vector2(117f, 216f));
        settingsTab.GetComponent<RectTransform>().anchoredPosition = new Vector2(-170f, 438f);
        settingsTab.onClick.AddListener(() => SelectTab(BookTab.Settings));
    }

    private void RegisterExistingButton(Transform parent, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Button button = parent.Find(buttonName)?.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void RefreshContent()
    {
        if (_leftGrid == null || _rightPage == null || _pageText == null)
        {
            return;
        }

        ClearChildren(_leftGrid);
        ClearChildren(_rightPage);
        _itemButtons.Clear();

        if (_currentTab == BookTab.Settings)
        {
            _pageText.text = "Settings";
            BuildSettingsPage();
            return;
        }

        int pageCount = GetPageCount();
        _currentPage = Mathf.Clamp(_currentPage, 0, pageCount - 1);
        _pageText.text = $"{_currentPage + 1}/{pageCount}";

        Object[] data = GetCurrentData();
        int start = _currentPage * ItemsPerPage;
        int end = Mathf.Min(start + ItemsPerPage, data.Length);

        if (start >= data.Length)
        {
            TMP_Text emptyLeft = CreateText("EmptyPageText", _leftGrid, "Reserved page", 24, TextAlignmentOptions.Center);
            emptyLeft.rectTransform.sizeDelta = new Vector2(280f, 80f);
            emptyLeft.rectTransform.anchoredPosition = Vector2.zero;

            TMP_Text emptyRight = CreateText("EmptyDetailText", _rightPage, "Future tower entries can be added here.", 24, TextAlignmentOptions.Center);
            emptyRight.rectTransform.sizeDelta = new Vector2(330f, 120f);
            emptyRight.rectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        for (int i = start; i < end; i++)
        {
            CreateGridItem(data[i], i, i - start);
        }

        if (data.Length > 0)
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex, start, end - 1);
            BuildDetailPage(data[_selectedIndex]);
        }
    }

    private void CreateGridItem(Object data, int dataIndex, int slotIndex)
    {
        Button button = CreateButton($"BookItem_{dataIndex}", _leftGrid, GetIcon(data), new Vector2(128f, 118f));
        RectTransform rt = button.GetComponent<RectTransform>();
        int column = slotIndex % gridItemColumns;
        int row = slotIndex / gridItemColumns;

        rt.anchoredPosition = new Vector2(gridItemTopLeftPosition.x + column * gridItemSpacing.x,gridItemTopLeftPosition.y + row * gridItemSpacing.y);
        button.onClick.AddListener(() =>
        {
            _selectedIndex = dataIndex;
            RefreshContent();
        });

        TMP_Text label = CreateText("ItemName", button.transform, GetDisplayName(data), 18, TextAlignmentOptions.Center);
        label.rectTransform.anchorMin = new Vector2(0f, 0f);
        label.rectTransform.anchorMax = new Vector2(1f, 0f);
        label.rectTransform.pivot = new Vector2(0.5f, 0f);
        label.rectTransform.anchoredPosition = new Vector2(0f, 6f);
        label.rectTransform.sizeDelta = new Vector2(-8f, 28f);
        _itemButtons.Add(button);
    }

    private void BuildDetailPage(Object data)
    {
        Image icon = CreateImage("DetailIcon", _rightPage, GetIcon(data), detailIconSize);
        icon.rectTransform.anchoredPosition = detailIconPosition;

        TMP_Text title = CreateText("DetailTitle", _rightPage, GetDisplayName(data), 30, TextAlignmentOptions.Center);
        title.rectTransform.sizeDelta = detailTitleSize;
        title.rectTransform.anchoredPosition = detailTitlePosition;

        TMP_Text stats = CreateText("DetailStats", _rightPage, GetStats(data), 22, TextAlignmentOptions.TopLeft);
        stats.rectTransform.sizeDelta = detailStatsSize;
        stats.rectTransform.anchoredPosition = detailStatsPosition;
        TMP_Text description = CreateText("DetailDescription",_rightPage,GetDescription(data),20,TextAlignmentOptions.TopLeft);
        description.rectTransform.sizeDelta = detailDescriptionSize;
        description.rectTransform.anchoredPosition = detailDescriptionPosition;
    }

    private void BuildSettingsPage()
    {
        _volumeValueText = null;

        TMP_Text title = CreateText("SettingsTitle", _leftGrid, "Settings", 34, TextAlignmentOptions.Center);
        title.rectTransform.sizeDelta = settingsTitleSize;
        title.rectTransform.anchoredPosition = settingsTitlePosition;

        TMP_Text volumeLabel = CreateText("VolumeLabel", _leftGrid, "Volume", 22, TextAlignmentOptions.Left);
        volumeLabel.rectTransform.sizeDelta = volumeLabelSize;
        volumeLabel.rectTransform.anchoredPosition = volumeLabelPosition;

        _volumeValueText = CreateText("VolumeValue", _leftGrid, "", 20, TextAlignmentOptions.Right);
        _volumeValueText.rectTransform.sizeDelta = volumeValueSize;
        _volumeValueText.rectTransform.anchoredPosition = volumeValuePosition;

        Slider volumeSlider = CreateSlider("VolumeSlider", _leftGrid, volumeSliderSize);
        volumeSlider.value = AudioManager.GetMasterVolume();
        volumeSlider.GetComponent<RectTransform>().anchoredPosition = volumeSliderPosition;
        UpdateVolumeLabel(volumeSlider.value);
        volumeSlider.onValueChanged.AddListener(value =>
        {
            AudioManager.SetMasterVolume(value);
            UpdateVolumeLabel(value);
        });

        TMP_Text languageLabel = CreateText("LanguageLabel", _leftGrid, "Language", 22, TextAlignmentOptions.Left);
        languageLabel.rectTransform.sizeDelta = languageLabelSize;
        languageLabel.rectTransform.anchoredPosition = languageLabelPosition;

        Button languageButton = CreateCycler("LanguageButton", _leftGrid, new[] { "Traditional Chinese", "English" }, languageButtonSize);
        languageButton.GetComponent<RectTransform>().anchoredPosition = languageButtonPosition;

        TMP_Text controlLabel = CreateText("ControlLabel", _leftGrid, "Control", 22, TextAlignmentOptions.Left);
        controlLabel.rectTransform.sizeDelta = controlLabelSize;
        controlLabel.rectTransform.anchoredPosition = controlLabelPosition;

        Button controlButton = CreateCycler("ControlButton", _leftGrid, new[] { "Mouse", "Keyboard", "Touch" }, controlButtonSize);
        controlButton.GetComponent<RectTransform>().anchoredPosition = controlButtonPosition;
    }

    private float GetFallbackTimeScale()
    {
        if (GameManager.Instance != null)
        {
            return Mathf.Max(1f, GameManager.Instance.GameSpeed);
        }

        return 1f;
    }

    private void UpdateVolumeLabel(float value)
    {
        if (_volumeValueText != null)
        {
            _volumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }

    private Object[] GetCurrentData()
    {
        if (_currentTab == BookTab.Towers)
        {
            return towers ?? System.Array.Empty<TowerData>();
        }

        return enemies ?? System.Array.Empty<EnemyData>();
    }

    private int GetPageCount()
    {
        if (_currentTab == BookTab.Towers)
        {
            return 2;
        }

        int count = GetCurrentData().Length;
        return Mathf.Max(1, Mathf.CeilToInt(count / (float)ItemsPerPage));
    }

    private string GetDisplayName(Object data)
    {
        return data != null ? data.name.Replace("Enemy_", "").Replace("_", "").Replace("Level1", "") : "Unknown";
    }

    private Sprite GetIcon(Object data)
    {
        if (data is TowerData tower)
        {
            return tower.sprite;
        }

        if (data is EnemyData enemy && enemy.splitPrefab != null)
        {
            SpriteRenderer renderer = enemy.splitPrefab.GetComponentInChildren<SpriteRenderer>();
            return renderer != null ? renderer.sprite : null;
        }

        if (data is EnemyData enemyData && enemies != null && enemyPrefabs != null)
        {
            int index = System.Array.IndexOf(enemies, enemyData);
            if (index >= 0 && index < enemyPrefabs.Length && enemyPrefabs[index] != null)
            {
                SpriteRenderer renderer = enemyPrefabs[index].GetComponentInChildren<SpriteRenderer>();
                return renderer != null ? renderer.sprite : null;
            }
        }

        return null;
    }

    private string GetStats(Object data)
    {
        if (data is TowerData tower)
        {
            return $"Damage: {tower.damage}\nRange: {tower.range}\nShoot Rate: {tower.shootInterval}\nProjectile Speed: {tower.projectileSpeed}\nCost: {tower.cost}";
        }

        if (data is EnemyData enemy)
        {
            return $"HP: {enemy.lives}\nDamage: {enemy.damage}\nSpeed: {enemy.speed}\nDEF: {enemy.def}\nMDEF: {enemy.mdef}\nReward: {enemy.resourceReward}";
        }

        return "";
    }

    private string GetDescription(Object data)
    {
        if (data is TowerData tower)
        {
            return string.IsNullOrWhiteSpace(tower.description)
                ? ""
                : tower.description;
        }

        if (data is EnemyData enemy)
        {
            return string.IsNullOrWhiteSpace(enemy.description)
                ? "No description yet."
                : enemy.description;
        }

        return "";
    }

    private static RectTransform CreateRect(string objectName, Transform parent, Vector2 size)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        return rt;
    }

    private static Image CreateImage(string objectName, Transform parent, Sprite sprite, Vector2 size)
    {
        RectTransform rt = CreateRect(objectName, parent, size);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = sprite != null ? Color.white : new Color(0.55f, 0.43f, 0.28f, 1f);
        return image;
    }

    private static Button CreateButton(string objectName, Transform parent, Sprite sprite, Vector2 size)
    {
        Image image = CreateImage(objectName, parent, sprite, size);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static Button CreateTextButton(string objectName, Transform parent, string label, Vector2 size)
    {
        Button button = CreateButton(objectName, parent, null, size);
        button.GetComponent<Image>().color = new Color(0.58f, 0.36f, 0.18f, 0.95f);
        TMP_Text text = CreateText("Label", button.transform, label, 28, TextAlignmentOptions.Center);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.sizeDelta = Vector2.zero;
        text.rectTransform.anchoredPosition = Vector2.zero;
        return button;
    }

    private static TMP_Text CreateText(string objectName, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
    {
        RectTransform rt = CreateRect(objectName, parent, new Vector2(220f, 40f));
        TMP_Text label = rt.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = new Color(0.18f, 0.12f, 0.07f, 1f);
        label.enableWordWrapping = true;
        return label;
    }

    private static Slider CreateSlider(string objectName, Transform parent, Vector2 size)
    {
        RectTransform root = CreateRect(objectName, parent, size);
        Image rootImage = root.gameObject.AddComponent<Image>();
        rootImage.color = new Color(1f, 1f, 1f, 0f);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;

        Image background = CreateImage("Background", root, null, size);
        background.color = new Color(0.27f, 0.17f, 0.09f, 1f);
        background.raycastTarget = false;

        RectTransform fillArea = CreateRect("FillArea", root, new Vector2(size.x - 30f, size.y * 0.55f));
        fillArea.anchoredPosition = Vector2.zero;

        Image fill = CreateImage("Fill", fillArea, null, fillArea.sizeDelta);
        fill.color = new Color(0.75f, 0.54f, 0.23f, 1f);
        fill.raycastTarget = false;
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.sizeDelta = Vector2.zero;
        slider.fillRect = fill.rectTransform;

        RectTransform handleArea = CreateRect("HandleSlideArea", root, new Vector2(size.x - 26f, size.y));
        handleArea.anchoredPosition = Vector2.zero;

        Image handle = CreateImage("Handle", handleArea, null, new Vector2(28f, size.y + 8f));
        handle.color = new Color(0.92f, 0.78f, 0.45f, 1f);
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    private static Button CreateCycler(string objectName, Transform parent, string[] options, Vector2 size)
    {
        Button button = CreateTextButton(objectName, parent, options[0], size);
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        int index = 0;
        button.onClick.AddListener(() =>
        {
            index = (index + 1) % options.Length;
            label.text = options[index];
        });
        return button;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }
}
