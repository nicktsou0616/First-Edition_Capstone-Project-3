//using UnityEngine;
//using UnityEditor;
//using RPGFramework.Kernel;

//namespace RPGFramework.EditorScripts
//{
//    [CustomEditor(typeof(SceneTransitionSystem))]
//    public class SceneTransitionSystemEditor : Editor
//    {
//        private string debugTeleportID = "";

//        public override void OnInspectorGUI()
//        {
//            SceneTransitionSystem transition = (SceneTransitionSystem)target;

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Runtime Room Registry", EditorStyles.boldLabel);

//            if (!Application.isPlaying)
//            {
//                EditorGUILayout.HelpBox("進入 Play Mode 才能看到房間", MessageType.Info);
//                return;
//            }

//            var registry = transition.GetRegisteredRooms();

//            foreach (var kvp in registry)
//            {
//                GUILayout.BeginHorizontal("box");

//                GUILayout.Label(kvp.Key);

//                GUILayout.Label(kvp.Value != null && kvp.Value.activeSelf ? "Active" : "Hidden");

//                GUILayout.EndHorizontal();
//            }

//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("Test Tools", EditorStyles.boldLabel);

//            if (GUILayout.Button("一鍵關閉所有房間"))
//            {
//                // ❌ 不再呼叫 HideAllRooms（避免錯誤）
//                foreach (var kvp in registry)
//                {
//                    if (kvp.Value != null)
//                        kvp.Value.SetActive(false);
//                }
//            }

//            EditorGUILayout.Space();
//            debugTeleportID = EditorGUILayout.TextField("Room ID", debugTeleportID);

//            if (GUILayout.Button("Teleport"))
//            {
//                if (registry.ContainsKey(debugTeleportID))
//                    transition.ProcessTeleport(debugTeleportID, true, null);
//                else
//                    Debug.LogError("Room not found: " + debugTeleportID);
//            }
//        }
//    }
//}