using UnityEngine;

namespace RPGFramework.Data
{
    public enum BootMode
    {
        Production,
        Test
    }

    [CreateAssetMenu(fileName = "BootConfig", menuName = "RPG/BootConfig")]
    public class BootConfig : ScriptableObject
    {
        [Header("啟動模式")]
        public BootMode bootMode = BootMode.Production;

        [Header("正式模式起始房間")]
        public RoomData productionStartRoom;

        [Header("測試模式起始房間")]
        public RoomData testStartRoom;
    }
}