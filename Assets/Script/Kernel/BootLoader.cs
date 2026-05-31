//// 檔案: BootLoader.cs
//using System.Collections;
//using UnityEngine;
//using RPGFramework.Data;

//namespace RPGFramework.Kernel
//{
//    public enum BootState { Uninitialized, Initializing, Ready, Error }

//    public class BootLoader : MonoBehaviour
//    {
//        public static BootLoader Instance { get; private set; }

//        [SerializeField] private BootConfig bootConfig;

//        public BootState CurrentState { get; private set; } = BootState.Uninitialized;
//        public string TargetRoomID { get; private set; } = "";

//        private void Awake()
//        {
//            if (Instance != null) { Destroy(gameObject); return; }
//            Instance = this;
//        }

//        private IEnumerator Start()
//        {
//            yield return new WaitUntil(() =>
//                GameKernel.Instance != null &&
//                SceneTransitionSystem.Instance != null &&
//                SceneTransitionSystem.Instance.IsReady
//            );

//            if (SceneTransitionSystem.Instance.GetRegisteredRooms().Count == 0)
//            {
//                Debug.LogError("[BootLoader] 沒有房間註冊");
//                CurrentState = BootState.Error;
//                yield break;
//            }

//            if (bootConfig == null)
//            {
//                Debug.LogError("[BootLoader] 沒 BootConfig");
//                CurrentState = BootState.Error;
//                yield break;
//            }

//            TargetRoomID =
//                bootConfig.bootMode == BootMode.Production
//                ? bootConfig.productionStartRoom.roomID
//                : bootConfig.testStartRoom.roomID;

//            // 🔥 新增：場景初始化同步（解決你現在 UI/Active 混亂核心）
//            SceneTransitionSystem.Instance.ForceSceneSync(TargetRoomID);

//            SceneTransitionSystem.Instance.ProcessTeleport(TargetRoomID, false, () =>
//            {
//                CurrentState = BootState.Ready;
//                Debug.Log("[BootLoader] 啟動完成");
//            });
//        }
//    }
//}