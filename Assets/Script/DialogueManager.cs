using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]

public class CharacterProfile
{
    public Image characterImage;
    public string characterName;
}

[System.Serializable]
public class DialogueLine
{
    public Image speakerImage;
    public string overrideName;
    [TextArea(2, 5)]
    public string content;
}

public class DialogueManager : MonoBehaviour
{
    [Header("👥 第一步：演員名單")]
    public List<CharacterProfile> characterProfiles = new List<CharacterProfile>();

    [Header("📖 第二步：劇本設定")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentIndex = 0;

    [Header("🖥️ 遊戲層 UI - 一般角色對話")]
    public RectTransform dialogueBoxContainer;
    public GameObject nameBoxObject;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;

    [Header("🖥️ 遊戲層 UI - 旁白專用系統")]
    [Tooltip("請把新建的 NarratorText 拖進來")]
    public TextMeshProUGUI narratorText;
    public Color normalTextColor = Color.white;
    public Color narratorTextColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("✨ 特效綁定區")]
    public float typeSpeed = 0.03f;
    public float slideDistance = 300f;
    public float slideDuration = 0.3f;

    private Vector2 originalBoxPosition;
    private Coroutine typingCoroutine;
    private Coroutine slideCoroutine;
    private bool isTyping = false;

    private TextMeshProUGUI currentActiveText;

    [Header("🎨 立繪明暗與顏色設定")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("🖥️ LOG 獨立頁面 UI")]
    public GameObject logPageObject;
    public TextMeshProUGUI logHistoryText;

    private string fullLogHistory = "";

    public RectTransform contentSlideTarget;//這個和下面那個都是為了讓人名不要有滑入特效
    private Vector2 originalContentPosition;

    void Start()
    {
        logPageObject.SetActive(false);
        if (dialogueBoxContainer != null) originalBoxPosition = dialogueBoxContainer.anchoredPosition;

        if (narratorText != null) narratorText.gameObject.SetActive(false);

        foreach (var profile in characterProfiles)
        {
            if (profile.characterImage != null) profile.characterImage.gameObject.SetActive(false);
        }

        if (dialogueLines.Count > 0) PlayLine(0);
        
        if (contentSlideTarget == null && contentText != null)
        contentSlideTarget = contentText.rectTransform;

        if (contentSlideTarget != null)
            originalContentPosition = contentSlideTarget.anchoredPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()) return;
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !logPageObject.activeSelf)
        {
            if (isTyping) FinishAnimationsEarly();
            else NextLine();
        }
    }

    public void PlayLine(int index)
    {
        DialogueLine line = dialogueLines[index];
        string currentSpeakerName = "";
        float startOffsetX = 0f;

        if (!string.IsNullOrEmpty(line.overrideName)) currentSpeakerName = line.overrideName;
        else if (line.speakerImage != null)
        {
            foreach (var profile in characterProfiles)
            {
                if (profile.characterImage == line.speakerImage)
                {
                    currentSpeakerName = profile.characterName;
                    break;
                }
            }
            if (line.speakerImage.transform.position.x < Screen.width / 2f) startOffsetX = -slideDistance;
            else startOffsetX = slideDistance;
        }

        bool isNarrator = string.IsNullOrEmpty(currentSpeakerName) && line.speakerImage == null;

        if (isNarrator)
        {
            if (dialogueBoxContainer != null) dialogueBoxContainer.gameObject.SetActive(false);

            if (narratorText != null)
            {
                narratorText.gameObject.SetActive(true);
                narratorText.color = narratorTextColor;
            }
            currentActiveText = narratorText;
        }
        else
        {
            if (dialogueBoxContainer != null) dialogueBoxContainer.gameObject.SetActive(true);
            if (nameBoxObject != null) nameBoxObject.SetActive(true);

            if (narratorText != null) narratorText.gameObject.SetActive(false);

            nameText.text = currentSpeakerName;
            contentText.color = normalTextColor;
            currentActiveText = contentText;

            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlideInRoutine(startOffsetX));
        }

        foreach (var profile in characterProfiles)
        {
            if (profile.characterImage == null) continue;

            if (line.speakerImage != null)
            {
                if (profile.characterImage == line.speakerImage)
                {
                    profile.characterImage.gameObject.SetActive(true);
                    profile.characterImage.color = activeColor;
                    profile.characterImage.transform.SetAsLastSibling();
                }
                else
                {
                    float dist = Vector2.Distance(profile.characterImage.rectTransform.anchoredPosition, line.speakerImage.rectTransform.anchoredPosition);
                    if (dist < 100f) profile.characterImage.gameObject.SetActive(false);
                    else if (profile.characterImage.gameObject.activeSelf) profile.characterImage.color = inactiveColor;
                }
            }
            else
            {
                profile.characterImage.gameObject.SetActive(false);
            }
        }

        if (isNarrator) fullLogHistory += $"{line.content}\n\n";
        else fullLogHistory += $"<b>{currentSpeakerName}</b> : {line.content}\n\n";
        logHistoryText.text = fullLogHistory;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (currentActiveText != null) currentActiveText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeWriterRoutine(line.content));
    }

    private IEnumerator SlideInRoutine(float offsetX)
    {
        if (contentSlideTarget == null) yield break;

        Vector2 startPos = originalContentPosition + new Vector2(offsetX, 0);
        Vector2 targetPos = originalContentPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            contentSlideTarget.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        contentSlideTarget.anchoredPosition = targetPos;
    }

    private IEnumerator TypeWriterRoutine(string textToType)
    {
        if (currentActiveText == null) yield break;

        isTyping = true;
        currentActiveText.text = textToType;
        currentActiveText.maxVisibleCharacters = 0;
        int totalChars = textToType.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            currentActiveText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    private void FinishAnimationsEarly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        if (currentActiveText != null) currentActiveText.maxVisibleCharacters = currentActiveText.text.Length;

        if (contentSlideTarget != null)
            contentSlideTarget.anchoredPosition = originalContentPosition;

        isTyping = false;
    }

    public void NextLine()
    {
        currentIndex++;
        if (currentIndex < dialogueLines.Count) PlayLine(currentIndex);
        else
        {
            Debug.Log("對話結束，模組自動關閉。");

            // ★ 關鍵修正：向上尋找爸爸，把整個最外層的對話模組關掉！
            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void OpenLogPage() { logPageObject.SetActive(true); Canvas.ForceUpdateCanvases(); }
    public void CloseLogPage() { logPageObject.SetActive(false); }
}