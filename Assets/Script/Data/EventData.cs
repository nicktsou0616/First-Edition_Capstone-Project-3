// 檔案: EventData.cs
using System.Collections.Generic;
using UnityEngine;
using RPGFramework.Kernel; // 引入 ActionCommand 所在命名空間

namespace RPGFramework.Data
{
    public enum TriggerType { OnEnter, OnInteract, OnTimer }
    public enum ExecutionBehavior { RunOnce, Repeatable }
    public enum ConditionLogic { All, Any }

    // ▼ 新增：需求 A 的純資料標籤 Enum ▼
    public enum EventRequirement { Required, Optional, Skip }

    [CreateAssetMenu(fileName = "NewEventData", menuName = "RPG/EventData")]
    public class EventData : ScriptableObject
    {
        public string eventID;

        // ▼ 新增：需求 A 的純資料標籤欄位 ▼
        public EventRequirement requirement;

        public TriggerType triggerType;
        public ExecutionBehavior executionBehavior;

        public List<int> allowedChapters = new List<int>();
        public List<RoomData> allowedRooms = new List<RoomData>();
        public bool reusableAcrossChapters;

        public ConditionLogic conditionLogic;
        public List<FlagData> requiredFlags = new List<FlagData>();

        public List<FlagData> waitFlags = new List<FlagData>();

        // ▼ 舊版欄位 (Legacy Actions) - 完全保留不可刪除 ▼
        public bool modifyPlayerVisibility;
        public bool targetVisibility;

        public bool lockPlayer;
        public bool playerLockState;

        public FlagData setFlagOnComplete;
        public int advanceSegmentTo = -1;

        public RoomData teleportTargetRoomID;
        public bool hideOldRoomAfterTeleport;

        // ▼ 新增：Data-Driven Action Pipeline ▼
        [Header("Data-Driven Action Pipeline")]
        public List<ActionCommand> actions = new List<ActionCommand>();
    }
}