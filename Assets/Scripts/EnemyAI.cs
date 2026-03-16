using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] float healHpThreshold = 0.4f;
    [SerializeField, Range(0f, 1f)] float restoreManaThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] float minHpToRestoreMana = 0.25f;
    [SerializeField, Range(0f, 1f)] float defenseHpThreshold = 0.6f;

    [SerializeField] float healPercentOfMaxHp = 0.15f;
    [SerializeField] float healManaPercentCost = 0.2f;
    [SerializeField] float restoreManaAmount = 20f;
    [SerializeField] int restoreManaSelfDamage = 5;
    [SerializeField] float defenseBonus = 5f;
    [SerializeField] float actionDelaySeconds = 2f;

    FighterStats stats;
    FighterAction action;
    bool healedLastTurn;

    void Awake()
    {
        stats = GetComponent<FighterStats>();
        action = GetComponent<FighterAction>();
    }

    public void TakeTurn(GameObject hero)
    {
        var controllerObj = GameObject.Find("GameControllerObject");
        var controller = controllerObj != null ? controllerObj.GetComponent<GameController>() : null;
        if (controller == null)
        {
            return;
        }

        if (controller.isBusy)
        {
            return;
        }

        if (stats == null)
        {
            stats = GetComponent<FighterStats>();
        }

        if (action == null)
        {
            action = GetComponent<FighterAction>();
        }

        if (stats == null || hero == null)
        {
            controller.isBusy = false;
            controller.NextTurn();
            return;
        }

        float hpMax = Mathf.Max(1f, stats.GetStartHealth());
        float manaMax = Mathf.Max(1f, stats.GetStartMagic());
        float hpPct = Mathf.Clamp01(stats.health / hpMax);
        float manaPct = Mathf.Clamp01(stats.magic / manaMax);

        float healManaCost = healManaPercentCost * manaMax;
        bool canHeal = stats.magic >= healManaCost && stats.health < stats.GetStartHealth();
        bool shouldHeal = canHeal && hpPct <= healHpThreshold;

        bool shouldRestoreMana = manaPct <= restoreManaThreshold && hpPct >= minHpToRestoreMana;

        bool shouldDefense = hpPct <= defenseHpThreshold && stats.magic >= 0f && defenseBonus > 0f;

        if (shouldHeal && !healedLastTurn)
        {
            controller.isBusy = true;
            var anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play("Heal");
            }
            stats.ReceiveHealing(healPercentOfMaxHp * hpMax);
            stats.updateMagicFill(healManaCost);
            healedLastTurn = true;
            return;
        }

        if (healedLastTurn)
        {
            controller.isBusy = true;
            if (action != null)
            {
                string attackType = Random.Range(0, 2) == 1 ? "melee" : "range";
                action.SelectAttack(attackType);
                healedLastTurn = false;
                return;
            }

            controller.isBusy = false;
            controller.NextTurn();
            return;
        }

        if (shouldRestoreMana)
        {
            controller.isBusy = true;
            stats.ModifyMagic(restoreManaAmount);
            stats.ReceiveDamage(restoreManaSelfDamage, false);
            healedLastTurn = false;
            return;
        }

        if (shouldDefense)
        {
            controller.isBusy = true;
            stats.defense += defenseBonus;
            if (controller.battleText != null)
            {
                controller.battleText.gameObject.SetActive(true);
                controller.battleText.text = "+" + Mathf.CeilToInt(defenseBonus).ToString();
            }
            StartCoroutine(FinishAfterDelay());
            healedLastTurn = false;
            return;
        }

        controller.isBusy = true;
        if (action != null)
        {
            string attackType = Random.Range(0, 2) == 1 ? "melee" : "range";
            action.SelectAttack(attackType);
            healedLastTurn = false;
            return;
        }

        controller.isBusy = false;
        controller.NextTurn();
    }

    IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSeconds(actionDelaySeconds);
        var controllerObj = GameObject.Find("GameControllerObject");
        var controller = controllerObj != null ? controllerObj.GetComponent<GameController>() : null;
        if (controller == null)
        {
            yield break;
        }

        controller.isBusy = false;
        controller.NextTurn();
    }
}
