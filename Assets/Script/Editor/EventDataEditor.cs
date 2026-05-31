// 檔案: EventDataEditor.cs
using UnityEngine;
using UnityEditor;
using RPGFramework.Data;

namespace RPGFramework.EditorScripts
{
    [CustomEditor(typeof(EventData))]
    public class EventDataEditor : Editor
    {
        private bool showScope = true;
        private bool showConditions = true;
        private bool showActions = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("基礎設定", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventID"), new GUIContent("事件 ID", "唯一識別碼"));

            SerializedProperty reqProp = serializedObject.FindProperty("requirement");
            reqProp.enumValueIndex = EditorGUILayout.Popup("事件必要性", reqProp.enumValueIndex, new string[] { "必須 (Required)", "可選 (Optional)", "跳過 (Skip)" });

            // Enum 中文化選單
            SerializedProperty triggerTypeProp = serializedObject.FindProperty("triggerType");
            triggerTypeProp.enumValueIndex = EditorGUILayout.Popup("觸發方式", triggerTypeProp.enumValueIndex, new string[] { "進入 (OnEnter)", "互動 (OnInteract)", "計時 (OnTimer)" });

            SerializedProperty executionProp = serializedObject.FindProperty("executionBehavior");
            executionProp.enumValueIndex = EditorGUILayout.Popup("執行行為", executionProp.enumValueIndex, new string[] { "只執行一次 (RunOnce)", "可重複 (Repeatable)" });

            EditorGUILayout.Space();

            showScope = EditorGUILayout.Foldout(showScope, "權限與作用域", true, EditorStyles.foldoutHeader);
            if (showScope)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowedChapters"), new GUIContent("允許章節"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowedRooms"), new GUIContent("允許房間 (RoomData)"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reusableAcrossChapters"), new GUIContent("跨章節重複"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showConditions = EditorGUILayout.Foldout(showConditions, "條件與阻塞", true, EditorStyles.foldoutHeader);
            if (showConditions)
            {
                EditorGUI.indentLevel++;
                SerializedProperty conditionLogicProp = serializedObject.FindProperty("conditionLogic");
                conditionLogicProp.enumValueIndex = EditorGUILayout.Popup("條件邏輯", conditionLogicProp.enumValueIndex, new string[] { "全部 (All)", "任意 (Any)" });

                EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredFlags"), new GUIContent("必須旗標 (FlagData)"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("waitFlags"), new GUIContent("等待旗標 (FlagData)"), true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showActions = EditorGUILayout.Foldout(showActions, "動作載荷", true, EditorStyles.foldoutHeader);
            if (showActions)
            {
                EditorGUI.indentLevel++;
                SerializedProperty modifyVis = serializedObject.FindProperty("modifyPlayerVisibility");
                EditorGUILayout.PropertyField(modifyVis, new GUIContent("玩家可見性變更"));
                if (modifyVis.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("targetVisibility"), new GUIContent("目標可見性"));
                }

                SerializedProperty lockPlayer = serializedObject.FindProperty("lockPlayer");
                EditorGUILayout.PropertyField(lockPlayer, new GUIContent("鎖定玩家"));
                if (lockPlayer.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("playerLockState"), new GUIContent("玩家鎖定狀態"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("setFlagOnComplete"), new GUIContent("完成後旗標 (FlagData)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("advanceSegmentTo"), new GUIContent("前進章節 (-1表示不變)"));

                SerializedProperty teleportProp = serializedObject.FindProperty("teleportTargetRoomID");
                EditorGUILayout.PropertyField(teleportProp, new GUIContent("傳送目標房間 (RoomData)"));

                if (teleportProp.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hideOldRoomAfterTeleport"), new GUIContent("傳送後隱藏舊房間"));
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
            // 檔案: EventDataEditor.cs (節錄修改處)

            // ... 上面是你原本的欄位繪製邏輯 ...

            // ▼ 新增：把 Data-Driven Action Pipeline 畫出來 ▼
            SerializedProperty actionsProp = serializedObject.FindProperty("actions");
            if (actionsProp != null)
            {
                EditorGUILayout.Space();
                // 傳入 true 是關鍵，這會告訴 Unity 展開 List 並為每個元素套用 Drawer
                EditorGUILayout.PropertyField(actionsProp, new GUIContent("模組化動作清單 (Action Pipeline)"), true);
            }

            // 這行原本就存在，一定要在它之前呼叫
            serializedObject.ApplyModifiedProperties();
        }
    }
}