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

    void RefreshTargets()
    {
        if (hero == null)
        {
            hero = GameObject.FindGameObjectWithTag("Hero");
        }

        if (enemy == null)
        {
            enemy = GameObject.FindGameObjectWithTag("Enemy");
        }
    }
    
    
    public void SelectAttack(string btn)
    {
        RefreshTargets();

        var controllerObj = GameObject.Find("GameControllerObject");
        if (controllerObj != null)
        {
            var controller = controllerObj.GetComponent<GameController>();
            if (controller != null)
            {
                controller.OnPlayerActionStarted();
                controller.isBusy = true;
            }
        }



        Debug.Log("Selected attack: " + btn);

        GameObject victim = hero;
        if (tag == "Hero")
        {
            victim = enemy;
        }

        if (victim == null)
        {
            GameObject.Find("GameControllerObject").GetComponent<GameController>().isBusy = false;
            GameObject.Find("GameControllerObject").GetComponent<GameController>().NextTurn();
            return;
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

    public void PlayCard(CardData card)
    {
        var controllerObj = GameObject.Find("GameControllerObject");
        if (controllerObj == null)
        {
            return;
        }

        var controller = controllerObj.GetComponent<GameController>();
        if (controller == null || controller.isBusy)
        {
            return;
        }

        if (card == null)
        {
            return;
        }

        controller.OnPlayerActionStarted();
        controller.isBusy = true;

        FighterStats stats = GetComponent<FighterStats>();
        if (stats == null)
        {
            controller.isBusy = false;
            return;
        }

        string name = (card.cardName ?? "").Trim().ToLowerInvariant();
        if (name == "wild card")
        {
            var options = new List<string> { "stab", "magic missle", "heal", "defense", "restore mana" };
            string chosen = options[Random.Range(0, options.Count)];
            ApplyCardEffect(chosen, stats, card, 2);
            return;
        }

        ApplyCardEffect(name, stats, card, 1);
    }

    void ApplyCardEffect(string name, FighterStats stats, CardData card, int effectMultiplier)
    {
        var controllerObj = GameObject.Find("GameControllerObject");
        var controller = controllerObj != null ? controllerObj.GetComponent<GameController>() : null;

        if (controller == null)
        {
            return;
        }

        int manaCost = card.cost;
        if (manaCost > 0 && stats.magic < manaCost)
        {
            controller.isBusy = false;
            controller.NextTurn();
            return;
        }

        if (manaCost > 0)
        {
            stats.updateMagicFill(manaCost);
        }

        var victim = enemy;
        if (tag != "Hero")
        {
            controller.isBusy = false;
            controller.NextTurn();
            return;
        }

        if (victim == null)
        {
            controller.isBusy = false;
            controller.NextTurn();
            return;
        }

        if (name == "stab" || name == "magic missle")
        {
            int baseDamage = Mathf.Max(0, card.damage * effectMultiplier);
            if (name == "stab" && meleePrefab != null)
            {
                var attack = meleePrefab.GetComponent<AttackScript>();
                if (attack != null)
                {
                    attack.AttackFixedDamage(victim, baseDamage);
                    return;
                }
            }

            if (name == "magic missle" && rangePrefab != null)
            {
                var attack = rangePrefab.GetComponent<AttackScript>();
                if (attack != null)
                {
                    attack.AttackFixedDamage(victim, baseDamage);
                    return;
                }
            }

            bool isCrit = Random.value < 0.25f;
            int finalDamage = isCrit ? baseDamage * 2 : baseDamage;
            controller.RecordDamageDealt("Hero", finalDamage);
            victim.GetComponent<FighterStats>().ReceiveDamage(finalDamage, isCrit);
            return;
        }

        if (name == "heal")
        {
            float heal = Mathf.Max(0, card.damage * effectMultiplier);
            stats.ReceiveHealing(heal);
            return;
        }

        if (name == "defense")
        {
            stats.defense += (card.damage * effectMultiplier);
            if (controller.battleText != null)
            {
                controller.battleText.gameObject.SetActive(true);
                controller.battleText.text = "+" + (card.damage * effectMultiplier).ToString();
            }
            StartCoroutine(ContinueAfter(2f));
            return;
        }

        if (name == "restore mana")
        {
            stats.ModifyMagic(20f * effectMultiplier);
            stats.ReceiveDamage(5 * effectMultiplier, false);
            return;
        }

        controller.isBusy = false;
        controller.NextTurn();
    }

    IEnumerator ContinueAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        var controllerObj = GameObject.Find("GameControllerObject");
        if (controllerObj == null)
        {
            yield break;
        }

        var controller = controllerObj.GetComponent<GameController>();
        if (controller == null)
        {
            yield break;
        }

        controller.isBusy = false;
        controller.NextTurn();
    }
}
