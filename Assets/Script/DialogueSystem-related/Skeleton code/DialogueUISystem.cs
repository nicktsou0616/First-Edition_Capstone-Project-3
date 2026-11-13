using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public struct DialogueAudioData { public string id; public AudioClip clip; }
[System.Serializable]
public struct DialogueVisualData { public string id; public GameObject visualPrefab; }

public class DialogueUISystem : MonoBehaviour
{
    [Header("🖥️ 文字與 UI 元件")]
    public GameObject dialogueBoxContainer;
    public GameObject nameBoxObject;
    public Image dividerLine;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI narratorText;
    public Image backgroundImage;

    [Header("📖 LOG 系統")]
    public GameObject logPageObject;
    public TextMeshProUGUI logHistoryText;
    private StringBuilder fullLogHistory = new StringBuilder();

    [Header("🌟 LOG 獨立按鈕控制 (程式綁定)")]
    [Tooltip("請將畫面上的 LOG UI 按鈕 (Button元件) 拖曳到這裡")]
    public Button logUIButton; // ★ 防誤觸修正：明確取得 UI Button 元件

    private RectTransform logButtonRect; // 隱藏：由程式自動從 Button 取出
    private Vector2 logPageOriginalAnchoredPos;
    private bool hasCachedLogPos = false;

    [Header("🔊 音效設定 (打字)")]
    public AudioSource typingAudioSource;
    public AudioClip soundA, soundB;
    public float typingSoundCooldown = 0.05f;

    [Header("🔊 UI 音效設定")]
    public AudioSource uiAudioSource;
    public AudioClip openLogClip;
    public AudioClip closeLogClip;

