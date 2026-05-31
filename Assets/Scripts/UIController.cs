using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Radial Menu System")]
    [SerializeField] private RadialMenuController radialMenu;
    // 這裡我們統一使用一個變數來控制狀態
    public bool IsPanelOpen { get; private set; } = false;

    [Header("Status HUD")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private GameObject NoResourcesText;

    private Platform _currentPlatform;

    private void Start()
    {
        HideTowerPanel();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
    }

    // --- HUD 更新 ---
    private void UpdateWaveText(int currentWave)
    {
        int totalWaves = Mathf.Max(1, Spawner.TotalWaves);
        int displayedWave = Mathf.Clamp(currentWave + 1, 1, totalWaves);
        waveText.text = $"{displayedWave}/{totalWaves}";
    }
    private void UpdateLivesText(int currentLives) => livesText.text = $"{currentLives}";
    private void UpdateResourcesText(int currentResources) => resourcesText.text = $"{currentResources}";

    // --- 環形選單核心邏輯 ---

    public void ShowRadialMenu(Vector2 screenPos, Platform platform)
    {
        if (radialMenu == null) return;

        _currentPlatform = platform;
    
        // 1. 先啟動物件，否則 RectTransform 可能無法正確計算
        radialMenu.gameObject.SetActive(true);

        // 2. 修正座標賦值邏輯
        // 如果你的 Canvas 是 Screen Space - Overlay，使用 anchoredPosition 是最保險的
        // 我們假設 radialMenu 的父物件就是 Canvas，且中心點 (Pivot) 是 (0.5, 0.5)
        RectTransform rt = radialMenu.GetComponent<RectTransform>();
        
        // 將螢幕座標轉換為 Canvas 內的局部座標
        Canvas rootCanvas = radialMenu.GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            Vector2 localPos;
            RectTransform canvasRT = rootCanvas.GetComponent<RectTransform>();
            
            // 進行螢幕座標轉換
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, 
                screenPos, 
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera, 
                out localPos
            );
            
            rt.anchoredPosition = localPos;
        }
        else
        {
            // 如果真的找不到 Canvas，就先用最簡單的方式
            rt.position = screenPos;
            Debug.LogWarning("找不到 Canvas，座標轉換可能不準確");
        }

        IsPanelOpen = true;
        Platform.towerPanelOpen = true; 
        GameManager.Instance.SetTimeScale(0.1f);
    }

    public void HideTowerPanel()
    {
        if (radialMenu != null)
        {
            radialMenu.gameObject.SetActive(false);
        }

        IsPanelOpen = false;
        Platform.towerPanelOpen = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyGameSpeed();
        }
    }

    public TowerData GetRadialSelectedTower()
    {
        return radialMenu != null ? radialMenu.GetSelectedTower() : null;
    }

    public void TryBuildTower(TowerData towerData)
    {
        if (towerData == null || _currentPlatform == null) return;

        if (GameManager.Instance.Resources >= towerData.cost)
        {
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
            HideTowerPanel();
        }
        else
        {
            // 資源不足，我們關閉面板並顯示警告
            HideTowerPanel();
            StartCoroutine(ShowNoResourcesMessage());
        }
    }

    // 當從環形選單確認建造時呼叫
    private void HandleTowerSelected(TowerData towerData)
    {
        if (towerData == null || _currentPlatform == null) return;

        if (GameManager.Instance.Resources >= towerData.cost)
        {
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
        }
        else
        {
            StartCoroutine(ShowNoResourcesMessage());
        }

        HideTowerPanel();
    }

    private IEnumerator ShowNoResourcesMessage()
    {
        NoResourcesText.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        NoResourcesText.SetActive(false);
    }
}
