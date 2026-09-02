using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(100)]
public class PushableObject : MonoBehaviour
{
    [SerializeField] private float smallBlockGrace = 0.15f;
    [Tooltip("Tile/ball contact friction.")]
    [SerializeField] private float rollFriction = 0.55f;
    [Header("Yer / Yuvarlanma")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundProbeDistance = 0.1f;
    [SerializeField, Range(0.2f, 1f)] private float minGroundNormalY = 0.45f;
    [SerializeField, Range(0.7f, 1f)] private float flatGroundNormalY = 0.92f;
    [SerializeField] private float stopSpeedThreshold = 0.08f;
    [Tooltip("0 = pure physics friction. 1 = perfect no-slip roll (omega = v/r).")]
    [SerializeField, Range(0f, 1f)] private float rollingCoupling = 0.85f;
    [SerializeField, Range(0f, 1f)] private float pushedFriction = 0.12f;
    [SerializeField] private float bigPlayerPushAccel = 42f;

    private static readonly ContactPoint2D[] ContactBuffer = new ContactPoint2D[8];

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private Collider2D[] colliders;
    private PlayerController player;
    private Collider2D playerCollider;
    private PhysicsMaterial2D ballMaterial;
    private PhysicsMaterial2D pushedMaterial;
    private float lockedPositionX;
    private bool hasLockedPositionX;
    private float smallBlockTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        colliders = GetComponents<Collider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.angularDamping = 0.05f;
        rb.linearDamping = 0.02f;
        rb.sleepMode = RigidbodySleepMode2D.StartAwake;
        lockedPositionX = rb.position.x;
        hasLockedPositionX = true;

        ballMaterial = new PhysicsMaterial2D("PushableBall")
        {
            friction = rollFriction,
            bounciness = 0f
        };
        pushedMaterial = new PhysicsMaterial2D("PushableBallPushed")
        {
            friction = pushedFriction,
            bounciness = 0f
        };
        rb.sharedMaterial = ballMaterial;
        ApplyMaterial(ballMaterial);

        if (!CompareTag("Pushable"))
        {
            tag = "Pushable";
        }
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        RefreshPlayerCollider();

        if (groundLayer.value == 0)
        {
            int groundMask = LayerMask.GetMask("Ground");
            if (groundMask != 0)
            {
                groundLayer = groundMask;
            }
        }
    }

    void FixedUpdate()
    {
        smallBlockTimer = Mathf.Max(0f, smallBlockTimer - Time.fixedDeltaTime);
        RefreshSmallBlockFromTouching();

        bool playerDriving =
            player != null && player.IsBigForm && playerCollider != null && IsTouchingPlayer();

        if (playerDriving)
        {
            ReleaseForBigPlayer();
            ApplyMaterial(pushedMaterial);
        }
        else
        {
            ApplyMaterial(ballMaterial);
        }

        if (smallBlockTimer > 0f)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = new Vector2(lockedPositionX, rb.position.y);
            return;
        }

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        bool grounded = IsGrounded(out float groundNormalY);
        bool onFlatGround = grounded && groundNormalY >= flatGroundNormalY;

        rb.constraints = RigidbodyConstraints2D.None;

        if (grounded)
        {
            ApplyRollingCoupling();
        }

        if (playerDriving)
        {
            ApplyBigPlayerPush();
        }

        bool horizontallyAtRest =
            !playerDriving && Mathf.Abs(rb.linearVelocity.x) < stopSpeedThreshold;

        if (horizontallyAtRest && onFlatGround)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.angularVelocity = 0f;

            if (!hasLockedPositionX)
            {
                lockedPositionX = rb.position.x;
                hasLockedPositionX = true;
            }

