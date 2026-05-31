//// 檔案: ActionCommandDrawer.cs (必須放在 Editor 資料夾)
//using UnityEngine;
//using UnityEditor;
//using RPGFramework.Kernel;

//namespace RPGFramework.EditorScripts
//{
//    [CustomPropertyDrawer(typeof(ActionCommand))]
//    public class ActionCommandDrawer : PropertyDrawer
//    {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            EditorGUI.BeginProperty(position, label, property);

//            GUI.Box(new Rect(position.x - 2, position.y - 2, position.width + 4, position.height + 4), GUIContent.none, EditorStyles.helpBox);
//            position.x += 2; position.y += 2; position.width -= 4;

//            SerializedProperty actionType = property.FindPropertyRelative("actionType");

//            Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
//            EditorGUI.PropertyField(typeRect, actionType, GUIContent.none);

//            Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
//            ActionType type = (ActionType)actionType.enumValueIndex;

//            // 全部轉換為 String ID 輸入框
//            switch (type)
//            {
//                case ActionType.SetPlayerVisibility:
//                case ActionType.SetPlayerLock:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("boolValue"), new GUIContent("目標狀態 (State)"));
//                    break;
//                case ActionType.SetWorldFlag:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("flagID"), new GUIContent("旗標 ID (flagID)"));
//                    break;
//                case ActionType.AdvanceSegment:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("intValue"), new GUIContent("前往章節 (-1無效)"));
//                    break;
//                case ActionType.TeleportToRoom:
//                    float half = fieldRect.width / 2;
//                    Rect leftRoom = new Rect(fieldRect.x, fieldRect.y, half - 2, fieldRect.height);
//                    Rect rightHide = new Rect(fieldRect.x + half + 2, fieldRect.y, half - 2, fieldRect.height);
//                    EditorGUI.PropertyField(leftRoom, property.FindPropertyRelative("roomID"), new GUIContent("房間 ID"));
//                    EditorGUI.PropertyField(rightHide, property.FindPropertyRelative("boolValue"), new GUIContent("隱藏舊房"));
//                    break;
//                case ActionType.SetGameObjectActive:
//                    float leftObjWidth = fieldRect.width * 0.7f;
//                    Rect leftObj = new Rect(fieldRect.x, fieldRect.y, leftObjWidth - 2, fieldRect.height);
//                    Rect rightActive = new Rect(fieldRect.x + leftObjWidth + 2, fieldRect.y, fieldRect.width * 0.3f - 2, fieldRect.height);
//                    EditorGUI.PropertyField(leftObj, property.FindPropertyRelative("targetModuleID"), new GUIContent("物件 ID"));
//                    EditorGUI.PropertyField(rightActive, property.FindPropertyRelative("boolValue"), new GUIContent("啟用"));
//                    break;
//                case ActionType.PlayTimeline:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("targetModuleID"), new GUIContent("Timeline ID"));
//                    break;
//                case ActionType.TriggerDialogue:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("targetModuleID"), new GUIContent("對話模組 ID"));
//                    break;
//                case ActionType.TriggerPopup:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("stringValue"), new GUIContent("提示文字 (Popup Text)"));
//                    break;
//                case ActionType.TriggerDoor:
//                    EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("targetModuleID"), new GUIContent("門模組 ID"));
//                    break;
//            }

//            EditorGUI.EndProperty();
//        }

//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            return (EditorGUIUtility.singleLineHeight * 2) + 8;
//        }
//    }
//}