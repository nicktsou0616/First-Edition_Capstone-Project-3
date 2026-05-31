using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    private CircleCollider2D _circleCollider;

    [SerializeField] private float clickRadius = 0.5f;
    private TowerRangeVisualizer _rangeVisualizer;

    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;

    private float _shootTimer;
    private Enemy _target; // 目前鎖定的目標

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        _shootTimer = data.shootInterval;

        _rangeVisualizer = GetComponent<TowerRangeVisualizer>();

        if (_rangeVisualizer == null)
        {
            _rangeVisualizer = gameObject.AddComponent<TowerRangeVisualizer>();
        }

        _rangeVisualizer.SetRange(data.range);
    }

    private void Update()
    {
        UpdateTarget(); // 確保每一幀都有正確的目標

        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            _shootTimer = data.shootInterval;
            
            // 只有在有目標且目標還在範圍內時才發射
            if (_target != null)
            {
                Shoot();
            }
        }
    }

    private void OnMouseDown()
    {
        if (Platform.towerPanelOpen) return;
        // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distanceToTower = Vector2.Distance(mouseWorldPos, transform.position);

        if (distanceToTower > clickRadius) return;

        if (_rangeVisualizer != null)
        {
            _rangeVisualizer.Toggle();
        }
    }

    private void UpdateTarget()
    {
        // 1. 清理列表中已消失或失效的敵人
        _enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

        // 2. 如果目前目標已經死亡、離開範圍或失效，則清除目標
        if (_target != null)
        {
            float distance = Vector2.Distance(transform.position, _target.transform.position);
            if (!_target.gameObject.activeInHierarchy || distance > data.range)
            {
                _target = null;
            }
        }

        // 3. 如果目前沒有鎖定目標，從列表中找「第一個」進入範圍的人
        if (_enemiesInRange.Count > 0)
        {
            Enemy leadingEnemy = null;
            float maxProgress = float.MinValue;

            foreach (Enemy enemy in _enemiesInRange)
            {
                // 這裡會呼叫剛才在 Enemy.cs 加入的函式
                float currentProgress = enemy.GetDistanceProgress();
                
                if (currentProgress > maxProgress)
                {
                    maxProgress = currentProgress;
                    leadingEnemy = enemy;
                }
            }

            // 將目標鎖定為目前跑在最前面的人
            _target = leadingEnemy;
        }
        else
        {
            _target = null;
        }
    }

    private void Shoot()
    {
        // 從物件池取得子彈
        GameObject projectileGO = _projectilePool.GetPooledObject();
        if (projectileGO != null)
        {
            projectileGO.transform.position = transform.position;
            projectileGO.SetActive(true);

            // 計算初始發射方向
            Vector2 shootDirection = (_target.transform.position - transform.position).normalized;

            // 呼叫修改後的 Projectile.Shoot，傳入目標以進行追蹤
            Projectile projectileScript = projectileGO.GetComponent<Projectile>();
            projectileScript.Shoot(data, shootDirection, _target);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
                
                // 如果離開的人剛好是目前鎖定的目標，清除它
                if (_target == enemy)
                {
                    _target = null;
                }
            }
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
        
        if (_target == enemy)
        {
            _target = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (data != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, data.range);
            
            // Debug：畫一條線連到目前目標
            if (_target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _target.transform.position);
            }
        }
    }
}