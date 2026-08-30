using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerGameplay : MonoBehaviour
{
    [SerializeField] private float fallDeathY = -10f;
    [SerializeField] private float fallMarginBelowCamera = 1.25f;
    [SerializeField] private bool useCameraFallDeath = true;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (HasFallenToDeath())
        {
            GameManager.Instance?.PlayerDied();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }
        Debug.Log($"[PlayerGameplay] Trigger algilandi -> Obje: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Trap"))
        {
            Debug.Log("[PlayerGameplay] Tuzaga carpildi.");
            GameManager.Instance?.PlayerDied();
        }
        else if (other.CompareTag("Star"))
        {
            Debug.Log("[PlayerGameplay] Yildiz toplandi.");
            GameManager.Instance?.CollectStar(other.gameObject);
        }
        else if (other.CompareTag("Finish"))
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[PlayerGameplay] GameManager bulunamadi!");
                return;
            }

            Debug.Log("[PlayerGameplay] Finish portal'a girildi.");
            GameManager.Instance.CompleteLevel();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Tilemap tilemap = collision.collider.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            Vector2 pointInsideTile = contact.point - contact.normal * 0.05f;
            Vector3Int cell = tilemap.WorldToCell(pointInsideTile);
            TileBase touchedTile = tilemap.GetTile(cell);

            if (touchedTile != null && touchedTile.name.StartsWith("SpikeTile"))
            {
                GameManager.Instance?.PlayerDied();
                return;
            }
        }
    }

    private bool HasFallenToDeath()
    {
        if (useCameraFallDeath)
        {
            Camera cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                float cameraBottom = cam.transform.position.y - cam.orthographicSize;
                if (transform.position.y < cameraBottom - fallMarginBelowCamera)
                {
                    return true;
                }
            }
        }

        return transform.position.y < fallDeathY;
    }
}
