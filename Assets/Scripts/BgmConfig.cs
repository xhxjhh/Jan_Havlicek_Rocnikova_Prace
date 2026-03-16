using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Bgm Config", fileName = "BgmConfig")]
public class BgmConfig : ScriptableObject
{
    public AudioClip mainMenu;
    public AudioClip battle;
    public AudioClip gameOver;
    [Range(0f, 1f)]
    public float volume = 0.5f;
}

