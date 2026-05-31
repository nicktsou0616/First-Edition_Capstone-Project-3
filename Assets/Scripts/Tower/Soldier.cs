using UnityEngine;
using System.Collections.Generic;

public class Soldier : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float attackInterval = 1f;
    public float damage = 10f;

    [Header("Detection & Blocking")]
    [Tooltip("士兵會自動跑去攔截怪物的偵測半徑")]
    public float detectionRange = 4f; 
    [Tooltip("多近才算攔截成功並讓怪物停下")]
    public float blockDistance = 1.2f;

    private Vector3 _targetPosition;
    private Enemy _blockedEnemy;
    private float _attackTimer;
    private SoldierTower _myTower;
    private SoldierTowerData _stats;
    
    [Header("UI")]
    [SerializeField] private Transform healthBar; // 拖入士兵血條的紅條 Transform
    private Vector3 _healthBarOriginalScale;
    private float _maxHealth; // 紀錄最大血量以計算比例

    private float _currentHealth;
    private bool _isDead = false;

    private float _moveSpeed;
    private float _attackInterval;

    [SerializeField] private ObjectPooler projectilePool;

    private void Awake()
    {
        if (healthBar != null)
        {
            _healthBarOriginalScale = healthBar.localScale;
        }
    }

    public void Initialize(SoldierTower tower, Vector3 startPos, SoldierTowerData data)
    {
        _myTower = tower;
        _targetPosition = startPos;
        transform.position = startPos;
        moveSpeed = data.soldierMoveSpeed;
        _stats = data;

        _attackInterval = data.shootInterval;
        _maxHealth = data.soldierMaxHealth;
        _currentHealth = _maxHealth;
        _isDead = false;
        gameObject.SetActive(true);

        UpdateHealthBar();
        gameObject.SetActive(true);
    }

    public void SetMoveTarget(Vector3 newTarget)
    {
        newTarget.z = 0;

        if (_myTower != null)
        {
            // 1. 取得塔的位置
            Vector3 towerPos = _myTower.transform.position;
            
            // 2. 宣告並計算距離 (修正 CS0103)
            float distanceToTower = Vector3.Distance(towerPos, newTarget);

            // 3. 限制在塔的 Range 內 (修正 CS1061)
            if (distanceToTower > _myTower.Range)
            {
                Vector3 direction = (newTarget - towerPos).normalized;
                newTarget = towerPos + (direction * _myTower.Range);
            }
        }

        ReleaseEnemy(); 
        _targetPosition = newTarget;
        Debug.Log($"{gameObject.name} 移動至限制範圍內座標: {newTarget}");
    }

    void Update()
    {
        // 1. 移動邏輯
        if (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
        }

        // 2. 戰鬥與阻擋邏輯
        HandleCombat();
    }

    private void HandleCombat()
    {
        _attackTimer -= Time.deltaTime;

        // 1. 尋找目標：優先處理目前阻擋中的，若無則找最近的
        if (_blockedEnemy != null)
        {
            float distToEnemy = Vector3.Distance(transform.position, _blockedEnemy.transform.position);

            // 如果怪死了、隱藏了，或者士兵跑太遠了
            if (!_blockedEnemy.gameObject.activeInHierarchy || distToEnemy > blockDistance + 0.5f)
            {
                ReleaseEnemy(); // 這會呼叫怪物 ResumeMoving 並清空 _blockedEnemy
            }
        }

        Enemy target = null;

        // 2. 阻擋行為
        if (_blockedEnemy == null)
        {
            target = GetClosestEnemy(true);
        }

        // 3. 執行阻擋
        if (target != null)
        {
            // 取得從怪物到士兵的方向
            Vector3 dirFromEnemyToSoldier = (transform.position - target.transform.position).normalized;
            
            // 如果士兵還沒開始阻擋，就移動到怪物的邊緣（距離 blockDistance 的位置）
            // 這樣士兵會停在怪物的面前，而不是怪物的中心
            _targetPosition = target.transform.position + (dirFromEnemyToSoldier * (blockDistance * 0.8f));

            // 檢查是否夠近可以觸發「阻擋」
            if (Vector3.Distance(transform.position, target.transform.position) <= blockDistance)
            {
                if (target.gameObject.activeInHierarchy)
                {
                _blockedEnemy = target;
                _blockedEnemy.StopMoving(this);
                
                // 進入阻擋狀態後，目標點就固定在當前位置，防止士兵在怪物身上抖動
                _targetPosition = transform.position;
                }
            }
        }

        // 4. 攻擊行為
        if (_attackTimer <= 0)
        {
            Enemy attackTarget = (_blockedEnemy != null) ? _blockedEnemy : GetClosestEnemy(false);
            if (attackTarget != null)
            {
                Attack(attackTarget);
                _attackTimer = attackInterval;
            }
        }

        
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.name} 受到傷害，剩餘血量: {_currentHealth}");
        _currentHealth = Mathf.Max(_currentHealth, 0);

        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || _maxHealth <=0) return;
        float healthPercent = _currentHealth / _maxHealth;

        Vector3 newScale = _healthBarOriginalScale;
        newScale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = newScale;
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("士兵陣亡！");
        ReleaseEnemy(); // 重要：死掉前先放開被擋住的怪物
        
        // 這裡暫時直接隱藏，建議後續加入回池 (ObjectPooler) 的邏輯
        gameObject.SetActive(false); 
    }

    private Enemy GetClosestEnemy(bool isForBlocking)
    {
        if (_myTower == null) return null;

        List<Enemy> enemies = _myTower.GetEnemiesInRange();
        Enemy closest = null;
        float minDistance = Mathf.Infinity;
        float maxCheckDist = isForBlocking ? detectionRange : _myTower.Range;

        foreach (Enemy e in enemies)
        {

            if (isForBlocking && e.IsBlocked) continue;
            
            float dist = Vector3.Distance(transform.position, e.transform.position);

            if (dist < minDistance && dist <= maxCheckDist)
            {
                minDistance = dist;
                closest = e;
            }
        }
        return closest;
    }

    private void Attack(Enemy target)
    {
        if (projectilePool == null || _stats == null) return;

        GameObject bulletGO = projectilePool.GetPooledObject();
        bulletGO.transform.position = transform.position;
        bulletGO.SetActive(true);
        
        Projectile projectile = bulletGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Debug.LogError($"{gameObject.name} 報錯：缺少 projectilePool (子彈池)！請檢查士兵 Prefab 上的 Pool 引用。");
            Vector3 direction = (target.transform.position - transform.position).normalized;
            // 傳入儲存好的 _stats
            projectile.Shoot(_stats, direction, target);
        }

        Debug.Log($"士兵攻擊: {target.name}");
        // target.TakeDamage(damage); 
    }

    private void ReleaseEnemy()
    {
        if (_blockedEnemy != null)
        {
            _blockedEnemy.ResumeMoving();
            _blockedEnemy = null;
        }
    }

    private void OnDrawGizmos()
    {
        // 繪製自動偵測怪物的範圍 (綠色)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 繪製實際產生阻擋判定的範圍 (紅色)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blockDistance);
    }

    private void OnDestroy()
    {
        ReleaseEnemy();
    }
}