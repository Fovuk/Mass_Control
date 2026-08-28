using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(100)]
public class PushableObject : MonoBehaviour
{
    [SerializeField] private float smallBlockGrace = 0.15f;
    [Tooltip("Surface friction — lets the ball roll from physics instead of scripted spin.")]
    [SerializeField] private float rollFriction = 0.55f;
    [SerializeField] private float stopSpeedThreshold = 0.12f;
    [SerializeField] private float stopAngularThreshold = 8f;

    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private PlayerController player;
    private Collider2D playerCollider;
    private float lockedPositionX;
    private bool hasLockedPositionX;
    private float smallBlockTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.angularDamping = 0.05f;
        rb.linearDamping = 0.02f;
        lockedPositionX = rb.position.x;
        hasLockedPositionX = true;

        var ballMaterial = new PhysicsMaterial2D("PushableBall")
        {
            friction = rollFriction,
            bounciness = 0f
        };
        rb.sharedMaterial = ballMaterial;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].sharedMaterial = ballMaterial;
            }
        }

        if (!CompareTag("Pushable"))
        {
            tag = "Pushable";
        }
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
        }
    }

    void FixedUpdate()
    {
        smallBlockTimer = Mathf.Max(0f, smallBlockTimer - Time.fixedDeltaTime);
        RefreshSmallBlockFromTouching();

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

        bool playerDriving =
            player != null && player.IsBigForm && playerCollider != null && IsTouchingPlayer();

        bool atRest =
            !playerDriving &&
            Mathf.Abs(rb.linearVelocity.x) < stopSpeedThreshold &&
            Mathf.Abs(rb.angularVelocity) < stopAngularThreshold;

        if (atRest)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
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

        // Free rotation + translation — physics drives roll from friction and collisions.
        rb.constraints = RigidbodyConstraints2D.None;
        lockedPositionX = rb.position.x;
        hasLockedPositionX = true;
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
        if (hitPlayer == null || hitPlayer.IsBigForm)
        {
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
        if (player == null || playerCollider == null || player.IsBigForm || !IsTouchingPlayer())
        {
            return;
        }

        smallBlockTimer = smallBlockGrace;
    }

    private bool IsTouchingPlayer()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col != null && col.enabled && col.IsTouching(playerCollider))
            {
                return true;
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
