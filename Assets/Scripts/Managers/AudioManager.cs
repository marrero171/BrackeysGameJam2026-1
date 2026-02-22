using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private bool playOnStart = true;

    [Header("SFX Settings")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void Start()
    {
        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        Debug.Log("[AudioManager] Audio sources initialized");
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Cannot play null music clip");
            return;
        }

        if (musicSource.isPlaying && musicSource.clip == clip)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
        Debug.Log($"[AudioManager] Playing music: {clip.name}");
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Music stopped");
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("[AudioManager] Music paused");
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
            Debug.Log("[AudioManager] Music resumed");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Cannot play null SFX clip");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        Debug.Log($"[AudioManager] Music volume set to {musicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        Debug.Log($"[AudioManager] SFX volume set to {sfxVolume}");
    }

    public void MuteMusic(bool mute)
    {
        if (musicSource != null)
        {
            musicSource.mute = mute;
            Debug.Log($"[AudioManager] Music {(mute ? "muted" : "unmuted")}");
        }
    }

    public void MuteSFX(bool mute)
    {
        if (sfxSource != null)
        {
            sfxSource.mute = mute;
            Debug.Log($"[AudioManager] SFX {(mute ? "muted" : "unmuted")}");
        }
    }

    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
}
