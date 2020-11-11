using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Header("Inputs")]
    public static List<KeyCode> confirmKeys = new List<KeyCode>() { KeyCode.Mouse0, KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter };
    public static List<KeyCode> pauseKeys = new List<KeyCode>() { KeyCode.P, KeyCode.Escape };
    public static List<KeyCode> inventoryKeys = new List<KeyCode>() { KeyCode.Escape, KeyCode.I };
    public static List<KeyCode> forwardKeys = new List<KeyCode>() { KeyCode.D, KeyCode.W, KeyCode.RightArrow, KeyCode.UpArrow };
    public static List<KeyCode> backKeys = new List<KeyCode>() { KeyCode.A, KeyCode.S, KeyCode.LeftArrow, KeyCode.DownArrow };

    [Header("Paths")]
    public static string audioPath = "Audio/Effects/";

    [Header("Volumes")]
    public static float musicVolume = 0.5f;
    public static float effectsVolume = 0.5f;

    [Header("Other")]
    public static float defaultTimeScale = 1f;
    public static float fadeInTime = 0.42f;
    public static bool isFading = false;
    public static int pixelsPerUnit = 16;
    public static float itemMouseReach = 1f;
    public enum Scenes { Map, Battle };

    public static Sound GetAudioClip(string clipName)
    {
        var clip = Resources.Load<Sound>(audioPath + clipName);
        return clip;
    }
    public static Vector3 GetSpriteOffset(Sprite sprite)
    {
        float itemSize = sprite.pivot.x + sprite.pivot.y;
        Vector2 offset = new Vector2(0, -(sprite.pivot.x - sprite.pivot.y) / itemSize);
        return offset;
    }
}
