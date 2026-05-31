using UnityEngine;

public class SkipToPlayablePoint : MonoBehaviour
{
    [Header("Skip Setting")]
    public KeyCode skipKey = KeyCode.F5;

    [Header("Optional")]
    public bool hideAllDialogueObjects = true;
    public bool forcePlayerVisible = true;

    private void Update()
    {
        if (Input.GetKeyDown(skipKey))
        {
            SkipNow();
        }
    }

    public void SkipNow()
    {
        CloseAllDialogueManagers();

        if (CoreSystem_GameManager.Instance != null)
        {
            CoreSystem_GameManager.Instance.ForcePlayableControl();
        }

        if (forcePlayerVisible &&
            StoryCoreManager.Instance != null &&
            StoryCoreManager.Instance.GlobalPlayer != null)
        {
            StoryCoreManager.Instance.GlobalPlayer.SetActive(true);
        }

        Debug.Log("[SkipToPlayablePoint] Skipped to playable control.");
    }

    private void CloseAllDialogueManagers()
    {
        DialogueManager[] dialogueManagers = FindObjectsOfType<DialogueManager>(true);

        foreach (DialogueManager dialogue in dialogueManagers)
        {
            if (dialogue == null) continue;

            Transform parent = dialogue.transform.parent;

            if (hideAllDialogueObjects && parent != null)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                dialogue.gameObject.SetActive(false);
            }
        }
    }
}