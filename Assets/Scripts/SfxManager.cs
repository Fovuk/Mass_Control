using System;
using System.Collections;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [SerializeField] private SfxLibrary library;
    [SerializeField] private float musicVolume = 0.45f;
    [SerializeField] private float musicFadeDuration = 1.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Coroutine musicFadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.ignoreListenerPause = true;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.ignoreListenerPause = true;

        if (library == null)
        {
            library = Resources.Load<SfxLibrary>("SfxLibrary");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(library?.mainMenuMusic);
    }

    public void PlayInGameMusic()
    {
        PlayMusic(library?.inGameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();
        FadeMusicTo(musicVolume);
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
        Play(library?.jump);
    }

    public void PlayStarCollect()
    {
        Play(library?.starCollect);
    }

    public void PlayDeath(Action onComplete)
    {
        if (library?.death == null)
        {
            onComplete?.Invoke();
            return;
        }

        Play(library.death);
        StartCoroutine(WaitForClip(library.death.length, onComplete));
    }

    public void PlayLevelComplete()
    {
        Play(library?.levelComplete);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
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
