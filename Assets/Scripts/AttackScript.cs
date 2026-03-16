using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public GameObject owner;

    [SerializeField]
    private string animationName;

    [SerializeField]
    private bool magicAttack;

    [SerializeField]
    private float magicCost;

    [SerializeField]
    private float minAttackMultiplier;

    [SerializeField]
    private float maxAttackMultiplier;

    [SerializeField]
    private float minDefenseMultiplier;

    [SerializeField]
    private float maxDefenseMultiplier;

    private FighterStats attackerStats;
    private FighterStats targetStats;
    private float damage = 0.0f;
    
    public void Attack(GameObject victim)
    {
        if (owner == null || victim == null)
        {
            Invoke("SkipTurnContinueGame", 2);
            return;
        }

        attackerStats = owner.GetComponent<FighterStats>();
        targetStats = victim.GetComponent<FighterStats>();
        if (attackerStats == null || targetStats == null)
        {
            Invoke("SkipTurnContinueGame", 2);
            return;
        }

        if (attackerStats.magic >= magicCost)
        {
            float multiplier = Random.Range(minAttackMultiplier, maxAttackMultiplier);

            damage = multiplier * attackerStats.melee;
            if (magicAttack)
            {
                damage = multiplier * attackerStats.magicRange;
            }

            float defenseMultiplier = Random.Range(minDefenseMultiplier, maxDefenseMultiplier);
            damage = Mathf.Max(0, damage - (defenseMultiplier * targetStats.defense));

            Debug.Log("Attacking with animation: " + animationName);

            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animationName);
            }

            bool isCrit = Random.value < 0.25f;
            float finalDamage = isCrit ? (damage * 2f) : damage;
            int dealtDamage = Mathf.CeilToInt(finalDamage);
            var controllerObj = GameObject.Find("GameControllerObject");
            if (controllerObj != null)
            {
                var controller = controllerObj.GetComponent<GameController>();
                if (controller != null && owner != null)
                {
                    controller.RecordDamageDealt(owner.tag, dealtDamage);
                }
            }

            targetStats.ReceiveDamage(dealtDamage, isCrit);
            attackerStats.updateMagicFill(magicCost);
        } else
        {
            Invoke("SkipTurnContinueGame", 2);
        }
    }

    public void AttackFixedDamage(GameObject victim, int baseDamage)
    {
        if (owner == null || victim == null)
        {
            Invoke("SkipTurnContinueGame", 2);
            return;
        }

        targetStats = victim.GetComponent<FighterStats>();
        if (targetStats == null)
        {
            Invoke("SkipTurnContinueGame", 2);
            return;
        }

        var animator = owner.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animationName);
        }

        bool isCrit = Random.value < 0.25f;
        int dealtDamage = isCrit ? (baseDamage * 2) : baseDamage;

        var controllerObj = GameObject.Find("GameControllerObject");
        if (controllerObj != null)
        {
            var controller = controllerObj.GetComponent<GameController>();
            if (controller != null)
            {
                controller.RecordDamageDealt(owner.tag, dealtDamage);
            }
        }

        targetStats.ReceiveDamage(dealtDamage, isCrit);
    }

    void SkipTurnContinueGame()
    {
        GameObject.Find("GameControllerObject").GetComponent<GameController>().NextTurn();
    }
}
