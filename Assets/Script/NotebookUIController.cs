using UnityEngine;

public class NotebookUIController : MonoBehaviour
{
    [SerializeField] private GameObject notebookRoot;
    [SerializeField] private GameObject launcherButton;
    [SerializeField] private NotebookManager notebookManager;

    private void Awake()
    {
        if (GetComponent<TaskGuideHUD>() == null)
            gameObject.AddComponent<TaskGuideHUD>();
        notebookRoot.SetActive(false);
        launcherButton.SetActive(true);
    }

    public void ShowNotebook()
    {
        notebookRoot.SetActive(true);
        launcherButton.SetActive(false);
    }

    public void HideNotebook()
    {
        notebookManager.CloseBook();
        notebookRoot.SetActive(false);
        launcherButton.SetActive(true);
    }
}
