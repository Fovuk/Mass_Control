using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Zıplama Ayarları")]
    public float jumpForce = 12f;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    void Start()
    {
        // Karakterin Rigidbody2D bileşenine kod üzerinden erişiyoruz
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Her karede karakterin yerde olup olmadığını küçük bir görünmez çemberle kontrol ediyoruz
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    void FixedUpdate()
    {
        // Fizik tabanlı hareketleri Rigidbody velocity (hız) ile yapıyoruz
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // Yeni Input System: Joystick hareket ettiğinde bu fonksiyon otomatik tetiklenir
    public void OnMove(InputValue value)
    {
        Vector2 moveVector = value.Get<Vector2>();
        horizontalInput = moveVector.x; // Sadece sağ-sol eksenini (X) alıyoruz
    }

    // Yeni Input System: Zıplama butonuna (Button South) basıldığında tetiklenir
    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}