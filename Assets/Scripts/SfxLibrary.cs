using UnityEngine;

[CreateAssetMenu(fileName = "SfxLibrary", menuName = "Audio/SFX Library")]
public class SfxLibrary : ScriptableObject
{
    public AudioClip jump;
    public AudioClip starCollect;
    public AudioClip death;
    public AudioClip levelComplete;
    public AudioClip inGameMusic;
    public AudioClip mainMenuMusic;
    public AudioClip buttonClick;
    public AudioClip walk;
    public AudioClip sizeChange;
}
