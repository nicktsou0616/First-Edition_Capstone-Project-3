using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class NotebookHUDLoader
{
    private const string PrefabPath = "NotebookHUD";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ShowForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowForScene(scene);
    }

    private static void ShowForScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name == "TitleScene") return;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("Notebook EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<NotebookUIController>(true) == null) continue;
            NotebookMapController existingMap = root.GetComponentInChildren<NotebookMapController>(true);
            if (existingMap != null) existingMap.RegisterScene(scene.name);
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[NotebookHUDLoader] Missing Resources/NotebookHUD prefab.");
            return;
        }

        GameObject hud = Object.Instantiate(prefab);
        NotebookMapController map = hud.GetComponentInChildren<NotebookMapController>(true);
        if (map != null) map.RegisterScene(scene.name);
    }
}
