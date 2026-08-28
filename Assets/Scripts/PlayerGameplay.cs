using UnityEngine;

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
