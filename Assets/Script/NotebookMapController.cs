using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Displays the visited room maps inside the notebook, without a separate HUD minimap.</summary>
public class NotebookMapController : MonoBehaviour
{
    [Serializable]
    public class MapArea
    {
        public string sceneName;
        public string areaId;
        public string displayName;
        public Sprite mapSprite;
    }

    [SerializeField] private List<MapArea> areas = new List<MapArea>();
    [SerializeField] private TMP_FontAsset chineseFont;
    public TMP_FontAsset ChineseFont => chineseFont;

    [Header("測試設定")]
    [SerializeField] private bool showAllMapsForTesting = false;

    private bool previousShowAllMappsForTesting;
    
    [SerializeField] private Image mapImage;
    [SerializeField] private RectTransform playerDot;
    [SerializeField] private RectTransform selector;
    [SerializeField] private TextMeshProUGUI areaNameText;

    private const string VisitedFlagPrefix = "NotebookMapVisited:";

    private readonly List<GameObject> areaButtons = new List<GameObject>();
    private GameProgress progress;
    private MapArea selectedArea;
    private TextMeshProUGUI emptyText;
    private bool uiBuilt;
    private Image taskMarker;
    private TaskMarkerIconSet markerIconSet;

    private void Awake()
    {
        BuildUI();
    }

    public void RegisterScene(string sceneName)
    {
        MapArea area = FindArea(sceneName);
        if (area == null) return;

        GameProgress gameProgress = GetProgress();
        if (gameProgress != null)
            gameProgress.AddFlag(VisitedFlagPrefix + area.areaId);
    }

    public void ShowMap()
    {
        if (!uiBuilt) BuildUI();
        if (!uiBuilt) return;

        MapArea currentArea = FindArea(SceneManager.GetActiveScene().name);
        if (currentArea != null) RegisterScene(currentArea.sceneName);

        selectedArea = currentArea;
        BuildVisitedButtons();
        previousShowAllMappsForTesting = showAllMapsForTesting;
        RefreshMap();
        RefreshTaskGuide();
    }

    private void Update()
    {
        if (previousShowAllMappsForTesting != showAllMapsForTesting)
        {
            previousShowAllMappsForTesting = showAllMapsForTesting;

            if (!IsVisited(selectedArea))
                selectedArea = FindArea(SceneManager.GetActiveScene().name);

            BuildVisitedButtons();
            RefreshMap();
        }
        
        if (selectedArea != null &&
            selectedArea.sceneName == SceneManager.GetActiveScene().name)
            RefreshPlayerDot();
        RefreshTaskGuide();
    }

    private GameProgress GetProgress()
    {
        if (progress == null)
            progress = Resources.Load<GameProgress>("StoryAssets/MainProgress");
        return progress;
    }

    private MapArea FindArea(string sceneName)
    {
        foreach (MapArea area in areas)
            if (area != null && area.sceneName == sceneName)
                return area;
        return null;
    }

    private bool IsVisited(MapArea area)
    {
        if (area == null) return false;

    #if UNITY_EDITOR
        if (showAllMapsForTesting) return true;
    #endif

        GameProgress gameProgress = GetProgress();
        return gameProgress != null && gameProgress.HasFlag(VisitedFlagPrefix + area.areaId);
    }

    private void BuildUI()
    {
        if (uiBuilt) return;
        if (mapImage == null || playerDot == null || selector == null || areaNameText == null)
        {
            Debug.LogError("NotebookMapController: FullMapRoot UI references are missing.", this);
            return;
        }
        uiBuilt = true;
        if (chineseFont != null) areaNameText.font = chineseFont;
        mapImage.preserveAspect = true;
        mapImage.raycastTarget = false;
        Image dotImage = playerDot.GetComponent<Image>();
        if (dotImage != null)
        {
            dotImage.sprite = CreateDotSprite();
            dotImage.raycastTarget = false;
        }

        emptyText = MakeText("MapEmptyText", transform, "此場景尚未設定地圖", 34, TextAlignmentOptions.Center);
        Place(emptyText.rectTransform, Vector2.zero, new Vector2(1200, 90));
        BuildTaskMarker();
    }

