using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Hareket Noktaları")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Ayarlar")]
    [Min(0.01f)]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool startAtPointA = true;
    [SerializeField] private bool waitAtEnds;
    [Min(0f)]
    [SerializeField] private float waitTime = 0.5f;

    private Rigidbody2D rb;
    private bool movingToPointB;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (!HasValidWaypoints())
        {
            enabled = false;
            return;
        }

        movingToPointB = startAtPointA;
        rb.position = startAtPointA ? pointA.position : pointB.position;
    }

    private void FixedUpdate()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        if (waitAtEnds && waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 targetPosition = movingToPointB ? pointB.position : pointA.position;
        float maxDistance = speed * Time.fixedDeltaTime;
        Vector2 currentPosition = rb.position;

        if ((targetPosition - currentPosition).sqrMagnitude <= maxDistance * maxDistance)
        {
            rb.MovePosition(targetPosition);
            movingToPointB = !movingToPointB;

            if (waitAtEnds)
            {
                waitTimer = waitTime;
            }

            return;
        }

        rb.MovePosition(Vector2.MoveTowards(currentPosition, targetPosition, maxDistance));
    }

    private bool HasValidWaypoints()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError($"{name}: Point A ve Point B atanmali.", this);
            return false;
        }

        if (pointA == pointB || (pointA.position - pointB.position).sqrMagnitude < 0.0001f)
        {
            Debug.LogError($"{name}: Point A ve Point B farkli konumlarda olmali.", this);
            return false;
        }

        if (pointA.IsChildOf(transform) || pointB.IsChildOf(transform))
        {
            Debug.LogError(
                $"{name}: Waypoint'ler moving spike'in child'i olamaz.",
                this);
            return false;
        }

        return true;
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0.01f, speed);
        waitTime = Mathf.Max(0f, waitTime);
    }

    private void OnDrawGizmosSelected()
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