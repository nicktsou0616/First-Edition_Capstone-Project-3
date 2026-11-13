using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 【新增】為了使用 IEnumerator

public class AudioSettingsUI : MonoBehaviour
{
    [Header("UI 元件")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // --- 【新增】SFX 預覽延遲設定與 Coroutine 變數 ---
    [Header("SFX 預覽設定")]
    [SerializeField] private float sfxPreviewDelay = 0.25f;
    private Coroutine sfxPreviewCoroutine;
    // --------------------------------------------------

    private void Start()
    {
        // 1. 確認 AudioManager 存在
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("場景中找不到 AudioManager！請確認有將它放入場景。");
            return;
        }

        // 2. 初始化：將 Slider 的位置同步為目前的音量存檔
        bgmSlider.value = AudioManager.Instance.GetSavedBGMVolume();
        sfxSlider.value = AudioManager.Instance.GetSavedSFXVolume();

        // 3. 綁定事件
        // BGM 保持原本的行為，即時呼叫
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);

        // SFX 改為呼叫我們自定義的方法，以處理即時音量 + 延遲預覽 【修改】
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    // --- 【新增】處理 SFX Slider 變化的方法 ---
    private void OnSFXSliderChanged(float value)
    {
        // 1. 原本功能：即時改變 SFX 音量 (必須優先執行)
        AudioManager.Instance.SetSFXVolume(value);

        // 2. 取消之前的 Preview 等待計時
        if (sfxPreviewCoroutine != null)
        {
            StopCoroutine(sfxPreviewCoroutine);
        }

        // 3. 重新開始等待計時
        sfxPreviewCoroutine = StartCoroutine(DelayedSFXPreview());
    }

    // --- 【新增】處理延遲播放的 Coroutine ---
    private IEnumerator DelayedSFXPreview()
    {
        // 等待指定的時間 (例如 0.25 秒)
        yield return new WaitForSeconds(sfxPreviewDelay);

        // 呼叫 AudioManager 播放預覽音效
        AudioManager.Instance.PlaySFXVolumePreview();

        // 清空 Coroutine 變數
        sfxPreviewCoroutine = null;
    }
    // ----------------------------------------

    private void OnDestroy()
    {
        // 最佳實務：物件銷毀時解除事件綁定，避免記憶體洩漏
        if (AudioManager.Instance != null)
        {
            bgmSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetBGMVolume);
            // 【修改】將 RemoveListener 對應到新的方法
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        }

        // --- 【新增】如果 UI 被銷毀時還在倒數，就停止它 ---
        if (sfxPreviewCoroutine != null)
        {
            StopCoroutine(sfxPreviewCoroutine);
        }
        // ------------------------------------------------
    }
}