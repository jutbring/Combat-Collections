using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/New Item", order = 2)]
public class Item : ScriptableObject
{
    public enum itemTypes { Stats, Sword, Helmet }
    public itemTypes itemType = itemTypes.Stats;
    public Sprite itemSprite = null;
    public int dangerFactor = 1;
    [Header("Offence")]
    public float damage = 1f;
    public float weakeningChance = 1f;
    public float weakeningStrength = 1f;
    public float critChance = 1f;
    public float critStrength = 1f;
    public float chargeStrength = 1f;
    public float poisonChance = 1f;
    public float poisonStrength = 1f;
    public float poisonAmount = 1f;
    public float ignitionChance = 1f;
    public float ignitionStrength = 1f;
    public float ignitionAmount = 1f;
    [Header("Defence")]
    public float resistance = 1f;
    public float blockStrength = 1f;
    [Header("Health")]
    public float maxHealth = 1f;
    public float healStrength = 1f;

    public int GetDangerFactor()
    {
        dangerFactor = Mathf.RoundToInt(((damage * maxHealth * resistance) / 50) + 0.5f);
        return dangerFactor;
    }
}
