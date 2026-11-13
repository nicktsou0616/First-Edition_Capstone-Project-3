using UnityEngine;
using Unity.Cinemachine; // Unity 6 的 Cinemachine 3.x 命名空間

[RequireComponent(typeof(CinemachineCamera))]
public class CameraTargetBinder : MonoBehaviour
{
    private CinemachineCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        // 訂閱 GameDirector 的玩家生成事件
        GameDirector.OnPlayerSpawned += BindTarget;

        // 防呆：如果腳本啟動時，場景裡已經有玩家了，就直接綁定
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            BindTarget(existingPlayer);
        }
    }

    private void OnDisable()
    {
        // 養成好習慣，物件關閉時取消訂閱，防止記憶體洩漏
        GameDirector.OnPlayerSpawned -= BindTarget;
    }

    private void BindTarget(GameObject playerObj)
    {
        if (vcam != null && playerObj != null)
        {
            // 將新生成的玩家設定為 Cinemachine 的跟隨目標
            vcam.Target.TrackingTarget = playerObj.transform;
            Debug.Log($"[CameraTargetBinder] 已成功將攝影機目標綁定至新生成的玩家：{playerObj.name}");
        }
    }
}