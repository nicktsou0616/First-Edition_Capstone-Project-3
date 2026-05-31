using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SoldierTower : MonoBehaviour
{
    [Header("Soldier Settings")]
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private int maxSoldiers = 3;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float range = 7f;

    [Header("Data Reference")]
    [SerializeField] private SoldierTowerData towerData;

    [Header("Pool Reference")]
    [SerializeField] private ObjectPooler soldierPool;

    [Header("UI & Visuals")]
    [SerializeField] private RallyPointVisual rallyPointPrefab; // 直接存 Prefab 的腳本類型
    private RallyPointVisual _rallyPointInstance; // 存產出來的實體

    [SerializeField] private float clickRadius = 0.5f;
    private CircleCollider2D _clickCollider;

    private List<Soldier> _activeSoldiers = new List<Soldier>();
    private float _spawnTimer;
    public float Range => range;
    private TowerRangeVisualizer _rangeVisualizer;

    private void Start()
    {
        ApplyData();
        _clickCollider = GetComponent<CircleCollider2D>();

        if (_clickCollider == null)
        {
            _clickCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        _clickCollider.isTrigger = true;
        _clickCollider.radius = clickRadius;

        _rangeVisualizer = GetComponent<TowerRangeVisualizer>();
        _spawnTimer = 2f;

        if (_rangeVisualizer == null)
        {
            _rangeVisualizer = gameObject.AddComponent<TowerRangeVisualizer>();
        }

        _rangeVisualizer.SetRange(range);

        _spawnTimer = 2f;

        if (rallyPointPrefab != null)
        {
            _rallyPointInstance = Instantiate(rallyPointPrefab, transform.position, Quaternion.identity);
            _rallyPointInstance.Hide();
        }
    }

    private void ApplyData()
    {
        if (towerData != null)
        {
            range = towerData.range;
            maxSoldiers = towerData.maxSoldiers;
            spawnInterval = towerData.spawnInterval;
        }
    }
    
    void Update()
    {

        _activeSoldiers.RemoveAll(s => s == null || !s.gameObject.activeInHierarchy);

        // 1. 定期生成士兵
        if (_activeSoldiers.Count < maxSoldiers)
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0)
            {
                SpawnSoldierFromPool();
                _spawnTimer = spawnInterval;
            }
        }

        // 2. 玩家操控 (點擊地面移動所有士兵到該位置)
        if (Input.GetMouseButtonDown(2)) // 建議改用特定按鍵或 UI 切換指令模式
        {
            if (Platform.towerPanelOpen || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (IsClickingThisTower())
            {
                return;
            }

            HandlePlayerCommand();
        }
    }

    private bool IsClickingThisTower()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        return hit != null && hit.transform.IsChildOf(transform);
    }

    private void OnMouseDown()
    {
        if (Platform.towerPanelOpen) return;
     // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (_rangeVisualizer != null)
        {
            _rangeVisualizer.Toggle();
        }
    }

    private void SpawnSoldierFromPool()
    {
        if (soldierPool == null) return;

        // 從物件池拿一個士兵
        GameObject go = soldierPool.GetPooledObject();
        
        Vector3 spawnOffset = (Vector3)Random.insideUnitCircle * 1.5f;
        Vector3 spawnPos = transform.position + spawnOffset;

        go.transform.position = spawnPos;
        go.transform.rotation = Quaternion.identity;

        Soldier s = go.GetComponent<Soldier>();
        if (s != null)
        {
            s.Initialize(this, spawnPos, towerData);
            _activeSoldiers.Add(s);
        }

        go.SetActive(true); // 激活士兵
    }

    private void HandlePlayerCommand()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        if (_rallyPointInstance != null)
        {
            _rallyPointInstance.ShowAt(mouseWorldPos);
        }
        // 檢查點擊位置是否在塔的移動範圍（range）內
        foreach (var soldier in _activeSoldiers)
        {
            if (soldier != null && soldier.gameObject.activeInHierarchy)
            {
                // 讓士兵稍微分散
                Vector3 offset = (Vector3)Random.insideUnitCircle * 1f;
                
                // 呼叫士兵移動。注意：士兵內部的 SetMoveTarget 已經有寫 Vector3.Distance 限制，
                // 所以如果滑鼠點太遠，士兵只會走到半徑邊緣，但旗幟會留在滑鼠點擊處。
                soldier.SetMoveTarget(mouseWorldPos + offset);
            }
        }
    }

    // 提供給士兵獲取範圍內敵人的列表
    public List<Enemy> GetEnemiesInRange()
    {
        List<Enemy> inRange = new List<Enemy>();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (var e in allEnemies)
        {
            if (e.gameObject.activeInHierarchy && Vector3.Distance(transform.position, e.transform.position) <= range)
            {
                inRange.Add(e);
            }
        }
        return inRange;
    }

    // 畫出範圍 (Gizmos) 方便在 Editor 查看
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}