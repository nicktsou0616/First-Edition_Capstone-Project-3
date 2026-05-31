//// 檔案: PlayerProcessEditor.cs
//using UnityEngine;
//using UnityEditor;
//using RPGFramework.Client;
//using RPGFramework.Kernel;

//namespace RPGFramework.EditorScripts
//{
//    [CustomEditor(typeof(PlayerProcess))]
//    public class PlayerProcessEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("動態狀態監控 (Control System)", EditorStyles.boldLabel);

//            if (Application.isPlaying && GameKernel.Instance != null)
//            {
//                var control = GameKernel.Instance.Control;

//                // 顯示當前狀態 (唯讀)
//                EditorGUI.BeginDisabledGroup(true);
//                EditorGUILayout.Toggle("Is Player Locked", control.IsPlayerLocked);
//                EditorGUILayout.Toggle("Is Player Visible", control.IsPlayerVisible);
//                EditorGUILayout.TextField("Player Mode", control.PlayerMode);
//                EditorGUI.EndDisabledGroup();

//                EditorGUILayout.Space();
//                EditorGUILayout.LabelField("模擬系統呼叫 (InternalSyscall 請求)", EditorStyles.boldLabel);

//                GUILayout.BeginHorizontal();
//                if (GUILayout.Button(control.IsPlayerLocked ? "Unlock Player" : "Lock Player"))
//                {
//                    GameKernel.Instance.Syscall.SetPlayerLock(!control.IsPlayerLocked);
//                }

//                if (GUILayout.Button(control.IsPlayerVisible ? "Hide Player" : "Show Player"))
//                {
//                    GameKernel.Instance.Syscall.SetPlayerVisibility(!control.IsPlayerVisible);
//                }
//                GUILayout.EndHorizontal();

//                if (GUILayout.Button("Reset Mode to 'Exploration'"))
//                {
//                    GameKernel.Instance.Syscall.SetPlayerMode("Exploration");
//                }
//            }
//            else
//            {
//                EditorGUILayout.HelpBox("Play Mode 啟動後，將在此顯示並控制 ControlSystem 的即時狀態。", MessageType.Info);
//            }
//        }
//    }
//}