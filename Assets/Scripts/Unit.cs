using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Organizer.Audio;
public class Unit : MonoBehaviour
{
    public string unitName = "Name";
    public enum EntityType { Player, Enemy, FlyingPlayer, Boss }
    public EntityType type = EntityType.Enemy;
    public bool isBoss = false;
    public Inventory inventory = null;

    [Header("Stats")]
    public float maxHealth = 100;
    public Item stats = null;
    public Item unitStats = null;
    public Item sword = null;
    public Item helmet = null;
    [SerializeField] float movementSpeed = 1f;
    [SerializeField] float rotationAmount = 1f;
    [SerializeField] float movementDrag = 1f;
    [SerializeField] float attackCooldown = 1f;

    [Header("Other")]
    public float currentHealth = 1f;
    public Animator animator = null;
    [SerializeField] Vector2 levelBoundaries = new Vector2(0, 0);
    [SerializeField] GameObject deathScreen = null;

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
    [SerializeField] float swordDistance = 1f;
    public Vector3 swordOffset = new Vector2(0, 0);
    [SerializeField, Range(0, 1)] float swordSpeed;
    [SerializeField] Transform body = null;

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
    bool isPlayer = false;
    bool isFinal = false;
    Vector3 desiredPosition = new Vector3(0, 0, 0);
    float attackTimer = 0f;
    Transform swordTransform = null;
    bool willHit = false;
    float invincibility = 0f;
    float invincibilityTimer = 0f;

    public enum StatusEffects { Attack, Poison, Burning, Blindness }

    public AudioOrganizer audioOrganizer = null;

