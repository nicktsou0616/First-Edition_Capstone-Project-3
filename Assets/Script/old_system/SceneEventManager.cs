using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEventManager : MonoBehaviour
{
    [Header("【這個房間裡的所有事件統一管理區】")]
    public List<EventBlock> SceneEvents = new List<EventBlock>();

    [Header("🛠️ Linux Syslog (日誌系統)")]
    [InspectorLabel("開啟詳細除錯警告 (預設關閉)")]
    public bool enableVerboseLog = false;

    private bool isAnyEventExecuting = false;
    private float interactInputBuffer = 0f;

    public enum EventRequirementType { Required, Optional }
    public EventRequirementType requirementType = EventRequirementType.Required;
    public bool IsPlaying = false;
    public bool IsEventExecuting => isAnyEventExecuting;

    private void OnEnable() { StartCoroutine(CheckAutoEventsRoutine()); }

    public void ResetEventExecutionState()
    {
        foreach (var evt in SceneEvents) evt.hasExecuted = false;
    }

    private IEnumerator CheckAutoEventsRoutine()
    {
        yield return null;

        // ★修改重點：改用 roomReady 判定，不再用 isTransitioning
        if (SceneTransitionManager.Instance != null)
        {
            yield return new WaitUntil(() => SceneTransitionManager.Instance.roomReady);
        }

        if (GameResultState.GetResult() >= 0)
        {
            yield break;
        }

        foreach (var evt in SceneEvents)
        {
            if (evt.TriggerType == TriggerType.OnEnterScene)
                TryTriggerEvent(evt, true);
        }
    }

    private void Update()
    {
        if (GameResultState.GetResult() >= 0)
        {
            HideInteractionPrompt();
            return;
        }

        if (isAnyEventExecuting)
        {
            HideInteractionPrompt();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            interactInputBuffer = 0.25f;

        if (interactInputBuffer > 0) interactInputBuffer -= Time.deltaTime;

        if (StoryCoreManager.Instance == null || StoryCoreManager.Instance.GlobalPlayer == null)
        {
            HideInteractionPrompt();
            return;
        }

        GameObject player = StoryCoreManager.Instance.GlobalPlayer;
        if (!player.activeSelf)
        {
            HideInteractionPrompt();
            return;
        }

        bool wantsToInteract = interactInputBuffer > 0;
        bool hasConsumedInput = false;
        bool hasInteractablePrompt = false;

        HashSet<GameObject> lockedTargetsThisFrame = new HashSet<GameObject>();

        foreach (var evt in SceneEvents)
        {
            if (evt.TriggerType == TriggerType.OnEnterScene || evt.TriggerType == TriggerType.FromFlow) continue;
            if (evt.TriggerTargets == null || evt.TriggerTargets.Count == 0) continue;

            bool isTargetLocked = false;
            foreach (var target in evt.TriggerTargets)
            {
                if (target != null && lockedTargetsThisFrame.Contains(target))
                {
                    isTargetLocked = true;
                    break;
                }
            }
            if (isTargetLocked) continue;

            bool conditionMet = false;

            if (evt.TargetCondition == ConditionType.Any)
            {
                foreach (var target in evt.TriggerTargets)
                {
                    if (target == null) continue;
                    if (GetEdgeDistance(player, target) <= evt.InteractDistance)
                    {
                        conditionMet = true;
                        break;
                    }
                }
            }
            else if (evt.TargetCondition == ConditionType.All)
            {
                conditionMet = true;
                bool hasValidTarget = false;
                foreach (var target in evt.TriggerTargets)
                {
                    if (target == null) continue;
                    hasValidTarget = true;
                    if (GetEdgeDistance(player, target) > evt.InteractDistance)
                    {
                        conditionMet = false;
                        break;
                    }
                }
                if (!hasValidTarget) conditionMet = false;
            }

            if (conditionMet)
            {
                if (evt.TriggerType == TriggerType.OnInteract && CanEventRun(evt) && HasVisibleInteraction(evt))
                {
                    hasInteractablePrompt = true;
                }

                bool triggered = false;

                if (evt.TriggerType == TriggerType.OnEnter)
                {
                    triggered = TryTriggerEvent(evt, false);
                }
                else if (evt.TriggerType == TriggerType.OnInteract && wantsToInteract && !hasConsumedInput)
                {
                    triggered = TryTriggerEvent(evt, false);
                    if (triggered)
                    {
                        hasConsumedInput = true;
                        interactInputBuffer = 0f;
                    }
                }

                if (triggered)
                {
                    foreach (var target in evt.TriggerTargets) if (target != null) lockedTargetsThisFrame.Add(target);
                }
            }
        }

        if (hasInteractablePrompt && !hasConsumedInput)
        {
            ShowInteractionPrompt(player.transform);
        }
        else
        {
            HideInteractionPrompt();
        }
    }

    private float GetEdgeDistance(GameObject player, GameObject target)
    {
        Collider2D playerCol = player.GetComponent<Collider2D>();
        Collider2D targetCol = target.GetComponent<Collider2D>();

        if (playerCol != null && targetCol != null)
        {
            ColliderDistance2D dist = Physics2D.Distance(playerCol, targetCol);
            if (dist.isValid) return dist.distance;
        }
        return Vector2.Distance(player.transform.position, target.transform.position);
    }

    private bool CanEventRun(EventBlock evt)
    {
        if (evt == null || CoreSystem_GameManager.Instance == null) return false;

        string eventSignature = evt.GetEventSignature();
        if (evt.ExecutionBehavior == ExecutionBehavior.RunOnce &&
            (evt.hasExecuted || CoreSystem_GameManager.Instance.IsEventCompleted(eventSignature)))
        {
            return false;
        }

        if (!CoreSystem_GameManager.Instance.CheckFlags(evt.RequireFlags, ConditionType.All)) return false;
        if (CoreSystem_GameManager.Instance.HasAnyFlag(evt.ExcludeFlags)) return false;

        return true;
    }

    private bool HasVisibleInteraction(EventBlock evt)
    {
        if (evt == null || evt.Steps == null) return false;

        foreach (StepData step in evt.Steps)
        {
            if (step == null) continue;

            switch (step.Action)
            {
                case ActionType.Dialogue:
                case ActionType.PlayCutscene:
                case ActionType.ShowPopupText:
                case ActionType.SwitchToNewModule:
                    return true;
            }
        }

        return false;
    }

    private void ShowInteractionPrompt(Transform player)
    {
        InteractionPromptUI.ShowFor(player);
    }

    private void HideInteractionPrompt()
    {
        InteractionPromptUI.HideCurrent();
    }

    public bool TriggerEventByName(string eventName)
    {
        var evt = SceneEvents.Find(e => e.EventName == eventName && e.TriggerType == TriggerType.FromFlow);
        if (evt != null) return TryTriggerEvent(evt, false);

        Debug.LogWarning($"[SceneEventManager] FromFlow event not found: {eventName}");
        return false;
    }

    private bool TryTriggerEvent(EventBlock evt, bool silentCheck)
    {
        if (isAnyEventExecuting && evt.TriggerType != TriggerType.OnInteract) return false;

        string eventSignature = evt.GetEventSignature();

        if (evt.ExecutionBehavior == ExecutionBehavior.RunOnce &&
            (evt.hasExecuted || CoreSystem_GameManager.Instance.IsEventCompleted(eventSignature)))
        {
            return false;
        }

        if (!CoreSystem_GameManager.Instance.CheckFlags(evt.RequireFlags, ConditionType.All)) return false;
        if (CoreSystem_GameManager.Instance.HasAnyFlag(evt.ExcludeFlags)) return false;

        StartCoroutine(ExecuteEventRoutine(evt, eventSignature));
        return true;
    }

    private IEnumerator ExecuteEventRoutine(EventBlock evt, string eventSignature = "")
    {
        isAnyEventExecuting = true;

        if (CoreSystem_GameManager.Instance != null)
        {
            CoreSystem_GameManager.Instance.SetMovementPermission(
                evt.EventGameMode == GameMode.FreeRoamMode
            );
        }

        foreach (var step in evt.Steps)
            yield return StartCoroutine(ProcessSingleStep(step, evt));

        if (evt.ExecutionBehavior == ExecutionBehavior.RunOnce)
        {
            evt.hasExecuted = true;

            if (CoreSystem_GameManager.Instance != null &&
                !string.IsNullOrEmpty(eventSignature))
            {
                CoreSystem_GameManager.Instance.MarkEventCompleted(eventSignature);
            }
        }

        isAnyEventExecuting = false;

        if (evt.teleportAfterEvent)
        {
            if (SceneTransitionManager.Instance != null)
            {
                StartCoroutine(
                    SceneTransitionManager.Instance.ExecuteRoomTransition(
                        evt.RoomToDisable,
                        evt.RoomToEnable,
                        evt.TeleportTarget,
                        evt.transitionType,
                        evt.transitionDuration
                    )
                );
            }
        }
    }

    private IEnumerator ProcessSingleStep(StepData step, EventBlock evt)
    {
        switch (step.Action)
        {
            case ActionType.Dialogue:
                CoreSystem_GameManager.Instance.PushControlLock("Step_Dialogue");
                if (step.TargetModule != null)
                {
                    yield return null;
                    step.TargetModule.SetActive(true);
                    yield return new WaitUntil(() => !step.TargetModule.activeSelf);
                }
                CoreSystem_GameManager.Instance.PopControlLock("Step_Dialogue");
                break;

            case ActionType.PlayCutscene:
                CoreSystem_GameManager.Instance.PushControlLock("Step_Cutscene");
                if (step.CutsceneAsset != null && step.TargetModule != null)
                {
                    var director = step.TargetModule.GetComponent<UnityEngine.Playables.PlayableDirector>();
                    if (director != null)
                    {
                        director.playableAsset = step.CutsceneAsset;
                        director.Play();
                        yield return new WaitUntil(() => director.state != UnityEngine.Playables.PlayState.Playing);
                    }
                }
                CoreSystem_GameManager.Instance.PopControlLock("Step_Cutscene");
                break;

            case ActionType.ChangeMode:
                CoreSystem_GameManager.Instance.SetMovementPermission(step.grantMovementPermission);
                break;

            case ActionType.Unlock:
                if (step.TargetModule != null)
                {
                    TeleportDoor door = step.TargetModule.GetComponent<TeleportDoor>();
                    if (door != null) door.isIndependentlyUnlocked = true;
                    var col = step.TargetModule.GetComponent<Collider2D>();
                    if (col != null) col.isTrigger = true;
                    step.TargetModule.SetActive(true);
                }
                break;

            case ActionType.SetFlag:
                foreach (var flag in step.TargetFlags)
                {
                    if (flag != null) CoreSystem_GameManager.Instance.AddFlag(flag);
                }
                break;

            case ActionType.WaitFlags:
                yield return new WaitUntil(() => CoreSystem_GameManager.Instance.CheckFlags(step.TargetFlags, step.FlagCondition));
                break;

            case ActionType.AdvanceSegment:
                if (StoryCoreManager.Instance != null) StoryCoreManager.Instance.AdvanceNextSegment();
                break;

            case ActionType.TogglePlayer:
                if (StoryCoreManager.Instance != null && StoryCoreManager.Instance.GlobalPlayer != null)
                {
                    StoryCoreManager.Instance.GlobalPlayer.SetActive(step.isPlayerVisible);
                }

                // 【補回缺失】讓一般場景物件也能被 SetActive 控制
                // 巧妙利用 TogglePlayer 的 isPlayerVisible 當作開關布林值
                if (step.TargetModule != null)
                {
                    step.TargetModule.SetActive(step.isPlayerVisible);
                }
                break;

            case ActionType.ShowPopupText:
                if (UIPopupManager.Instance != null)
                {
                    UIPopupManager.Instance.gameObject.SendMessage("ShowText", step.popupTextContent, SendMessageOptions.DontRequireReceiver);
                    yield return new WaitForSeconds(2.0f);
                }
                break; // 👈 這是原本舊系統的結尾

            // ========================================================
            // 👇 新增這裡：處理新舊系統一鍵交接邏輯 (帶有防禦性防呆)
            // ========================================================
            case ActionType.SwitchToNewModule:
                // 使用 Unity 最新的 Find 寫法，高效率抓取轉場中介器
                ModuleSwitcher switcher = Object.FindAnyObjectByType<ModuleSwitcher>();
                if (switcher != null)
                {
                    switcher.SwitchNow();
                }
                else
                {
                    Debug.LogError("⚠️ [SceneEventManager] 警告：事件執行了切換新模組命令，但在場景中找不到 [ModuleSwitcher] 組件！");
                }
                break;
        }
    }
}
