using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterAction : MonoBehaviour
{
    private GameObject hero;
    private GameObject enemy;

    [SerializeField]
    private GameObject meleePrefab;

    [SerializeField]
    private GameObject rangePrefab;

    [SerializeField]
    private Sprite faceIcon;

    private GameObject currentAttack;
    
    void Awake()
    {
        hero = GameObject.FindGameObjectWithTag("Hero");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
    }

    
    public void SelectAttack(string btn)
    {

        GameObject.Find("GameControllerObject").GetComponent<GameController>().isBusy = true;


        Debug.Log("Selected attack: " + btn);

        GameObject victim = hero;
        if (tag == "Hero")
        {
            victim = enemy;
        }
        if (btn.CompareTo("melee") == 0)
        {
            Debug.Log("Melee attack on: " + victim.name);
            meleePrefab.GetComponent<AttackScript>().Attack(victim);

        } else if (btn.CompareTo("range") == 0)
        {
            Debug.Log("Range attack on: " + victim.name);
            rangePrefab.GetComponent<AttackScript>().Attack(victim);
        } else if (btn.CompareTo("run") == 0)
        {
            Debug.Log("Run card used: Healing + Mana cost");

            FighterStats stats = GetComponent<FighterStats>();

            float healAmount = 0.15f * stats.GetStartHealth();
            float manaCost = 0.20f * stats.GetStartMagic();

            if (stats.magic >= manaCost)
            {
                if (stats.GetComponent<Animator>() != null)
                {
                    stats.GetComponent<Animator>().Play("Heal"); // 👈 animation call
                }

                stats.ReceiveHealing(healAmount);
                stats.updateMagicFill(manaCost);
            }
            else
            {
                Debug.Log("Not enough mana to use Run card!");
                GameObject.Find("GameControllerObject").GetComponent<GameController>().NextTurn();
            }
        }
    }
}