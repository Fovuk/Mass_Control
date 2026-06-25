using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float baseMoveSpeed = 8f;
    private float currentMoveSpeed;
    private float horizontalInput;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 12f;
    private bool isGrounded;
    public LayerMask groundLayer;
    private bool canDoubleJump;

    [Header("Boyut / Kütle Değişim Ayarları")]
    private bool isBig = false;
    public Vector3 smallScale = new Vector3(1f, 1f, 1f);
    public float smallMass = 1f;
    public Vector3 bigScale = new Vector3(2f, 2f, 2f);
    public float bigMass = 999f;

    public bool IsBigForm => isBig;

    [Header("Büyük Form Momentum")]
    [SerializeField] private float bigMoveSpeedMultiplier = 0.6f;
    [SerializeField] private float bigMomentumMultiplier = 1.5f;
    [SerializeField] private float smallMorphVelocityScale = 0.85f;
    [SerializeField] private float bigAcceleration = 18f;
    [SerializeField] private float bigMomentumDecay = 12f;
    [SerializeField] private float wallCheckDistance = 0.08f;

    private static readonly Collider2D[] OverlapBuffer = new Collider2D[4];

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        currentMoveSpeed = baseMoveSpeed;
    }

    void Update()
    {
        isGrounded = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
        }
    }

    void FixedUpdate()
    {
        float targetVelocityX = horizontalInput * currentMoveSpeed;
        float newVelocityX;

        if (isBig)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f)
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
            newVelocityX = targetVelocityX;
        }

        if (IsBlockedHorizontally(newVelocityX))
        {
            newVelocityX = 0f;
        }
        else if (!isBig && IsTouchingPushable())
        {
            newVelocityX = 0f;
        }

        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    private bool IsTouchingPushable()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        int count = boxCollider.Overlap(filter, OverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            if (OverlapBuffer[i] != null && OverlapBuffer[i].CompareTag("Pushable"))
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

        Vector2 direction = new Vector2(Mathf.Sign(velocityX), 0f);
        Vector2 castSize = boxCollider.bounds.size;
        castSize.x *= 0.95f;
        castSize.y *= 0.95f;

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            castSize,
            0f,
            direction,
            wallCheckDistance,
            groundLayer);

        return hit.collider != null;
    }

    public void OnMove(InputValue value)
    {
        Vector2 moveVector = value.Get<Vector2>();
        horizontalInput = moveVector.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (canDoubleJump && !isBig)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.9f);
                canDoubleJump = false;
            }
        }
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
    }
}
