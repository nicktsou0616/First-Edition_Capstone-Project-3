using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class DialogueManager : MonoBehaviour
{
    [Header("🔗 系統綁定")]
    public DialogueCharacterSystem charSystem;
    public DialogueUISystem uiSystem;

    [Header("📊 資料設定")]
    public TextAsset dialogueCSV;

    [Header("⏩ 播放控制系統 (選填)")]
    public DialoguePlaybackController playbackController;

    private readonly List<string[]> dialogueLines = new List<string[]>();
    private int currentIndex = 0;

    // ★ 用於判斷 CSV 格式版本
    private bool useExpressColumns = false;

    public enum State
    {
        Idle,
        Animating,
        Typing,
        WaitingForInput,
        EffectWaiting,
        WaitingForAfterClick,
        Ending
    }

    private State currentState = State.Idle;
    public State CurrentState => currentState;

    public bool IsLastLine => currentIndex >= (dialogueLines.Count - 1);

    private Coroutine mainSequence;
    private string cachedAfterClickVisualID = "";
    private float cachedAfterClickWait = 0f;

    void Start()
    {
        ParseCSV();
        if (dialogueLines.Count > 0)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (currentState == State.Ending)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainSequence != null)
            {
                StopCoroutine(mainSequence);
                mainSequence = null;
            }

            if (uiSystem != null)
                uiSystem.ForceStopTyping();

            EndDialogue();
            return;
        }

        if (Input.GetMouseButtonDown(0) &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (uiSystem.IsLogOpen()) return;

            if (currentState == State.EffectWaiting || uiSystem.isInlineEffectWaiting) return;

            if (currentState == State.WaitingForAfterClick)
            {
                TriggerAfterClickVisual();
                return;
            }

            switch (currentState)
            {
                case State.Animating:
                case State.Typing:
                    SkipToEndOfLine();
                    break;

                case State.WaitingForInput:
                    NextLine();
                    break;
            }
        }
    }

    private void StartDialogue()
    {
        uiSystem.InitializeUI();
        charSystem.InitializeSystem();
        PlayLine(currentIndex, true);
    }

    private void NextLine()
    {
        currentIndex++;
        if (currentIndex < dialogueLines.Count)
        {
            PlayLine(currentIndex, false);
        }
        else
        {
            EndDialogue();
        }
    }

    private void PlayLine(int index, bool isFirstLine)
    {
        if (index < 0 || index >= dialogueLines.Count) return;

        if (mainSequence != null) StopCoroutine(mainSequence);
        mainSequence = StartCoroutine(DialogueSequence(dialogueLines[index], isFirstLine));
    }

    private IEnumerator DialogueSequence(string[] row, bool isFirstLine)
    {
        currentState = State.Animating;

        string leftID = row[0].Trim();
        string rightID = row[1].Trim();

        // ★ 解析 Express 與 Speaker
        string leftExpress = "";
        string rightExpress = "";
        string speaker = "";

        int offset = 0; // 用於偏移後續欄位的讀取索引

        if (useExpressColumns && row.Length >= 5)
        {
            leftExpress = row[2].Trim();
            rightExpress = row[3].Trim();
            speaker = row[4].Trim();
            offset = 2; // 新增了兩個欄位，後續索引向後推2
        }
        else
        {
            speaker = row[2].Trim();
            // 保持 leftExpress / rightExpress 為空
        }

        bool isNarrator = string.IsNullOrEmpty(speaker);

        string audioID = "", audioMode = "", visualID = "", visualMode = "", text = "";
        float audioWait = 0f, visualWait = 0f;
        int audioTrigger = -1, visualTrigger = -1;

        // ★ 加入 offset 計算欄位位置
        int audioStartIndex = 3 + offset;
        int visualStartIndex = 7 + offset;
        int textStartIndex = 11 + offset;

        if (row.Length >= textStartIndex + 1)
        {
            audioID = row[audioStartIndex].Trim();
            audioMode = row[audioStartIndex + 1].Trim();
            float.TryParse(row[audioStartIndex + 2], out audioWait);
            int.TryParse(row[audioStartIndex + 3], out audioTrigger);

            visualID = row[visualStartIndex].Trim();
            visualMode = row[visualStartIndex + 1].Trim();
            float.TryParse(row[visualStartIndex + 2], out visualWait);
            int.TryParse(row[visualStartIndex + 3], out visualTrigger);

            text = string.Join(",", row, textStartIndex, row.Length - textStartIndex).Trim();
        }
        else
        {
            int fallbackTextIndex = 3 + offset;
            if (row.Length > fallbackTextIndex)
                text = string.Join(",", row, fallbackTextIndex, row.Length - fallbackTextIndex).Trim();
        }

        if (text.Contains("START_AUTO"))
        {
            if (playbackController != null) playbackController.SetAuto(true);
            NextLine();
            yield break;
        }
        if (text.Contains("END_AUTO"))
        {
            if (playbackController != null) playbackController.SetAuto(false);
            NextLine();
            yield break;
        }

        if (audioID == "STOP" && string.IsNullOrEmpty(audioMode)) audioMode = "Sync";

        uiSystem.UpdateLogHistory(speaker, text, isNarrator);

        if (isFirstLine) yield return uiSystem.FadeBackground(1f, 0.5f);

        // ★ 傳遞 Express 參數給 CharacterSystem
        yield return charSystem.UpdateCharactersRoutine(leftID, rightID, leftExpress, rightExpress, speaker, isNarrator);

        uiSystem.ClearContentOnly();
        yield return uiSystem.UpdateSpeakerRoutine(speaker, isNarrator, isFirstLine);

        bool hasBefore = false;
        float maxBeforeWait = 0f;

        if (audioMode.Equals("Before", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(audioID))
        {
            uiSystem.PlayAudioEffect(audioID);
            maxBeforeWait = Mathf.Max(maxBeforeWait, audioWait);
            hasBefore = true;
        }
        if (visualMode.Equals("Before", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(visualID))
        {
            uiSystem.PlayVisualEffect(visualID);
            maxBeforeWait = Mathf.Max(maxBeforeWait, visualWait);
            hasBefore = true;
        }

        if (hasBefore)
        {
            currentState = State.EffectWaiting;
            yield return uiSystem.WaitWithLogPause(maxBeforeWait);
        }

        if (audioMode.Equals("Sync", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(audioID))
            uiSystem.PlayAudioEffect(audioID);
        if (visualMode.Equals("Sync", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(visualID))
            uiSystem.PlayVisualEffect(visualID);

        uiSystem.SetInlineEffects(
            audioMode.Equals("Inline", System.StringComparison.OrdinalIgnoreCase) ? audioID : "", audioWait, audioTrigger,
            visualMode.Equals("Inline", System.StringComparison.OrdinalIgnoreCase) ? visualID : "", visualWait, visualTrigger
        );

        currentState = State.Typing;
        yield return uiSystem.TypeTextRoutine(text, isNarrator, speaker);

        if (visualMode.Equals("AfterClick", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(visualID))
        {
            cachedAfterClickVisualID = visualID;
            cachedAfterClickWait = visualWait;
            currentState = State.WaitingForAfterClick;
        }
        else
        {
            currentState = State.WaitingForInput;
        }
    }

    private void TriggerAfterClickVisual()
    {
        currentState = State.EffectWaiting;
        StartCoroutine(AfterClickRoutine());
    }

    private IEnumerator AfterClickRoutine()
    {
        uiSystem.PlayVisualEffect(cachedAfterClickVisualID);
        yield return uiSystem.WaitWithLogPause(cachedAfterClickWait);
        NextLine();
    }

    private void SkipToEndOfLine()
    {
        if (mainSequence != null)
        {
            StopCoroutine(mainSequence);
            mainSequence = null;
        }

        uiSystem.ForceStopTyping();

        string[] row = dialogueLines[currentIndex];
        string leftID = row[0].Trim();
        string rightID = row[1].Trim();

        // ★ 解析 Express 與 Speaker
        string leftExpress = "";
        string rightExpress = "";
        string speaker = "";
        int offset = 0;

        if (useExpressColumns && row.Length >= 5)
        {
            leftExpress = row[2].Trim();
            rightExpress = row[3].Trim();
            speaker = row[4].Trim();
            offset = 2;
        }
        else
        {
            speaker = row[2].Trim();
        }

        bool isNarrator = string.IsNullOrEmpty(speaker);

        string text = "", visualMode = "", visualID = "";
        float visualWait = 0f;

        int visualStartIndex = 7 + offset;
        int textStartIndex = 11 + offset;

        if (row.Length >= textStartIndex + 1)
        {
            visualID = row[visualStartIndex].Trim();
            visualMode = row[visualStartIndex + 1].Trim();
            float.TryParse(row[visualStartIndex + 2], out visualWait);
            text = string.Join(",", row, textStartIndex, row.Length - textStartIndex).Trim();
        }
        else
        {
            int fallbackTextIndex = 3 + offset;
            if (row.Length > fallbackTextIndex)
                text = string.Join(",", row, fallbackTextIndex, row.Length - fallbackTextIndex).Trim();
        }

        // ★ 傳遞 Express 參數給 CharacterSystem
        charSystem.SnapToState(leftID, rightID, leftExpress, rightExpress, speaker, isNarrator);
        uiSystem.SnapToState(speaker, text, isNarrator);

        if (visualMode.Equals("AfterClick", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(visualID))
        {
            cachedAfterClickVisualID = visualID;
            cachedAfterClickWait = visualWait;
            currentState = State.WaitingForAfterClick;
        }
        else
        {
            currentState = State.WaitingForInput;
        }
    }

    public void ForceSkipLine()
    {
        if (currentState == State.Ending) return;
        if (currentState == State.Typing || currentState == State.Animating)
        {
            SkipToEndOfLine();
        }
    }

    public void ForceNextLine()
    {
        if (currentState == State.Ending) return;
        if (currentState == State.WaitingForAfterClick) TriggerAfterClickVisual();
        else if (currentState == State.WaitingForInput) NextLine();
    }

    public void ForceEndDialogue()
    {
        if (currentState == State.Ending) return;
        EndDialogue();
    }

    private void EndDialogue()
    {
        if (currentState == State.Ending) return;
        currentState = State.Ending;
        StartCoroutine(EndRoutine());
    }

    private IEnumerator EndRoutine()
    {
        Coroutine charOut = StartCoroutine(charSystem.SlideOutAllRoutine());
        Coroutine uiOut = StartCoroutine(uiSystem.FadeOutAllRoutine());

        yield return charOut;
        yield return uiOut;

        gameObject.SetActive(false);
    }

    private void ParseCSV()
    {
        if (dialogueCSV == null) return;
        string[] lines = dialogueCSV.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0) return;

        // ★ 檢查第一行(標題列)是否包含新的 Express 欄位，決定解析模式
        string headerRow = lines[0].ToLower();
        if (headerRow.Contains("leftexpress") || headerRow.Contains("rightexpress"))
        {
            useExpressColumns = true;
            Debug.Log("[DialogueManager] 偵測到新版 CSV 格式，啟用 Express 欄位解析。");
        }
        else
        {
            useExpressColumns = false;
            Debug.Log("[DialogueManager] 偵測到舊版 CSV 格式，忽略 Express 欄位。");
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 1; i < lines.Length; i++)
        {
            List<string> row = new List<string>();
            bool inQuotes = false;
            sb.Clear();
            foreach (char c in lines[i])
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes)
                {
                    row.Add(sb.ToString());
                    sb.Clear();
                }
                else sb.Append(c);
            }
            row.Add(sb.ToString());
            if (row.Count >= 3) dialogueLines.Add(row.ToArray()); // 至少需要 ID 才能運作
        }
    }
}