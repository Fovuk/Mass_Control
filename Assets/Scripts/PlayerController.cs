using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerSquashStretch), typeof(PlayerLandingJuice), typeof(PlayerWalkSfx))]
public class PlayerController : MonoBehaviour
{
    public event Action Jumped;

    // Yere degdigi andaki dusus hizini (pozitif deger) tasir.
    public event Action<float> Landed;

    public bool IsGrounded => isGrounded;

    [Header("Hareket Ayarları")]
    public float baseMoveSpeed = 8f;
    private float currentMoveSpeed;
    private float horizontalInput;
    private float verticalInput;

    [Header("Hızlanma / Yavaşlama (Damping)")]
    [Tooltip("Yerde hedef hıza ulaşma ivmesi (birim/sn²). Yüksek = daha ani, düşük = daha kaygan.")]
    [SerializeField] private float groundAcceleration = 70f;
    [Tooltip("Yerde tuşu bıraktıktan sonra durma ivmesi (birim/sn²).")]
    [SerializeField] private float groundDeceleration = 90f;
    [Tooltip("Yerde ters yöne dönerken kullanılan ivme. Genelde en yüksek değer olmalı.")]
    [SerializeField] private float turnAcceleration = 110f;
    [Tooltip("Havadayken hedef hıza ulaşma ivmesi. Düşük tutmak hava kontrolünü azaltır.")]
    [SerializeField] private float airAcceleration = 45f;
    [Tooltip("Havadayken durma ivmesi.")]
    [SerializeField] private float airDeceleration = 35f;

    private const float InputDeadzone = 0.01f;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 12f;
    private bool isGrounded;
    public LayerMask groundLayer;
    private bool canDoubleJump;
    private float fallSpeed;

    [Header("Asagi Hizli Dusus")]
    [Tooltip("S tusuna havadayken basildiginda hedeflenen asagi hiz.")]
    [SerializeField, Min(0.1f)] private float fastFallSpeed = 16f;
    [Tooltip("Oyuncunun asagi hiza ne kadar cabuk ulasacagi.")]
    [SerializeField, Min(0.1f)] private float fastFallAcceleration = 70f;

    [Header("Coyote Time / Jump Buffer")]
    [Tooltip("How long jumping remains allowed after walking off a ledge.")]
    [SerializeField, Range(0f, 0.3f)] private float coyoteTime = 0.12f;
    [Tooltip("How long an early jump press is remembered before landing.")]
    [SerializeField, Range(0f, 0.3f)] private float jumpBufferTime = 0.12f;

    private float coyoteTimeRemaining;
    private float jumpBufferRemaining;
    private bool ignoreGroundUntilAirborne;

    [Header("Boyut / Kütle Değişim Ayarları")]
    private bool isBig = false;
    public Vector3 smallScale = new Vector3(1f, 1f, 1f);
    public float smallMass = 1f;
    public Vector3 bigScale = new Vector3(2f, 2f, 2f);
    public float bigMass = 999f;

    public bool IsBigForm => isBig;

    public float HorizontalVelocity => rb != null ? rb.linearVelocity.x : 0f;

    [Header("Büyük Form Momentum")]
    [SerializeField] private float bigMoveSpeedMultiplier = 0.6f;
    [SerializeField] private float bigMomentumMultiplier = 1.5f;
    [SerializeField] private float smallMorphVelocityScale = 0.85f;
    [SerializeField] private float bigAcceleration = 18f;
    [SerializeField] private float bigMomentumDecay = 12f;
    [SerializeField] private float wallCheckDistance = 0.08f;
    [Header("Yer Algilama")]
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private float groundCheckWidthScale = 0.55f;
    [SerializeField, Range(0.4f, 1f)] private float minGroundNormalY = 0.65f;

    private static readonly Collider2D[] OverlapBuffer = new Collider2D[4];

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        // The visual effect creates/scales a SpriteRenderer child at runtime.
        // The Rigidbody2D and Collider2D remain on this unscaled root object.
        if (GetComponent<PlayerSquashStretch>() == null)
        {
            gameObject.AddComponent<PlayerSquashStretch>();
        }

        if (GetComponent<PlayerLandingJuice>() == null)
        {
            gameObject.AddComponent<PlayerLandingJuice>();
        }

        if (GetComponent<PlayerWalkSfx>() == null)
        {
            gameObject.AddComponent<PlayerWalkSfx>();
        }

