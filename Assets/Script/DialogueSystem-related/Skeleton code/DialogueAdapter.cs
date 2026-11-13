using UnityEngine;
using System;
using System.Collections;

public class DialogueAdapter : MonoBehaviour
{
    // ★ 變更：改為 Action<bool>。true = 正常/Skip，false = 被 ESC 中斷
    private Action<bool> onCompleteCallback;
    private DialogueManager originalManager;
    private bool isFinished = false;
    private bool wasEscaped = false; // 紀錄是否按過 ESC

    public void PlayDialogue(Action<bool> onComplete)
    {
        onCompleteCallback = onComplete;
        isFinished = false;
        wasEscaped = false;

        originalManager = GetComponentInChildren<DialogueManager>(true);

        if (originalManager != null)
        {
            StartCoroutine(MonitorManagerRoutine());
        }
        else
        {
            Debug.LogError("[DialogueAdapter] 在 Prefab 內找不到 DialogueManager！");
            TriggerComplete();
        }
    }

    private void Update()
    {
        // ★ 核心魔法：橋接器自己在背景監聽 ESC，完全不用改動 DialogueManager！
        if (!isFinished && originalManager != null && originalManager.gameObject.activeInHierarchy)
        {
            // 如果偵測到 ESC 鍵，就標記為中斷 (因為 DialogueManager 內部也是偵測這個按鍵)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                wasEscaped = true;
            }
        }
    }

    private IEnumerator MonitorManagerRoutine()
    {
        yield return null;

        while (originalManager != null && originalManager.gameObject.activeInHierarchy)
        {
            yield return null;
        }

        TriggerComplete();
    }

    private void TriggerComplete()
    {
        if (!isFinished)
        {
            isFinished = true;
            // 正常結束回傳 true，如果是 ESC 關閉的回傳 false
            onCompleteCallback?.Invoke(!wasEscaped);
        }
    }
}