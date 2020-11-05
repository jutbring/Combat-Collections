using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Header("Inputs")]
    public static List<KeyCode> confirmKeys = new List<KeyCode>() { KeyCode.Mouse0, KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter };
    public static List<KeyCode> pauseKeys = new List<KeyCode>() { KeyCode.P, KeyCode.Escape };
    public static List<KeyCode> inventoryKeys = new List<KeyCode>() { KeyCode.Escape, KeyCode.I };

    [Header("Paths")]
    public static string audioPath = "Audio/Effects/";

    [Header("Volumes")]
    public static float musicVolume = 0.5f;
    public static float effectsVolume = 0.5f;

    [Header("Other")]
    public static float defaultTimeScale = 1f;
    public static float fadeInTime = 0.5f;
    public static bool isFading = false;
    public static int pixelsPerUnit = 16;
    public static float itemMouseReach = 1f;
    public enum Scenes { Map, Battle };

    public static Sound GetAudioClip(string clipName)
    {
        var clip = Resources.Load<Sound>(audioPath + clipName);
        return clip;
    }
}
