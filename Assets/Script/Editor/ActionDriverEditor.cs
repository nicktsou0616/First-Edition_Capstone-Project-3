//// 檔案: ActionDriverEditor.cs (必須放在 Editor 資料夾)
//using UnityEngine;
//using UnityEditor;
//using UnityEditorInternal;
//using RPGFramework.Kernel;

//namespace RPGFramework.EditorScripts
//{
//    [CustomEditor(typeof(ActionDriver))]
//    public class ActionDriverEditor : Editor
//    {
//        private SerializedProperty transitionSystemProp;
//        private SerializedProperty uiBridgeProp;
//        private SerializedProperty sceneModulesProp;
//        private SerializedProperty sceneEventBindingsProp;
//        private ReorderableList bindingList;

//        private void OnEnable()
//        {
//            // 綁定所有 Runtime 欄位
//            transitionSystemProp = serializedObject.FindProperty("transitionSystem");
//            uiBridgeProp = serializedObject.FindProperty("uiBridge");
//            sceneModulesProp = serializedObject.FindProperty("sceneModules");
//            sceneEventBindingsProp = serializedObject.FindProperty("sceneEventBindings");

//            // 建立乾淨的事件登記清單 (只做 EventData 的資料引用)
//            bindingList = new ReorderableList(serializedObject, sceneEventBindingsProp, true, true, true, true)
//            {
//                drawHeaderCallback = (Rect rect) =>
//                {
//                    EditorGUI.LabelField(rect, "已登記之場景事件 (Registered Scene Events Reference)", EditorStyles.boldLabel);
//                },
//                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
//                {
//                    SerializedProperty element = sceneEventBindingsProp.GetArrayElementAtIndex(index);
//                    SerializedProperty triggerEvent = element.FindPropertyRelative("triggerEvent");

//                    rect.y += 2;
//                    Rect eventRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

//                    // 僅保留 EventData 的拖曳框，不進行任何 ActionPipeline 子欄位展開
//                    EditorGUI.PropertyField(eventRect, triggerEvent, new GUIContent($"事件資產 [{index}]"));
//                },
//                elementHeightCallback = (int index) =>
//                {
//                    // 縮減回歸單行標準高度，徹底移出動態長度計算
//                    return EditorGUIUtility.singleLineHeight + 6;
//                }
//            };
//        }

//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//            // 【1】系統層參考 (SYSTEM REFERENCES)
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("核心系統參考 (System References)", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(transitionSystemProp, new GUIContent("轉場系統"));
//            EditorGUILayout.PropertyField(uiBridgeProp, new GUIContent("UI 橋接層"));

//            // 【2】場景綁定層 (SCENE BINDINGS)
//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField("場景物件映射表 (Scene Bindings)", EditorStyles.boldLabel);
//            EditorGUILayout.PropertyField(sceneModulesProp, new GUIContent("場景模組對照清單"), true);

//            // 【3】資料引用層 (EVENT DATA REFERENCE)
//            EditorGUILayout.Space();
//            bindingList.DoLayoutList();

//            serializedObject.ApplyModifiedProperties();

//            // 友善的企劃導引提示
//            EditorGUILayout.Space();
//            EditorGUILayout.HelpBox("💡 視覺優化提示：\nAction 執行鏈的唯一編輯入口已收束至 EventData 資產中。請雙擊上方列表中的 EventData 進行管線配置；此處僅負責場景 ID 與 Hierarchy 物件之映射綁定。", MessageType.Info);
//        }
//    }
//}