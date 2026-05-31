// 檔案: RoomDataEditor.cs (必須放在 Editor 資料夾)
using UnityEngine;
using UnityEditor;
using RPGFramework.Data;

namespace RPGFramework.EditorScripts
{
    [CustomEditor(typeof(RoomData))]
    public class RoomDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime 辨識資料 (Pure Metadata Asset)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roomID"), new GUIContent("房間 ID (roomID)"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}