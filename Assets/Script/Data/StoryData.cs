using UnityEngine;
using System.Collections.Generic;

public enum StepType
{
    Dialogue,
    Choice,
    CheckFlag,
    AddFlag,
    GetEvidence,
    TriggerDeduction,
    ChangeScene
}

[System.Serializable]
public class StoryStep
{
    public StepType stepType;

    [Header("=== Dialogue 設定 ===")]
    public GameObject dialoguePrefab;

    [Tooltip("★ 新增：正常看完(含Skip)後，要自動給予的 Flag (可留空)")]
    public string dialogueDoneFlag;

    [Header("任務完成回報")]
    public bool reportTaskCompletion;
    public TaskCompletionType taskCompletionType = TaskCompletionType.TalkToNpc;
    public string taskCompletionTargetId;


    // ==========================================
    // ★ 更新：Choice 區塊加入題目欄位
    // ==========================================
    [Header("=== Choice 設定 ===")]
    public GameObject choicePrefab;

    [Tooltip("請輸入這題選擇題的題目文字")]
    [TextArea(2, 4)]
    public string choiceQuestion; // ★ 新增這行：讓你可以直接在 Inspector 打題目！

    public string[] choices;
    [Tooltip("對應選項後要播放的全新劇情 (StoryData)，數量需與選項一致")]
    public StoryData[] choiceNextStories;
    // ==========================================
    // ★ 升級：CheckFlag 改為指定全新劇本
    // ==========================================
    [Header("=== Flag 設定 ===")]
    public string targetFlag;
    [Tooltip("擁有該 Flag 時，要切換過去的全新劇情")]
    public StoryData trueStory;
    [Tooltip("沒有該 Flag 時，要切換過去的全新劇情")]
    public StoryData falseStory;

    [Header("=== Evidence 設定 ===")]
    public EvidenceData evidenceToGive;

    [Header("=== Deduction 設定 ===")]
    public GameObject deductionPrefab;
    public StoryData successStory;
    public StoryData failStory;

    [Header("=== Scene 設定 ===")]
    public string sceneToLoad;
    [Tooltip("★ 新增：請拖入 TransitionManager 轉場特效 (黑/白幕) Prefab。若留空則直接硬切")]
    public GameObject transitionPrefab;
    [Tooltip("★ 新增：抵達新場景後，玩家要出現的出生點 ID (對應 PlayerSpawnPoint)。若留空則維持場景預設位置")]
    public string targetSpawnID;
}

[CreateAssetMenu(fileName = "NewStory", menuName = "StorySystem/StoryData")]
public class StoryData : ScriptableObject
{
    [Header("劇本流程")]
    public List<StoryStep> steps = new List<StoryStep>();
}
