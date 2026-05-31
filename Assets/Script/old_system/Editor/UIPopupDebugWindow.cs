using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class UIPopupDebugWindow : EditorWindow
{
    private GameObject target;

    [MenuItem("Tools/UI Popup Debug Checker")]
    public static void ShowWindow()
    {
        GetWindow<UIPopupDebugWindow>("UI Popup Debug");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Popup Debug Checker", EditorStyles.boldLabel);

        target = (GameObject)EditorGUILayout.ObjectField(
            "Target UI Root",
            target,
            typeof(GameObject),
            true
        );

        if (target == null)
        {
            EditorGUILayout.HelpBox("請拖入你的 Popup UI Root（含 CanvasGroup 那個物件）", MessageType.Info);
            return;
        }

        if (GUILayout.Button("🔍 Check UI Status"))
        {
            CheckUI(target);
        }
    }

    private void CheckUI(GameObject root)
    {
        Debug.Log("===== UI POPUP DEBUG START =====");

        // 1. Active 狀態
        Debug.Log($"ActiveSelf: {root.activeSelf}");
        Debug.Log($"ActiveInHierarchy: {root.activeInHierarchy}");

        // 2. CanvasGroup
        var cg = root.GetComponentInChildren<CanvasGroup>();
        if (cg != null)
        {
            Debug.Log($"CanvasGroup alpha: {cg.alpha}");
            Debug.Log($"CanvasGroup interactable: {cg.interactable}");
            Debug.Log($"CanvasGroup blocksRaycasts: {cg.blocksRaycasts}");
        }
        else
        {
            Debug.LogError("❌ 找不到 CanvasGroup");
        }

        // 3. TextMeshPro
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            Debug.Log($"TMP text: {tmp.text}");
            Debug.Log($"TMP enabled: {tmp.enabled}");
            Debug.Log($"TMP color: {tmp.color}");
        }
        else
        {
            Debug.LogError("❌ 找不到 TextMeshProUGUI");
        }

        // 4. RectTransform
        var rt = root.GetComponentInChildren<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"Rect position: {rt.anchoredPosition}");
            Debug.Log($"Rect scale: {rt.localScale}");
            Debug.Log($"Rect size: {rt.sizeDelta}");
        }

        // 5. Canvas
        var canvas = root.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas renderMode: {canvas.renderMode}");
            Debug.Log($"Canvas sortingOrder: {canvas.sortingOrder}");
        }
        else
        {
            Debug.LogError("❌ 找不到 Canvas");
        }

        Debug.Log("===== UI POPUP DEBUG END =====");
    }
}