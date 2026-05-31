// 檔案: WorldState.cs
using System.Collections.Generic;

namespace RPGFramework.Kernel
{
    /// <summary>
    /// 唯讀狀態核心。受保護的資料區段。
    /// </summary>
    public class WorldState
    {
        private HashSet<string> _activeFlags = new HashSet<string>();
        private int _currentSegment = 0; // 對應 Chapter
        private string _currentRoomID = "";

        public bool HasFlag(string flag) => _activeFlags.Contains(flag);
        public int GetSegment() => _currentSegment;
        public string GetRoom() => _currentRoomID;

        // 原子操作區域 (僅限 Syscall)
        internal void InternalSetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) _activeFlags.Add(flag);
        }

        internal void InternalAdvanceSegment(int newSegment)
        {
            if (newSegment > _currentSegment) _currentSegment = newSegment;
        }

        internal void InternalSetRoom(string roomID)
        {
            _currentRoomID = roomID;
        }
    }
}