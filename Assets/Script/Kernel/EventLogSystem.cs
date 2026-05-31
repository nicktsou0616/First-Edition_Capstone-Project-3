// 檔案: EventLogSystem.cs
using System.Collections.Generic;
using UnityEngine;

namespace RPGFramework.Kernel
{
    /// <summary>
    /// 獨立的事件歷史紀錄系統 (非 Kernel 核心元件)
    /// 專注於純資料紀錄，不參與 Syscall 與排程邏輯
    /// </summary>
    public class EventLogSystem : MonoBehaviour
    {
        public static EventLogSystem Instance { get; private set; }

        // 依據規格使用 List<string>
        private List<string> _completedEvents = new List<string>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 紀錄已完成的事件
        /// </summary>
        public void LogEvent(string eventID)
        {
            if (!string.IsNullOrEmpty(eventID) && !_completedEvents.Contains(eventID))
            {
                _completedEvents.Add(eventID);
                Debug.Log($"[EventLogSystem] 成功紀錄事件: {eventID}");
            }
        }

        /// <summary>
        /// 檢查事件是否已完成
        /// </summary>
        public bool HasEvent(string eventID)
        {
            if (string.IsNullOrEmpty(eventID)) return false;
            return _completedEvents.Contains(eventID);
        }

        /// <summary>
        /// 清空所有事件紀錄 (可用於讀檔或重置遊戲)
        /// </summary>
        public void ClearLog()
        {
            _completedEvents.Clear();
            Debug.Log("[EventLogSystem] 已清空所有事件紀錄。");
        }
    }
}