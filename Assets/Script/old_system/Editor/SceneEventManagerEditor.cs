using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SceneEventManager))]
public class SceneEventManagerEditor : Editor
{
    private SerializedProperty sceneEvents;
    private ReorderableList eventList;

    private Dictionary<string, ReorderableList> stepLists = new Dictionary<string, ReorderableList>();

    private void OnEnable()
    {
        sceneEvents = serializedObject.FindProperty("SceneEvents");

        // =========================
        // 🎮 外層 Event List
        // =========================
        eventList = new ReorderableList(serializedObject, sceneEvents, true, true, true, true);

        eventList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "🎮 Scene Events (拖曳 ≡ 排序)", EditorStyles.boldLabel);
        };

        eventList.elementHeightCallback = (int index) =>
        {
            if (index >= sceneEvents.arraySize) return 0;

            SerializedProperty evt = sceneEvents.GetArrayElementAtIndex(index);

            float height = EditorGUIUtility.singleLineHeight + 6;

            if (!evt.isExpanded)
                return height;

            SerializedProperty copy = evt.Copy();
            SerializedProperty end = copy.GetEndProperty();

            bool enter = true;

            while (copy.NextVisible(enter))
            {
                if (SerializedProperty.EqualContents(copy, end))
                    break;

                enter = false;

                if (copy.name == "Steps")
                {
                    var list = GetStepList(evt);
                    height += list.GetHeight() + 6;
                }
                else
                {
                    height += EditorGUI.GetPropertyHeight(copy, true) + 2;
                }
            }

            return height + 8;
        };

        eventList.drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
        {
            if (index >= sceneEvents.arraySize) return;

            SerializedProperty evt = sceneEvents.GetArrayElementAtIndex(index);

            float y = rect.y + 2;

            string name = evt.FindPropertyRelative("EventName").stringValue;
            if (string.IsNullOrEmpty(name)) name = $"Event {index}";

            Rect header = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
            evt.isExpanded = EditorGUI.Foldout(header, evt.isExpanded, "📌 " + name, true);

            y += EditorGUIUtility.singleLineHeight + 4;

            if (!evt.isExpanded) return;

            SerializedProperty copy = evt.Copy();
            SerializedProperty end = copy.GetEndProperty();
            bool enter = true;

            while (copy.NextVisible(enter))
            {
                if (SerializedProperty.EqualContents(copy, end))
                    break;

                enter = false;

                if (copy.name == "Steps")
                {
                    var list = GetStepList(evt);
                    Rect r = new Rect(rect.x, y, rect.width, list.GetHeight());
                    list.DoList(r);
                    y += list.GetHeight() + 6;
                }
                else
                {
                    float h = EditorGUI.GetPropertyHeight(copy, true);
                    Rect r = new Rect(rect.x, y, rect.width, h);
                    EditorGUI.PropertyField(r, copy, true);
                    y += h + 2;
                }
            }
        };

        // =========================
        // ➕ Add Event（修正版）
        // =========================
        eventList.onAddCallback = (list) =>
        {
            sceneEvents.arraySize++;
            serializedObject.ApplyModifiedProperties();
            stepLists.Clear();

            int newIndex = sceneEvents.arraySize - 1;
            if (newIndex >= 0)
                sceneEvents.GetArrayElementAtIndex(newIndex).isExpanded = true;
        };

        // =========================
        // ➖ Remove Event（修正版）
        // =========================
        eventList.onRemoveCallback = (list) =>
        {
            sceneEvents.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
            stepLists.Clear();
        };

        eventList.onReorderCallback = (list) =>
        {
            stepLists.Clear();
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        eventList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    // =========================
    // 🎬 Step List（內層拖曳）
    // =========================
    private ReorderableList GetStepList(SerializedProperty evt)
    {
        SerializedProperty steps = evt.FindPropertyRelative("Steps");
        string key = steps.propertyPath;

        if (stepLists.TryGetValue(key, out var existing))
            return existing;

        var list = new ReorderableList(steps.serializedObject, steps, true, true, true, true);

        list.drawHeaderCallback = (Rect r) =>
        {
            EditorGUI.LabelField(r, "🎬 Steps (拖曳 ≡ 排序)");
        };

        list.elementHeightCallback = (int index) =>
        {
            if (index >= steps.arraySize) return 0;

            SerializedProperty step = steps.GetArrayElementAtIndex(index);

            float h = EditorGUIUtility.singleLineHeight + 6;

            if (!step.isExpanded)
                return h;

            SerializedProperty copy = step.Copy();
            SerializedProperty end = copy.GetEndProperty();

            bool enter = true;

            while (copy.NextVisible(enter))
            {
                if (SerializedProperty.EqualContents(copy, end))
                    break;

                enter = false;
                h += EditorGUI.GetPropertyHeight(copy, true) + 2;
            }

            return h + 6;
        };

        list.drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
        {
            if (index >= steps.arraySize) return;

            SerializedProperty step = steps.GetArrayElementAtIndex(index);

            float y = rect.y + 2;

            string actionName = step.FindPropertyRelative("Action").enumDisplayNames[step.FindPropertyRelative("Action").enumValueIndex];

            Rect header = new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
            step.isExpanded = EditorGUI.Foldout(header, step.isExpanded, $"▶ Step {index} - {actionName}", true);

            y += EditorGUIUtility.singleLineHeight + 4;

            if (!step.isExpanded) return;

            SerializedProperty copy = step.Copy();
            SerializedProperty end = copy.GetEndProperty();

            bool enter = true;

            while (copy.NextVisible(enter))
            {
                if (SerializedProperty.EqualContents(copy, end))
                    break;

                enter = false;

                float h = EditorGUI.GetPropertyHeight(copy, true);
                Rect r = new Rect(rect.x, y, rect.width, h);
                EditorGUI.PropertyField(r, copy, true);
                y += h + 2;
            }
        };

        // ➕ Add Step（正確版）
        list.onAddCallback = (l) =>
        {
            steps.arraySize++;
            serializedObject.ApplyModifiedProperties();
        };

        stepLists[key] = list;
        return list;
    }
}