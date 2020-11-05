using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "ScriptableObjects/New Sound", order = 1)]
public class Sound : ScriptableObject
{
    public AudioClip clip = null;
    [Range(0, 1)] public float volume = 1f;
    public Vector2 extremePitches = new Vector2(1, 1);
}
