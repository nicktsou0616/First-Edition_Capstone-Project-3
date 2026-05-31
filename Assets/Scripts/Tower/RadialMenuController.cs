using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RadialMenuController : MonoBehaviour
{
    [Header("Data & Prefabs")]
    [SerializeField] private List<TowerData> availableTowers; // 系統性擴展點：直接把塔丟進這裡
    [SerializeField] private GameObject sectorPrefab;      // 使用你原本的 TowerCard 改製
    [SerializeField] private RectTransform selectionHighlight; // 圓盤中間的取消圓形或發光圈

    [Header("Settings")]
    public float radius = 150f;
    public float cancelDeadZone = 60f; // 中心圓半徑，滑鼠在此區域代表「取消」
    public float scaleUpMultiplier = 1.3f;

    private List<TowerCard> _spawnedCards = new List<TowerCard>();
    private int _selectedIndex = -1; // -1 代表取消
    

    private void Awake()
    {
        InitializeMenu();
        // gameObject.SetActive(false); // 初始隱藏
    }

    private void InitializeMenu()
    {
        // 系統性自動生成：根據清單數量平分圓周
        float angleStep = 360f / availableTowers.Count;

        for (int i = 0; i < availableTowers.Count; i++)
        {
            // 數學計算位置：0度在正上方
            float angle = i * angleStep;
            float radian = (angle + 90f) * Mathf.Deg2Rad; // 修正偏移量使第一座塔在正上方
            Vector3 pos = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0) * radius;

            GameObject go = Instantiate(sectorPrefab, transform);
            go.GetComponent<RectTransform>().anchoredPosition = pos;

            TowerCard card = go.GetComponent<TowerCard>();
            card.Initialize(availableTowers[i]);
            _spawnedCards.Add(card);
        }
    }

    private void Update()
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 menuScreenPos = RectTransformUtility.WorldToScreenPoint(null, transform.position);
    
        if (GetComponentInParent<Canvas>().renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // 如果是 Camera 模式，需要傳入 Camera
            menuScreenPos = RectTransformUtility.WorldToScreenPoint(GetComponentInParent<Canvas>().worldCamera, transform.position);
        }

        Vector2 direction = mousePos - menuScreenPos;
        float distance = direction.magnitude;

        // 這裡記得根據解析度縮放 cancelDeadZone，或是直接調整 Inspector 數值
        if (distance < cancelDeadZone)
        {
            _selectedIndex = -1;
        }
        else
        {
            // 計算角度：Atan2 回傳範圍是 -180 ~ 180
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 將角度轉換為 0 ~ 360，並修正起始偏移（使 0 度在正上方）
            // 由於 InitializeMenu 裡第一座塔在 (angle + 90f)，這裡也要同步
            float normalizedAngle = (angle + 360f) % 360f; 
            
            float angleStep = 360f / availableTowers.Count;
            
            // 修正索引計算邏輯：對齊 InitializeMenu 的分布
            // InitializeMenu 是逆時針增加角度，所以這裡用角度除以步長
            // 如果你的第一座塔在正上方 (90度)，我們需要補償這個偏移
            float offsetAngle = (normalizedAngle - 90f + 360f) % 360f;
            
            // 由於 Atan2 是逆時針增加，而你的分佈也是逆時針，索引直接計算即可
            _selectedIndex = Mathf.RoundToInt(offsetAngle / angleStep) % availableTowers.Count;
        }

        if (selectionHighlight != null)
        {
            // 當 _selectedIndex == -1 代表滑鼠在中心死區，縮放放大
            float centerTargetScale = (_selectedIndex == -1) ? scaleUpMultiplier : 1.0f;
            
            selectionHighlight.localScale = Vector3.Lerp(
                selectionHighlight.localScale, 
                Vector3.one * centerTargetScale, 
                Time.unscaledDeltaTime * 15f
            );
        }
        // 動態縮放效果 (保持不變)
        for (int i = 0; i < _spawnedCards.Count; i++)
        {
            float targetScale = (i == _selectedIndex) ? scaleUpMultiplier : 1.0f;
            _spawnedCards[i].transform.localScale = Vector3.Lerp(_spawnedCards[i].transform.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * 15f);
        }
    }

    public TowerData GetSelectedTower()
    {
        if (_selectedIndex == -1) return null;
        return availableTowers[_selectedIndex];
    }
}