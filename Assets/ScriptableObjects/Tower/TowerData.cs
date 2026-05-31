using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
}

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Book Entry")]
    [TextArea(3, 6)]
    public string description;

    
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float projectileSize;
    public float damage;

    public DamageType damageType;

    [Header("AOE Settings")]
    public bool isExplosive;
    public float explosionRadius;
    public GameObject explosionEffectPrefab; // 改由 Data 來持有特效 Prefab

    public int cost;
    public Sprite sprite;

    public GameObject prefab;
}
