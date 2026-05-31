using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// =========================================
// 1. 定義標籤：讓所有的腳本編譯不再報錯
// =========================================
public class InspectorLabelAttribute : PropertyAttribute
{
    public string label;
    public InspectorLabelAttribute(string label)
    {
        this.label = label;
    }
}

// =========================================
// 2. 面板繪製：讓 Unity Inspector 真的把英文變數變成中文
// =========================================
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorLabelAttribute))]
public class InspectorLabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 抓取括號裡的中文文字
        InspectorLabelAttribute customLabel = (InspectorLabelAttribute)attribute;
        label.text = customLabel.label; 
        
        // 畫出變數欄位
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif