using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Scene Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool restartIfSameClip;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolumeScale = 0.35f;
    [SerializeField] private bool restoreMasterVolumeIfMuted;
    [Range(0f, 1f)]
    [SerializeField] private float restoredMasterVolume = 1f;
    [Min(0f)]
    [SerializeField] private float fadeDuration = 0.25f;

    private void Start()
    {
        if (!playOnStart)
        {
            return;
        }

        Play();
    }

    public void Play()
    {
        if (backgroundMusic == null)
        {
            return;
        }

        if (restoreMasterVolumeIfMuted && AudioManager.GetMasterVolume() <= 0f)
        {
            AudioManager.SetMasterVolume(restoredMasterVolume);
        }

        AudioManager.EnsureExists().PlayMusic(backgroundMusic, loop, fadeDuration, restartIfSameClip, musicVolumeScale);
    }

    public void Stop()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeDuration);
        }
    }
}
