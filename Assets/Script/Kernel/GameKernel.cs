//// 檔案: GameKernel.cs (包含更新的 InternalSyscall)
//using UnityEngine;
//using RPGFramework.Data;

//namespace RPGFramework.Kernel
//{
//    public class GameKernel : MonoBehaviour
//    {
//        public static GameKernel Instance { get; private set; }

//        public WorldState World { get; private set; }
//        public ControlSystem Control { get; private set; }
//        public EventQueue EventQueue { get; private set; }
//        public Scheduler Scheduler { get; private set; }
//        public InternalSyscall Syscall { get; private set; }

//        [SerializeField] private ActionDriver actionDriver;

//        private void Awake()
//        {
//            if (Instance != null) { Destroy(gameObject); return; }
//            Instance = this;
//            DontDestroyOnLoad(gameObject);

//            World = new WorldState();
//            Control = new ControlSystem();
//            EventQueue = new EventQueue();
//            Scheduler = new Scheduler();

//            EventQueue.Initialize(World);
//            Scheduler.Initialize(EventQueue, actionDriver);

//            Syscall = new InternalSyscall(World, Control);
//        }

//        private void Update() => Scheduler.Tick();

//        public class InternalSyscall
//        {
//            private WorldState _world;
//            private ControlSystem _control;

//            public InternalSyscall(WorldState world, ControlSystem control)
//            {
//                _world = world;
//                _control = control;
//            }

//            public void SetFlag(FlagData flag) { if (flag != null) _world.InternalSetFlag(flag.flagName); }
//            public void AdvanceSegment(int segment) => _world.InternalAdvanceSegment(segment);
//            public void SetCurrentRoom(string roomID) => _world.InternalSetRoom(roomID);

//            public void SetPlayerLock(bool isLocked) => _control.InternalSetLock(isLocked);
//            public void SetPlayerVisibility(bool isVisible) => _control.InternalSetVisibility(isVisible);
//            public void SetPlayerMode(string mode) => _control.InternalSetMode(mode);
//        }
//    }
//}