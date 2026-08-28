using UnityEngine;

[RequireComponent(typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private Color deathTint = new Color(1f, 0.35f, 0.35f, 1f);

    private PlayerController controller;
    private PlayerSquashStretch squashStretch;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private float originalGravityScale = 1f;
    private bool applied;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        squashStretch = GetComponent<PlayerSquashStretch>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = FindVisualRenderer();
        originalGravityScale = rb.gravityScale;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Dead)
        {
            ApplyDeathPresentation();
        }
        else if (state == GameState.Playing)
        {
            ResetPresentation();
        }
    }

    private void ApplyDeathPresentation()
    {
        if (applied)
        {
            return;
        }

        applied = true;

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = deathTint;
        }

        squashStretch?.Squash(0.42f);
    }

    private void ResetPresentation()
    {
        applied = false;

        if (controller != null)
        {
            controller.enabled = true;
        }

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private SpriteRenderer FindVisualRenderer()
    {
        Transform visual = transform.Find("Visual");
        if (visual != null && visual.TryGetComponent(out SpriteRenderer visualRenderer))
        {
            return visualRenderer;
        }

        return GetComponent<SpriteRenderer>();
    }
}
