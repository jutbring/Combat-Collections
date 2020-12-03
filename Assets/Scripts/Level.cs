using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/New Level", order = 1)]
public class Level : ScriptableObject
{
    public bool tutorial = false;
    public List<Item> potentialLoot = new List<Item>();
    public List<GameObject> enemies = new List<GameObject>();
    public Sprite background = null;
    public AudioClip battleMusicCalm = null;
    public AudioClip battleMusicIntense = null;

    public int GetLevelIndex()
    {
        return int.Parse(name);
    }
    public string GetLevelName()
    {
        return name;
    }
}
