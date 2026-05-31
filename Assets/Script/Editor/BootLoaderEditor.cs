//// 檔案: BootLoaderEditor.cs
//using UnityEngine;
//using UnityEditor;
//using RPGFramework.Kernel;

//namespace RPGFramework.EditorScripts
//{
//    [CustomEditor(typeof(BootLoader))]
//    public class BootLoaderEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();

//            BootLoader loader = (BootLoader)target;

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Runtime 啟動監控 (Boot State)", EditorStyles.boldLabel);

//            EditorGUI.BeginDisabledGroup(true);
//            EditorGUILayout.EnumPopup("當前狀態", loader.CurrentState);
//            EditorGUILayout.TextField("目標鎖定房間", loader.TargetRoomID);
//            EditorGUI.EndDisabledGroup();

//            if (Application.isPlaying)
//            {
//                if (loader.CurrentState == BootState.Ready)
//                    EditorGUILayout.HelpBox("✅ 系統已初始化完畢，Runtime 正常運行中。", MessageType.Info);
//                else if (loader.CurrentState == BootState.Error)
//                    EditorGUILayout.HelpBox("❌ 啟動失敗，請檢查 Console 錯誤訊息。", MessageType.Error);
//                else
//                    EditorGUILayout.HelpBox("⏳ 正在初始化核心管線...", MessageType.Warning);
//            }
//            else
//            {
//                EditorGUILayout.HelpBox("Play Mode 啟動後將在此監控 Pipeline 流程。", MessageType.Info);
//            }
//        }
//    }
//}