using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;
    public static event Action OnGameOver; // 新增：遊戲結束事件


    private int _lives = 5;
    private int _resources = 175;
    public int Lives => _lives;
    public int Resources => _resources;

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;

    private bool _isGameOver = false; // 確保遊戲結束邏輯只執行一次

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resources);
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        if (_isGameOver) return; // 如果已經結束了就不再處理

        // 加上 Null 檢查，如果 data 是空的就預設扣 1 點血
        int damage = (data != null) ? data.damage : 1;
        
        _lives = Mathf.Max(0, _lives - 1);
        OnLivesChanged?.Invoke(_lives);

        // 檢查是否遊戲結束
        if (_lives <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        
        Debug.Log("<color=red>Game Over!</color>");
        GameResultState.SetResultByLives(_lives);
        BattleReportController.ShowReport(_lives, BattleReportController.CountBuildings(), _resources);
        OnGameOver?.Invoke(); // 通知 UIController 或其他系統顯示結算畫面
        
        // 通常遊戲結束會稍微卡住或暫停
        // SetTimeScale(0.1f); 
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddResources(Mathf.RoundToInt(enemy.Data.resourceReward));
    }

    private void AddResources(int amount)
    {
        _resources += amount;
        OnResourcesChanged?.Invoke(_resources);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void SetGameSpeed(float speed)
    {
        _gameSpeed = Mathf.Clamp(speed, 1f, 3f);
        SetTimeScale(_gameSpeed);
    }

    public void ApplyGameSpeed()
    {
        SetTimeScale(_gameSpeed);
    }


    public void SpendResources(int amount)
    {
        if (_resources >= amount)
        {

            _resources -= amount;
            OnResourcesChanged?.Invoke(_resources);
        }
    }
}
