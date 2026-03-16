﻿﻿﻿﻿﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Transactions;
using UnityEngine.SocialPlatforms;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    private List<FighterStats> fighterStats;

    private GameObject battleMenu;

    public Text battleText;

    public bool isBusy = false;

    private bool gameEnded = false;
    private int turnsElapsed = 0;
    private int heroDamageDealt = 0;
    private CardManager cardManager;


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
    void Start()
    {
        turnsElapsed = 0;
        heroDamageDealt = 0;
        PlayerPrefs.SetInt("TurnsElapsed", 0);
        PlayerPrefs.SetInt("HeroDamageDealt", 0);

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
        currentEnemyStats.CalculateNextTurn(0);
        fighterStats.Add(currentEnemyStats);

        fighterStats.Sort();
        

        NextTurn();
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
