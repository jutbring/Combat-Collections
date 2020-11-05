using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "ScriptableObjects/New Inventory", order = 3)]
public class Inventory : ScriptableObject
{
    public List<Item> items = new List<Item>();
    public int maxItems = 16;
    public List<bool> levelsCleared = new List<bool>();
    public int lastItemCount = 0;
    public bool AddToList(Item item)
    {
        bool added = false;
        if (items.Count < maxItems)
        {
            items.Add(item);
            added = true;
        }
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
        return true;
    }
    public void RemoveFromList(int index)
    {
        items.RemoveAt(index);
    }
}
