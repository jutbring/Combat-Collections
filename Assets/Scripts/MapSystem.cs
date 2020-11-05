using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System;

public class MapSystem : MonoBehaviour
{
    [Header("UI Elements")]

    [Header("Map Setup")]
    [SerializeField] float minLevelSize = 0.5f;
    [SerializeField] float positionIncrease = 1f;
    [SerializeField] float scrollDampen = 1f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] Transform map = null;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float scrollSensitivity = 1f;
    [SerializeField] float startSize = 1f;
    [SerializeField] Vector2 extremeSizes = new Vector2(1, 1);
    [SerializeField] float extremeMagnitude = 1f;

    [Header("Structure")]
    [SerializeField] Menu inventoryMenu = null;
    public Inventory playerInventory = null;
    public Fade fade = null;
    [SerializeField] SpeechBubble speechBubblePrefab = null;
    [SerializeField] Transform speechBubblePosition;

    [Header("Visual")]
    [SerializeField] float levelParallax = 0f;

    GameObject speechBubble = null;
    Menu inventory = null;
    List<LevelSetup> levels = new List<LevelSetup>();
    List<ButtonBehaviour> levelButtons = new List<ButtonBehaviour>();
    Vector3 desiredSize = new Vector3(1, 1, 1);
    Vector3 desiredPosition = new Vector3(0, 0, 0);
    void Start()
    {
        Time.timeScale = GameSettings.defaultTimeScale;
        levels.Clear();
        levelButtons.Clear();
        foreach (LevelSetup level in map.GetComponentsInChildren<LevelSetup>())
        {
            levels.Add(level);
            levelButtons.Add(level.GetComponentInChildren<ButtonBehaviour>());
        }
        for (int i = 0; i < levels.Count; i++)
        {
            if (i >= playerInventory.levelsCleared.Count)
                playerInventory.levelsCleared.Add(false);
            playerInventory.levelsCleared[0] = true;
            levelButtons[i].AddUse(Convert.ToInt32(playerInventory.levelsCleared[i]));
        }
        map.localScale = new Vector3(startSize, startSize, startSize);
    }

    void Update()
    {
        GetInputs();
        UpdateMap();
        UpdateInventory();
    }
    void GetInputs()
    {
        for (int i = 0; i < GameSettings.inventoryKeys.Count; i++)
        {
            if (Input.GetKeyDown(GameSettings.inventoryKeys[i]) && inventory == null)
            {
                inventory = Instantiate(inventoryMenu.gameObject, transform).GetComponent<Menu>();
                inventory.transform.parent = null;
                if (speechBubble)
                    speechBubble.SetActive(false);
            }
        }
    }
    void UpdateMap()
    {
        //if (inventory)
        //    return;
        if (GameSettings.isFading)
            return;
        desiredSize = new Vector3(
            Mathf.Clamp(desiredSize.x + (Input.mouseScrollDelta.y * scrollSensitivity * Time.unscaledDeltaTime * (desiredSize.x / scrollDampen)), extremeSizes.x, extremeSizes.y),
            Mathf.Clamp(desiredSize.y + (Input.mouseScrollDelta.y * scrollSensitivity * Time.unscaledDeltaTime * (desiredSize.y / scrollDampen)), extremeSizes.x, extremeSizes.y),
            1);
        map.localScale = Vector3.Lerp(map.localScale, desiredSize, scrollSpeed * 100f * Time.unscaledDeltaTime * Mathf.Clamp(Vector3.Distance(map.localScale, desiredSize), 1, 100));
        desiredPosition = Vector3.ClampMagnitude(
            map.position - (new Vector3(
               Input.GetAxis("Horizontal"),
               Input.GetAxis("Vertical"),
               0) * moveSpeed * Time.unscaledDeltaTime * map.localScale.x),
            extremeMagnitude + ((-1 + map.localScale.x) * positionIncrease));
        map.localPosition = desiredPosition;
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].transform.parent)
            {
                levels[i].transform.localScale = new Vector3(Mathf.Clamp(1 / map.localScale.x, minLevelSize, 2), Mathf.Clamp(1 / map.localScale.y, minLevelSize, 2), 1);
                levels[i].transform.GetChild(0).transform.localPosition = map.localPosition * levelParallax;
            }
        }
    }
    void UpdateInventory()
    {
        if (!inventory && !speechBubble && playerInventory.lastItemCount != playerInventory.items.Count)
        {
            playerInventory.lastItemCount = playerInventory.items.Count;
            var message = Instantiate(speechBubblePrefab.gameObject, speechBubblePosition).GetComponent<SpeechBubble>();
            message.message = @"New item!";
            message.transform.parent = null;
            speechBubble = message.gameObject;
        }
    }
}
