using UnityEngine;

public static class AudioSettingsRuntime
{
    public const string MasterVolumeKey = "MasterVolume";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume()
    {
        AudioListener.volume = GetMasterVolume();
    }

    public static float GetMasterVolume()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
    }

    public static void SetMasterVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(MasterVolumeKey, v);
        PlayerPrefs.Save();
    }
}

