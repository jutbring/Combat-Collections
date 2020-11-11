using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/New Item", order = 2)]
public class Item : ScriptableObject
{
    public enum itemTypes { Stats, Sword, Helmet, Amulet }
    public itemTypes itemType = itemTypes.Stats;
    public Sprite itemSprite = null;
    public int dangerFactor = 0;
    [Header("Offence")]
    public float damage = 0f;
    public float weakeningChance = 0f;
    public float weakeningStrength = 0f;
    public float critChance = 0f;
    public float critStrength = 0f;
    public float chargeStrength = 0f;
    public float poisonChance = 0f;
    public float poisonStrength = 0f;
    public float poisonAmount = 0f;
    public float ignitionChance = 0f;
    public float ignitionStrength = 0f;
    public float ignitionAmount = 0f;
    [Header("Defense")]
    public float resistance = 0f;
    public float blockStrength = 0f;
    [Header("Health")]
    public float maxHealth = 0f;
    public float healStrength = 0f;
    [Header("Upgrade")]
    public Item upgradeItem = null;

    public int GetDangerFactor()
    {
        dangerFactor = Mathf.RoundToInt(((damage * maxHealth * resistance) / 50) + 0.5f);
        return dangerFactor;
    }
}