    [Header("✨ 演出參數")]
    public float uiFadeDuration = 0.2f;
    public float typeSpeed = 0.03f;
    public Color normalTextColor = Color.white;
    public Color narratorTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("🎬 劇情演出資源 (Audio/Visual)")]
    public AudioSource storyAudioSource;
    public List<DialogueAudioData> audioDatabase;
    public List<DialogueVisualData> visualDatabase;

    [Header("⏩ 播放控制系統 (選填)")]
    public DialoguePlaybackController playbackController; // ★ 補回：為了支援 Skip/Auto 加速與靜音

    public bool isInlineEffectWaiting { get; private set; }

    private string inlineAudioID; private float inlineAudioWait; private int inlineAudioTrigger;
    private string inlineVisualID; private float inlineVisualWait; private int inlineVisualTrigger;

    private float lastSoundTime = 0f;
    private bool useSoundA = true;
    private string lastSpeaker = "⚠️INITIAL_EMPTY_SPEAKER⚠️";
    private string lastSoundSpeaker = "";
    private bool isDialoguePaused = false;

    public void InitializeUI()
    {
        // ★ 補回：強制清除 Inspector 誤拉的 Log 綁定，改用純代碼綁定
        if (logUIButton != null)
        {
            logButtonRect = logUIButton.GetComponent<RectTransform>();
            logUIButton.onClick.RemoveAllListeners();
            logUIButton.onClick.AddListener(ToggleLogPage);
        }

        SetAlpha(backgroundImage, 0f);
        logPageObject.SetActive(false);
        isDialoguePaused = false;
        ForceStopTyping();

        dialogueBoxContainer.SetActive(true);
        nameBoxObject.SetActive(true);
        narratorText.gameObject.SetActive(false);

        ClearContentOnly();

        SetAlpha(nameText, 0f);
        SetAlpha(dividerLine, 0f);
        if (nameBoxObject.GetComponent<Image>() != null) SetAlpha(nameBoxObject.GetComponent<Image>(), 0f);
    }

    public void ClearContentOnly()
    {
        contentText.text = "";
        contentText.maxVisibleCharacters = 0;
        narratorText.text = "";
        narratorText.maxVisibleCharacters = 0;
    }

    public IEnumerator UpdateSpeakerRoutine(string speaker, bool isNarrator, bool isFirstLine)
    {
        bool speakerChanged = (speaker != lastSpeaker) || isFirstLine;

        if (isNarrator)
        {
            dialogueBoxContainer.SetActive(false);
            narratorText.gameObject.SetActive(true);
            SetAlpha(narratorText, 1f);
            lastSpeaker = speaker;
            yield break;
        }

        dialogueBoxContainer.SetActive(true);
        narratorText.gameObject.SetActive(false);

        if (speakerChanged)
        {
            nameText.text = speaker;
            float t = 0;
            Image nameBoxImg = nameBoxObject.GetComponent<Image>();

            while (t < 1f)
            {
                t += Time.deltaTime / uiFadeDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                SetAlpha(nameText, smoothT);
                SetAlpha(dividerLine, smoothT);
                if (nameBoxImg != null) SetAlpha(nameBoxImg, smoothT);
                yield return null;
            }

            SetAlpha(nameText, 1f);
            SetAlpha(dividerLine, 1f);
            if (nameBoxImg != null) SetAlpha(nameBoxImg, 1f);

            lastSpeaker = speaker;
        }
        else
        {
            SetAlpha(nameText, 1f);
            SetAlpha(dividerLine, 1f);
            if (nameBoxObject.GetComponent<Image>() != null) SetAlpha(nameBoxObject.GetComponent<Image>(), 1f);
        }

        SetAlpha(contentText, 1f);
    }

    public void SetInlineEffects(string aID, float aWait, int aTrigger, string vID, float vWait, int vTrigger)
    {
        inlineAudioID = aID; inlineAudioWait = aWait; inlineAudioTrigger = aTrigger;
        inlineVisualID = vID; inlineVisualWait = vWait; inlineVisualTrigger = vTrigger;
        isInlineEffectWaiting = false;
    }

    public void ForceStopTyping()
    {
        isInlineEffectWaiting = false;
        inlineAudioID = ""; inlineVisualID = "";
    }

    public IEnumerator TypeTextRoutine(string text, bool isNarrator, string speaker)
    {
        TextMeshProUGUI activeText = isNarrator ? narratorText : contentText;
        activeText.text = text;
        activeText.maxVisibleCharacters = 0;

        AudioClip currentClip = null;

        // ★ 補回：向 Controller 查詢動態速度與靜音狀態
        float currentTypeSpeed = typeSpeed;
        bool muteAudio = false;
        if (playbackController != null)
        {
            currentTypeSpeed = playbackController.GetCurrentTypeSpeed(typeSpeed);
            muteAudio = playbackController.ShouldMuteAudio();
        }

        if (!isNarrator)
        {
            if (speaker != lastSoundSpeaker)
            {
                useSoundA = !useSoundA;
                lastSoundSpeaker = speaker;
            }
            currentClip = useSoundA ? soundA : soundB;
        }

        for (int i = 0; i <= text.Length; i++)
        {
            while (isDialoguePaused) yield return null;

            activeText.maxVisibleCharacters = i;

            // ★ 補回：Skip/Auto 時打字音靜音
            if (!muteAudio && currentClip != null && Time.time - lastSoundTime >= typingSoundCooldown && i < text.Length)
            {
                if (typingAudioSource != null) typingAudioSource.PlayOneShot(currentClip);
                lastSoundTime = Time.time;
            }

            bool triggerAudio = (!string.IsNullOrEmpty(inlineAudioID) && i == Mathf.Clamp(inlineAudioTrigger, 0, text.Length));
            bool triggerVisual = (!string.IsNullOrEmpty(inlineVisualID) && i == Mathf.Clamp(inlineVisualTrigger, 0, text.Length));

            if (triggerAudio || triggerVisual)
            {
                isInlineEffectWaiting = true;
                float waitTime = 0f;

                if (triggerAudio)
                {
                    PlayAudioEffect(inlineAudioID);
                    waitTime = Mathf.Max(waitTime, inlineAudioWait);
                    inlineAudioID = "";
                }

                if (triggerVisual)
                {
                    PlayVisualEffect(inlineVisualID);
                    waitTime = Mathf.Max(waitTime, inlineVisualWait);
                    inlineVisualID = "";
                }

                if (waitTime > 0f)
                {
                    yield return WaitWithLogPause(waitTime);
                }

                isInlineEffectWaiting = false;
            }

            // ★ 補回：套用 Auto 的極快打字速度 或 Skip 的瞬間完成
            if (i < text.Length)
            {
                if (currentTypeSpeed <= 0f) yield return null;
                else yield return new WaitForSeconds(currentTypeSpeed);
            }
        }

        lastSpeaker = speaker;
    }

    public void SnapToState(string speaker, string text, bool isNarrator)
    {
        if (isNarrator)
        {
            dialogueBoxContainer.SetActive(false);
            narratorText.gameObject.SetActive(true);
            narratorText.text = text;
            narratorText.maxVisibleCharacters = text.Length;
            SetAlpha(narratorText, 1f);
        }
        else
        {
            dialogueBoxContainer.SetActive(true);
            narratorText.gameObject.SetActive(false);

            nameText.text = speaker;
            contentText.text = text;
            contentText.maxVisibleCharacters = text.Length;

            SetAlpha(nameText, 1f);
            SetAlpha(contentText, 1f);
            SetAlpha(dividerLine, 1f);
            if (nameBoxObject.GetComponent<Image>() != null) SetAlpha(nameBoxObject.GetComponent<Image>(), 1f);
        }
        lastSpeaker = speaker;
    }

    public void PlayAudioEffect(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        // ★ 補回：如果 Auto 或 Skip，連劇情演出音效也靜音
        if (playbackController != null && playbackController.ShouldMuteAudio()) return;

        AudioClip clip = audioDatabase.Find(x => x.id == id).clip;
        if (clip != null && storyAudioSource != null)
        {
            storyAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayVisualEffect(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        GameObject prefab = visualDatabase.Find(x => x.id == id).visualPrefab;
        if (prefab != null)
        {
            Instantiate(prefab);
        }
    }

    public IEnumerator WaitWithLogPause(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!isDialoguePaused)
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }
    }

    public IEnumerator FadeBackground(float targetAlpha, float duration)
    {
        float startAlpha = backgroundImage.color.a;
        float t = 0;
        Color c = backgroundImage.color;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            backgroundImage.color = c;
            yield return null;
        }
    }

    public IEnumerator FadeOutAllRoutine()
    {
        yield return FadeBackground(0f, uiFadeDuration);

        float t = 0;
        Image nameBoxImg = nameBoxObject.GetComponent<Image>();
        Image boxImg = dialogueBoxContainer.GetComponent<Image>();

        while (t < 1f)
        {
            t += Time.deltaTime / uiFadeDuration;
            float invAlpha = 1f - t;

            SetAlpha(nameText, invAlpha);
            SetAlpha(contentText, invAlpha);
            SetAlpha(narratorText, invAlpha);
            SetAlpha(dividerLine, invAlpha);
            if (nameBoxImg != null) SetAlpha(nameBoxImg, invAlpha);
            if (boxImg != null) SetAlpha(boxImg, invAlpha);

            yield return null;
        }
        ClearContentOnly();
    }

    public void UpdateLogHistory(string speaker, string text, bool isNarrator)
    {
        if (isNarrator) fullLogHistory.Append(text).Append("\n\n");
        else fullLogHistory.Append("<b>").Append(speaker).Append("</b> : ").Append(text).Append("\n\n");

        if (logHistoryText != null) logHistoryText.text = fullLogHistory.ToString();
    }

    public bool IsLogOpen() => logPageObject.activeSelf;

    // ★ 補回：Log 的唯一開關入口
    public void ToggleLogPage()
    {
        if (logPageObject == null) return;
        if (IsLogOpen()) CloseLogPage();
        else OpenLogPage();
    }

    public void OpenLogPage() { StartCoroutine(OpenLogRoutine()); }
    private IEnumerator OpenLogRoutine()
    {
        isDialoguePaused = true;
        if (typingAudioSource != null) typingAudioSource.Stop();
        float duration = 0.4f;
        if (uiAudioSource != null && openLogClip != null) { uiAudioSource.PlayOneShot(openLogClip); duration = openLogClip.length; }

        if (!hasCachedLogPos) { logPageOriginalAnchoredPos = logPageObject.GetComponent<RectTransform>().anchoredPosition; hasCachedLogPos = true; }
        CanvasGroup cg = logPageObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = logPageObject.AddComponent<CanvasGroup>();

        Vector2 startPos = logPageOriginalAnchoredPos;
        if (logButtonRect != null && logPageObject.transform.parent != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(logPageObject.transform.parent as RectTransform, logButtonRect.position, null, out startPos);
        }

        logPageObject.SetActive(true); cg.alpha = 0f; logPageObject.transform.localScale = new Vector3(0.05f, 0.05f, 1f); logPageObject.GetComponent<RectTransform>().anchoredPosition = startPos;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; float t = Mathf.Clamp01(elapsed / duration);
            if (t < 0.7f)
            {
                float phaseT = t / 0.7f; float easeOutT = 1f - Mathf.Pow(1f - phaseT, 3f);
                float scale = Mathf.Lerp(0.05f, 1.05f, easeOutT);
                logPageObject.transform.localScale = new Vector3(scale, scale, 1f);
                logPageObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, logPageOriginalAnchoredPos, easeOutT);
                cg.alpha = Mathf.Clamp01(phaseT * 1.5f);
            }
            else
            {
                float phaseT = (t - 0.7f) / 0.3f; float smoothT = Mathf.SmoothStep(0f, 1f, phaseT);
                float scale = Mathf.Lerp(1.05f, 1f, smoothT);
                logPageObject.transform.localScale = new Vector3(scale, scale, 1f);
                logPageObject.GetComponent<RectTransform>().anchoredPosition = logPageOriginalAnchoredPos;
                cg.alpha = 1f;
            }
            yield return null;
        }
        logPageObject.transform.localScale = Vector3.one; logPageObject.GetComponent<RectTransform>().anchoredPosition = logPageOriginalAnchoredPos; cg.alpha = 1f;
    }

    public void CloseLogPage() { StartCoroutine(CloseLogRoutine()); }
    private IEnumerator CloseLogRoutine()
    {
        float duration = 0.4f;
        if (uiAudioSource != null && closeLogClip != null) { uiAudioSource.PlayOneShot(closeLogClip); duration = closeLogClip.length; }

        CanvasGroup cg = logPageObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = logPageObject.AddComponent<CanvasGroup>();

        Vector2 targetPos = logPageOriginalAnchoredPos;
        if (logButtonRect != null && logPageObject.transform.parent != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(logPageObject.transform.parent as RectTransform, logButtonRect.position, null, out targetPos);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; float t = Mathf.Clamp01(elapsed / duration);
            if (t < 0.3f)
            {
                float phaseT = t / 0.3f; float smoothT = Mathf.SmoothStep(0f, 1f, phaseT);
                float scale = Mathf.Lerp(1f, 1.05f, smoothT);
                logPageObject.transform.localScale = new Vector3(scale, scale, 1f);
                logPageObject.GetComponent<RectTransform>().anchoredPosition = logPageOriginalAnchoredPos; cg.alpha = 1f;
            }
            else
            {
                float phaseT = (t - 0.3f) / 0.7f; float easeInT = Mathf.Pow(phaseT, 3f);
                float scale = Mathf.Lerp(1.05f, 0.05f, easeInT);
                logPageObject.transform.localScale = new Vector3(scale, scale, 1f);
                logPageObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(logPageOriginalAnchoredPos, targetPos, easeInT);
                cg.alpha = Mathf.Lerp(1f, 0f, (phaseT - 0.2f) * 1.25f);
            }
            yield return null;
        }
        logPageObject.transform.localScale = Vector3.zero; logPageObject.GetComponent<RectTransform>().anchoredPosition = logPageOriginalAnchoredPos; cg.alpha = 0f; logPageObject.SetActive(false);
        isDialoguePaused = false;
    }

    private void SetAlpha(Graphic g, float alpha)
    {
        if (g == null) return;
        Color c = g.color; c.a = alpha; g.color = c;
    }
}