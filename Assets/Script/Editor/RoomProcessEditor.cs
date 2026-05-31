//using UnityEngine;
//using UnityEditor;
//using RPGFramework.Client;
//using System.Reflection;

//namespace RPGFramework.EditorScripts
//{
//    [CustomEditor(typeof(RoomProcess))]
//    public class RoomProcessEditor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

//            if (!Application.isPlaying)
//            {
//                EditorGUILayout.HelpBox("Play Mode 才能測試", MessageType.Info);
//                return;
//            }

//            RoomProcess room = (RoomProcess)target;

//            if (GUILayout.Button("Trigger Events"))
//            {
//                // ❌ 不直接呼叫 OnInteract（避免不存在）
//                var method = typeof(RoomProcess).GetMethod(
//                    "TryTriggerEvents",
//                    BindingFlags.NonPublic | BindingFlags.Instance
//                );

//                method?.Invoke(room, null);
//            }
//        }
//    }
//}