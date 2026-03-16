using UnityEngine;

public static class HeroSelection
{
    public const string SelectedHeroIndexKey = "SelectedHeroIndex";

    public static int GetSelectedIndex(int fallback = 0)
    {
        return PlayerPrefs.GetInt(SelectedHeroIndexKey, fallback);
    }

    public static void SetSelectedIndex(int index)
    {
        PlayerPrefs.SetInt(SelectedHeroIndexKey, index);
        PlayerPrefs.Save();
    }
}

