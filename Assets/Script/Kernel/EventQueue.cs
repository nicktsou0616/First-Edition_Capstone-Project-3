// 檔案: EventQueue.cs
using System.Collections.Generic;
using RPGFramework.Data;

namespace RPGFramework.Kernel
{
    public class EventQueue
    {
        private Queue<EventData> _readyQueue = new Queue<EventData>();
        private List<EventData> _blockedQueue = new List<EventData>();
        private WorldState _worldState;

        public void Initialize(WorldState state) => _worldState = state;

        public void Enqueue(EventData evt)
        {
            if (IsBlocked(evt)) _blockedQueue.Add(evt);
            else _readyQueue.Enqueue(evt);
        }

        public void UpdateBlockedQueue()
        {
            for (int i = _blockedQueue.Count - 1; i >= 0; i--)
            {
                if (!IsBlocked(_blockedQueue[i]))
                {
                    _readyQueue.Enqueue(_blockedQueue[i]);
                    _blockedQueue.RemoveAt(i);
                }
            }
        }

        public bool HasReadyEvent() => _readyQueue.Count > 0;
        public EventData DequeueReadyEvent() => _readyQueue.Dequeue();

        // 供 Scheduler 讀取 WorldState
        public WorldState World => _worldState;

        // 檔案: EventQueue.cs (節錄 IsBlocked 方法的修改)
        // 請將原本的 IsBlocked 方法替換為以下內容：
        private bool IsBlocked(EventData evt)
        {
            int currentChap = _worldState.GetSegment();
            string currentRoom = _worldState.GetRoom();

            if (evt.allowedChapters.Count > 0 && !evt.allowedChapters.Contains(currentChap)) return true;

            if (evt.allowedRooms.Count > 0)
            {
                bool roomMatch = false;
                foreach (var r in evt.allowedRooms)
                {
                    if (r != null && r.roomID == currentRoom) { roomMatch = true; break; }
                }
                if (!roomMatch) return true;
            }

            foreach (var flagObj in evt.waitFlags)
            {
                if (flagObj != null && !_worldState.HasFlag(flagObj.flagName)) return true;
            }

            return false;
        }
    }
}