        if (GetComponent<PlayerDeathHandler>() == null)
        {
            gameObject.AddComponent<PlayerDeathHandler>();
        }

        rb = GetComponent<Rigidbody2D>();
        ConfigureBodyCollider();
    }

    void Start()
    {
        currentMoveSpeed = baseMoveSpeed;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // Default friction makes the box collider snag on tile corners.
        var slip = new PhysicsMaterial2D("PlayerSlip")
        {
            friction = 0f,
            bounciness = 0f
        };
        rb.sharedMaterial = slip;
        if (bodyCollider != null)
        {
            bodyCollider.sharedMaterial = slip;
        }
    }

    private void ConfigureBodyCollider()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();

        if (capsule != null)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }

            bodyCollider = capsule;
            return;
        }

        if (boxCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
            return;
        }

        capsule = gameObject.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(boxCollider.size.x * 0.82f, boxCollider.size.y * 0.96f);
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.offset = boxCollider.offset;
        boxCollider.enabled = false;
        bodyCollider = capsule;
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 castSize = new Vector2(bounds.size.x * groundCheckWidthScale, bounds.size.y * 0.2f);
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + castSize.y * 0.5f);

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            castSize,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer);

        return hit.collider != null && hit.normal.y >= minGroundNormalY;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        bool groundedNow = CheckGrounded();

        // A ground cast can still touch the floor for a frame immediately after
        // jumping. Ignore it until the player has genuinely left the ground,
        // otherwise coyote time could incorrectly grant another ground jump.
        if (ignoreGroundUntilAirborne)
        {
            if (groundedNow)
            {
                groundedNow = false;
            }
            else
            {
                ignoreGroundUntilAirborne = false;
            }
        }

        if (groundedNow && !isGrounded)
        {
            Landed?.Invoke(fallSpeed);
        }

        isGrounded = groundedNow;

        if (isGrounded)
        {
            coyoteTimeRemaining = coyoteTime;
            canDoubleJump = true;
            fallSpeed = 0f;
        }
        else
        {
            coyoteTimeRemaining =
                Mathf.Max(0f, coyoteTimeRemaining - Time.deltaTime);
            fallSpeed = Mathf.Max(0f, -rb.linearVelocity.y);
        }

        jumpBufferRemaining =
            Mathf.Max(0f, jumpBufferRemaining - Time.deltaTime);
        TryConsumeJump();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        float targetVelocityX = horizontalInput * currentMoveSpeed;
        float newVelocityX;

        if (isBig)
        {
            if (Mathf.Abs(horizontalInput) < InputDeadzone)
            {
                newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, 0f, bigMomentumDecay * Time.fixedDeltaTime);
            }
            else
            {
                newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, bigAcceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            newVelocityX = Damp(rb.linearVelocity.x, targetVelocityX);
        }

        if (IsBlockedHorizontally(newVelocityX))
        {
            newVelocityX = 0f;
        }
        else if (!isBig && IsTouchingPushable())
        {
            newVelocityX = 0f;
        }

        float newVelocityY = rb.linearVelocity.y;
        if (verticalInput < -InputDeadzone && !isGrounded && newVelocityY > -fastFallSpeed)
        {
            // Pressing down cancels upward momentum immediately, then starts a
            // controlled fast fall without slowing an already faster descent.
            newVelocityY = Mathf.Min(newVelocityY, 0f);
            newVelocityY = Mathf.MoveTowards(
                newVelocityY,
                -fastFallSpeed,
                fastFallAcceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(newVelocityX, newVelocityY);
    }

    private float Damp(float currentVelocityX, float targetVelocityX)
    {
        return Mathf.MoveTowards(currentVelocityX, targetVelocityX, SelectRate(currentVelocityX, targetVelocityX) * Time.fixedDeltaTime);
    }

    private float SelectRate(float currentVelocityX, float targetVelocityX)
    {
        if (Mathf.Abs(horizontalInput) < InputDeadzone)
        {
            return isGrounded ? groundDeceleration : airDeceleration;
        }

        bool isMoving = Mathf.Abs(currentVelocityX) > InputDeadzone;
        if (isMoving && Mathf.Sign(targetVelocityX) != Mathf.Sign(currentVelocityX))
        {
            return isGrounded ? turnAcceleration : airAcceleration;
        }

        // Morph momentum can leave us above top speed; bleed it off instead of accelerating.
        if (Mathf.Abs(currentVelocityX) > Mathf.Abs(targetVelocityX))
        {
            return isGrounded ? groundDeceleration : airDeceleration;
        }

        return isGrounded ? groundAcceleration : airAcceleration;
    }

    private bool IsTouchingPushable()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        int count = bodyCollider.Overlap(filter, OverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D other = OverlapBuffer[i];
            if (other != null && other.CompareTag("Pushable") && bodyCollider.IsTouching(other))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockedHorizontally(float velocityX)
    {
        if (Mathf.Abs(velocityX) < 0.01f)
        {
            return false;
        }

        // Keep the cast well above the feet. A full-height box hits the tiny
        // vertical seams between adjacent ground tiles and zeroes movement.
        Bounds bounds = bodyCollider.bounds;
        Vector2 direction = new Vector2(Mathf.Sign(velocityX), 0f);
        Vector2 castSize = bounds.size;
        castSize.x *= 0.9f;
        castSize.y *= 0.55f;
        Vector2 origin = (Vector2)bounds.center + Vector2.up * (bounds.extents.y * 0.2f);

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            castSize,
            0f,
            direction,
            wallCheckDistance,
            groundLayer);

        // Floor/ceiling contacts have a mostly vertical normal; ignore those.
        return hit.collider != null && Mathf.Abs(hit.normal.x) > 0.7f;
    }

    public void OnMove(InputValue value)
    {
        Vector2 moveVector = value.Get<Vector2>();
        horizontalInput = moveVector.x;
        verticalInput = moveVector.y;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            // Keep normal jumping functional even when buffering is set to 0.
            jumpBufferRemaining =
                jumpBufferTime > 0f ? jumpBufferTime : float.Epsilon;
            TryConsumeJump();
        }
    }

    private void TryConsumeJump()
    {
        if (jumpBufferRemaining <= 0f || rb == null)
        {
            return;
        }

        // Ground and coyote jumps have priority over the double jump.
        if (isGrounded || coyoteTimeRemaining > 0f)
        {
            PerformJump(jumpForce);
            isGrounded = false;
            coyoteTimeRemaining = 0f;
            jumpBufferRemaining = 0f;
            ignoreGroundUntilAirborne = true;
            return;
        }

        // Preserve the existing small-form double jump. If it has already
        // been spent (or the player is big), the request stays buffered and
        // can fire on landing.
        if (canDoubleJump && !isBig && !ShouldHoldJumpForLanding())
        {
            PerformJump(jumpForce * 0.9f);
            canDoubleJump = false;
            jumpBufferRemaining = 0f;
        }
    }

    private void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        SfxManager.Instance?.PlayJump();
        Jumped?.Invoke();
    }

    private bool ShouldHoldJumpForLanding()
    {
        if (jumpBufferTime <= 0f || rb.linearVelocity.y >= 0f)
        {
            return false;
        }

        // If the player will likely touch ground during the buffer window,
        // preserve the double jump and execute a ground jump on contact.
        float predictedFallDistance =
            -rb.linearVelocity.y * jumpBufferTime + 0.1f;
        Bounds bounds = bodyCollider.bounds;
        Vector2 castSize = new Vector2(bounds.size.x * 0.9f, bounds.size.y * 0.95f);
        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            castSize,
            0f,
            Vector2.down,
            predictedFallDistance,
            groundLayer);

        return hit.collider != null && hit.normal.y >= minGroundNormalY;
    }

    public void OnMorph(InputValue value)
    {
        if (value.isPressed)
        {
            ToggleMorph();
        }
    }

    public void ToggleMorph()
    {
        if (!isBig)
        {
            transform.localScale = bigScale;
            rb.mass = bigMass;
            currentMoveSpeed = baseMoveSpeed * bigMoveSpeedMultiplier;
            isBig = true;

            float boostedVelocityX = rb.linearVelocity.x * bigMomentumMultiplier;
            if (IsBlockedHorizontally(boostedVelocityX))
            {
                boostedVelocityX = 0f;
            }

            rb.linearVelocity = new Vector2(boostedVelocityX, rb.linearVelocity.y);
        }
        else
        {
            transform.localScale = smallScale;
            rb.mass = smallMass;
            currentMoveSpeed = baseMoveSpeed;
            isBig = false;

            float reducedVelocityX = rb.linearVelocity.x * smallMorphVelocityScale;
            rb.linearVelocity = new Vector2(reducedVelocityX, rb.linearVelocity.y);
        }

        SfxManager.Instance?.PlaySizeChange();
    }
}
