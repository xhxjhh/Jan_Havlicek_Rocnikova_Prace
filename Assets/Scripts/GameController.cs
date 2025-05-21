using System.Collections;
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


    private void Awake()
    {
        battleMenu = GameObject.Find("ActionMenu");
    }
    void Start()
    {
        fighterStats = new List<FighterStats>();
        GameObject hero = GameObject.FindGameObjectWithTag("Hero");
        FighterStats currentFighterStats = hero.GetComponent<FighterStats>();
        currentFighterStats.CalculateNextTurn(0);
        fighterStats.Add(currentFighterStats);

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        FighterStats currentEnemyStats = enemy.GetComponent<FighterStats>();
        currentEnemyStats.CalculateNextTurn(0);
        fighterStats.Add(currentEnemyStats);

        fighterStats.Sort();
        

        NextTurn();
    }

    public void NextTurn()
    {
        battleText.gameObject.SetActive(false);
        FighterStats currentFighterStats = fighterStats[0];
        fighterStats.Remove(currentFighterStats);

Debug.Log("Next turn for: " + currentFighterStats.gameObject.name);

        if (!currentFighterStats.GetDead())
        {
            GameObject currentUnit = currentFighterStats.gameObject;
            currentFighterStats.CalculateNextTurn(currentFighterStats.nextActTurn);
            fighterStats.Add(currentFighterStats);
            fighterStats.Sort();
            if(currentUnit.tag == "Hero")
            {
                Debug.Log("Hero's turn");
                battleMenu.SetActive(true);
                

            } else
            {
                Debug.Log("Enemy's turn");
                this.battleMenu.SetActive(false);
                string attackType = Random.Range(0, 2) == 1 ? "melee" : "range";
                Debug.Log("Enemy selected attack: " + attackType);
                currentUnit.GetComponent<FighterAction>().SelectAttack(attackType);
            }
        } else
{
    Debug.Log("Unit is dead. Skipping turn.");

    // Check if the Hero or Enemy still exist in the scene
    GameObject hero = GameObject.FindGameObjectWithTag("Hero");
    GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

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
        PlayerPrefs.SetString("GameResult", result);
        SceneManager.LoadScene("GameOverScene");
    }

}