using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    private static StoryManager _instance;
    public static StoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StoryManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[Auto-Generated] StoryManager");
                    _instance = go.AddComponent<StoryManager>();
                }
            }
            return _instance;
        }
    }

    [Header("玩家劇情進度檔案")]
    public GameProgress gameProgress;

    // ★ 新增：用來跨場景傳遞的目標出生點 ID
    public static string nextSpawnID = "";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("[Auto-Generated] EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }

        if (gameProgress == null)
        {
            gameProgress = Resources.Load<GameProgress>("StoryAssets/MainProgress");
        }
    }

    [Header("正在執行的劇本")]
    public StoryData currentStory;
    public bool isPlayingStory { get; private set; } = false;

    private int currentStepIndex = 0;
    private GameObject currentDialogueInstance;
    private GameObject currentChoiceInstance;
    private GameObject currentDeductionInstance;

    public void PlayStory(StoryData storyData)
    {
        if (isPlayingStory) return;
        if (storyData == null || storyData.steps.Count == 0) return;

        isPlayingStory = true;
        PlayerControlLock.LockPlayer(true);

        currentStory = storyData;
        currentStepIndex = 0;
        ExecuteCurrentStep();
    }

    private void StopStory()
    {
        isPlayingStory = false;
        PlayerControlLock.LockPlayer(false);
    }

    private void ExecuteCurrentStep()
    {
        if (currentStepIndex >= currentStory.steps.Count)
        {
            Debug.Log("[StoryManager] 劇本全數播放完畢！");
            StopStory();
            return;
        }

        StoryStep step = currentStory.steps[currentStepIndex];

        switch (step.stepType)
        {
            case StepType.Dialogue: PlayDialogueStep(step); break;
            case StepType.Choice: PlayChoiceStep(step); break;
            case StepType.CheckFlag: ProcessCheckFlagStep(step); break;
            case StepType.AddFlag: ProcessAddFlagStep(step); break;
            case StepType.GetEvidence: ProcessGetEvidenceStep(step); break;
            case StepType.TriggerDeduction: PlayDeductionStep(step); break;
            case StepType.ChangeScene: ProcessChangeSceneStep(step); break;
        }
    }

    // --- 對話系統 ---
    private void PlayDialogueStep(StoryStep step)
    {
        if (step.dialoguePrefab == null) { NextStep(); return; }
        currentDialogueInstance = Instantiate(step.dialoguePrefab);
        DialogueAdapter adapter = currentDialogueInstance.GetComponent<DialogueAdapter>();
        if (adapter == null) adapter = currentDialogueInstance.AddComponent<DialogueAdapter>();

        adapter.PlayDialogue(OnDialogueComplete);
    }

    private void OnDialogueComplete(bool isCompletedNormally)
    {
        if (currentDialogueInstance != null) Destroy(currentDialogueInstance);
        StoryStep currentStep = currentStory.steps[currentStepIndex];

        if (isCompletedNormally)
        {
            Debug.Log("[StoryManager] 對話正常結束 (含 Skip)。");
            if (!string.IsNullOrEmpty(currentStep.dialogueDoneFlag) && gameProgress != null)
                gameProgress.AddFlag(currentStep.dialogueDoneFlag);
            if (currentStep.reportTaskCompletion && TaskGuideManager.Instance != null)
                TaskGuideManager.Instance.ReportCompletion(
                    currentStep.taskCompletionTargetId, currentStep.taskCompletionType);
            NextStep();
        }
        else
        {
            Debug.Log("[StoryManager] 玩家按下 ESC，直接中斷並結束整個劇情！");
            StopStory();
        }
    }

    // --- 選項系統 ---
    private void PlayChoiceStep(StoryStep step)
    {
        if (step.choicePrefab == null) { NextStep(); return; }
        currentChoiceInstance = Instantiate(step.choicePrefab);
        SingleChoiceModule choiceModule = currentChoiceInstance.GetComponentInChildren<SingleChoiceModule>(true);
        if (choiceModule != null)
        {
            choiceModule.SetupChoices(step.choiceQuestion, step.choices);
            choiceModule.ShowQuestion(OnChoiceComplete);
        }
    }

    private void OnChoiceComplete(int selectedIndex)
    {
        if (currentChoiceInstance != null) Destroy(currentChoiceInstance);
        StoryStep currentStep = currentStory.steps[currentStepIndex];

        if (currentStep.choiceNextStories != null && selectedIndex < currentStep.choiceNextStories.Length)
        {
            StoryData nextStory = currentStep.choiceNextStories[selectedIndex];
            StopStory();
            if (nextStory != null) { PlayStory(nextStory); return; }
        }
        NextStep();
    }

    // --- 邏輯分支與狀態系統 ---
    private void ProcessCheckFlagStep(StoryStep step)
    {
        if (gameProgress != null && !string.IsNullOrEmpty(step.targetFlag))
        {
            bool hasFlag = gameProgress.HasFlag(step.targetFlag);
            StoryData nextStory = hasFlag ? step.trueStory : step.falseStory;
            StopStory();
            if (nextStory != null) { PlayStory(nextStory); return; }
        }
        NextStep();
    }

    private void ProcessAddFlagStep(StoryStep step) { if (gameProgress != null && !string.IsNullOrEmpty(step.targetFlag)) gameProgress.AddFlag(step.targetFlag); NextStep(); }
    private void ProcessGetEvidenceStep(StoryStep step) { if (gameProgress != null && step.evidenceToGive != null) gameProgress.AddEvidence(step.evidenceToGive); NextStep(); }

    // --- 場景切換系統 (★ Phase 5 修正重點) ---
    private void ProcessChangeSceneStep(StoryStep step)
    {
        if (!string.IsNullOrEmpty(step.sceneToLoad))
        {
            // ★ 將設定的 Target Spawn ID 暫存到全域，供下一張地圖讀取
            nextSpawnID = step.targetSpawnID;

            StopStory();
            if (step.transitionPrefab != null) TransitionManager.LoadScene(step.sceneToLoad, step.transitionPrefab);
            else SceneManager.LoadScene(step.sceneToLoad);
        }
        else NextStep();
    }

    // --- 推理系統 ---
    private void PlayDeductionStep(StoryStep step)
    {
        if (step.deductionPrefab == null) { NextStep(); return; }
        currentDeductionInstance = Instantiate(step.deductionPrefab);
        InvestigationModule module = currentDeductionInstance.GetComponentInChildren<InvestigationModule>(true);
        if (module != null) module.StartDeduction(OnDeductionComplete);
        else { NextStep(); }
    }

    private void OnDeductionComplete(bool isSuccess)
    {
        if (currentDeductionInstance != null) Destroy(currentDeductionInstance);
        StoryStep currentStep = currentStory.steps[currentStepIndex];
        StoryData nextStory = isSuccess ? currentStep.successStory : currentStep.failStory;
        StopStory();
        if (nextStory != null) { PlayStory(nextStory); return; }
        NextStep();
    }

    private void NextStep()
    {
        currentStepIndex++;
        ExecuteCurrentStep();
    }
}