    private void BuildVisitedButtons()
    {
        foreach (GameObject areaButton in areaButtons)
            if (areaButton != null) Destroy(areaButton);
        areaButtons.Clear();

        foreach (MapArea area in areas)
        {
            if (!IsVisited(area)) continue;

            Image image = MakeImage("Area_" + area.areaId, selector, AreaColor(area));
            Place(image.rectTransform, Vector2.zero, new Vector2(220, 66));
            areaButtons.Add(image.gameObject);

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            MapArea clickedArea = area;
            button.onClick.AddListener(() => SelectArea(clickedArea));

            TextMeshProUGUI label = MakeText("Label", image.transform, area.displayName, 29, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
        }
    }

    private void SelectArea(MapArea area)
    {
        if (!IsVisited(area)) return;
        selectedArea = area;
        for (int i = 0, j = 0; i < areas.Count; i++)
        {
            if (!IsVisited(areas[i])) continue;
            if (j < areaButtons.Count)
                areaButtons[j].GetComponent<Image>().color = AreaColor(areas[i]);
            j++;
        }
        RefreshMap();
        RefreshTaskGuide();
    }

    private Color AreaColor(MapArea area)
    {
        return area == selectedArea
            ? new Color(0.12f, 0.43f, 0.72f, 0.95f)
            : new Color(0.05f, 0.20f, 0.40f, 0.88f);
    }

    private void RefreshMap()
    {
        bool hasMap = selectedArea != null && selectedArea.mapSprite != null;
        mapImage.gameObject.SetActive(hasMap);
        emptyText.gameObject.SetActive(!hasMap);
        if (!hasMap)
        {
            areaNameText.text = "地圖";
            return;
        }

        mapImage.sprite = selectedArea.mapSprite;
        bool isCurrent = selectedArea.sceneName == SceneManager.GetActiveScene().name;
        areaNameText.text = (isCurrent ? "目前位置：" : "查看地圖：") + selectedArea.displayName;
        playerDot.gameObject.SetActive(isCurrent);
        if (isCurrent) RefreshPlayerDot();
        RefreshTaskGuide();
    }

    private void BuildTaskMarker()
    {
        Transform existing = mapImage.transform.Find("FullTaskMarker");
        if (existing != null) taskMarker = existing.GetComponent<Image>();
        if (taskMarker == null) taskMarker = MakeImage("FullTaskMarker", mapImage.transform, Color.blue);
        taskMarker.sprite = CreateDotSprite();
        taskMarker.color = new Color(0.15f, 0.45f, 1f, 1f);
        taskMarker.raycastTarget = false;
        taskMarker.rectTransform.sizeDelta = new Vector2(26f, 26f);
        taskMarker.gameObject.SetActive(false);
        markerIconSet = Resources.Load<TaskMarkerIconSet>("TaskGuideAssets/MainTaskMarkerIcons");
    }

    private void RefreshTaskGuide()
    {
        if (taskMarker == null) return;
        TaskGuideManager manager = TaskGuideManager.Instance;
        TaskGuideData task = manager != null ? manager.CurrentTask : null;
        bool show = task != null && manager.IsTracked && task.showMarker &&
                    selectedArea != null && selectedArea.mapSprite != null &&
                    selectedArea.areaId == task.targetAreaId && IsVisited(selectedArea);
        taskMarker.gameObject.SetActive(show);
        if (!show) return;

        TaskMarkerIconEntry icon;
        if (markerIconSet != null && markerIconSet.TryGetIcon(task.markerIconId, out icon))
        {
            if (icon.sprite != null) taskMarker.sprite = icon.sprite;
            taskMarker.color = icon.color;
            if (icon.fullSize.x > 0f && icon.fullSize.y > 0f)
                taskMarker.rectTransform.sizeDelta = icon.fullSize;
        }

        float x01 = task.targetX01;
        TaskGuidePoint point;
        if (selectedArea.sceneName == SceneManager.GetActiveScene().name &&
            TaskGuidePointRegistry.Instance != null &&
            TaskGuidePointRegistry.Instance.TryGetPoint(task.targetPointId, out point) &&
            point != null && point.areaId == task.targetAreaId &&
            point.gameObject.scene == SceneManager.GetActiveScene())
        {
            Transform left = FindSceneTransform(SceneManager.GetActiveScene(), "Left Boundary");
            Transform right = FindSceneTransform(SceneManager.GetActiveScene(), "Right Boundary");
            if (left != null && right != null && !Mathf.Approximately(left.position.x, right.position.x))
                x01 = Mathf.InverseLerp(Mathf.Min(left.position.x, right.position.x),
                    Mathf.Max(left.position.x, right.position.x), point.transform.position.x);
        }
        RectTransform rect = mapImage.rectTransform;
        float aspect = mapImage.sprite.rect.width / mapImage.sprite.rect.height;
        float visibleWidth = Mathf.Min(rect.rect.width, rect.rect.height * aspect);
        taskMarker.rectTransform.anchoredPosition = new Vector2(
            Mathf.Lerp(-visibleWidth * 0.5f, visibleWidth * 0.5f, Mathf.Clamp01(x01)), 30f);
    }

    private void RefreshPlayerDot()
    {
        if (playerDot == null || mapImage == null || mapImage.sprite == null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            playerDot.gameObject.SetActive(false);
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        Transform left = FindSceneTransform(scene, "Left Boundary");
        Transform right = FindSceneTransform(scene, "Right Boundary");
        if (left == null || right == null || Mathf.Approximately(left.position.x, right.position.x))
        {
            playerDot.gameObject.SetActive(false);
            return;
        }

        float minX = Mathf.Min(left.position.x, right.position.x);
        float maxX = Mathf.Max(left.position.x, right.position.x);
        float x01 = Mathf.InverseLerp(minX, maxX, player.transform.position.x);
        RectTransform rect = mapImage.rectTransform;
        float imageAspect = mapImage.sprite.rect.width / mapImage.sprite.rect.height;
        float visibleWidth = Mathf.Min(rect.rect.width, rect.rect.height * imageAspect);
        playerDot.anchoredPosition = new Vector2(Mathf.Lerp(-visibleWidth * 0.5f, visibleWidth * 0.5f, x01), 0);
        playerDot.gameObject.SetActive(true);
    }

    private static Transform FindSceneTransform(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindChildNamed(root.transform, objectName);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindChildNamed(Transform parent, string objectName)
    {
        if (parent.name == objectName) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindChildNamed(child, objectName);
            if (found != null) return found;
        }
        return null;
    }

    private TextMeshProUGUI MakeText(string objectName, Transform parent, string value, float size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.font = chineseFont != null ? chineseFont : TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = alignment;
        label.text = value;
        label.raycastTarget = false;
        return label;
    }

    private static Image MakeImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite CreateDotSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - (size - 1) * 0.5f;
                float dy = y - (size - 1) * 0.5f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 220f ? Color.white : Color.clear);
            }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
