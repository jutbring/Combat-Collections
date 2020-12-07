using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSystem1 : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TMP_Text dialogueText = null;

    [Header("Map Setup")]
    [SerializeField] float scrollSensitivity = 1f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] float scrollDampen = 1f;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float moveLerpSpeed = 1f;
    [SerializeField] float minLevelSize = 0.5f;
    [SerializeField] float positionIncrease = 1f;
    [SerializeField] Transform map = null;
    [SerializeField] Vector2 extremeSizes = new Vector2(1, 1);
    [SerializeField] float extremeMagnitude = 1f;

    [Header("Structure")]
    [SerializeField] bool manualController = false;
    [SerializeField] Menu inventoryMenu = null;
    [SerializeField] Menu resetMenu = null;
    public Inventory playerInventory = null;
    public Fade fade = null;
    [SerializeField] SpeechBubble speechBubblePrefab = null;
    [SerializeField] Transform speechBubblePosition = null;
    [SerializeField] CameraController cameraController = null;
    [SerializeField] Animator effectsAnimator = null;

    [Header("Visual")]
    [SerializeField] float levelParallax = 0f;
    [SerializeField] Transform mapTop = null;
    [SerializeField] float mapParallax = 0f;
    [SerializeField] Vector2 startMapSize = Vector2.one;
    [SerializeField] Vector3 levelSelectOffset = Vector3.zero;

    GameObject speechBubble = null;
    Menu inventory = null;
    Menu resetPanel = null;
    List<LevelSetup> levels = new List<LevelSetup>();
    List<ButtonBehaviour> levelButtons = new List<ButtonBehaviour>();
    Vector3 desiredSize = new Vector3(1, 1, 1);
    Vector3 desiredPosition = new Vector3(0, 0, 0);
    int selectedLevel = 0;
    void Start()
    {
        GameSettings.musicTime = 0;
        if (playerInventory.reset)
        {
            playerInventory.Reset();
        }
        if (fade.gameObject.scene.name == null)
        {
            Fade newFade = FindObjectOfType<Fade>();
            if (!newFade)
            {
                newFade = Instantiate(fade.gameObject, transform).GetComponent<Fade>();
                newFade.transform.SetParent(null);
            }
            fade = newFade;
            // fade.FadeOut();
        }
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
        desiredSize = new Vector3(startMapSize.x, startMapSize.x, 1);
        map.localScale = Vector3.Lerp(map.localScale, desiredSize, scrollSpeed * 100f * Time.unscaledDeltaTime);
        if (GameSettings.isFading)
        {
            fade.FadeOut();
        }
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
                InstantiateInventory();
            }
        }
    }
    public void InstantiateInventory()
    {
        if (inventory != null)
        {
            inventory.Close();
            return;
        }
        inventory = Instantiate(inventoryMenu.gameObject, transform).GetComponent<Menu>();
        inventory.transform.parent = null;
        if (speechBubble)
            speechBubble.SetActive(false);
    }
    public void InstantiateReset()
    {
        if (resetPanel != null)
        {
            resetPanel.Close();
            return;
        }
        resetPanel = Instantiate(resetMenu.gameObject, transform).GetComponent<Menu>();
        resetPanel.transform.parent = null;
        if (speechBubble)
            speechBubble.SetActive(false);
    }

    void UpdateMap()
    {
        if (resetPanel)
            return;
        if (GameSettings.isFading)
            return;
        bool controllerConnected = (Input.GetJoystickNames().Length > 1);
        if (controllerConnected || manualController)
        {
            for (int i = 0; i < GameSettings.backKeys.Count; i++)
            {
                if (Input.GetKeyDown(GameSettings.backKeys[i]))
                {
                    ChangeSelectedLevel(-1);
                }
            }
            levelButtons[selectedLevel].SelectButton();
            desiredSize = new Vector3(startMapSize.y, startMapSize.y, 1);
            desiredPosition = (-levels[selectedLevel].transform.localPosition + levelSelectOffset) * map.localScale.x;
            for (int i = 0; i < GameSettings.forwardKeys.Count; i++)
            {
                if (Input.GetKeyDown(GameSettings.forwardKeys[i]))
                {
                    ChangeSelectedLevel(1);
                }
            }
        }
        else
        {
            desiredSize = new Vector3(
                Mathf.Clamp(desiredSize.x + (Input.mouseScrollDelta.y * scrollSensitivity * (desiredSize.x / scrollDampen)), extremeSizes.x, extremeSizes.y),
                Mathf.Clamp(desiredSize.y + (Input.mouseScrollDelta.y * scrollSensitivity * (desiredSize.y / scrollDampen)), extremeSizes.x, extremeSizes.y),
                1);
            desiredPosition = Vector3.ClampMagnitude(
                map.position - (new Vector3(
                   Input.GetAxis("Horizontal"),
                   Input.GetAxis("Vertical"),
                   0) * moveSpeed * map.localScale.x),
                extremeMagnitude + ((-1 + map.localScale.x) * positionIncrease));
        }
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].transform.parent)
            {
                levels[i].transform.localScale = new Vector3(Mathf.Clamp(1 / map.localScale.x, minLevelSize, 2), Mathf.Clamp(1 / map.localScale.y, minLevelSize, 2), 1);
                levels[i].transform.GetChild(0).transform.localPosition = map.localPosition * levelParallax;
            }
        }
        map.localScale = Vector3.Lerp(map.localScale, desiredSize, scrollSpeed * 100f * Time.unscaledDeltaTime);
        map.localPosition = Vector3.Lerp(map.localPosition, desiredPosition, moveLerpSpeed * 100f * Time.unscaledDeltaTime);
        mapTop.transform.localPosition = map.localPosition * mapParallax;
    }
    void ChangeSelectedLevel(int amount)
    {
        amount = Mathf.Clamp(amount, -1, 1);
        if (selectedLevel == levels.Count - 1 && amount == 1)
        {
            selectedLevel = 0;
            return;
        }
        else if (selectedLevel == 0 && amount == -1)
        {
            selectedLevel = levels.Count - 1;
            return;
        }
        selectedLevel += amount;
    }
    void UpdateInventory()
    {
        if (!inventory && !speechBubble)
        {
            if (playerInventory.lastItemCount < playerInventory.items.Count)
            {
                playerInventory.lastItemCount = playerInventory.items.Count;
                var message = Instantiate(speechBubblePrefab.gameObject, speechBubblePosition).GetComponent<SpeechBubble>();
                message.message = @"New item!";
                message.transform.parent = null;
                speechBubble = message.gameObject;
            }
            playerInventory.lastItemCount = playerInventory.items.Count;
        }
        if (inventory)
        {
            dialogueText.text = @"Press ""I"" to toggle your Inventory";
        }
        else
        {
            dialogueText.text = @"Select a level";
        }
    }
    public void PlayImpactEffect(int effectStrength, float shakeStrength)
    {
        effectsAnimator.SetInteger("Strength", effectStrength);
        effectsAnimator.SetTrigger("Impact");
        cameraController.shakeCameraImpact(shakeStrength);
    }
    public void ResetGame()
    {
        playerInventory.reset = true;
        StartCoroutine(ResetScene());
    }
    IEnumerator ResetScene()
    {
        fade.FadeIn();
        fade.FadeOut();
        yield return new WaitForSeconds(GameSettings.fadeInTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        InstantiateInventory();
    }
    public void QuitGame()
    {
        if (GameSettings.isFading) return;
        StartCoroutine(Quit());
    }
    IEnumerator Quit()
    {
        fade.FadeIn();
        yield return new WaitForSeconds(GameSettings.fadeInTime + 0.5f);
        Application.Quit();
    }
}
