using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/New Item", order = 2)]
public class Item : ScriptableObject
{
    public enum itemTypes { Player, Enemy, Sword, Helmet, Crystal }
    public itemTypes itemType = itemTypes.Enemy;
    public Sprite itemSprite = null;
    public int dangerFactor = 0;
    [Header("Offence")]
    public float damage = 0f;
    public float weakeningChance = 0f;
    public float weakeningStrength = 0f;
    public float critChance = 0f;
    public float critStrength = 0f;
    public float chargeStrength = 0f;
    [Header("Effects")]
    public float poisonChance = 0f;
    public float poisonStrength = 0f;
    public float poisonAmount = 0f;
    public float ignitionChance = 0f;
    public float ignitionStrength = 0f;
    public float ignitionAmount = 0f;
    public float missChance = 0f;
    public float blindingChance = 0f;
    public float blindingStrength = 0f;
    [Header("Defense")]
    public float resistance = 0f;
    public float blockStrength = 0f;
    [Header("Health")]
    public float healStrength = 0f;
    [Header("Upgrade")]
    public Item upgradeItem = null;

    public int GetDangerFactor()
    {
        // dangerFactor = Mathf.RoundToInt(((damage + healStrength) / 2 * resistance) + (poisonChance / 100 * poisonStrength * poisonAmount) + (ignitionChance / 100 * ignitionStrength * ignitionAmount) + (blindingChance / 100 * blindingStrength / 100) + 0.5f);
        // dangerFactor = Mathf.RoundToInt(((damage * resistance) + (poisonChance / 100 * poisonStrength * poisonAmount) + (ignitionChance / 100 * ignitionStrength * ignitionAmount) + (blindingChance / 100 * blindingStrength / 100)) / 2 + 0.5f);
        dangerFactor = Mathf.RoundToInt((damage + (healStrength - (5 * Convert.ToInt32(itemType == itemTypes.Player)))) * resistance / 3.7f * (1 + (0.1f * Convert.ToInt32(itemType == itemTypes.Enemy))) - 4.5f);
        dangerFactor = Mathf.Clamp(dangerFactor, 1, 1000);
        return dangerFactor;
    }
}
