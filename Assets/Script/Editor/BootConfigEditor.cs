// 檔案: BootConfigEditor.cs
using UnityEngine;
using UnityEditor;
using RPGFramework.Data;

namespace RPGFramework.EditorScripts
{
    [CustomEditor(typeof(BootConfig))]
    public class BootConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            BootConfig config = (BootConfig)target;

            EditorGUILayout.Space();
            GUI.color = config.bootMode == BootMode.Production ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.8f, 0.4f);
            EditorGUILayout.HelpBox($"當前啟動模式：{config.bootMode.ToString().ToUpper()}", MessageType.None);
            GUI.color = Color.white;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("bootMode"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("房間配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("productionStartRoom"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("testStartRoom"));

            // 防呆警告
            if (config.bootMode == BootMode.Test && config.testStartRoom == null)
            {
                EditorGUILayout.HelpBox("錯誤：測試模式未配置起始房間！", MessageType.Error);
            }
            if (config.bootMode == BootMode.Production && config.productionStartRoom == null)
            {
                EditorGUILayout.HelpBox("錯誤：正式模式未配置起始房間！", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}