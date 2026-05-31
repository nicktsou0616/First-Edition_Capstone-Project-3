using UnityEngine;

public class SimpleSfxPlayer : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volumeScale = 1f;

    public void Play()
    {
        if (clip == null)
        {
            return;
        }

        AudioManager.EnsureExists().PlaySfx(clip, volumeScale);
    }
}
