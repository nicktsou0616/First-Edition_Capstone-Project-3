using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected EnemyData data; 
    public EnemyData Data => data;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    protected Path _currentPath; 
    protected Vector3 _targetPosition; 
    protected int _currentWaypoint; 
    
    private float _lives;
    private float _maxLives;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;

    protected bool _hasBeenCounted = false; 
    protected int _currentGeneration = 0; 
    protected float _speedMultiplier = 1f;

    private Soldier _blockingSoldier; // 紀錄是哪個士兵擋住我
    private float _attackTimer;
    private bool _isAttacking = false; // 確保攻擊動畫執行時不會重疊
    
    public bool IsBlocked {get; private set;}

    // 提供 Getter 讓子類別呼叫
    public bool IsDead() => _hasBeenCounted;
    public Path GetCurrentPath() => _currentPath;

    public float GetDistanceProgress()
    {
        if (_currentPath == null) return 0f;
        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);
        return (_currentWaypoint * 1000f) - distanceToTarget; 
    }

    private void Awake()
    {
        _currentPath = GameObject.Find("Path1").GetComponent<Path>();
        _healthBarOriginalScale = healthBar.localScale;
    }

    public void Initialize(float healthMultiplier, int generation = 0, float speedMultiplier = 1f)
    {
        _currentGeneration = generation;
        _maxLives = data.lives * healthMultiplier;
        _lives = _maxLives;
        _hasBeenCounted = false;
        _speedMultiplier = speedMultiplier;

        float scale = Mathf.Pow(0.7f, _currentGeneration);
        transform.localScale = Vector3.one * scale;
        UpdateHealthBar();
    }

    protected virtual void OnEnable()
    {
        _currentWaypoint = 0;
        if (_currentPath != null)
            _targetPosition = _currentPath.GetPosition(_currentWaypoint);
    }

    protected virtual void Update()
    {
        if (_hasBeenCounted) return;

        if (IsBlocked && !_isAttacking)
        {
            HandleCombat();
        }
        else if (!_isAttacking) // 沒被擋且沒在攻擊動畫中才移動
        {
            float actualSpeed = data.speed * _speedMultiplier;
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, actualSpeed * Time.deltaTime);

            // ... 原有的路徑檢查邏輯 ...
            float relativeDistance = (transform.position - _targetPosition).magnitude;
            if (relativeDistance < 0.1f)
            {
                if (_currentWaypoint < _currentPath.Waypoints.Length - 1)
                {
                    _currentWaypoint++;
                    _targetPosition = _currentPath.GetPosition(_currentWaypoint);
                }
                else
                {
                    _hasBeenCounted = true;
                    OnEnemyReachedEnd?.Invoke(data);
                    gameObject.SetActive(false);
                }
            }
        }
    }

    private void HandleCombat()
    {
            // 確保士兵還活著且在場
        if (_blockingSoldier == null || !_blockingSoldier.gameObject.activeInHierarchy)
        {
            ResumeMoving();
            return;
        }

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0 && !_isAttacking)
        {
            StartCoroutine(AttackRoutine(_blockingSoldier));
            _attackTimer = data.attackInterval; // 之後的攻擊才進入正常冷卻
        }
    }

    public void StopMoving(Soldier soldier)
    {
        if (soldier == null) return; 
        if (IsBlocked) return; // 防止重複觸發
        
        IsBlocked = true;
        _blockingSoldier = soldier;
        
        // 關鍵修正：將第一擊的等待時間縮短 (例如 0.2 秒)
        // 這樣怪物一停下來就會立刻準備撞擊，而不是發呆等冷卻
        _attackTimer = 0.2f; 
        
        _speedMultiplier = 0f; 
        Debug.Log($"{gameObject.name} 被阻擋，準備攻擊士兵");
    }

    public void ResumeMoving()
    {
        IsBlocked = false;
        _blockingSoldier = null;
        _speedMultiplier = 1f;
    }

    private IEnumerator AttackRoutine(Soldier target)
    {
        _isAttacking = true;
        Vector3 originalPos = transform.position;

        // 取得衝撞時目標的最更新位置
        if (target == null) { _isAttacking = false; yield break; }
        Vector3 targetPos = target.transform.position;

        // 1. 快速向前撞擊
        float elapsed = 0f;
        while (elapsed < data.attackDashTime)
        {
            // 衝撞途中士兵消失了就中斷
            if (target == null || !target.gameObject.activeInHierarchy) break;
            
            elapsed += Time.deltaTime;
            // 計算平滑比例
            float percent = elapsed / data.attackDashTime;
            transform.position = Vector3.Lerp(originalPos, targetPos, percent);
            yield return null;
        }

        // 2. 造成傷害
        if (target != null && target.gameObject.activeInHierarchy)
        {
            target.TakeDamage(data.damage);
        }

        // 3. 快速回彈
        elapsed = 0f;
        Vector3 currentPos = transform.position;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / data.attackReturnTime;
            transform.position = Vector3.Lerp(currentPos, originalPos, percent);
            yield return null;
        }

        transform.position = originalPos;
        _isAttacking = false;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    public void TakeDamage(float damage, DamageType type)
    {
        float finalDamage = damage;
        if (type == DamageType.Physical) finalDamage = damage / data.def;
        else if (type == DamageType.Magical) finalDamage = damage / data.mdef;

        _lives -= finalDamage;
        _lives = Math.Max(_lives, 0);
        UpdateHealthBar();

        if (_lives <= 0 && !_hasBeenCounted)
        {
            Die();
        }
    }

    

    private void Die()
    {
        _hasBeenCounted = true;
        
        // 只有在 data 不為空時檢查分裂
        if (data != null && data.canSplit && _currentGeneration < data.maxSplitGeneration && data.splitPrefab != null)
        {
            SpawnMinions();
        }

        OnEnemyDestroyed?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected void SpawnMinions()
    {
        for (int i = 0; i < data.splitCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            GameObject minionGO = Instantiate(data.splitPrefab, spawnPos, Quaternion.identity);
            Enemy minion = minionGO.GetComponent<Enemy>();

            if (minion != null)
            {
                minion.SetSplitInfo(_currentPath, _currentWaypoint, _targetPosition);
                minion.Initialize(1.0f, _currentGeneration + 1);
            }
        }
    }

    public void SetSplitInfo(Path path, int waypoint, Vector3 target)
    {
        _currentPath = path;
        _currentWaypoint = waypoint;
        _targetPosition = target;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || _maxLives <= 0) return;
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }
}