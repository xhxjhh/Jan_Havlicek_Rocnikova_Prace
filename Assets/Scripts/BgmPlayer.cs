using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmPlayer : MonoBehaviour
{
    static BgmPlayer instance;
    AudioSource source;
    BgmConfig config;
    [SerializeField] AudioClip mainMenu;
    [SerializeField] AudioClip battle;
    [SerializeField] AudioClip gameOver;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Object.FindFirstObjectByType<BgmPlayer>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        var go = new GameObject("BgmPlayer");
        go.AddComponent<BgmPlayer>();
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

        source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = true;
        config = Resources.Load<BgmConfig>("BgmConfig");
        if (config != null)
        {
            source.volume = config.volume;
        }
        else
        {
            source.volume = volume;
        }

        AudioListener.volume = AudioSettingsRuntime.GetMasterVolume();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyForScene(scene.name);
    }

    void ApplyForScene(string sceneName)
    {
        if (config == null)
        {
            config = Resources.Load<BgmConfig>("BgmConfig");
            if (config != null)
            {
                source.volume = config.volume;
            }
            else
            {
                var found = Resources.FindObjectsOfTypeAll<BgmConfig>();
                if (found != null && found.Length > 0)
                {
                    config = found[0];
                    source.volume = config.volume;
                }
            }
        }

        AudioClip clip = null;
        if (sceneName == SceneNames.MainMenu)
        {
            clip = config != null ? config.mainMenu : mainMenu;
        }
        else if (sceneName == SceneNames.Battle)
        {
            clip = config != null ? config.battle : battle;
        }
        else if (sceneName == SceneNames.GameOver)
        {
            clip = config != null ? config.gameOver : gameOver;
        }

        if (clip == null)
        {
            source.Stop();
            source.clip = null;
            if (config == null && mainMenu == null && battle == null && gameOver == null)
            {
                Debug.LogWarning("BgmPlayer: No BgmConfig found and no clips assigned on BgmPlayer.");
            }
            return;
        }

        if (source.clip == clip && source.isPlaying)
        {
            return;
        }

        source.clip = clip;
        source.Play();
    }
}
