using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Organizer.Audio;

public class Unit : MonoBehaviour
{
    public string unitName = "Name";
    public bool player = false;
    public Inventory inventory = null;

    [Header("Stats")]
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
    [SerializeField] GameObject critEffect = null;
    [SerializeField] SpriteRenderer helmetSprite = null;
    [SerializeField] SpriteRenderer swordSprite = null;

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

    public enum StatusEffects { Attack, Poison, Burning }

    public AudioOrganizer audioOrganizer = null;

    void Awake()
    {
        stats = Item.CreateInstance<Item>();
        animator = GetComponent<Animator>();
        animator.SetBool("Player", player);
        SetStats(unitStats);
        SetStats(helmet);
        SetStats(sword);
        currentHealth = stats.maxHealth;
    }
    void SetStats(Item item)
    {
        if (item)
        {
            if (item == sword)
                swordSprite.sprite = item.itemSprite;
            if (item == helmet)
                helmetSprite.sprite = item.itemSprite;
            stats.damage *= item.damage;
            stats.weakeningChance *= item.weakeningChance;
            stats.weakeningStrength *= item.weakeningStrength;
            stats.critChance *= item.critChance;
            stats.critStrength *= item.critStrength;
            stats.chargeStrength *= item.chargeStrength;
            stats.poisonAmount *= item.poisonAmount;
            stats.poisonChance *= item.poisonChance;
            stats.poisonStrength *= item.poisonStrength;
            stats.ignitionAmount *= item.ignitionAmount;
            stats.ignitionChance *= item.ignitionChance;
            stats.ignitionStrength *= item.ignitionStrength;

            stats.resistance *= item.resistance;
            stats.blockStrength *= item.blockStrength;

            stats.maxHealth *= item.maxHealth;
            stats.healStrength *= item.healStrength;
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
        animator.SetBool("Charging", charging);
        animator.SetBool("Blocking", blocking);
        animator.SetBool("Weakened", weakened);
    }
    public Vector2 Attack(Unit target)
    {
        bool isCrit = GetChance(stats.critChance);
        float damage = stats.damage;
        if (charging)
        {
            damage *= stats.chargeStrength;
        }
        if (isCrit)
        {
            damage *= stats.critStrength;
        }
        if (weakened)
        {
            damage /= weakenedStrength;
        }
        bool isDead = target.DealDamage(damage, false, StatusEffects.Attack);
        if (isCrit)
        {
            var crit = Instantiate(critEffect, target.transform);
            crit.transform.parent = null;
        }
        if (GetChance(stats.poisonChance))
        {
            target.GiveStatusEffect(stats.poisonAmount, stats.poisonStrength, StatusEffects.Poison);
        }
        if (GetChance(stats.ignitionChance))
        {
            target.GiveStatusEffect(stats.ignitionAmount, stats.ignitionStrength, StatusEffects.Burning);
        }
        if (GetChance(stats.weakeningChance))
        {
            target.weakened = true;
            target.weakenedStrength = stats.weakeningStrength;
        }
        charging = false;
        weakened = false;
        return new Vector2(Convert.ToInt32(isDead), Convert.ToInt32(isCrit));
    }
    public bool DealDamage(float damage, bool piercing, StatusEffects effectType)
    {
        float appliedDamage = damage / Mathf.Max(stats.resistance, 1);
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
                animator.SetTrigger("TakeDamage");
                break;
        }
        currentHealth = Mathf.Clamp(currentHealth - (appliedDamage / stats.resistance), 0, stats.maxHealth);

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
            DealDamage(poisonStrength, false, StatusEffects.Poison);
        }
        if (burning > 0 && effectType == StatusEffects.Burning)
        {
            burning--;
            DealDamage(burningStrength, false, StatusEffects.Burning);
        }
        return currentHealth == 0;
    }
    public void StartAnimation(string trigger)
    {
        animator.SetTrigger(trigger);
    }
    public void DamageParticle()
    {
        damageParticle.Play();
    }
    public void BurningParticle()
    {
        audioOrganizer.PlayAudio(GameSettings.GetAudioClip("Burning"));
        burningParticle.Play();
    }
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, stats.maxHealth);
    }
    public void HealParticle()
    {
        healParticle.Play();
    }
    public void Block()
    {
        blocking = true;
    }
    public void Charge()
    {
        charging = true;
    }
    public void DeathParticle()
    {
        deathParticle.Play();
    }
    bool GetChance(float oddsPercent)
    {
        if (oddsPercent <= 1)
            return false;
        float randomFloat = UnityEngine.Random.Range(0.00f, 100.01f);
        return randomFloat < oddsPercent;
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
