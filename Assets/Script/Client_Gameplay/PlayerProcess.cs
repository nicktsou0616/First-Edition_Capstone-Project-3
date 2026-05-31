//// 檔案: PlayerProcess.cs
//using UnityEngine;
//using RPGFramework.Kernel;

//namespace RPGFramework.Client
//{
//    /// <summary>
//    /// 玩家本體行為，只能請求 Syscall，不可直接修改 WorldState
//    /// </summary>
//    public class PlayerProcess : MonoBehaviour
//    {
//        private void Update()
//        {
//            // 如果被鎖定，禁止移動與互動
//            if (GameKernel.Instance.Control.IsPlayerLocked) return;

//            HandleMovement();
//            HandleInteraction();
//            HandleStateRequestExample();
//        }

//        private void HandleMovement()
//        {
//            // 實作一般移動邏輯
//        }

//        private void HandleInteraction()
//        {
//            if (Input.GetKeyDown(KeyCode.E))
//            {
//                // 這裡示範呼叫前方的 RoomProcess 進行互動
//                Debug.Log("[PlayerProcess] 嘗試互動...");
//            }
//        }

//        private void HandleStateRequestExample()
//        {
//            if (Input.GetKeyDown(KeyCode.H))
//            {
//                // 玩家發出請求：隱藏自己 (合法寫入路徑)
//                bool currentVisibility = GameKernel.Instance.Control.IsPlayerVisible;
//                GameKernel.Instance.Syscall.SetPlayerVisibility(!currentVisibility);
//                Debug.Log($"[PlayerProcess] 請求切換顯示狀態至: {!currentVisibility}");
//            }
//        }
//    }
//}