using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;

    private static Spawner _instance;
    public static int TotalWaves => _instance != null ? _instance.WaveCount : 1;

    [SerializeField] private WaveData[] waves;
#if UNITY_EDITOR
    [SerializeField] private string waveDataFolder = "Assets/ScriptableObjects/Wave";
#endif

    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private WaveData CurrentWave => HasWaves ? waves[_currentWaveIndex] : null;
    private int WaveCount => Mathf.Max(1, waves != null ? waves.Length : 0);
    private bool HasWaves => waves != null && waves.Length > 0;
    private float[] _spawnTimers;
    private int[] _spawnCounters;

    private int _enemiesRemoved;

    [SerializeField] private ObjectPooler slime1Pool;
    [SerializeField] private ObjectPooler dragonPool;
    [SerializeField] private ObjectPooler slime2Pool;
    [SerializeField] private ObjectPooler slime3Pool;
    [SerializeField] private ObjectPooler slime_kPool;

    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;

    private float _timeBetweenWaves = 1f;
    private float _waveCooldown;
    private bool _isBetweenWaves = false;
    private bool _allWavesCompleted = false;

    [Header("Start Settings")]
    [SerializeField] private bool _hasGameStarted = false;

    private void Awake()
    {
#if UNITY_EDITOR
        SyncWavesFromAssets();
#endif
        _instance = this;
        ClampWaveIndex();

        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            { EnemyType.Slime1, slime1Pool },
            { EnemyType.Dragon, dragonPool },
            { EnemyType.Slime2, slime2Pool },
            { EnemyType.Slime3, slime3Pool },
            { EnemyType.Slime_k, slime_kPool }
        };
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

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        SyncWaveCounter();
        OnWaveChanged?.Invoke(_currentWaveIndex);
        _isBetweenWaves = false;
    }

    public void StartFirstWave()
    {
        if (_hasGameStarted || !HasWaves) return;

        _hasGameStarted = true;
        _allWavesCompleted = false;
        ClampWaveIndex();
        SyncWaveCounter();
        PrepareWaveSpawningState();
        OnWaveChanged?.Invoke(_currentWaveIndex);
    }

    private void Update()
    {
        if (!_hasGameStarted || _allWavesCompleted || !HasWaves) return;

        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f)
            {
                PrepareNextWave();
            }

            return;
        }

        WaveData currentWave = CurrentWave;
        if (currentWave == null || currentWave.enemies == null || currentWave.enemies.Count == 0) return;

        bool allEnemiesSpawned = true;

        for (int i = 0; i < currentWave.enemies.Count; i++)
        {
            WaveEnemySpawnData enemySpawnData = currentWave.enemies[i];
            if (enemySpawnData == null) continue;

            int enemiesThisGroup = Mathf.Max(0, enemySpawnData.enemiesPerWave);

            if (_spawnCounters[i] >= enemiesThisGroup) continue;

            allEnemiesSpawned = false;
            _spawnTimers[i] -= Time.deltaTime;

            if (_spawnTimers[i] <= 0f)
            {
                SpawnEnemy(enemySpawnData.enemyType);
                _spawnCounters[i]++;
                _spawnTimers[i] = Mathf.Max(0.01f, enemySpawnData.spawnInterval);
            }
        }

        if (allEnemiesSpawned && IsFieldClear())
        {
            if (IsLastWave())
            {
                CompleteAllWaves();
            }
            else
            {
                _isBetweenWaves = true;
                _waveCooldown = _timeBetweenWaves;
            }
        }
    }

    private bool IsFieldClear()
    {
        Enemy[] remainingEnemies = FindObjectsOfType<Enemy>();

        int activeCount = 0;
        foreach (var e in remainingEnemies)
        {
            if (e.gameObject.activeInHierarchy)
            {
                activeCount++;
            }
        }

        return activeCount == 0;
    }

    private void SpawnEnemy(EnemyType enemyType)
    {
        if (!_poolDictionary.TryGetValue(enemyType, out var pool) || pool == null)
        {
            Debug.LogWarning($"No object pool assigned for enemy type: {enemyType}");
            return;
        }

        GameObject spawnedObject = pool.GetPooledObject();
        if (spawnedObject == null) return;

        spawnedObject.transform.position = transform.position;

        float healthMultiplier = 1f + (_waveCounter * 0.1f);
        Enemy enemy = spawnedObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Initialize(healthMultiplier, 0, 1f);
            spawnedObject.SetActive(true);
        }
    }

    private void PrepareNextWave()
    {
        if (!HasWaves || IsLastWave())
        {
            CompleteAllWaves();
            return;
        }

        _currentWaveIndex++;
        SyncWaveCounter();
        PrepareWaveSpawningState();
        OnWaveChanged?.Invoke(_currentWaveIndex);
        _isBetweenWaves = false;
    }

    public void SkipToNextWave()
    {
        if (!_hasGameStarted || _allWavesCompleted || !HasWaves) return;

        if (IsLastWave())
        {
            CompleteAllWaves();
            return;
        }

        _isBetweenWaves = false;
        _waveCooldown = 0f;

        PrepareNextWave();
    }

    private void PrepareWaveSpawningState()
    {
        WaveData currentWave = CurrentWave;
        int enemyGroupCount = currentWave != null && currentWave.enemies != null ? currentWave.enemies.Count : 0;

        _spawnTimers = new float[enemyGroupCount];
        _spawnCounters = new int[enemyGroupCount];

        float accumulatedDelay = 0f;

        for (int i = 0; i < enemyGroupCount; i++)
        {
            WaveEnemySpawnData enemySpawnData = currentWave.enemies[i];

            if (enemySpawnData != null)
            {
                accumulatedDelay += Mathf.Max(0f, enemySpawnData.delayAfterPreviousElement);
            }

            _spawnTimers[i] = accumulatedDelay;
            _spawnCounters[i] = 0;
        }
    }

    private bool IsLastWave()
    {
        return _currentWaveIndex >= WaveCount - 1;
    }

    private void CompleteAllWaves()
    {
        _allWavesCompleted = true;
        _isBetweenWaves = false;
        _currentWaveIndex = Mathf.Max(0, WaveCount - 1);
        SyncWaveCounter();
        OnWaveChanged?.Invoke(_currentWaveIndex);
        if (GameManager.Instance != null)
        {
            GameResultState.SetResultByLives(GameManager.Instance.Lives);
            BattleReportController.ShowReport(
                GameManager.Instance.Lives,
                BattleReportController.CountBuildings(),
                GameManager.Instance.Resources
            );
        }
        Debug.Log("All waves completed.");
    }

    private void ClampWaveIndex()
    {
        _currentWaveIndex = Mathf.Clamp(_currentWaveIndex, 0, WaveCount - 1);
    }

    private void SyncWaveCounter()
    {
        _waveCounter = _currentWaveIndex;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        SyncWavesFromAssets();
#endif
        if (_instance == this)
        {
            ClampWaveIndex();
            OnWaveChanged?.Invoke(_currentWaveIndex);
        }
    }