            rb.position = new Vector2(lockedPositionX, rb.position.y);
            return;
        }

        lockedPositionX = rb.position.x;
        hasLockedPositionX = true;
    }

    private void ApplyRollingCoupling()
    {
        if (rollingCoupling <= 0f)
        {
            return;
        }

        float radius = GetWorldRadius();
        if (radius <= 0.001f)
        {
            return;
        }

        float targetAngular = (rb.linearVelocity.x / radius) * Mathf.Rad2Deg;
        rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, targetAngular, rollingCoupling);
    }

    private void ApplyBigPlayerPush()
    {
        if (player == null)
        {
            return;
        }

        // Use input intent, not measured velocity: collision resolution can pin
        // the player against a frozen ball and drive HorizontalVelocity to ~0.
        float targetVx = player.HasBigPushInput
            ? player.BigPushTargetVelocity
            : player.HorizontalVelocity;

        if (Mathf.Abs(targetVx) < 0.05f)
        {
            return;
        }

        float newVx = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetVx,
            bigPlayerPushAccel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
    }

    private float GetWorldRadius()
    {
        if (circleCollider != null)
        {
            Vector3 scale = transform.lossyScale;
            return circleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }

        if (colliders.Length > 0 && colliders[0] != null)
        {
            return colliders[0].bounds.extents.x;
        }

        return 0.5f;
    }

    private bool IsGrounded(out float groundNormalY)
    {
        groundNormalY = 0f;

        if (TryGetBestGroundContact(out ContactPoint2D contact))
        {
            groundNormalY = contact.normal.y;
            return true;
        }

        if (groundLayer.value == 0 || colliders.Length == 0 || colliders[0] == null)
        {
            return false;
        }

        Bounds bounds = colliders[0].bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundProbeDistance, groundLayer);
        if (hit.collider != null && hit.normal.y >= minGroundNormalY)
        {
            groundNormalY = hit.normal.y;
            return true;
        }

        return false;
    }

    private bool TryGetBestGroundContact(out ContactPoint2D bestContact)
    {
        bestContact = default;
        float bestNormalY = 0f;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null || !col.enabled)
            {
                continue;
            }

            int count = col.GetContacts(filter, ContactBuffer);
            for (int j = 0; j < count; j++)
            {
                ContactPoint2D contact = ContactBuffer[j];
                Collider2D other = contact.collider;
                if (other == null || other.isTrigger)
                {
                    continue;
                }

                if (other.CompareTag("Player") || other.CompareTag("Pushable"))
                {
                    continue;
                }

                if (contact.normal.y <= minGroundNormalY)
                {
                    continue;
                }

                if (contact.normal.y > bestNormalY)
                {
                    bestNormalY = contact.normal.y;
                    bestContact = contact;
                }
            }
        }

        return bestNormalY > 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        RegisterSmallPlayerCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RegisterSmallPlayerCollision(collision);
    }

    private void RegisterSmallPlayerCollision(Collision2D collision)
    {
        PlayerController hitPlayer = GetPlayerController(collision.collider);
        if (hitPlayer == null)
        {
            return;
        }

        if (hitPlayer.IsBigForm)
        {
            ReleaseForBigPlayer();
            return;
        }

        smallBlockTimer = smallBlockGrace;

        if (!hasLockedPositionX)
        {
            lockedPositionX = rb.position.x;
            hasLockedPositionX = true;
        }
    }

    private void RefreshSmallBlockFromTouching()
    {
        if (player == null || playerCollider == null || !IsTouchingPlayer())
        {
            return;
        }

        if (player.IsBigForm)
        {
            ReleaseForBigPlayer();
            return;
        }

        smallBlockTimer = smallBlockGrace;
    }

    public void ReleaseForBigPlayer()
    {
        smallBlockTimer = 0f;

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        rb.constraints = RigidbodyConstraints2D.None;
    }

    private void ApplyMaterial(PhysicsMaterial2D material)
    {
        rb.sharedMaterial = material;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].sharedMaterial = material;
            }
        }
    }

    private void RefreshPlayerCollider()
    {
        if (player == null)
        {
            playerCollider = null;
            return;
        }

        playerCollider = player.BodyCollider;
        if (playerCollider != null && playerCollider.enabled)
        {
            return;
        }

        Collider2D[] playerColliders = player.GetComponents<Collider2D>();
        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D col = playerColliders[i];
            if (col != null && col.enabled)
            {
                playerCollider = col;
                return;
            }
        }
    }

    private bool IsTouchingPlayer()
    {
        if (playerCollider == null)
        {
            RefreshPlayerCollider();
        }

        if (playerCollider != null && playerCollider.enabled)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D col = colliders[i];
                if (col != null && col.enabled && col.IsTouching(playerCollider))
                {
                    return true;
                }
            }
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null || !col.enabled)
            {
                continue;
            }

            int count = col.GetContacts(filter, ContactBuffer);
            for (int j = 0; j < count; j++)
            {
                Collider2D other = ContactBuffer[j].collider;
                if (other != null && other.enabled && other.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static PlayerController GetPlayerController(Collider2D collider)
    {
        if (!collider.CompareTag("Player"))
        {
            return null;
        }

        PlayerController hitPlayer = collider.GetComponent<PlayerController>();
        if (hitPlayer != null)
        {
            return hitPlayer;
        }

        return collider.GetComponentInParent<PlayerController>();
    }
}
