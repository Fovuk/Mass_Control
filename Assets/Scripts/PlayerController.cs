using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanıyorsanız bunu ekleyin. Düz Text ise 'using UnityEngine.UI;' yazın.

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

    [Header("Skor / UI Ayarları")]
    public TextMeshProUGUI starText; // Arayüzdeki yazı objesini buraya bağlayacağız
    private int collectedStars = 0;   // Toplanan yıldız sayısı

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        currentMoveSpeed = baseMoveSpeed;
        
        // Oyun başında arayüzü güncelle
        UpdateStarUI();
    }

    void Update()
    {
        isGrounded = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        if (transform.position.y < -10f)
        {
            DieAndRestart();
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

    // ÇARPIŞMA KONTROLLERİ
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💀 Tuzak Kontrolü
        if (other.CompareTag("Trap"))
        {
            DieAndRestart();
        }
        
        // 🌟 Yıldız Kontrolü (Yeni Eklenen Alan)
        if (other.CompareTag("Star"))
        {
            collectedStars++;       // Skoru 1 artır
            UpdateStarUI();         // Arayüzü güncelle
            Destroy(other.gameObject); // Toplanan yıldızı sahneden sil
        }
    }

    private void DieAndRestart()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    // Arayüz yazısını güncelleyen fonksiyon (Yeni Eklenen Alan)
    private void UpdateStarUI()
    {
        if (starText != null)
        {
            starText.text = "Yildiz: " + collectedStars;
        }
    }
}