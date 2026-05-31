// 檔案: ActionCommand.cs
using System;
using System.Collections.Generic;
using RPGFramework.Data;

namespace RPGFramework.Kernel
{
    public enum ActionType
    {
        SetPlayerVisibility,
        SetPlayerLock,
        SetWorldFlag,
        AdvanceSegment,
        TeleportToRoom,

        SetGameObjectActive,
        PlayTimeline,
        TriggerDialogue,
        TriggerPopup,
        TriggerDoor,
        SwitchToNewModule
    }

    [System.Serializable]
    public class ActionCommand
    {
        public ActionType actionType;

        public bool boolValue;
        public int intValue;

        // ▼ 全部改為 String ID 驅動 (無任何 Unity Reference) ▼
        public string stringValue;
        public string targetModuleID;
        public string roomID;
        public string flagID;
    }

    [Serializable]
    public class EventActionBinding
    {
        public EventData triggerEvent;
        public List<ActionCommand> actions = new List<ActionCommand>();
    }
}