using UnityEngine;

[CreateAssetMenu(fileName = "StarUiSprites", menuName = "UI/Star UI Sprites")]
public class StarUiSprites : ScriptableObject
{
    public Sprite emptyStar;
    public Sprite filledStar;

    private static Sprite cachedEmptyStar;
    private static Sprite cachedFilledStar;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void PreloadSprites()
    {
        GetEmptyStar();
        GetFilledStar();
    }

    public static Sprite GetEmptyStar()
    {
        EnsureLoaded();
        return cachedEmptyStar;
    }

    public static Sprite GetFilledStar()
    {
        EnsureLoaded();
        return cachedFilledStar;
    }

    private static void EnsureLoaded()
    {
        if (cachedEmptyStar != null && cachedFilledStar != null)
        {
            return;
        }

        cachedEmptyStar = Resources.Load<Sprite>("Kenney/Grey/star_outline");
        cachedFilledStar = Resources.Load<Sprite>("Kenney/Blue/star");

        if (cachedEmptyStar == null)
        {
            cachedEmptyStar = Resources.Load<Sprite>("Kenney/Blue/star_outline");
        }

        if (cachedEmptyStar != null && cachedFilledStar != null)
        {
            return;
        }

        StarUiSprites library = Resources.Load<StarUiSprites>("StarUiSprites");
        if (library == null)
        {
            return;
        }

        cachedEmptyStar ??= library.emptyStar;
        cachedFilledStar ??= library.filledStar;
    }
}
