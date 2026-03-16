using UnityEngine;
using UnityEngine.SceneManagement;

public static class HeroSelectionRuntime
{
    static bool registered;
    static int appliedForBattleSceneFrame = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        if (registered)
        {
            return;
        }

        registered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneNames.Battle)
        {
            return;
        }

        if (appliedForBattleSceneFrame == Time.frameCount)
        {
            return;
        }
        appliedForBattleSceneFrame = Time.frameCount;

        var roster = Resources.Load<HeroRoster>("HeroRoster");
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        int index = HeroSelection.GetSelectedIndex(roster.defaultIndex);
        if (index < 0 || index >= roster.heroes.Count)
        {
            index = Mathf.Clamp(index, 0, roster.heroes.Count - 1);
            HeroSelection.SetSelectedIndex(index);
        }

        var entry = roster.heroes[index];
        if (entry == null || entry.prefab == null)
        {
            return;
        }

        var existingHero = GameObject.FindGameObjectWithTag("Hero");
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        Transform parent = null;
        GameObject healthFill = null;
        GameObject magicFill = null;

        if (existingHero != null)
        {
            var t = existingHero.transform;
            pos = t.position;
            rot = t.rotation;
            parent = t.parent;

            var existingStats = existingHero.GetComponent<FighterStats>();
            if (existingStats != null)
            {
                healthFill = existingStats.GetHealthFill();
                magicFill = existingStats.GetMagicFill();
            }

            existingHero.SetActive(false);
        }

        var newHero = Object.Instantiate(entry.prefab, pos, rot, parent);
        newHero.tag = "Hero";
        var newStats = newHero.GetComponent<FighterStats>();
        if (newStats != null && (healthFill != null || magicFill != null))
        {
            newStats.SetBars(healthFill, magicFill);
        }

        if (existingHero != null)
        {
            Object.Destroy(existingHero);
        }
    }
}
