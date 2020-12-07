using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Organizer.Audio;
public class Unit : MonoBehaviour
{
    public string unitName = "Name";
    public bool isPlayer = false;
    public bool isBoss = false;
    public Inventory inventory = null;

    [Header("Stats")]
    public float maxHealth = 100;
    public Item stats = null;
    public Item unitStats = null;
    public Item sword = null;
    public Item helmet = null;

    [Header("Other")]
    public float currentHealth = 1f;
    public Animator animator = null;

    [Header("Visual")]
    [SerializeField] ParticleSystem healParticle = null;
    [SerializeField] ParticleSystem damageParticle = null;
    [SerializeField] ParticleSystem deathParticle = null;
    [SerializeField] ParticleSystem burningParticle = null;
    [SerializeField] ParticleSystem sparkleParticle = null;
    [SerializeField] GameObject critEffect = null;
    [SerializeField] GameObject missEffect = null;
    [SerializeField] SpriteRenderer helmetSprite = null;
    [SerializeField] SpriteRenderer swordSprite = null;
    [SerializeField] SpeechBubble hitSplash = null;

    public List<bool> indicatorLastStates = new List<bool>();
    public bool blocking = false;
    public bool charging = false;
    public int poisoned = 0;
    public float poisonStrength = 1f;
    public int burning = 0;
    public float burningStrength = 1f;
    public bool isDead = false;
    public bool weakened = false;
    public float weakenedStrength = 1f;
    public bool blinded = false;
    public float blindedStrength = 1f;

    public enum StatusEffects { Attack, Poison, Burning, Blindness }

    public AudioOrganizer audioOrganizer = null;

