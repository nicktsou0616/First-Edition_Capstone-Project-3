using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("出生點設定")]
    [Tooltip("這個出生點的唯一 ID，例如 'Door_Left'")]
    public string spawnID;

    [Tooltip("如果直接 Play 這個場景(沒有前置切換)，是否預設把玩家生在這裡？")]
    public bool isDefault = false;

    // 繪製輔助線圈，讓企劃在 Unity 編輯器裡能清楚看到出生點在哪
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
    }
}