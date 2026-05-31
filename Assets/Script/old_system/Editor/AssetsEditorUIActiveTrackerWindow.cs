using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class UIActiveTrackerWindow : EditorWindow
{
    private GameObject target;

    private bool lastActiveState;
    private bool tracking;

    [MenuItem("Tools/UI Active Tracker")]
    public static void Open()
    {
        GetWindow<UIActiveTrackerWindow>("UI Active Tracker");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Active Tracker (Runtime Watcher)", EditorStyles.boldLabel);

        target = (GameObject)EditorGUILayout.ObjectField(
            "Target UI Object",
            target,
            typeof(GameObject),
            true
        );

        if (target == null)
        {
            EditorGUILayout.HelpBox("請拖入你懷疑會被關掉的 UI（Canvas / Prompt）", MessageType.Info);
            return;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("請進入 Play Mode 才能監控", MessageType.Warning);
            return;
        }

        if (!tracking)
        {
            if (GUILayout.Button("▶ Start Tracking"))
            {
                StartTracking();
            }
        }
        else
        {
            if (GUILayout.Button("⛔ Stop Tracking"))
            {
                tracking = false;
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Current State:", target.activeSelf ? "ACTIVE" : "INACTIVE");
    }

    private void StartTracking()
    {
        tracking = true;
        lastActiveState = target.activeSelf;

        EditorApplication.update += Watch;
    }

    private void Watch()
    {
        if (!tracking || target == null)
        {
            EditorApplication.update -= Watch;
            return;
        }

        if (target.activeSelf != lastActiveState)
        {
            Debug.LogError(
                $"🚨 UI ACTIVE STATE CHANGED!\n" +
                $"Object: {target.name}\n" +
                $"Active: {target.activeSelf}\n" +
                $"Scene: {target.scene.name}\n" +
                $"Frame: {Time.frameCount}"
            );

            lastActiveState = target.activeSelf;

            PrintStackTrace();
        }
    }

    private void PrintStackTrace()
    {
        Debug.LogWarning("🔍 Stack trace not fully visible in EditorWindow, check Console above for source.");
    }
}