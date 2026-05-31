using UnityEngine;
public class SlimeKing : Enemy
{
    [Header("King Settings")]
    [SerializeField] private bool canSummon = true; // 新增：是否具備召喚能力
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float firstSpawnDelay = 2f;
    
    private float _spawnTimer;


    protected override void OnEnable()
    {
        base.OnEnable();
        // 關鍵：初始計時器設為「首波延遲」
        _spawnTimer = firstSpawnDelay; 
    }


    protected override void Update()
    {
        base.Update();

        if (IsDead() || !canSummon) return; // 如果不是「王」，就跳過召喚邏輯

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            SpawnSingleMinion();
        }
    }

    private void SpawnSingleMinion()
    {
        if (minionPrefab == null) return;

        Vector3 spawnPos = transform.position;
        GameObject minionGO = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

        Enemy minion = minionGO.GetComponent<Enemy>();
        if (minion != null)
        {
            minion.SetSplitInfo(_currentPath, _currentWaypoint, _targetPosition);
            minion.Initialize(0.4f, 1, 0.4f);
        }
    }
}