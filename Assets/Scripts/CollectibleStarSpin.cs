using UnityEngine;
using UnityEngine.SceneManagement;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= AttachToCollectibleStars;
        SceneManager.sceneLoaded += AttachToCollectibleStars;
    }

    static void AttachToCollectibleStars(Scene scene, LoadSceneMode mode)
    {
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        for (int i = 0; i < stars.Length; i++)
        {
            TryAttach(stars[i]);
        }
    }

    static void TryAttach(GameObject star)
    {
        if (star == null || star.GetComponent<CollectibleStarSpin>() != null)
        {
            return;
        }

        if (star.GetComponent<RectTransform>() != null)
        {
            return;
        }

        Collider2D collider = star.GetComponent<Collider2D>();
        if (collider == null || !collider.isTrigger)
        {
            return;
        }

        star.AddComponent<CollectibleStarSpin>();
    }
}
