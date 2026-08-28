using UnityEngine;

[DisallowMultipleComponent]
public class CollectibleStarSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 110f;
    [SerializeField] private float speedVariance = 20f;

    private float signedSpeed;

    void Awake()
    {
        signedSpeed = spinSpeed * (Random.value < 0.5f ? -1f : 1f);
        signedSpeed += Random.Range(-speedVariance, speedVariance);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, signedSpeed * Time.deltaTime, Space.Self);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AttachToCollectibleStars()
    {
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        for (int i = 0; i < stars.Length; i++)
        {
            GameObject star = stars[i];
            if (star == null || star.GetComponent<CollectibleStarSpin>() != null)
            {
                continue;
            }

            if (star.GetComponent<RectTransform>() != null)
            {
                continue;
            }

            Collider2D collider = star.GetComponent<Collider2D>();
            if (collider == null || !collider.isTrigger)
            {
                continue;
            }

            star.AddComponent<CollectibleStarSpin>();
        }
    }
}
