//// 檔案: SceneTransitionSystem.cs
//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace RPGFramework.Kernel
//{
//    public class SceneTransitionSystem : MonoBehaviour
//    {
//        public bool IsReady { get; private set; }
//        public static SceneTransitionSystem Instance { get; private set; }

//        private Dictionary<string, GameObject> _registeredRooms = new();

//        private void Awake()
//        {
//            if (Instance != null && Instance != this)
//            {
//                Destroy(gameObject);
//                return;
//            }

//            Instance = this;
//        }

//        private void Start()
//        {
//            StartCoroutine(InitRoutine());
//        }

//        private IEnumerator InitRoutine()
//        {
//            yield return null;
//            yield return null;

//            IsReady = true;
//            Debug.Log($"[STS] Ready Rooms = {_registeredRooms.Count}");
//        }

//        public void RegisterRoom(string id, GameObject obj)
//        {
//            if (!_registeredRooms.ContainsKey(id))
//                _registeredRooms.Add(id, obj);
//        }

//        public Dictionary<string, GameObject> GetRegisteredRooms()
//            => _registeredRooms;

//        // 檔案: SceneTransitionSystem.cs (節錄替換處)

//        [Header("Scene Rooms (手動綁定所有房間)")]
//        [SerializeField] private List<RPGFramework.Client.RoomProcess> allRooms = new List<RPGFramework.Client.RoomProcess>();

//        // (保留原本的 RegisterRoom, IsReady, _registeredRooms 等架構不變...)

//        // =====================================================
//        // 🔥 場景初始化同步（核心修復點）
//        // =====================================================
//        public void ForceSceneSync(string startRoomID)
//        {
//            foreach (var r in allRooms)
//            {
//                if (r == null)
//                    continue;

//                bool isTarget = r.GetRoomID() == startRoomID;

//                r.gameObject.SetActive(isTarget);

//                if (isTarget)
//                {
//                    r.OnRoomActivated();
//                }
//            }
//        }

//        public void ProcessTeleport(
//            string targetRoomID,
//            bool useFade,
//            Action onDone)
//        {
//            StartCoroutine(Teleport(targetRoomID, onDone));
//        }

//        private IEnumerator Teleport(string targetRoomID, Action onDone)
//        {
//            yield return new WaitForSeconds(0.3f);

//            foreach (var r in allRooms)
//            {
//                if (r != null)
//                {
//                    bool isTarget = r.GetRoomID() == targetRoomID;
//                    r.gameObject.SetActive(isTarget);

//                    if (isTarget)
//                        r.OnRoomActivated(); // 手動註冊到 Registry
//                }
//            }
//            GameKernel.Instance.Syscall.SetCurrentRoom(targetRoomID);
//            yield return new WaitForSeconds(0.3f);

//            onDone?.Invoke();
//        }
//    }
//}