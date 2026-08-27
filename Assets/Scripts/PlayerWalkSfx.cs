using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerWalkSfx : MonoBehaviour
{
    [SerializeField] private AudioClip walkingClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.85f;
    [SerializeField] private float minSpeed = 0.2f;

    private PlayerController controller;
    private Rigidbody2D rb;
    private AudioSource source;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (walkingClip == null)
        {
            walkingClip = Resources.Load<AudioClip>("Audio/Walking");
        }

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.clip = walkingClip;
        if (walkingClip != null)
        {
            walkingClip.LoadAudioData();
        }
    }

    void Update()
    {
        bool shouldPlay =
            enabled
            && walkingClip != null
            && controller.IsGrounded
            && Mathf.Abs(rb.linearVelocity.x) >= minSpeed;

        if (shouldPlay)
        {
            source.volume = volume;
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
        else if (source.isPlaying)
        {
            source.Stop();
        }
    }

    void OnDisable()
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }
}
