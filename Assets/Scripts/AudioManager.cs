using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    private const string MasterVolumeKey = "Audio.MasterVolume";
    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";

    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Coroutine fadeRoutine;
    private float activeMusicVolumeScale = 1f;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    public static AudioManager EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject("AudioManager");
        return managerObject.AddComponent<AudioManager>();
    }

    public static float GetMasterVolume()
    {
        if (Instance != null)
        {
            return Instance.masterVolume;
        }

        return PlayerPrefs.GetFloat(MasterVolumeKey, AudioListener.volume);
    }

    public static void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (Application.isPlaying)
        {
            EnsureExists().SetMasterVolumeInternal(volume, true);
            return;
        }

        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        LoadVolumes();
        ApplyVolumes();
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0f, bool restartIfSameClip = false, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        EnsureSources();
        activeMusicVolumeScale = Mathf.Clamp01(volumeScale);

        if (!restartIfSameClip && musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.loop = loop;
            musicSource.volume = GetTargetMusicVolume();
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (fadeDuration > 0f && musicSource.isPlaying)
        {
            fadeRoutine = StartCoroutine(FadeToMusic(clip, loop, fadeDuration));
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = GetTargetMusicVolume();
        musicSource.Play();
    }

    public void StopMusic(float fadeDuration = 0f)
    {
        EnsureSources();

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (fadeDuration > 0f && musicSource.isPlaying)
        {
            fadeRoutine = StartCoroutine(FadeOutMusic(fadeDuration));
            return;
        }

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        EnsureSources();
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    private void SetMasterVolumeInternal(float volume, bool save)
    {
        masterVolume = Mathf.Clamp01(volume);

        if (save)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.Save();
        }

        ApplyVolumes();
    }

    private void EnsureSources()
    {
        if (musicSource == null)
        {
            musicSource = CreateSource("MusicSource");
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = CreateSource("SfxSource");
            sfxSource.loop = false;
        }
    }

    private AudioSource CreateSource(string sourceName)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    private void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
    }

    private void ApplyVolumes()
    {
        AudioListener.volume = masterVolume;

        if (musicSource != null)
        {
            musicSource.volume = GetTargetMusicVolume();
        }
    }

    private float GetTargetMusicVolume()
    {
        return musicVolume * activeMusicVolumeScale;
    }

    private IEnumerator FadeToMusic(AudioClip clip, bool loop, float duration)
    {
        float startVolume = musicSource.volume;
        float targetVolume = GetTargetMusicVolume();

        for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        fadeRoutine = null;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;

        for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = GetTargetMusicVolume();
        fadeRoutine = null;
    }
}
