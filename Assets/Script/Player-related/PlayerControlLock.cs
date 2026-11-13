using UnityEngine;

public class PlayerControlLock : MonoBehaviour
{
    private static PlayerControlLock instance;
    private PlayerController playerController;

    void Awake()
    {
        instance = this;
        // 自動抓取掛在同一個 Player 身上的移動腳本
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogWarning("[PlayerControlLock] 警告：這個物件身上找不到 PlayerController！請確保兩者掛在一起。");
        }
    }

    public static void LockPlayer(bool shouldLock)
    {
        if (instance == null || instance.playerController == null) return;

        if (shouldLock)
        {
            Debug.Log("[PlayerControlLock] 玩家操作已鎖定 🔒 (移動腳本已停用)");
            instance.playerController.enabled = false;
        }
        else
        {
            Debug.Log("[PlayerControlLock] 玩家操作已解鎖 🔓 (移動腳本已恢復)");
            instance.playerController.enabled = true;
        }
    }
}