using UnityEngine;

// Yay tabanli squash & stretch. Bu component Player kokunde durur ama
// SADECE 'visual' child transform'unu olcekler; Rigidbody2D ve Collider2D
// bulunan kok obje hicbir zaman olceklenmez, yani hitbox etkilenmez.
[RequireComponent(typeof(PlayerController))]
public class PlayerSquashStretch : MonoBehaviour
{
    [Header("Referans")]
    [Tooltip("Sadece bu transform olceklenir. SpriteRenderer'in bulundugu child obje.")]
    [SerializeField] private Transform visual;

    [Header("Ziplama / Inis Siddeti")]
    [Tooltip("Ziplarken dikey uzama miktari. 0.28 = %28 uzar.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float jumpStretch = 0.28f;
    [Tooltip("Yere inerken ezilme miktari.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float landSquash = 0.32f;
    [Tooltip("Bu dusus hizinin altindaki inisler minimum ezilme uretir.")]
    [SerializeField] private float minImpactSpeed = 4f;
    [Tooltip("Bu dusus hizi ve ustu tam ezilme uretir.")]
    [SerializeField] private float maxImpactSpeed = 22f;

    [Header("Havada Esneme")]
    [Tooltip("Havadayken dikey hiza gore surekli esneme uygulanir.")]
    [SerializeField] private bool stretchWithAirVelocity = true;
    [Tooltip("Birim dikey hiz basina esneme.")]
    [SerializeField] private float airStretchPerSpeed = 0.012f;
    [Range(0f, 0.9f)]
    [SerializeField] private float maxAirStretch = 0.18f;

    [Header("Yay (Elastik Geri Donus)")]
    [Tooltip("Yay frekansi (Hz). Yuksek = daha hizli titresim.")]
    [SerializeField] private float springFrequency = 4.5f;
    [Tooltip("0'a yakin = uzun sure zipar, 1 = hic zipramadan durur. 0.2-0.4 arasi canli hissettirir.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float springDamping = 0.28f;
    [Tooltip("Deformasyonun ulasabilecegi tavan. Sprite'in asiri bozulmasini engeller.")]
    [Range(0f, 0.9f)]
    [SerializeField] private float maxDeform = 0.45f;
    [Tooltip("Acikken dikeyde uzayinca yatayda incelir (hacim korunur).")]
    [SerializeField] private bool preserveVolume = true;

    // Kare atlamalarinda yayin patlamasini engellemek icin ust sinir.
    private const float MaxTimeStep = 1f / 30f;

    private PlayerController controller;
    private Rigidbody2D rb;
    private Vector3 baseScale;

    // +deger dikey uzama, -deger ezilme.
    private float deform;
    private float deformVelocity;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (visual == null)
        {
            visual = FindOrCreateVisual();
        }

        baseScale = visual.localScale;
    }

    void OnEnable()
    {
        controller.Jumped += HandleJumped;
        controller.Landed += HandleLanded;
    }

    void OnDisable()
    {
        controller.Jumped -= HandleJumped;
        controller.Landed -= HandleLanded;
    }

    void LateUpdate()
    {
        Integrate(Mathf.Min(Time.deltaTime, MaxTimeStep));
        ApplyToVisual();
    }

    // Sonlu farkli sonumlu yay: hedefe dogru cekerken sonum orani kadar salinim birakir.
    private void Integrate(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        float target = TargetDeform();
        float omega = 2f * Mathf.PI * springFrequency;
        float acceleration = -(omega * omega) * (deform - target) - 2f * springDamping * omega * deformVelocity;

        deformVelocity += acceleration * deltaTime;
        deform = Mathf.Clamp(deform + deformVelocity * deltaTime, -maxDeform, maxDeform);
    }

    private float TargetDeform()
    {
        if (!stretchWithAirVelocity || rb == null || controller.IsGrounded)
        {
            return 0f;
        }

        return Mathf.Clamp(rb.linearVelocity.y * airStretchPerSpeed, -maxAirStretch, maxAirStretch);
    }

    private void ApplyToVisual()
    {
        float scaleY = 1f + deform;
        float scaleX = preserveVolume ? 1f / Mathf.Max(scaleY, 0.01f) : 1f - deform;

        visual.localScale = new Vector3(baseScale.x * scaleX, baseScale.y * scaleY, baseScale.z);
    }

    private void HandleJumped()
    {
        Stretch(jumpStretch);
    }

    private void HandleLanded(float impactSpeed)
    {
        if (impactSpeed < 1f)
        {
            return;
        }

        float impact = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed);
        Squash(landSquash * Mathf.Lerp(0.35f, 1f, impact));
    }

    // Disaridan tetiklemek icin (ornegin morph aninda ekstra juice).
    public void Stretch(float amount)
    {
        deform = Mathf.Max(deform, Mathf.Clamp(amount, 0f, maxDeform));
        deformVelocity = Mathf.Max(deformVelocity, 0f);
    }

    public void Squash(float amount)
    {
        deform = Mathf.Min(deform, -Mathf.Clamp(amount, 0f, maxDeform));
        deformVelocity = Mathf.Min(deformVelocity, 0f);
    }

    private Transform FindOrCreateVisual()
    {
        Transform existing = transform.Find("Visual");
        if (existing != null && existing.GetComponent<SpriteRenderer>() != null)
        {
            return existing;
        }

        SpriteRenderer source = GetComponent<SpriteRenderer>();
        if (source == null)
        {
            Debug.LogError($"{name}: SpriteRenderer bulunamadi; squash & stretch kurulamadi.", this);
            enabled = false;
            return transform;
        }

        GameObject visualObject = new GameObject("Visual");
        visualObject.layer = gameObject.layer;
        visualObject.transform.SetParent(transform, false);

        SpriteRenderer target = visualObject.AddComponent<SpriteRenderer>();
        CopyRenderer(source, target);
        source.enabled = false;

        return visualObject.transform;
    }

    private static void CopyRenderer(SpriteRenderer source, SpriteRenderer target)
    {
        target.sprite = source.sprite;
        target.color = source.color;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.tileMode = source.tileMode;
        target.maskInteraction = source.maskInteraction;
        target.spriteSortPoint = source.spriteSortPoint;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.sharedMaterial = source.sharedMaterial;
        target.shadowCastingMode = source.shadowCastingMode;
        target.receiveShadows = source.receiveShadows;
        target.lightProbeUsage = source.lightProbeUsage;
        target.reflectionProbeUsage = source.reflectionProbeUsage;
    }
}
