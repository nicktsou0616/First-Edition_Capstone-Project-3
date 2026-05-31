using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResultPopupUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;

    public TextMeshProUGUI messageText;

    public Button confirmButton;

    private Action onConfirmCallback;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    public void Show(string message, Action callback)
    {
        panelRoot.SetActive(true);

        messageText.text = message;

        messageText.ForceMeshUpdate();

        onConfirmCallback = callback;
    }

    private void OnClickConfirm()
    {
        gameObject.SetActive(false);

        onConfirmCallback?.Invoke();
    }
}