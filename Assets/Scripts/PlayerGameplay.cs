using UnityEngine;

public class PlayerGameplay : MonoBehaviour
{
    [SerializeField] private float fallDeathY = -10f;

    void Update()
    {
        if (transform.position.y < fallDeathY)
        {
            GameManager.Instance?.PlayerDied();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
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
}
