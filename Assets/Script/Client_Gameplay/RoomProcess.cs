//// 檔案: RoomProcess.cs
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using RPGFramework.Data;
//using RPGFramework.Kernel;

//namespace RPGFramework.Client
//{
//    public class RoomProcess : MonoBehaviour
//    {
//        [SerializeField] private RoomData roomData;
//        [SerializeField] private List<EventData> targetEvents;
//        [SerializeField] private TriggerType listenTrigger;

//        // 檔案: RoomProcess.cs (節錄修改處)
//        // 替換原本的 IEnumerator Start() 與相關註冊邏輯

//        private bool _registered = false;
//        private bool _triggering = false;

//        // ❌ 禁止在 Start 直接向 Kernel 註冊
//        private void Start()
//        {
//            // 不做任何事，等待手動啟動
//        }

//        public string GetRoomID()
//        {
//            return roomData != null ? roomData.roomID : "";
//        }

//        // ✅ 正確做法：由 ActionDriver / SceneTransitionSystem 在真正切房時手動呼叫
//        public void OnRoomActivated()
//        {
//            if (_registered) return;

//            if (roomData == null)
//            {
//                Debug.LogError($"[RoomProcess] {gameObject.name} 沒有 RoomData");
//                return;
//            }

//            if (SceneTransitionSystem.Instance != null)
//            {
//                SceneTransitionSystem.Instance.RegisterRoom(roomData.roomID, gameObject);
//                _registered = true;
//                Debug.Log($"[RoomProcess] 已由 Kernel 手動註冊並啟動：{roomData.roomID}");
//            }
//        }

//        private void OnTriggerEnter(Collider other)
//        {
//            if (!isActiveAndEnabled)
//                return;
//            if (other.CompareTag("Player") &&
//                listenTrigger == TriggerType.OnEnter)
//            {
//                TryTriggerEvents();
//            }
//        }

//        private void OnInteract()
//        {
//            if (listenTrigger == TriggerType.OnInteract)
//                TryTriggerEvents();
//        }

//        private void TryTriggerEvents()
//        {
//            if (_triggering)
//                return;

//            _triggering = true;

//            if (GameKernel.Instance == null)
//            {
//                _triggering = false;
//                return;
//            }

//            var world = GameKernel.Instance.World;

//            foreach (var evt in targetEvents)
//            {
//                if (evt == null)
//                    continue;

//                if (evt.allowedRooms.Count > 0 &&
//                    !evt.allowedRooms.Exists(
//                        r => r.roomID == world.GetRoom()))
//                {
//                    continue;
//                }

//                GameKernel.Instance.EventQueue.Enqueue(evt);
//            }

//            _triggering = false;
//        }
//    }
//}