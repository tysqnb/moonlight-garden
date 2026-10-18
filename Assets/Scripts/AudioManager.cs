using UnityEngine;

/// <summary>
/// One persistent audio layer for every screen. It keeps the ambience running
/// between scenes, plays short interaction cues, and reads the current page
/// aloud when the narration control is on.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public static bool SoundEnabled = true;
    public static bool NarrationEnabled = true;

    public AudioClip ambient;
    public AudioClip click;
    public AudioClip chime;
    public AudioClip water;
    public AudioClip wind;
    public AudioClip bloom;
    public AudioClip[] narration = new AudioClip[6];

    AudioSource ambientSource;
    AudioSource sfxSource;
    AudioSource voiceSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        voiceSource = gameObject.AddComponent<AudioSource>();

        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = 0.2f;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.9f;
        voiceSource.playOnAwake = false;
        voiceSource.volume = 0.95f;

        if (ambient != null)
        {
            ambientSource.clip = ambient;
            ambientSource.Play();
        }

        ApplySoundState();
    }

    public void PlayClick()
    {
        Play(click, 0.55f);
    }

    public void PlayChime()
    {
        Play(chime, 0.75f);
    }

    public void PlayWater()
    {
        Play(water, 0.85f);
    }

    public void PlayWind()
    {
        Play(wind, 0.9f);
    }

    public void PlayBloom()
    {
        Play(bloom, 0.9f);
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (!SoundEnabled || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayNarration(int page)
    {
        if (voiceSource == null || page < 0 || page >= narration.Length) return;
        voiceSource.Stop();
        if (!SoundEnabled || !NarrationEnabled || narration[page] == null) return;
        voiceSource.clip = narration[page];
        voiceSource.Play();
    }

    public void SetSound(bool enabled)
    {
        SoundEnabled = enabled;
        ApplySoundState();
    }

    public void SetNarration(bool enabled)
    {
        NarrationEnabled = enabled;
        if (!enabled && voiceSource != null) voiceSource.Stop();
    }

    void ApplySoundState()
    {
        if (ambientSource != null) ambientSource.mute = !SoundEnabled;
        if (sfxSource != null) sfxSource.mute = !SoundEnabled;
        if (voiceSource != null) voiceSource.mute = !SoundEnabled;
    }
}
