using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro; // ★ 確保有這行

[RequireComponent(typeof(CanvasGroup))]
public class SingleChoiceModule : MonoBehaviour
{
    [Header("UI 參考")]
    public CanvasGroup moduleCanvasGroup;
    public TMP_Text questionTextUI; // ★ 新增這行：用來綁定你的問題 Text (TMP)
    public List<ChoiceButton> choiceButtons; // 支援任意數量的按鈕

    [Header("模組動畫設定")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    private Action<int> onAnswerCallback;

    private void Awake()
    {
        if (moduleCanvasGroup == null) moduleCanvasGroup = GetComponent<CanvasGroup>();

        // 初始化按鈕並註冊事件
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            choiceButtons[i].choiceIndex = i;
            choiceButtons[i].OnChoiceMade += HandleChoiceMade;
            choiceButtons[i].OnChoiceAnimDone += HandleChoiceAnimDone;
        }
    }

    // ==========================================
    // ★ 新增：供 StoryManager 動態設定選項文字與數量
    // ==========================================
    // ★ 修改：多接收一個 string question 參數
    public void SetupChoices(string question, string[] choiceTexts)
    {
        // 1. 替換題目文字
        if (questionTextUI != null)
        {
            questionTextUI.text = question;
        }

        // 2. 替換按鈕文字
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i < choiceTexts.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceButtons[i].buttonText != null)
                {
                    choiceButtons[i].buttonText.text = choiceTexts[i];
                }
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
    // ==========================================

    /// <summary>
    /// 顯示選擇題模組
    /// </summary>
    public void ShowQuestion(Action<int> callback)
    {
        onAnswerCallback = callback;

        // ★ 修正重點：先啟動物件！這樣 Awake() 才會觸發，確保 CanvasGroup 被成功抓取
        gameObject.SetActive(true);

        ResetAll(); // 現在重置就不會報錯了
        StartCoroutine(FadeModuleRoutine(1f, fadeInDuration));
    }

    /// <summary>
    /// 答錯時重新進場
    /// </summary>
    public void ShowQuestionAgain()
    {
        // ★ 修正重點：同樣先啟動物件
        gameObject.SetActive(true);

        ResetAll();
        StartCoroutine(FadeModuleRoutine(1f, fadeInDuration));
    }

    private void ResetAll()
    {
        StopAllCoroutines();
        moduleCanvasGroup.alpha = 0f;

        foreach (var btn in choiceButtons)
        {
            btn.ResetState();
        }
    }

    private void HandleChoiceMade(ChoiceButton selectedBtn)
    {
        foreach (var btn in choiceButtons)
        {
            btn.SetInteractable(false);
        }
    }

    private void HandleChoiceAnimDone(int selectedIndex)
    {
        StartCoroutine(HideQuestionRoutine(selectedIndex));
    }

    private IEnumerator HideQuestionRoutine(int selectedIndex)
    {
        yield return StartCoroutine(FadeModuleRoutine(0f, fadeOutDuration));
        gameObject.SetActive(false);
        onAnswerCallback?.Invoke(selectedIndex);
    }

    private IEnumerator FadeModuleRoutine(float targetAlpha, float duration)
    {
        float startAlpha = moduleCanvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            moduleCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        moduleCanvasGroup.alpha = targetAlpha;
    }
}