//using System.Collections.Generic;
//using UnityEngine;

//namespace RPGFramework.Client
//{
//    public class ModuleBootstrapper : MonoBehaviour
//    {
//        [Header("All Room Modules")]
//        [SerializeField] private List<GameObject> roomModules = new();

//        [Header("All Dialogue Modules")]
//        [SerializeField] private List<GameObject> dialogueModules = new();

//        [Header("Initial Room")]
//        [SerializeField] private GameObject initialRoom;

//        private bool _initialized;

//        private void Awake()
//        {
//            if (_initialized) return;

//            InitializeModules();
//            _initialized = true;
//        }

//        private void InitializeModules()
//        {
//            if (initialRoom == null)
//            {
//                Debug.LogError("[ModuleBootstrapper] Initial Room is NULL.");
//                enabled = false;
//                return;
//            }

//            DisableAllRooms();
//            DisableAllDialogues();
//            ActivateInitialRoom();

//            Debug.Log("[ModuleBootstrapper] Bootstrap Completed.");
//        }

//        private void DisableAllRooms()
//        {
//            foreach (var room in roomModules)
//            {
//                if (room == null) continue;
//                room.SetActive(false);
//            }
//        }

//        private void DisableAllDialogues()
//        {
//            foreach (var dialogue in dialogueModules)
//            {
//                if (dialogue == null) continue;
//                dialogue.SetActive(false);
//            }
//        }

//        private void ActivateInitialRoom()
//        {
//            initialRoom.SetActive(true);

//            var rp = initialRoom.GetComponentInChildren<RPGFramework.Client.RoomProcess>();

//            if (rp == null)
//            {
//                Debug.LogError("[Bootstrapper] initialRoom 找不到 RoomProcess");
//                return;
//            }

//            rp.OnRoomActivated();
//        }
//    }
//}