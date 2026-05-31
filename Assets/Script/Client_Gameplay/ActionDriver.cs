//// 檔案: ActionDriver.cs
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Playables;
//using RPGFramework.Data;

//namespace RPGFramework.Kernel
//{
//    [System.Serializable]
//    public struct SceneModuleMapping
//    {
//        public string moduleID;
//        public GameObject targetObject;
//        public PlayableDirector timeline;
//    }

//    public class ActionDriver : MonoBehaviour
//    {
//        [SerializeField] private SceneTransitionSystem transitionSystem;
//        [SerializeField] private UIActionBridge uiBridge;

//        [Header("場景物件映射表 (ID -> Scene Object)")]
//        [SerializeField] private List<SceneModuleMapping> sceneModules = new List<SceneModuleMapping>();

//        [Header("Data-Driven Action Pipeline")]
//        [SerializeField]
//        private List<EventActionBinding> sceneEventBindings = new List<EventActionBinding>();

//        // 檔案: ActionDriver.cs (節錄修改處)

//        public void ExecuteEvent(EventData evt)
//        {
//            Debug.Log($"[ActionDriver] 執行事件: {evt.eventID}");
//            var syscall = GameKernel.Instance.Syscall;

//            if (evt.actions != null)
//            {
//                foreach (var action in evt.actions) ExecuteAction(action, syscall);
//            }

//            EventActionBinding binding = sceneEventBindings.Find(b => b.triggerEvent == evt);
//            if (binding != null && binding.actions != null)
//            {
//                foreach (var action in binding.actions) ExecuteAction(action, syscall);
//            }

//            ExecuteLegacyPayload(evt, syscall);

//            // ▼ 唯一修改處：移出 Syscall，改交由獨立的 EventLogSystem 紀錄 ▼
//            if (!string.IsNullOrEmpty(evt.eventID))
//            {
//                if (EventLogSystem.Instance != null)
//                {
//                    EventLogSystem.Instance.LogEvent(evt.eventID);
//                }
//                else
//                {
//                    Debug.LogWarning("[ActionDriver] 找不到 EventLogSystem 實例，無法紀錄事件！");
//                }
//            }
//        }

//        private void ExecuteAction(ActionCommand cmd, GameKernel.InternalSyscall syscall)
//        {
//            switch (cmd.actionType)
//            {
//                case ActionType.SetPlayerVisibility:
//                    syscall.SetPlayerVisibility(cmd.boolValue);
//                    break;
//                case ActionType.SetPlayerLock:
//                    syscall.SetPlayerLock(cmd.boolValue);
//                    break;
//                case ActionType.SetWorldFlag:
//                    if (!string.IsNullOrEmpty(cmd.flagID))
//                    {
//                        // 🏆 核心保護：因為不能改 GameKernel，所以我們動態實例化一個暫時的 FlagData 來餵給原有的 Syscall
//                        FlagData tempFlag = ScriptableObject.CreateInstance<FlagData>();
//                        tempFlag.flagName = cmd.flagID;
//                        syscall.SetFlag(tempFlag);
//                        Destroy(tempFlag); // 送出後銷毀
//                    }
//                    break;
//                case ActionType.AdvanceSegment:
//                    syscall.AdvanceSegment(cmd.intValue);
//                    break;
//                case ActionType.TeleportToRoom:
//                    if (!string.IsNullOrEmpty(cmd.roomID))
//                    {
//                        syscall.SetPlayerLock(true);
//                        transitionSystem.ProcessTeleport(cmd.roomID, cmd.boolValue, () => syscall.SetPlayerLock(false));
//                    }
//                    break;

//                // ▼ 路由給 UI 橋接層 (純 ID 驅動) ▼
//                case ActionType.TriggerDialogue:
//                    if (uiBridge != null) uiBridge.ShowDialogue(cmd.targetModuleID);
//                    break;
//                case ActionType.TriggerPopup:
//                    if (uiBridge != null) uiBridge.ShowPopup(cmd.stringValue);
//                    break;

//                // ▼ 場景模組查表驅動 ▼
//                case ActionType.SetGameObjectActive:
//                    var objMap = GetModuleMapping(cmd.targetModuleID);
//                    if (objMap.targetObject != null) objMap.targetObject.SetActive(cmd.boolValue);
//                    break;
//                case ActionType.PlayTimeline:
//                    var timeMap = GetModuleMapping(cmd.targetModuleID);
//                    if (timeMap.timeline != null) timeMap.timeline.Play();
//                    break;
//                case ActionType.TriggerDoor:
//                    var doorMap = GetModuleMapping(cmd.targetModuleID);
//                    if (doorMap.targetObject != null)
//                    {
//                        doorMap.targetObject.SendMessage("Execute", SendMessageOptions.DontRequireReceiver);
//                        doorMap.targetObject.SendMessage("Trigger", SendMessageOptions.DontRequireReceiver);
//                    }
//                    break;
//            }
//        }

//        private SceneModuleMapping GetModuleMapping(string id)
//        {
//            if (string.IsNullOrEmpty(id)) return default;
//            foreach (var map in sceneModules)
//                if (map.moduleID == id) return map;
//            return default;
//        }

//        private void ExecuteLegacyPayload(EventData evt, GameKernel.InternalSyscall syscall)
//        {
//            if (evt.modifyPlayerVisibility) syscall.SetPlayerVisibility(evt.targetVisibility);
//            if (evt.setFlagOnComplete != null) syscall.SetFlag(evt.setFlagOnComplete);
//            if (evt.advanceSegmentTo != -1) syscall.AdvanceSegment(evt.advanceSegmentTo);
//            if (evt.teleportTargetRoomID != null)
//            {
//                syscall.SetPlayerLock(true);
//                transitionSystem.ProcessTeleport(evt.teleportTargetRoomID.roomID, evt.hideOldRoomAfterTeleport, () => syscall.SetPlayerLock(false));
//            }
//        }
//    }
//}