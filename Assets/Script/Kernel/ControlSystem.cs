// 檔案: ControlSystem.cs
namespace RPGFramework.Kernel
{
    /// <summary>
    /// 單一控制系統，負責玩家鎖定與顯示狀態
    /// </summary>
    public class ControlSystem
    {
        public bool IsPlayerLocked { get; private set; }
        public bool IsPlayerVisible { get; private set; } = true;
        public string PlayerMode { get; private set; } = "Exploration";

        // 以下方法僅供 InternalSyscall 呼叫
        internal void InternalSetLock(bool isLocked) => IsPlayerLocked = isLocked;
        internal void InternalSetVisibility(bool isVisible) => IsPlayerVisible = isVisible;
        internal void InternalSetMode(string mode) => PlayerMode = mode;
    }
}