    void Awake()
    {
        stats = Item.CreateInstance<Item>();
        if (isPlayer)
            stats.itemType = Item.itemTypes.Player;
        animator = GetComponent<Animator>();
        animator.SetBool("Player", isPlayer);
        if (inventory)
        {
            helmet = inventory.GetEquippedItem(Item.itemTypes.Helmet);
            sword = inventory.GetEquippedItem(Item.itemTypes.Sword);
        }
        SetStats(unitStats);
        SetStats(helmet);
        SetStats(sword);
        currentHealth = maxHealth;
    }
    void SetStats(Item item)
    {
        if (item)
        {
            if (item == sword)
                swordSprite.sprite = item.itemSprite;
            if (item == helmet)
                helmetSprite.sprite = item.itemSprite;
            stats.damage += item.damage;
            stats.weakeningChance += item.weakeningChance;
            stats.weakeningStrength += item.weakeningStrength;
            stats.critChance += item.critChance;
            stats.critStrength += item.critStrength;
            stats.chargeStrength += item.chargeStrength;

            stats.poisonAmount += item.poisonAmount;
            stats.poisonChance += item.poisonChance;
            stats.poisonStrength += item.poisonStrength;
            stats.ignitionAmount += item.ignitionAmount;
            stats.ignitionChance += item.ignitionChance;
            stats.ignitionStrength += item.ignitionStrength;
            stats.missChance += item.missChance;
            stats.blindingChance += item.blindingChance;
            stats.blindingStrength += item.blindingStrength;

            stats.resistance += item.resistance;
            stats.blockStrength += item.blockStrength;

            stats.healStrength += item.healStrength;
        }
    }
    private void Update()
    {
        CheckHealth();
        UpdatePosition();
        UpdateOutline();
    }
    void CheckHealth()
    {
        if (currentHealth == 0 && !isDead)
        {
            isDead = true;
            animator.SetBool("Dead", true);
            animator.SetTrigger("Die");
        }
    }
    void UpdatePosition()
    {
        transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(0, 0), 0.125f * Time.deltaTime * 100);
    }
    void UpdateOutline()
    {
        animator.SetBool("Charging", charging && !isDead);
        animator.SetBool("Blocking", blocking && !isDead);
        animator.SetBool("Weakened", weakened && !isDead);
        animator.SetBool("UnequippedOutline", ((charging && !sword) || (blocking && !helmet)) && !isDead);
    }
    public Vector3 Attack(Unit target)
    {
        float damage = stats.damage + UnityEngine.Random.Range(stats.damage / 10, -stats.damage / 10);
        if (charging)
        {
            damage *= stats.chargeStrength;
        }
        if (weakened)
        {
            damage /= weakenedStrength;
        }
        bool isMiss = GetChance(stats.missChance);
        bool isCrit = GetChance(stats.critChance);
        if (blinded)
        {
            bool blindedMiss = GetChance(blindedStrength);
            isMiss = blindedMiss || isMiss;
            blinded = false;
        }
        if (isMiss)
        {
            var miss = Instantiate(missEffect, target.transform);
            miss.transform.parent = null;
            target.DealDamage(this, 0, false, StatusEffects.Attack, false);
            return new Vector3(0, 0, 1);
        }
        else if (isCrit)
        {
            damage *= stats.critStrength;
            var crit = Instantiate(critEffect, target.transform);
            crit.transform.parent = null;
        }
        bool isDead = target.DealDamage(this, damage, false, StatusEffects.Attack, isCrit);
        if (GetChance(stats.poisonChance))
        {
            target.GiveStatusEffect(stats.poisonAmount, stats.poisonStrength, StatusEffects.Poison);
        }
        if (GetChance(stats.ignitionChance))
        {
            target.GiveStatusEffect(stats.ignitionAmount, stats.ignitionStrength, StatusEffects.Burning);
        }
        if (GetChance(stats.blindingChance))
        {
            target.blinded = true;
            animator.SetBool("Invisible", true);
            target.blindedStrength = stats.blindingStrength;
        }
        if (GetChance(stats.weakeningChance))
        {
            if (target.charging)
            {
                target.charging = false;
            }
            else
            {
                target.weakened = true;
                target.weakenedStrength = stats.weakeningStrength;
            }
        }
        if (isDead)
        {
            animator.SetBool("Invisible", false);
        }
        charging = false;
        weakened = false;
        return new Vector3(Convert.ToInt32(isDead), Convert.ToInt32(isCrit), Convert.ToInt32(isMiss));
    }
    public bool DealDamage(Unit attacker, float damage, bool piercing, StatusEffects effectType, bool isCrit)
    {
        if (attacker)
            animator.SetBool("Invisible", attacker.blinded);
        if (damage <= 0)
            return false;
        float appliedDamage = damage / Mathf.Max(0, stats.resistance);
        if (!piercing && effectType == StatusEffects.Attack)
        {
            if (blocking)
            {
                audioOrganizer.PlayAudio(GameSettings.GetAudioClip("ShieldHit"));
                appliedDamage /= stats.blockStrength;
            }
            blocking = false;
        }
        switch (effectType)
        {
            case StatusEffects.Poison:
                animator.SetTrigger("TakeDamagePoison");
                break;
            case StatusEffects.Burning:
                animator.SetTrigger("TakeDamageBurning");
                break;
            default:
                if (isCrit)
                    animator.SetTrigger("TakeDamageCrit");
                else
                    animator.SetTrigger("TakeDamage");
                break;
        }
        SpeechBubble splash = Instantiate(hitSplash.gameObject, transform).GetComponent<SpeechBubble>();
        splash.message = Mathf.RoundToInt(appliedDamage * GameSettings.damageScale).ToString("######0");
        splash.transform.parent = null;
        splash.enemy = !isPlayer;
        currentHealth = Mathf.Clamp(currentHealth - appliedDamage, 0, maxHealth);

        bool isDead = currentHealth == 0;
        return (isDead);
    }
    public void GiveStatusEffect(float amount, float strength, StatusEffects effectType)
    {
        switch (effectType)
        {
            case StatusEffects.Poison:
                poisoned = Math.Max(Mathf.RoundToInt(amount + 0.5f), poisoned);
                if (poisoned != 0)
                    poisonStrength = Math.Max(strength, poisonStrength);
                else
                    poisonStrength = strength;
                break;
            case StatusEffects.Burning:
                burning = Math.Max(Mathf.RoundToInt(amount + 0.5f), burning);
                if (burning != 0)
                    burningStrength = Math.Max(strength, burningStrength);
                else
                    burningStrength = strength;
                break;
        }
    }
    public bool ApplyEffects(StatusEffects effectType)
    {
        if (poisoned > 0 && effectType == StatusEffects.Poison)
        {
            poisoned--;
            DealDamage(null, poisonStrength, false, StatusEffects.Poison, false);
        }
        if (burning > 0 && effectType == StatusEffects.Burning)
        {
            burning--;
            DealDamage(null, burningStrength, false, StatusEffects.Burning, false);
        }
        return currentHealth == 0;
    }
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        poisoned = 0;
        burning = 0;
        blinded = false;
    }
    public void StartAnimation(string trigger) { animator.SetTrigger(trigger); }
    public void DamageParticle() { damageParticle.Play(); }
    public void BurningParticle() { audioOrganizer.PlayAudio(GameSettings.GetAudioClip("Burning")); burningParticle.Play(); }
    public void HealParticle() { healParticle.Play(); }
    public void SparkleParticle() { sparkleParticle.Play(); }
    public void Block() { blocking = true; }
    public void Charge()
    {
        if (weakened)
            weakened = false;
        else
            charging = true;
    }
    public void DeathParticle() { deathParticle.Play(); }
    bool GetChance(float oddsPercent)
    {
        if (oddsPercent <= 0)
            return false;
        float randomFloat = UnityEngine.Random.Range(0.00f, 100.01f);
        return randomFloat < oddsPercent;
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
