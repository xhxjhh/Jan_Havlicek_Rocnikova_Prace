using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleHeroHudUpdater : MonoBehaviour
{
    static BattleHeroHudUpdater instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Object.FindFirstObjectByType<BattleHeroHudUpdater>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        var go = new GameObject("BattleHeroHudUpdater");
        go.AddComponent<BattleHeroHudUpdater>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneNames.Battle)
        {
            return;
        }

        StartCoroutine(ApplyNextFrame());
    }

    IEnumerator ApplyNextFrame()
    {
        yield return null;
        Apply();
    }

    void Apply()
    {
        var roster = Resources.Load<HeroRoster>("HeroRoster");
        if (roster == null || roster.heroes == null || roster.heroes.Count == 0)
        {
            return;
        }

        int index = HeroSelection.GetSelectedIndex(roster.defaultIndex);
        if (index < 0 || index >= roster.heroes.Count)
        {
            index = Mathf.Clamp(index, 0, roster.heroes.Count - 1);
        }

        var entry = roster.heroes[index];
        if (entry == null)
        {
            return;
        }

        var heroInfo = FindByNameIncludingInactive("HeroInfo");
        if (heroInfo == null)
        {
            return;
        }

        var nameTransform = heroInfo.transform.Find("Text (TMP)");
        if (nameTransform != null)
        {
            var tmp = nameTransform.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = string.IsNullOrWhiteSpace(entry.displayName) ? tmp.text : entry.displayName;
            }
        }

        if (entry.portrait != null)
        {
            var faceTransform = heroInfo.transform.Find("Frame/Face");
            if (faceTransform != null)
            {
                var img = faceTransform.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = entry.portrait;
                }
            }
        }
    }

    static GameObject FindByNameIncludingInactive(string objectName)
    {
        var transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in transforms)
        {
            if (t == null || t.name != objectName)
            {
                continue;
            }

            var scene = t.gameObject.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}

