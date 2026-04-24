﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    private const string StageIndexKey = "StageIndex";
    private const string HasSaveKey = "HasSave";
    private const string SaveDataKey = "SaveData";
    private const string LoadOnBattleStartKey = "LoadOnBattleStart";
    private const string SaveLocationBattle = "Battle";
    private const string SaveLocationGameOver = "GameOver";
    private const string SaveLocationKey = "SaveLocation";

    private List<FighterStats> fighterStats;

    private GameObject battleMenu;

    public Text battleText;

    public bool isBusy = false;

    private bool gameEnded = false;
    private int turnsElapsed = 0;
    private int heroDamageDealt = 0;
    private CardManager cardManager;
    private bool awaitingPlayerInput = false;

    [SerializeField]
    private float enemyStatMultiplierPerStage = 1.1f;

    [SerializeField]
    private Sprite[] stageBackgrounds;

    [SerializeField]
    private string backgroundObjectName = "Background";

    [SerializeField]
    private SpriteRenderer backgroundSpriteRenderer;

    [SerializeField]
    private Image backgroundImage;

    [System.Serializable]
    private class UnitSaveData
    {
        public string tag;
        public float health;
        public float magic;
        public float melee;
        public float magicRange;
        public float defense;
        public float speed;
        public float experience;
        public float startHealth;
        public float startMagic;
        public int nextActTurn;
    }

    [System.Serializable]
    private class GameSaveData
    {
        public int version;
        public string saveLocation;
        public string gameResult;
        public int stageIndex;
        public int turnsElapsed;
        public int heroDamageDealt;
        public bool awaitingPlayerInput;
        public bool wasBusy;
        public string[] handCardNames;
        public UnitSaveData hero;
        public UnitSaveData enemy;
    }


    private void Awake()
    {
        battleMenu = GameObject.Find("ActionMenu");
        if (battleMenu == null)
        {
            battleMenu = FindByNameIncludingInactive("ActionMenu");
        }

        if (battleText == null)
        {
            var battleTextObj = FindByNameIncludingInactive("BattleMessage");
            if (battleTextObj != null)
            {
                battleText = battleTextObj.GetComponent<Text>();
            }
        }

        cardManager = Object.FindFirstObjectByType<CardManager>();
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

    void ApplyStageBackground(int stageIndex)
    {
        if (stageBackgrounds == null || stageBackgrounds.Length == 0)
        {
            return;
        }

        int idx = Mathf.Abs(stageIndex - 1) % stageBackgrounds.Length;
        var sprite = stageBackgrounds[idx];
        if (sprite == null)
        {
            return;
        }

        if (backgroundSpriteRenderer != null)
        {
            backgroundSpriteRenderer.sprite = sprite;
            return;
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.enabled = sprite != null;
            return;
        }

        GameObject bg = GameObject.Find(backgroundObjectName);
        if (bg == null && !string.IsNullOrWhiteSpace(backgroundObjectName))
        {
            bg = FindByNameIncludingInactive(backgroundObjectName);
        }

        if (bg == null)
        {
            return;
        }

        var spriteRenderer = bg.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = bg.GetComponentInChildren<SpriteRenderer>(true);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            return;
        }

        var image = bg.GetComponent<Image>();
        if (image == null)
        {
            image = bg.GetComponentInChildren<Image>(true);
        }
        if (image != null)
        {
            image.sprite = sprite;
            image.enabled = sprite != null;
        }
    }

    void Start()
    {
        bool shouldLoad = PlayerPrefs.GetInt(LoadOnBattleStartKey, 0) == 1 && PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
        if (shouldLoad)
        {
            PlayerPrefs.SetInt(LoadOnBattleStartKey, 0);
            PlayerPrefs.Save();
            if (TryLoadFromPrefs())
            {
                return;
            }
        }

        InitNewBattle();
    }

    void InitNewBattle()
    {
        turnsElapsed = 0;
        heroDamageDealt = 0;
        awaitingPlayerInput = false;
        PlayerPrefs.SetInt("TurnsElapsed", 0);
        PlayerPrefs.SetInt("HeroDamageDealt", 0);

        int stageIndex = Mathf.Max(1, PlayerPrefs.GetInt(StageIndexKey, 1));
        PlayerPrefs.SetInt(StageIndexKey, stageIndex);
        ApplyStageBackground(stageIndex);

        fighterStats = new List<FighterStats>();
        GameObject hero = GameObject.FindGameObjectWithTag("Hero");
        if (hero == null)
        {
            Debug.LogError("Hero with tag 'Hero' was not found.");
            return;
        }

        FighterStats currentFighterStats = hero.GetComponent<FighterStats>();
        currentFighterStats.CalculateNextTurn(0);
        fighterStats.Add(currentFighterStats);

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy == null)
        {
            Debug.LogError("Enemy with tag 'Enemy' was not found.");
            return;
        }

        FighterStats currentEnemyStats = enemy.GetComponent<FighterStats>();
        if (currentEnemyStats == null)
        {
            Debug.LogError("Enemy is missing FighterStats component.");
            return;
        }

        float multiplier = Mathf.Pow(enemyStatMultiplierPerStage, stageIndex - 1);
        currentEnemyStats.ApplyStatMultiplier(multiplier);
        currentEnemyStats.CalculateNextTurn(0);
        fighterStats.Add(currentEnemyStats);

        fighterStats.Sort();
        NextTurn();
    }

    bool TryLoadFromPrefs()
    {
        string json = PlayerPrefs.GetString(SaveDataKey, "");
        if (string.IsNullOrWhiteSpace(json))
        {
            PlayerPrefs.SetInt(HasSaveKey, 0);
            PlayerPrefs.Save();
            return false;
        }

        GameSaveData data;
        try
        {
            data = JsonUtility.FromJson<GameSaveData>(json);
        }
        catch
        {
            PlayerPrefs.SetInt(HasSaveKey, 0);
            PlayerPrefs.Save();
            return false;
        }

        if (data == null || data.hero == null || data.enemy == null)
        {
            PlayerPrefs.SetInt(HasSaveKey, 0);
            PlayerPrefs.Save();
            return false;
        }

        int stageIndex = Mathf.Max(1, data.stageIndex);
        PlayerPrefs.SetInt(StageIndexKey, stageIndex);
        ApplyStageBackground(stageIndex);

        turnsElapsed = Mathf.Max(0, data.turnsElapsed);
        heroDamageDealt = Mathf.Max(0, data.heroDamageDealt);
        PlayerPrefs.SetInt("TurnsElapsed", turnsElapsed);
        PlayerPrefs.SetInt("HeroDamageDealt", heroDamageDealt);

        awaitingPlayerInput = data.awaitingPlayerInput;
        isBusy = false;
        gameEnded = false;

        GameObject heroObj = GameObject.FindGameObjectWithTag("Hero");
        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (heroObj == null || enemyObj == null)
        {
            return false;
        }

        var heroStats = heroObj.GetComponent<FighterStats>();
        var enemyStats = enemyObj.GetComponent<FighterStats>();
        if (heroStats == null || enemyStats == null)
        {
            return false;
        }

        ApplyUnitSave(heroStats, data.hero);
        ApplyUnitSave(enemyStats, data.enemy);

        fighterStats = new List<FighterStats> { heroStats, enemyStats };
        fighterStats.Sort();

        if (data.wasBusy)
        {
            awaitingPlayerInput = false;
            NextTurn();
            return true;
        }

        if (awaitingPlayerInput)
        {
            if (battleMenu == null)
            {
                battleMenu = FindByNameIncludingInactive("ActionMenu");
            }
            if (battleMenu != null)
            {
                battleMenu.SetActive(true);
            }
            if (cardManager != null)
            {
                if (data.handCardNames != null && data.handCardNames.Length > 0)
                {
                    cardManager.RestoreHand(data.handCardNames);
                }
                else
                {
                    cardManager.DrawCards();
                }
            }
            return true;
        }

        NextTurn();
        return true;
    }

    static void ApplyUnitSave(FighterStats stats, UnitSaveData data)
    {
        stats.health = data.health;
        stats.magic = data.magic;
        stats.melee = data.melee;
        stats.magicRange = data.magicRange;
        stats.defense = data.defense;
        stats.speed = data.speed;
        stats.experience = data.experience;
        stats.startHealth = data.startHealth;
        stats.startMagic = data.startMagic;
        stats.nextActTurn = data.nextActTurn;
        stats.RefreshBars();
    }

    public void NextTurn()
    {
        if (gameEnded)
        {
            return;
        }

        var hero = GameObject.FindGameObjectWithTag("Hero");
        var enemy = GameObject.FindGameObjectWithTag("Enemy");

        if (hero == null && enemy == null)
        {
            EndGame("Both units are gone?");
            return;
        }

        if (hero == null)
        {
            EndGame("You Lost...");
            return;
        }

        if (enemy == null)
        {
            EndGame("You Won!");
            return;
        }

        if (battleText != null)
        {
            battleText.gameObject.SetActive(false);
        }

        fighterStats.RemoveAll(s => s == null);
        if (fighterStats.Count == 0)
        {
            EndGame("Game Over");
            return;
        }

        FighterStats currentFighterStats = fighterStats[0];
        fighterStats.Remove(currentFighterStats);

        if (currentFighterStats == null)
        {
            NextTurn();
            return;
        }

        Debug.Log("Next turn for: " + currentFighterStats.gameObject.name);

        if (!currentFighterStats.GetDead())
        {
            turnsElapsed++;
            GameObject currentUnit = currentFighterStats.gameObject;
            currentFighterStats.CalculateNextTurn(currentFighterStats.nextActTurn);
            fighterStats.Add(currentFighterStats);
            fighterStats.Sort();
            if(currentUnit.tag == "Hero")
            {
                Debug.Log("Hero's turn");
                awaitingPlayerInput = true;
                if (battleMenu == null)
                {
                    battleMenu = FindByNameIncludingInactive("ActionMenu");
                }
                if (battleMenu != null)
                {
                    battleMenu.SetActive(true);
                }
                if (cardManager != null)
                {
                    cardManager.DrawCards();
                }
                

            } else
            {
                Debug.Log("Enemy's turn");
                awaitingPlayerInput = false;
                if (battleMenu == null)
                {
                    battleMenu = FindByNameIncludingInactive("ActionMenu");
                }
                if (battleMenu != null)
                {
                    battleMenu.SetActive(false);
                }
                var ai = currentUnit.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.TakeTurn(hero);
                }
                else
                {
                    string attackType = Random.Range(0, 2) == 1 ? "melee" : "range";
                    Debug.Log("Enemy selected attack: " + attackType);
                    currentUnit.GetComponent<FighterAction>().SelectAttack(attackType);
                }
            }
        } else
{
    Debug.Log("Unit is dead. Skipping turn.");

    if (hero == null && enemy == null)
    {
        EndGame("Both units are gone?");
    }
    else if (hero == null)
    {
        EndGame("You Lost...");
    }
    else if (enemy == null)
    {
        EndGame("You Won!");
    }
    else
    {
        NextTurn(); // Proceed to next fighter
    }
}


    }

    public void OnPlayerActionStarted()
    {
        awaitingPlayerInput = false;
    }

    public void SaveGame()
    {
        var heroObj = GameObject.FindGameObjectWithTag("Hero");
        var enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (heroObj == null || enemyObj == null)
        {
            return;
        }

        var heroStats = heroObj.GetComponent<FighterStats>();
        var enemyStats = enemyObj.GetComponent<FighterStats>();
        if (heroStats == null || enemyStats == null)
        {
            return;
        }

        var data = new GameSaveData
        {
            version = 1,
            saveLocation = SaveLocationBattle,
            gameResult = PlayerPrefs.GetString("GameResult", ""),
            stageIndex = Mathf.Max(1, PlayerPrefs.GetInt(StageIndexKey, 1)),
            turnsElapsed = turnsElapsed,
            heroDamageDealt = heroDamageDealt,
            awaitingPlayerInput = awaitingPlayerInput,
            wasBusy = isBusy,
            handCardNames = GetHandCardNames(cardManager),
            hero = MakeUnitSave(heroStats, "Hero"),
            enemy = MakeUnitSave(enemyStats, "Enemy")
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveDataKey, json);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(SaveLocationKey, SaveLocationBattle);
        PlayerPrefs.Save();

        if (battleText != null)
        {
            battleText.gameObject.SetActive(true);
            battleText.text = "Uloženo";
        }
    }

    static UnitSaveData MakeUnitSave(FighterStats stats, string tag)
    {
        return new UnitSaveData
        {
            tag = tag,
            health = stats.health,
            magic = stats.magic,
            melee = stats.melee,
            magicRange = stats.magicRange,
            defense = stats.defense,
            speed = stats.speed,
            experience = stats.experience,
            startHealth = stats.startHealth,
            startMagic = stats.startMagic,
            nextActTurn = stats.nextActTurn
        };
    }

    static string[] GetHandCardNames(CardManager manager)
    {
        if (manager == null || manager.hand == null || manager.hand.Count == 0)
        {
            return new string[0];
        }

        var names = new List<string>(manager.hand.Count);
        foreach (var card in manager.hand)
        {
            if (card == null)
            {
                continue;
            }
            names.Add(card.cardName ?? "");
        }
        return names.ToArray();
    }

    public static void RequestLoadSavedGame()
    {
        PlayerPrefs.SetInt(LoadOnBattleStartKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.Battle);
    }

    public static void ContinueFromSave()
    {
        if (PlayerPrefs.GetInt(HasSaveKey, 0) != 1)
        {
            SceneManager.LoadScene(SceneNames.Battle);
            return;
        }

        string json = PlayerPrefs.GetString(SaveDataKey, "");
        if (string.IsNullOrWhiteSpace(json))
        {
            PlayerPrefs.SetInt(HasSaveKey, 0);
            PlayerPrefs.DeleteKey(SaveLocationKey);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneNames.Battle);
            return;
        }

        GameSaveData data;
        try
        {
            data = JsonUtility.FromJson<GameSaveData>(json);
        }
        catch
        {
            PlayerPrefs.SetInt(HasSaveKey, 0);
            PlayerPrefs.DeleteKey(SaveLocationKey);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneNames.Battle);
            return;
        }

        if (data == null)
        {
            SceneManager.LoadScene(SceneNames.Battle);
            return;
        }

        PlayerPrefs.SetInt(StageIndexKey, Mathf.Max(1, data.stageIndex));
        PlayerPrefs.SetInt("TurnsElapsed", Mathf.Max(0, data.turnsElapsed));
        PlayerPrefs.SetInt("HeroDamageDealt", Mathf.Max(0, data.heroDamageDealt));
        if (!string.IsNullOrWhiteSpace(data.gameResult))
        {
            PlayerPrefs.SetString("GameResult", data.gameResult);
        }
        PlayerPrefs.Save();

        string location = PlayerPrefs.GetString(SaveLocationKey, "");
        if (string.IsNullOrWhiteSpace(location))
        {
            location = string.IsNullOrWhiteSpace(data.saveLocation) ? SaveLocationBattle : data.saveLocation;
        }
        if (location == SaveLocationGameOver)
        {
            SceneManager.LoadScene(SceneNames.GameOver);
            return;
        }

        RequestLoadSavedGame();
    }

    public static void SaveBetweenStages()
    {
        string result = PlayerPrefs.GetString("GameResult", "Game Over");
        if (result != "You Won!")
        {
            return;
        }

        var data = new GameSaveData
        {
            version = 1,
            saveLocation = SaveLocationGameOver,
            gameResult = result,
            stageIndex = Mathf.Max(1, PlayerPrefs.GetInt(StageIndexKey, 1)),
            turnsElapsed = Mathf.Max(0, PlayerPrefs.GetInt("TurnsElapsed", 0)),
            heroDamageDealt = Mathf.Max(0, PlayerPrefs.GetInt("HeroDamageDealt", 0)),
            awaitingPlayerInput = false,
            wasBusy = false,
            handCardNames = new string[0],
            hero = null,
            enemy = null
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveDataKey, json);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(SaveLocationKey, SaveLocationGameOver);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    private void EndGame(string result)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        PlayerPrefs.SetInt("TurnsElapsed", turnsElapsed);
        PlayerPrefs.SetInt("HeroDamageDealt", heroDamageDealt);
        PlayerPrefs.SetString("GameResult", result);
        if (result != "You Won!")
        {
            PlayerPrefs.SetInt(StageIndexKey, 1);
        }
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneNames.GameOver);
    }

    public void RecordDamageDealt(string attackerTag, int damage)
    {
        if (gameEnded)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        if (attackerTag == "Hero")
        {
            heroDamageDealt += damage;
        }
    }

    public void NotifyUnitDied(string unitTag)
    {
        if (gameEnded)
        {
            return;
        }

        if (unitTag == "Hero")
        {
            EndGame("You Lost...");
            return;
        }

        if (unitTag == "Enemy")
        {
            EndGame("You Won!");
            return;
        }

        var hero = GameObject.FindGameObjectWithTag("Hero");
        var enemy = GameObject.FindGameObjectWithTag("Enemy");

        if (hero == null && enemy == null)
        {
            EndGame("Both units are gone?");
            return;
        }

        if (hero == null)
        {
            EndGame("You Lost...");
            return;
        }

        if (enemy == null)
        {
            EndGame("You Won!");
            return;
        }
    }

}
