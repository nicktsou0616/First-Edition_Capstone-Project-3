// 檔案: RoomData.cs
using UnityEngine;

namespace RPGFramework.Data
{
    [CreateAssetMenu(fileName = "NewRoom", menuName = "RPG/RoomData")]
    public class RoomData : ScriptableObject
    {
        [Tooltip("用於內部辨識的唯一房間/場景 ID")]
        public string roomID;

        [Tooltip("勾選表示這是遊戲開始的房間，啟動時會自動 SetActive(true)")]
        public bool isStartRoom = false; // ✅ 新增初始房間欄位

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(roomID))
            {
                roomID = this.name; // 預設使用檔案名稱作為 ID
            }
        }
    }
}