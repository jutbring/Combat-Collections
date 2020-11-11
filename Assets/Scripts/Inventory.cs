using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "ScriptableObjects/New Inventory", order = 3)]
public class Inventory : ScriptableObject
{
    public bool reset = true;
    [Header("Inventory")]
    public List<Item> items = new List<Item>();
    public int maxItems = 16;
    public List<bool> levelsCleared = new List<bool>();
    public int lastItemCount = 0;
    public Item.itemTypes lastItemType = Item.itemTypes.Helmet;
    Item equippedSword = null;
    Item equippedHelmet = null;
    public bool AddToList(Item item)
    {
        bool added = false;
        if (items.Count < maxItems)
        {
            items.Add(item);
            added = true;
        }
        lastItemType = item.itemType;
        return added;
    }
    public bool ReplaceItem(Item item, int index)
    {
        List<Item> itemsCopy = new List<Item>();
        for (int i = 0; i < items.Count; i++)
        {
            itemsCopy.Add(items[i]);
        }
        items.Clear();
        for (int i = 0; i < itemsCopy.Count; i++)
        {
            if (i != index)
            {
                items.Add(itemsCopy[i]);
            }
            else
            {
                items.Add(item);
            }
        }
        lastItemType = item.itemType;
        return true;
    }
    public void RemoveFromList(int index)
    {
        items.RemoveAt(index);
    }
    public void EquipItem(Item item)
    {
        switch (item.itemType)
        {
            case Item.itemTypes.Helmet:
                equippedHelmet = item;
                break;
            case Item.itemTypes.Sword:
                equippedSword = item;
                break;
            default:
                break;
        }
    }
    public Item GetEquippedItem(Item.itemTypes itemType)
    {
        bool equippedExists = false;
        switch (itemType)
        {
            case Item.itemTypes.Sword:
                if (!equippedSword)
                    return null;
                for (int i = 0; i < items.Count; i++)
                {
                    equippedExists = items[i].itemSprite == equippedSword.itemSprite || equippedExists;
                }
                if (equippedExists)
                {
                    return equippedSword;
                }
                else
                {
                    equippedSword = null;
                    return null;
                }

            case Item.itemTypes.Helmet:
                if (!equippedHelmet)
                    return null;
                for (int i = 0; i < items.Count; i++)
                {
                    equippedExists = items[i].itemSprite == equippedHelmet.itemSprite || equippedExists;
                }
                if (equippedExists)
                {
                    return equippedHelmet;
                }
                else
                {
                    equippedHelmet = null;
                    return null;
                }
            default: return null;
        }
    }
    public void Reset()
    {
        items.Clear();
        levelsCleared.Clear();
        equippedHelmet = null;
        equippedSword = null;
        lastItemCount = 0;
        lastItemType = Item.itemTypes.Helmet;
        reset = false;
    }
}
