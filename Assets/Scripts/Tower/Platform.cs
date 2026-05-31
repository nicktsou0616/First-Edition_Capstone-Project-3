using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Platform : MonoBehaviour
{
    // 事件保留給其他系統監聽（例如成就或音效）
    public static event Action<Platform> OnPlatformClicked;
    [SerializeField] private LayerMask platformLayerMask;

    // 狀態統一由 UIController 控制最安全，但保留此靜態變數方便外部判斷
    public static bool towerPanelOpen { get; set; } = false;

    private static Platform _currentSelectedPlatform;
    private GameObject _placedTower;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        OnPlatformClicked = null;
        towerPanelOpen = false;
        _currentSelectedPlatform = null;
    }

    private void OnEnable() => OnPlatformClicked += SetAsSelected;
    private void OnDisable() => OnPlatformClicked -= SetAsSelected;

    private void SetAsSelected(Platform platform)
    {
        _currentSelectedPlatform = platform;
    }

    private void Update()
    {
        // 1. 如果面板開著，處理「釋放右鍵」
        if (towerPanelOpen)
        {
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                ConfirmBuilding();
            }
            return; 
        }

        // 2. 排除 UI 點擊干擾
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
            return;

        // 3. 偵測「按下右鍵」開啟選單
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandlePlatformClicked();
        }
    }

    private void HandlePlatformClicked()
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        LayerMask clickMask = platformLayerMask.value != 0 ? platformLayerMask : 1 << gameObject.layer;
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint, clickMask);

        if (hitCollider != null)
        {
            Platform platform = hitCollider.GetComponent<Platform>();
            if (platform != null && platform == this)
            {
                // 觸發事件並設為選中
                OnPlatformClicked?.Invoke(platform);
                
                // 開啟環形選單
                UIController.Instance.ShowRadialMenu(Mouse.current.position.ReadValue(), platform);
            }
        }
    }

    private void ConfirmBuilding()
    {
        // 獲取目前滑鼠指向的塔 (由 RadialMenuController 計算角度)
        TowerData selectedData = UIController.Instance.GetRadialSelectedTower();

        if (selectedData != null)
        {
            // 讓 UI 控制器處理後續：檢查錢 -> 蓋塔 -> 關面板
            UIController.Instance.TryBuildTower(selectedData);
        }
        else
        {
            // 如果滑鼠在中心死區釋放，取消建造
            CloseTowerPanel();
        }
    }

    public void PlaceTower(TowerData data)
    {
        // 蓋塔前的最後防線：檢查是否已有塔
        if (_placedTower != null)
        {
            Debug.LogWarning($"{gameObject.name} 上已經有建築了！");
            return;
        }

        Vector3 towerPosition = transform.position;
        towerPosition.z -= 0.1f;

        _placedTower = Instantiate(data.prefab, towerPosition, Quaternion.identity);
    }

    public void CloseTowerPanel()
    {
        // 呼叫 UIController 統一執行關閉流程
        UIController.Instance.HideTowerPanel();
    }
}
