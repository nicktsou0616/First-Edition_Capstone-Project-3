//// 檔案: Scheduler.cs
//using System.Collections.Generic;
//using RPGFramework.Data;

//namespace RPGFramework.Kernel
//{
//    public class Scheduler
//    {
//        private EventQueue _queue;
//        private ActionDriver _actionDriver;
//        private HashSet<string> _executedRunOnceEvents = new HashSet<string>();

//        public void Initialize(EventQueue queue, ActionDriver driver)
//        {
//            _queue = queue;
//            _actionDriver = driver;
//        }

//        public void Tick()
//        {
//            _queue.UpdateBlockedQueue();

//            while (_queue.HasReadyEvent())
//            {
//                EventData evt = _queue.DequeueReadyEvent();

//                if (evt.executionBehavior == ExecutionBehavior.RunOnce)
//                {
//                    // 根據 reusableAcrossChapters 決定 RunOnce 的作用域金鑰 (Global vs Chapter-Specific)
//                    string executionKey = evt.reusableAcrossChapters
//                        ? $"{evt.eventID}_chap_{_queue.World.GetSegment()}"
//                        : evt.eventID;

//                    if (_executedRunOnceEvents.Contains(executionKey)) continue;
//                    _executedRunOnceEvents.Add(executionKey);
//                }

//                _actionDriver.ExecuteEvent(evt);
//            }
//        }
//    }
//}