#if UNITY_EDITOR
    private void SyncWavesFromAssets()
    {
        if (string.IsNullOrEmpty(waveDataFolder)) return;
        if (!AssetDatabase.IsValidFolder(waveDataFolder)) return;

        string[] guids = AssetDatabase.FindAssets("t:WaveData", new[] { waveDataFolder });
        Array.Sort(guids, CompareWaveAssetNames);

        List<WaveData> syncedWaves = new List<WaveData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WaveData wave = AssetDatabase.LoadAssetAtPath<WaveData>(path);
            if (wave != null)
            {
                syncedWaves.Add(wave);
            }
        }

        WaveData[] syncedWaveArray = syncedWaves.ToArray();
        if (!AreWaveArraysEqual(waves, syncedWaveArray))
        {
            waves = syncedWaveArray;
        }

        ClampWaveIndex();
    }

    private static bool AreWaveArraysEqual(WaveData[] current, WaveData[] next)
    {
        if (current == null || next == null) return current == next;
        if (current.Length != next.Length) return false;

        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != next[i]) return false;
        }

        return true;
    }

    private static int CompareWaveAssetNames(string leftGuid, string rightGuid)
    {
        string leftName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(leftGuid));
        string rightName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(rightGuid));
        string leftPrefix = GetNamePrefix(leftName);
        string rightPrefix = GetNamePrefix(rightName);
        int prefixCompare = string.Compare(leftPrefix, rightPrefix, StringComparison.OrdinalIgnoreCase);
        if (prefixCompare != 0) return prefixCompare;

        if (TryGetTrailingNumber(leftName, out int leftNumber) && TryGetTrailingNumber(rightName, out int rightNumber))
        {
            int numberCompare = leftNumber.CompareTo(rightNumber);
            if (numberCompare != 0) return numberCompare;
        }

        return string.Compare(leftName, rightName, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetNamePrefix(string value)
    {
        int index = value.Length - 1;
        while (index >= 0 && char.IsDigit(value[index]))
        {
            index--;
        }

        return value.Substring(0, index + 1);
    }

    private static bool TryGetTrailingNumber(string value, out int number)
    {
        int index = value.Length - 1;
        while (index >= 0 && char.IsDigit(value[index]))
        {
            index--;
        }

        if (index == value.Length - 1)
        {
            number = 0;
            return false;
        }

        return int.TryParse(value.Substring(index + 1), out number);
    }
#endif

    private void HandleEnemyReachedEnd(EnemyData data) {}
    private void HandleEnemyDestroyed(Enemy enemy) {}
}
