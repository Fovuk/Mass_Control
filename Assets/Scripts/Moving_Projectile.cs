using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Hareket Noktaları")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Ayarlar")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool startAtPointA = true;
    [SerializeField] private bool waitAtEnds;
    [SerializeField] private float waitTime = 0.5f;

    private Rigidbody2D rb;
    private Transform target;
    private float waitTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning($"{name}: pointA ve pointB atanmali.", this);
            return;
        }

        target = startAtPointA ? pointB : pointA;
        rb.position = startAtPointA ? pointA.position : pointB.position;
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null || target == null)
        {
            return;
        }

        if (waitAtEnds && waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, target.position, speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);

        if (Vector2.Distance(rb.position, target.position) < 0.01f)
        {
            target = target == pointA ? pointB : pointA;

            if (waitAtEnds)
            {
                waitTimer = waitTime;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA.position, 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB.position, 0.15f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA.position, pointB.position);
    }
} 