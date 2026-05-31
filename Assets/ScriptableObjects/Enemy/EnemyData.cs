using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Book Entry")]
    [TextArea(3, 6)]
    public string description;

    public float lives;
    public int damage;
    public float speed;
    public float def;
    public float mdef;
    public float attackInterval = 1.5f; // 新增：攻擊間隔（秒）
    public float attackDashTime = 0.1f;   // 衝撞所需時間 (愈小愈快)
    public float attackReturnTime = 0.2f; // 回彈所需時間

    [Header("Split Settings")]
    public bool canSplit;          // 是否會分裂
    public GameObject splitPrefab; // 分裂產生的 Prefab
    public int splitCount = 3;     // 一次分裂出幾隻
    public int maxSplitGeneration = 2; // 最多分裂幾代 (0:母體, 1:子體...)


    public float resourceReward;
}
