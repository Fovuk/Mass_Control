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
    public float bigMass = 4f;

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
        rb.linearVelocity = new Vector2(horizontalInput * currentMoveSpeed, rb.linearVelocity.y);
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
            currentMoveSpeed = baseMoveSpeed * 0.6f;
            isBig = true;
        }
        else
        {
            transform.localScale = smallScale;
            rb.mass = smallMass;
            currentMoveSpeed = baseMoveSpeed;
            isBig = false;
        }
    }
}
