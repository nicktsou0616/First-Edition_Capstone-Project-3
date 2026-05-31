using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneFlowController
{
    private const string TitleSceneName = "TitleScene";
    private const string SampleSceneName = "SampleScene";
    private const string EndSceneName = "EndScene";

    private static bool isSubscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        isSubscribed = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (!isSubscribed)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            isSubscribed = true;
        }

        WireCurrentScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WireCurrentScene(scene);
    }

    private static void WireCurrentScene(Scene scene)
    {
        if (scene.name == TitleSceneName)
        {
            WireButton("StartButton", LoadSampleScene);
        }
        else if (scene.name == EndSceneName)
        {
            WireButton("RestartButton", LoadTitleScene);
        }
    }

    private static void WireButton(string buttonObjectName, UnityAction action)
    {
        Button button = FindButtonInActiveScene(buttonObjectName);
        if (button == null)
        {
            Debug.LogWarning($"[SceneFlowController] Button not found: {buttonObjectName}");
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static Button FindButtonInActiveScene(string buttonObjectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            Button[] buttons = rootObject.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.name == buttonObjectName)
                {
                    return button;
                }
            }
        }

        return null;
    }

    public static void LoadSampleScene()
    {
        Time.timeScale = 1f;
        GameResultState.ClearResult();
        SceneManager.LoadScene(SampleSceneName);
    }

    public static void LoadTitleScene()
    {
        Time.timeScale = 1f;
        GameResultState.ClearResult();
        AudioManager.Instance?.StopMusic(0f);
        SceneManager.LoadScene(TitleSceneName);
    }

    public static void LoadEndScene()
    {
        Time.timeScale = 1f;
        GameResultState.ClearResult();
        SceneManager.LoadScene(EndSceneName);
    }
}