    void Awake()
    {

        isPlayer = type == EntityType.Player || type == EntityType.FlyingPlayer;
        isFinal = type == EntityType.FlyingPlayer || type == EntityType.Boss;
        stats = Item.CreateInstance<Item>();
        if (isPlayer)
        {
            if (isFinal)
                invincibility = 0.5f;
            stats.itemType = Item.itemTypes.Player;
        }
        animator = GetComponent<Animator>();
        animator.SetBool("Player", isPlayer);
        animator.SetBool("Final", isFinal);
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
            if (item == helmet && type != EntityType.FlyingPlayer)
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
        UpdateFlyingPlayer();
        UpdateFlyingBoss();
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
        if (isFinal) return;
        transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(0, 0), 0.125f * Time.deltaTime * 100);
    }
    void UpdateOutline()
    {
        animator.SetBool("Charging", charging && !isDead);
        animator.SetBool("Blocking", blocking && !isDead);
        animator.SetBool("Weakened", weakened && !isDead);
        animator.SetBool("UnequippedOutline", ((charging && !sword) || (blocking && !helmet)) && !isDead);
    }
    void UpdateFlyingPlayer()
    {
        if (Time.timeScale == 0) return;
        invincibilityTimer = Mathf.Max(invincibilityTimer - Time.deltaTime, 0);
        attackTimer = Math.Max(attackTimer - Time.deltaTime, 0);

        // Sword Movement
        if (type != EntityType.FlyingPlayer) return;
        swordTransform = swordSprite.transform.parent;
        Vector3 lookPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - body.position - swordOffset;
        lookPosition.z = 0;
        lookPosition.Normalize();
        swordTransform.position = body.position + swordOffset + swordTransform.up * swordDistance;
        if (attackTimer == 0 && Time.timeScale > 0)
            swordTransform.up = Vector3.Lerp(swordTransform.up, lookPosition, swordSpeed);
        //swordSprite.transform.localScale = new Vector2(1 - (2 * Convert.ToInt32(swordTransform.localPosition.x < 0)), 1);
        if (swordSprite.transform.localPosition.y > 0)
            swordSprite.sortingOrder = 20;
        else
            swordSprite.sortingOrder = 30;

        // Player Movement
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;
        //float totalMovement = Math.Abs(movement.x) + Mathf.Abs(movement.y);
        desiredPosition = Vector3.Lerp(desiredPosition, movement * movementSpeed * Time.deltaTime, movementDrag);
        transform.position += desiredPosition;
        float rotation = movement.x * -rotationAmount;
        if (movement.x != 0)
            rotation += movement.y * (rotationAmount) / 2 * movement.x / Mathf.Abs(movement.x);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, rotation), movementDrag * Time.timeScale);
        if (attackTimer == 0)
            swordTransform.rotation = Quaternion.Lerp(swordTransform.rotation, Quaternion.Euler(0, 0, -rotation), movementDrag);
        Vector2 startSize = new Vector2(Math.Abs(transform.localScale.x), transform.localScale.y);
        transform.position = new Vector2(Mathf.Clamp(transform.position.x, -levelBoundaries.x, levelBoundaries.x), Mathf.Clamp(transform.position.y, -levelBoundaries.y - 0.65f, levelBoundaries.y - 0.65f));

        // Attack
        if (attackTimer == 0 && Time.timeScale > 0)
        {
            bool looksRight = lookPosition.x > 0;
            bool looksLeft = lookPosition.x < 0;
            if (looksLeft)
            {
                transform.localScale = new Vector3(-startSize.x, startSize.y, 1);
            }
            else if (looksRight)
            {
                transform.localScale = new Vector3(startSize.x, startSize.y, 1);
            }
        }
        for (int i = 0; i < GameSettings.confirmKeys.Count; i++)
        {
            if (Input.GetKeyDown(GameSettings.confirmKeys[i]) && Time.timeScale > 0)
            {
                willHit = true;
                if (attackTimer == 0)
                {
                    swordTransform.up = lookPosition;
                    Attack(null);
                }
            }
        }
        if (willHit && attackTimer == 0)
        {
            swordTransform.up = lookPosition;
            Attack(null);
        }
    }
    void UpdateFlyingBoss()
    {
        if (type != EntityType.Boss) return;

        //Visual
        animator.SetFloat("PumpingSpeed", 1 + (0.5f * Convert.ToInt32(currentHealth < maxHealth / 2)));
    }
    public Vector3 Attack(Unit target)
    {
        willHit = false;
        bool isMiss = GetChance(stats.missChance);
        bool isCrit = GetChance(stats.critChance);
        float damage = stats.damage + UnityEngine.Random.Range(stats.damage / 10, -stats.damage / 10);
        if (!isFinal)
        {
            if (charging)
            {
                damage *= stats.chargeStrength;
            }
            if (weakened)
            {
                damage /= weakenedStrength;
            }
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
        }
        else if (isFinal)
        {
            attackTimer = attackCooldown;
            if (isPlayer)
            {
                animator.SetTrigger("Attack");
                Collider2D[] enemy = Physics2D.OverlapCircleAll(swordTransform.up * 1.85f + body.position + swordOffset, 1f);
                for (int i = 0; i < enemy.Length; i++)
                {
                    if (i == 0)
                        FindObjectOfType<BattleSystem>().PlayImpactEffect(-1, 0.8f);
                    Unit enemyUnit = enemy[i].transform.parent.GetComponentInChildren<Unit>();
                    if (enemyUnit)
                        enemyUnit.DealDamage(this, damage, false, StatusEffects.Attack, false);
                }
            }
            else
            {
                target.DealDamage(this, damage, false, StatusEffects.Attack, false);
            }
        }
        return new Vector3(Convert.ToInt32(isDead), Convert.ToInt32(isCrit), Convert.ToInt32(isMiss));
    }
    public void OnDrawGizmos()
    {
        if (body && isPlayer)
            Gizmos.DrawSphere(swordTransform.up * 1.85f + body.position + swordOffset, 1f);
    }
    public bool DealDamage(Unit attacker, float damage, bool piercing, StatusEffects effectType, bool isCrit)
    {
        if (invincibilityTimer != 0) return false;
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
        SpeechBubble splash = null;
        if (body)
            splash = Instantiate(hitSplash.gameObject, body.parent).GetComponent<SpeechBubble>();
        else
            splash = Instantiate(hitSplash.gameObject, transform).GetComponent<SpeechBubble>();
        splash.message = Mathf.RoundToInt(appliedDamage * GameSettings.damageScale * stats.resistance).ToString("######0");
        splash.messageScale = stats.resistance;
        splash.transform.parent = null;
        Vector3 splashSize = new Vector3( Mathf.Abs( splash.transform.localScale.x), Mathf.Abs( splash.transform.localScale.y), 0);
        splash.transform.localScale = splashSize;
        if (isFinal)
            splash.transform.localScale = transform.localScale;
        splash.enemy = !isPlayer;
        currentHealth = Mathf.Clamp(currentHealth - appliedDamage, 0, maxHealth);
        invincibilityTimer = invincibility;

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
        if (isBoss && isFinal)
        {
            transform.parent = null;
            deathScreen.SetActive(true);
            deathScreen.transform.SetParent(null);
        }
        Destroy(gameObject);
    }
}
