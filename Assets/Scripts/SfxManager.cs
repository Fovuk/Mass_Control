using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    private const string MusicVolumeKey = "Settings_MusicVolume";
    private const string SfxVolumeKey = "Settings_SfxVolume";

    [SerializeField] private SfxLibrary library;

    [Header("Music Volumes")]
    [SerializeField, Range(0f, 1f)] private float mainMenuMusicVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float inGameMusicVolume = 0.2f;
    [SerializeField] private float defaultMusicVolume = 0.45f;
    [SerializeField] private float defaultSfxVolume = 1f;
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("SFX Volumes")]
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float buttonVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float walkingVolume = 0.85f;
    [SerializeField, Range(0f, 1f)] private float sizeChangeVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    [Header("Pitch Variation")]
    [Tooltip("Subtle random pitch range for jump and size-change SFX. 1 is normal.")]
    [SerializeField, Range(0.8f, 1f)] private float randomPitchMin = 0.94f;
    [SerializeField, Range(1f, 1.2f)] private float randomPitchMax = 1.06f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource walkingSource;
    private Coroutine musicFadeCoroutine;
    private bool walking;
    private float lastMusicTarget;
    private float lastSfxPreviewTime = -1f;

    private AudioClip fallbackJump;
    private AudioClip fallbackButton;
    private AudioClip fallbackWalk;
    private AudioClip fallbackSizeChange;
    private AudioClip fallbackDeath;

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.ignoreListenerPause = true;
        sfxSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;

        walkingSource = gameObject.AddComponent<AudioSource>();
        walkingSource.playOnAwake = false;
        walkingSource.loop = true;
        walkingSource.spatialBlend = 0f;

        if (library == null)
        {
            library = Resources.Load<SfxLibrary>("SfxLibrary");
        }

        LoadVolumes();
        CreateFallbackClips();
        ResolveLibraryClips();
        ApplySfxVolume();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void Start()
    {
        AddButtonSounds();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }

        DestroyFallbackClips();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.volume = ScaledMusicVolume(lastMusicTarget);
        }
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        ApplySfxVolume();

        if (Time.unscaledTime - lastSfxPreviewTime > 0.12f)
        {
            lastSfxPreviewTime = Time.unscaledTime;
            PlayJump();
        }
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(library?.mainMenuMusic, mainMenuMusicVolume);
    }

    public void PlayInGameMusic()
    {
        PlayMusic(library?.inGameMusic, inGameMusicVolume);
    }

    private void PlayMusic(AudioClip clip, float targetVolume)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        lastMusicTarget = targetVolume;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();
        FadeMusicTo(ScaledMusicVolume(targetVolume));
    }

    public void StopInGameMusic()
    {
        if (musicSource == null || !musicSource.isPlaying)
        {
            return;
        }

        FadeMusicTo(0f, () => musicSource.Stop());
    }

    public void PlayJump()
    {
        Play(ResolveClip(library?.jump, "Audio/JumpSfx") ?? fallbackJump, jumpVolume, true);
    }

    public void PlayButtonClick()
    {
        Play(ResolveClip(library?.buttonClick, "Audio/ButtonClick") ?? fallbackButton, buttonVolume);
    }

    public void SetWalking(bool isWalking)
    {
        if (walking == isWalking || walkingSource == null)
        {
            return;
        }

        walking = isWalking;
        if (walking)
        {
            walkingSource.clip = ResolveClip(library?.walk, "Audio/Walking") ?? fallbackWalk;
            walkingSource.volume = ScaledSfxVolume(walkingVolume);
            if (walkingSource.clip != null)
            {
                walkingSource.clip.LoadAudioData();
                walkingSource.Play();
            }
        }
        else
        {
            walkingSource.Stop();
        }
    }

    public void PlaySizeChange()
    {
        Play(library?.sizeChange ?? fallbackSizeChange, sizeChangeVolume, true);
    }

    public void PlayStarCollect()
    {
        Play(library?.starCollect);
    }

    public void PlayDeath(Action onComplete)
    {
        SetWalking(false);
        AudioClip clip = library?.death ?? fallbackDeath;
        Play(clip, deathVolume);
        StartCoroutine(WaitForClip(clip != null ? clip.length : 0f, onComplete));
    }

    public void PlayLevelComplete()
    {
        Play(library?.levelComplete);
    }

    private void LoadVolumes()
    {
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume));
    }

    private void ApplySfxVolume()
    {
        if (walkingSource != null && walking)
        {
            walkingSource.volume = ScaledSfxVolume(walkingVolume);
        }
    }

    private float ScaledMusicVolume(float volume)
    {
        return volume * MusicVolume;
    }

    private float ScaledSfxVolume(float volume)
    {
        return volume * SfxVolume;
    }

    private void Play(AudioClip clip, float volume = 1f, bool randomizePitch = false)
    {
        if (clip == null || sfxSource == null || SfxVolume <= 0f)
        {
            return;
        }

        float previousPitch = sfxSource.pitch;
        if (randomizePitch)
        {
            float minPitch = Mathf.Min(randomPitchMin, randomPitchMax);
            float maxPitch = Mathf.Max(randomPitchMin, randomPitchMax);
            sfxSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        }

        sfxSource.PlayOneShot(clip, ScaledSfxVolume(volume));
        sfxSource.pitch = previousPitch;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetWalking(false);
        AddButtonSounds();
    }

    private void ResolveLibraryClips()
    {
        if (library == null)
        {
            return;
        }

        if (library.jump == null)
        {
            library.jump = Resources.Load<AudioClip>("Audio/JumpSfx");
        }

        if (library.buttonClick == null)
        {
            library.buttonClick = Resources.Load<AudioClip>("Audio/ButtonClick");
        }

        if (library.walk == null)
        {
            library.walk = Resources.Load<AudioClip>("Audio/Walking");
        }
    }

    private static AudioClip ResolveClip(AudioClip clip, string resourcePath)
    {
        return clip != null ? clip : Resources.Load<AudioClip>(resourcePath);
    }

    private void AddButtonSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button button in buttons)
        {
            if (button.GetComponent<OnScreenButton>() != null)
            {
                continue;
            }

            if (button.GetComponent<ButtonClickSfx>() == null)
            {
                button.gameObject.AddComponent<ButtonClickSfx>();
            }
        }
    }

    private void CreateFallbackClips()
    {
        fallbackJump = CreateSweep("Jump Fallback", 0.13f, 280f, 680f, 0.05f);
        fallbackButton = CreateSweep("Button Fallback", 0.055f, 700f, 920f, 0.02f);
        fallbackWalk = CreateSweep("Walk Fallback", 0.075f, 115f, 75f, 0.35f);
        fallbackSizeChange = CreateSweep("Size Change Fallback", 0.2f, 180f, 520f, 0.12f);
        fallbackDeath = CreateSweep("Death Fallback", 0.55f, 320f, 65f, 0.18f);
    }

    private static AudioClip CreateSweep(
        string clipName,
        float duration,
        float startFrequency,
        float endFrequency,
        float noiseAmount)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.CeilToInt(duration * sampleRate);
        var samples = new float[sampleCount];
        var random = new System.Random(clipName.GetHashCode());
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            phase += 2f * Mathf.PI * frequency / sampleRate;

            float attack = Mathf.Clamp01(t / 0.04f);
            float release = 1f - Mathf.SmoothStep(0.15f, 1f, t);
            float envelope = attack * release;
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            samples[i] =
                (Mathf.Sin(phase) * (1f - noiseAmount) + noise * noiseAmount)
                * envelope * 0.55f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void DestroyFallbackClips()
    {
        Destroy(fallbackJump);
        Destroy(fallbackButton);
        Destroy(fallbackWalk);
        Destroy(fallbackSizeChange);
        Destroy(fallbackDeath);
    }

    private void FadeMusicTo(float targetVolume, Action onComplete = null)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(targetVolume, onComplete));
    }

    private IEnumerator FadeMusicCoroutine(float targetVolume, Action onComplete)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = musicFadeDuration > 0f ? elapsed / musicFadeDuration : 1f;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFadeCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator WaitForClip(float duration, Action onComplete)
    {
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
        }

        onComplete?.Invoke();
    }
}
