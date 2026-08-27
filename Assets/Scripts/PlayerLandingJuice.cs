using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController), typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerLandingJuice : MonoBehaviour
{
    [Header("Overall Strength")]
    [Tooltip("Master multiplier for dust amount, size, and speed.")]
    [SerializeField, Range(0.25f, 3f)] private float dustIntensity = 1.7f;
    [Tooltip("Master multiplier for landing shake. 3.2 is obvious but still below a major-event shake.")]
    [SerializeField, Range(0.25f, 6f)] private float shakeStrength = 3.2f;

    [Header("Landing Dust")]
    [SerializeField] private bool enableLandingDust = true;
    [SerializeField] private Color dustColor = new Color(0.86f, 0.82f, 0.72f, 0.75f);
    [SerializeField, Range(1, 20)] private int landingParticleCount = 8;
    [SerializeField] private float minLandingImpact = 3f;
    [SerializeField] private float maxLandingImpact = 18f;
    [SerializeField] private Vector2 landingDustSpeed = new Vector2(0.8f, 1.8f);

    [Header("Movement Dust")]
    [SerializeField] private bool enableMovementDust = true;
    [SerializeField] private float movementSpeedThreshold = 1.5f;
    [SerializeField, Min(0.03f)] private float movementDustInterval = 0.13f;
    [SerializeField, Range(1, 5)] private int movementParticleCount = 2;

    [Header("Dust Shape")]
    [SerializeField] private Vector2 particleLifetime = new Vector2(0.22f, 0.4f);
    [SerializeField] private Vector2 particleSize = new Vector2(0.12f, 0.28f);
    [SerializeField] private float footOffsetY = 0.02f;

    [Header("Subtle Landing Shake")]
    [SerializeField] private bool enableLandingShake = true;
    [Tooltip("World units. Keep this near 0.03-0.07 for a subtle landing.")]
    [SerializeField, Range(0f, 0.2f)] private float shakeMagnitude = 0.05f;
    [SerializeField, Range(0.02f, 0.3f)] private float shakeDuration = 0.09f;

    private PlayerController controller;
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private ParticleSystem dust;
    private CinemachineImpulseSource impulseSource;
    private float nextMovementDustTime;
    private Material dustMaterial;
    private Texture2D dustTexture;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        dust = CreateDustSystem();
    }

    void OnEnable()
    {
        controller.Landed += HandleLanded;
    }

    void OnDisable()
    {
        controller.Landed -= HandleLanded;
    }

    void OnDestroy()
    {
        if (dustMaterial != null)
        {
            Destroy(dustMaterial);
        }

        if (dustTexture != null)
        {
            Destroy(dustTexture);
        }
    }

    void Update()
    {
        if (!enableMovementDust || !controller.IsGrounded)
        {
            return;
        }

        float speed = Mathf.Abs(rb.linearVelocity.x);
        if (speed < movementSpeedThreshold || Time.time < nextMovementDustTime)
        {
            return;
        }

        nextMovementDustTime = Time.time + movementDustInterval;
        int count = Mathf.Max(1, Mathf.RoundToInt(movementParticleCount * dustIntensity));
        EmitDust(count, 0.45f * dustIntensity, -Mathf.Sign(rb.linearVelocity.x));
    }

    private void HandleLanded(float impactSpeed)
    {
        float impact = Mathf.InverseLerp(minLandingImpact, maxLandingImpact, impactSpeed);
        if (impactSpeed < minLandingImpact)
        {
            return;
        }

        if (enableLandingDust)
        {
            int count = Mathf.Max(
                1,
                Mathf.RoundToInt(landingParticleCount * dustIntensity * Mathf.Lerp(0.55f, 1f, impact)));
            EmitDust(count, dustIntensity * Mathf.Lerp(0.7f, 1f, impact), 0f);
        }

        if (enableLandingShake)
        {
            EnsureImpulseSystem();

            float duration =
                shakeDuration * Mathf.Sqrt(shakeStrength) * Mathf.Lerp(0.75f, 1f, impact);
            float strength =
                shakeMagnitude * shakeStrength * Mathf.Lerp(0.45f, 1f, impact);

            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            impulseSource.GenerateImpulseWithVelocity(
                new Vector3(Random.Range(-0.2f, 0.2f), -1f, 0f).normalized * strength);
        }
    }

    private void EmitDust(int count, float intensity, float directionBias)
    {
        if (dust == null)
        {
            return;
        }

        if (!dust.isPlaying)
        {
            dust.Play();
        }

        dust.transform.position = new Vector3(
            bodyCollider.bounds.center.x,
            bodyCollider.bounds.min.y + footOffsetY,
            transform.position.z);

        for (int i = 0; i < count; i++)
        {
            float horizontal = directionBias == 0f
                ? Random.Range(-1f, 1f)
                : Mathf.Clamp(directionBias + Random.Range(-0.55f, 0.55f), -1f, 1f);

            var particle = new ParticleSystem.EmitParams
            {
                position = dust.transform.position,
                velocity = new Vector3(
                    horizontal * Random.Range(landingDustSpeed.x, landingDustSpeed.y) * intensity,
                    Random.Range(0.2f, 0.8f) * intensity,
                    0f),
                startLifetime = Random.Range(particleLifetime.x, particleLifetime.y),
                startSize = Random.Range(particleSize.x, particleSize.y) * intensity,
                startColor = dustColor,
                rotation = Random.Range(0f, Mathf.PI * 2f)
            };

            dust.Emit(particle, 1);
        }
    }

    private ParticleSystem CreateDustSystem()
    {
        var dustObject = new GameObject("Landing Dust");
        dustObject.transform.SetParent(transform, false);

        var particles = dustObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 48;
        main.gravityModifier = 0.15f;

        var emission = particles.emission;
        emission.enabled = false;

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0f, 1f)
                },
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                }
            });

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f, new AnimationCurve(
                new Keyframe(0f, 0.45f),
                new Keyframe(0.25f, 1f),
                new Keyframe(1f, 1.35f)));

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sharedMaterial = CreateDustMaterial();

        SpriteRenderer playerRenderer = GetComponentInChildren<SpriteRenderer>();
        if (playerRenderer != null)
        {
            particleRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            particleRenderer.sortingOrder = playerRenderer.sortingOrder - 1;
        }

        return particles;
    }

    private Material CreateDustMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            return null;
        }

        dustTexture = CreateSoftCircleTexture(32);
        dustMaterial = new Material(shader)
        {
            name = "Runtime Dust Material",
            mainTexture = dustTexture,
            hideFlags = HideFlags.DontSave
        };
        return dustMaterial;
    }

    private static Texture2D CreateSoftCircleTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Runtime Dust Puff",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (x + 0.5f) / size * 2f - 1f;
                float normalizedY = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                float alpha = 1f - Mathf.SmoothStep(0.35f, 1f, distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private void EnsureImpulseSystem()
    {
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }

            if (impulseSource.ImpulseDefinition == null)
            {
                impulseSource.ImpulseDefinition = new CinemachineImpulseDefinition();
            }

            impulseSource.ImpulseDefinition.ImpulseShape =
                CinemachineImpulseDefinition.ImpulseShapes.Bump;
            impulseSource.ImpulseDefinition.ImpulseDuration = shakeDuration;
            impulseSource.DefaultVelocity = Vector3.down;
        }

        CinemachineCamera virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogWarning("[PlayerLandingJuice] CinemachineCamera bulunamadi; ekran sarsintisi calisamaz.", this);
            return;
        }

        CinemachineImpulseListener listener =
            virtualCamera.GetComponent<CinemachineImpulseListener>();
        if (listener == null)
        {
            listener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        }

        listener.ChannelMask = -1;
        listener.Gain = 1f;
        listener.Use2DDistance = true;
    }
}
