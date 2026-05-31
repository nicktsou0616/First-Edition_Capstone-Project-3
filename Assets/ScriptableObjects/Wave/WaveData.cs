using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    public List<WaveEnemySpawnData> enemies = new List<WaveEnemySpawnData>();

    [SerializeField, HideInInspector, FormerlySerializedAs("enemyType")]
    private EnemyType legacyEnemyType;

    [SerializeField, HideInInspector, FormerlySerializedAs("spawnInterval")]
    private float legacySpawnInterval;

    [SerializeField, HideInInspector, FormerlySerializedAs("enemiesPerWave")]
    private int legacyEnemiesPerWave;

    [SerializeField, HideInInspector]
    private bool migratedFromSingleEnemy;

    private void OnValidate()
    {
        if (migratedFromSingleEnemy || enemies.Count > 0) return;

        enemies.Add(new WaveEnemySpawnData
        {
            enemyType = legacyEnemyType,
            spawnInterval = legacySpawnInterval > 0f ? legacySpawnInterval : 1f,
            enemiesPerWave = Mathf.Max(0, legacyEnemiesPerWave)
        });

        migratedFromSingleEnemy = true;
    }
}

[Serializable]
public class WaveEnemySpawnData
{
    public EnemyType enemyType;
    public float delayAfterPreviousElement = 0f;
    public float spawnInterval = 1f;
    public int enemiesPerWave = 1;
}