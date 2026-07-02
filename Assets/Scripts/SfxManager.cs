using System;
using System.Collections;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [SerializeField] private SfxLibrary library;

    private AudioSource sfxSource;

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

    private IEnumerator WaitForClip(float duration, Action onComplete)
    {
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
        }

        onComplete?.Invoke();
    }
}
