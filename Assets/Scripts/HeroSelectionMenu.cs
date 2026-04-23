using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HeroSelectionMenu : MonoBehaviour
{
    [SerializeField] Text heroNameText;
    [SerializeField] Image heroPortraitImage;
    [SerializeField] HeroRoster rosterOverride;

    HeroRoster roster;
    int index;

    void Awake()
    {
        roster = rosterOverride != null ? rosterOverride : Resources.Load<HeroRoster>("HeroRoster");
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        index = HeroSelection.GetSelectedIndex(roster.defaultIndex);
        if (index < 0 || index >= roster.heroes.Count)
        {
            index = Mathf.Clamp(index, 0, roster.heroes.Count - 1);
            HeroSelection.SetSelectedIndex(index);
        }

        Refresh();
    }

    public void NextHero()
    {
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        index = (index + 1) % roster.heroes.Count;
        HeroSelection.SetSelectedIndex(index);
        Refresh();
    }

    public void PreviousHero()
    {
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        index = (index - 1 + roster.heroes.Count) % roster.heroes.Count;
        HeroSelection.SetSelectedIndex(index);
        Refresh();
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("StageIndex", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.Battle);
    }

    public void ContinueGame()
    {
        if (GameController.HasSave())
        {
            GameController.ContinueFromSave();
            return;
        }

        StartGame();
    }

    void Refresh()
    {
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        var entry = roster.heroes[index];
        if (entry == null)
        {
            return;
        }

        if (heroNameText != null)
        {
            heroNameText.text = string.IsNullOrWhiteSpace(entry.displayName) ? "Hero" : entry.displayName;
        }

        if (heroPortraitImage != null)
        {
            heroPortraitImage.sprite = entry.portrait;
            heroPortraitImage.enabled = entry.portrait != null;
        }
    }
}
