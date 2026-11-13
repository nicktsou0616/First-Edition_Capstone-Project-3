using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Singleton 實例
    public static AudioManager Instance { get; private set; }

    [Header("音效設定 (Audio Settings)")]
    [SerializeField] private AudioMixer audioMixer;

    // --- 【新增】SFX 預覽設定 ---
    [Header("SFX 預覽設定 (Preview Settings)")]
    [SerializeField] private AudioClip sfxPreviewClip;
    [SerializeField] private AudioSource sfxPreviewSource;
    // ----------------------------

    // 定義 PlayerPrefs 的存檔 Key
    private const string BGM_PREFS_KEY = "BGMVolume";
    private const string SFX_PREFS_KEY = "SFXVolume";

    // 定義 Audio Mixer 中 Exposed Parameters 的名稱
    private const string MIXER_BGM_PARAM = "BGMVolume";
    private const string MIXER_SFX_PARAM = "SFXVolume";

    private void Awake()
    {
        // 確保整個遊戲只有一個 AudioManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景不銷毀
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 遊戲啟動時讀取音量設定
        LoadVolumeSettings();
    }

    /// <summary>
    /// 設定 BGM 音量，供外部（如 UI）呼叫
    /// </summary>
    /// <param name="sliderValue">Slider 的值 (0.0 到 1.0)</param>
    public void SetBGMVolume(float sliderValue)
    {
        SetMixerVolume(MIXER_BGM_PARAM, sliderValue);
        PlayerPrefs.SetFloat(BGM_PREFS_KEY, sliderValue);
    }

    /// <summary>
    /// 設定 SFX 音量，供外部（如 UI）呼叫
    /// </summary>
    /// <param name="sliderValue">Slider 的值 (0.0 到 1.0)</param>
    public void SetSFXVolume(float sliderValue)
    {
        SetMixerVolume(MIXER_SFX_PARAM, sliderValue);
        PlayerPrefs.SetFloat(SFX_PREFS_KEY, sliderValue);
    }

    /// <summary>
    /// 將線性 Slider 值轉換為對數分貝 (dB) 並應用到 Mixer
    /// </summary>
    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        // 避免數值為 0 導致 Log10 計算出負無限大
        float clampedValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        // 將線性值轉換為 dB (Unity Mixer 的範圍通常是 -80dB 到 0dB)
        float dbValue = Mathf.Log10(clampedValue) * 20f;

        audioMixer.SetFloat(parameterName, dbValue);
    }

    /// <summary>
    /// 讀取存檔，若無存檔則預設為 1f (100%)
    /// </summary>
    private void LoadVolumeSettings()
    {
        float savedBGM = GetSavedBGMVolume();
        float savedSFX = GetSavedSFXVolume();

        SetMixerVolume(MIXER_BGM_PARAM, savedBGM);
        SetMixerVolume(MIXER_SFX_PARAM, savedSFX);
    }

    // 提供給 UI 讀取當前設定值的方法
    public float GetSavedBGMVolume() => PlayerPrefs.GetFloat(BGM_PREFS_KEY, 1f);
    public float GetSavedSFXVolume() => PlayerPrefs.GetFloat(SFX_PREFS_KEY, 1f);

    // --- 【新增】播放 SFX 預覽音效的方法 ---
    public void PlaySFXVolumePreview()
    {
        // 確保 Clip 與 Source 都有設定，避免 NullReferenceException
        if (sfxPreviewSource != null && sfxPreviewClip != null)
        {
            sfxPreviewSource.PlayOneShot(sfxPreviewClip);
        }
        else
        {
            Debug.LogWarning("SFX Preview 播放失敗：請確認 AudioManager 的 Inspector 中有指定 Clip 與 Source！");
        }
    }
    // ----------------------------------------

    // NEW: Button SFX - 提供一個通用的方法來播放指定的 AudioClip，優先使用現有的 sfxPreviewSource
    public void PlaySFX(AudioClip clip)
    {
        if (sfxPreviewSource != null && clip != null)
        {
            sfxPreviewSource.PlayOneShot(clip);
        }
        else if (clip != null)
        {
            Debug.LogWarning("UI SFX 播放失敗：AudioManager 的 sfxPreviewSource 未設定！");
        }
    }
}