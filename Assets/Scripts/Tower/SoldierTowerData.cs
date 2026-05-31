using UnityEngine;

[CreateAssetMenu(fileName = "NewSoldierTowerData", menuName = "Towers/Soldier Tower Data")]
public class SoldierTowerData : TowerData // 繼承原本的 TowerData
{

    [Header("Soldier Settings")]
    public GameObject soldierPrefab;
    
    [Header("Soldier Stats")]
    public int maxSoldiers = 3;
    public float spawnInterval = 10f;
    public float soldierMoveSpeed = 3f;
    public float soldierDamage = 10f;
    public float soldierMaxHealth = 50f;
}