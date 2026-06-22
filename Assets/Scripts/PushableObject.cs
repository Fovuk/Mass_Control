using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(100)]
public class PushableObject : MonoBehaviour
{
    private static readonly Collider2D[] OverlapBuffer = new Collider2D[8];

    private Rigidbody2D rb;
    private Collider2D objectCollider;
    private float lockedPositionX;
    private bool hasLockedPositionX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        objectCollider = GetComponent<Collider2D>();
        lockedPositionX = rb.position.x;
        hasLockedPositionX = true;

        if (!CompareTag("Pushable"))
        {
            tag = "Pushable";
        }
    }

    void FixedUpdate()
    {
        if (IsBlockedBySmallPlayer())
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.position = new Vector2(lockedPositionX, rb.position.y);
            return;
        }

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (IsTouchingBigPlayer())
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            lockedPositionX = rb.position.x;
            hasLockedPositionX = true;
            return;
        }

        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (!hasLockedPositionX)
        {
            lockedPositionX = rb.position.x;
            hasLockedPositionX = true;
        }

        rb.position = new Vector2(lockedPositionX, rb.position.y);
    }

    private bool IsBlockedBySmallPlayer()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        int count = objectCollider.Overlap(filter, OverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D other = OverlapBuffer[i];
            if (other == null || !IsSmallPlayer(other))
            {
                continue;
            }

            if (!hasLockedPositionX)
            {
                lockedPositionX = rb.position.x;
                hasLockedPositionX = true;
            }

            return true;
        }

        return false;
    }

    private bool IsTouchingBigPlayer()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        int count = objectCollider.Overlap(filter, OverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            if (IsBigPlayer(OverlapBuffer[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSmallPlayer(Collider2D collider)
    {
        PlayerController player = GetPlayerController(collider);
        return player != null && !player.IsBigForm;
    }

    private static bool IsBigPlayer(Collider2D collider)
    {
        PlayerController player = GetPlayerController(collider);
        return player != null && player.IsBigForm;
    }

    private static PlayerController GetPlayerController(Collider2D collider)
    {
        if (!collider.CompareTag("Player"))
        {
            return null;
        }

        PlayerController player = collider.GetComponent<PlayerController>();
        if (player != null)
        {
            return player;
        }

        return collider.GetComponentInParent<PlayerController>();
    }
}
