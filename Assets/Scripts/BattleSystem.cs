using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using Organizer.Audio;
public enum BattleStates { Start, PlayerTurn, EnemyTurn, Won, Lost }
public enum Options { None, ActionSelection }
public class BattleSystem : MonoBehaviour
{
    public BattleStates state = BattleStates.Start;
    public Options optionState = Options.None;

    [Header("UI Elements")]
    [SerializeField] TMP_Text dialogueText = null;
    public BattleHUD playerHUD = null;
    public BattleHUD enemyHUD = null;
    [SerializeField] List<ButtonBehaviour> uiButtons = null;
    [SerializeField] Image timeIndicator = null;
    [SerializeField] Sprite timePausedImage = null;
    [SerializeField] Sprite timeSkippingImage = null;
    [SerializeField] Sprite timeSlowedImage = null;

    [Header("Battle Setup")]
    public bool started = false;
    public Level levelStats = null;
    [SerializeField] GameObject playerPrefab = null;
    Unit player = null;
    Unit enemy = null;
    int currentEnemy = 0;
    [SerializeField] Transform PlayerBattleStation = null;
    [SerializeField] Transform EnemyBattleStation = null;
    [SerializeField] List<SpriteRenderer> backgrounds = null;

    [Header("Structure")]
    public Fade fade = null;
    [SerializeField] float SkippingTimeScale = 5f;
    [SerializeField] float introDialogueWaitTime = 3f;
    [SerializeField] float actionTime = 1f;
    [SerializeField] float aftermathTime = 1f;
    [SerializeField] Animator effectsAnimator = null;
    [SerializeField] CameraController cameraController = null;
    [SerializeField] ItemHolder itemHolder = null;
    [SerializeField] int turnsBetweenButtonUse = 5;
    [SerializeField] AudioSource musicSourceIntense = null;
    [SerializeField] AudioSource musicSourceCalm = null;
    [SerializeField] AudioOrganizer audioOrganizer = null;
    [SerializeField] Menu pauseMenu = null;
    [SerializeField] Menu defeatMenu = null;
    [SerializeField] Menu victoryMenu = null;

    [Header("Visual")]
    [SerializeField] Volume pauseEffect = null;
    [SerializeField, Range(-1, 2)] float backgroundMovementFactor = 1f;
    [SerializeField, Range(-1, 2)] float backgroundRotationFactor = 1f;
    [SerializeField] Transform stationParent = null;
    [SerializeField, Range(-1, 2)] float stationsMovementFactor = 1f;
    [SerializeField, Range(-1, 2)] float stationsRotationFactor = 1f;

    List<Button> buttons = new List<Button>();
    List<int> turnsSinceUse = new List<int>();
    bool gameEnded = false;
    float lastTimeScale = 1f;
    bool attacknext = true;
    bool buttonsVisible = true;
    int lastSelected = 0;
    int clicksUntilSkip = 2;
    bool paused = false;
    bool victoryMenuSet = false;
    bool musicPlaying = true;
    bool intenseMusic = false;
    Animator pauseMenuAnimator = null;
    private void Start()
    {
        if (FindObjectOfType<LevelSetup>() == null)
        {
            StartGame(levelStats);
        }
    }
    public void StartGame(Level level)
    {
        started = true;
        Settings();
        currentEnemy = 0;
        levelStats = level;
        musicSourceCalm.clip = levelStats.battleMusicCalm;
        musicSourceIntense.clip = levelStats.battleMusicIntense;
        musicSourceCalm.Play();
        musicSourceIntense.Play();
        for (int i = 0; i < backgrounds.Count; i++)
        {
            if (levelStats)
            {
                backgrounds[i].sprite = levelStats.background;
            }
        }
        StartCoroutine(SetUpBattle());
    }
    public void Settings()
    {
        Time.timeScale = GameSettings.defaultTimeScale;
        buttons.Clear();
        for (int i = 0; i < uiButtons.Count; i++)
        {
            buttons.Add(uiButtons[i].GetComponent<Button>());
        }
        if (!cameraController)
        {
            cameraController = Camera.main.transform.parent.GetComponent<CameraController>();
        }
        foreach (SpriteRenderer spriteRenderer in backgrounds[0].GetComponentsInChildren<SpriteRenderer>())
        {
            if (spriteRenderer != backgrounds[0])
            {
                backgrounds.Add(spriteRenderer);
            }
        }
        if (!effectsAnimator)
        {
            effectsAnimator = GetComponent<Animator>();
        }
    }
    void Update()
    {
        UpdateGameState();
        GetInputs();
        UpdateTimeScaleEffect();
        UpdateParallax();
        UpdateMusic();
    }
    void UpdateGameState()
    {
        gameEnded = state == BattleStates.Won || state == BattleStates.Lost;
        switch (optionState)
        {
            case Options.ActionSelection:
                if (!buttonsVisible)
                {
                    for (int i = 0; i < uiButtons.Count; i++)
                    {
                        uiButtons[i].gameObject.SetActive(true);
                    }
                    if (uiButtons[lastSelected].uses > 0)
                        buttons[lastSelected].Select();
                    else
                        buttons[0].Select();
                    uiButtons[lastSelected].GetComponent<Animator>().SetTrigger("Selected");
                    buttonsVisible = true;
                }
                break;
            default:
                if (buttonsVisible)
                {
                    for (int i = 0; i < uiButtons.Count; i++)
                    {
                        uiButtons[i].gameObject.SetActive(false);
                    }
                    buttonsVisible = false;
                }
                break;
        }
        for (int i = 0; i < uiButtons.Count; i++)
        {
            uiButtons[i].gameObject.SetActive(!paused && buttonsVisible);
        }
        switch (state)
        {
            case BattleStates.Won:
                if (!itemHolder)
                {
                    if (!victoryMenuSet)
                    {
                        victoryMenuSet = true;
                        InstantiateMenu(victoryMenu);
                    }
                }
                break;
        }
    }
    void GetInputs()
    {
        for (int i = 0; i < GameSettings.confirmKeys.Count; i++)
        {
            if (Input.GetKeyDown(GameSettings.confirmKeys[i]) && !gameEnded && !paused)
            {
                if (GameSettings.confirmKeys[i] == KeyCode.Mouse0)
                    clicksUntilSkip = Mathf.Max(clicksUntilSkip - 2, 0);
                else
                    clicksUntilSkip = Mathf.Max(clicksUntilSkip - 1, 0);
                if (clicksUntilSkip == 0)
                {
                    SkipState();
                }
            }
        }
        for (int i = 0; i < GameSettings.pauseKeys.Count; i++)
        {
            if (Input.GetKeyDown(GameSettings.pauseKeys[i]))
            {
                TogglePause();
            }
        }
    }
    public void TogglePause()
    {
        if (GameSettings.isFading || gameEnded) return;
        paused = !paused;
        Time.timeScale = Convert.ToInt32(!paused) * GameSettings.defaultTimeScale;
        pauseEffect.weight = Convert.ToInt32(paused);
        if (paused)
        {
            Animator menu = Instantiate(pauseMenu.gameObject, transform).GetComponent<Animator>();
            menu.GetComponent<Menu>().instantiator = this;
            pauseMenuAnimator = menu;
        }
        else
        {
            pauseMenuAnimator.SetTrigger("Close");
        }
    }
    void UpdateTimeScaleEffect()
    {
        timeIndicator.enabled = Time.timeScale != GameSettings.defaultTimeScale;
        if (Time.timeScale != lastTimeScale)
        {
            lastTimeScale = Time.timeScale;
            effectsAnimator.SetTrigger("TimeSpeedChange");
        }
        if (Time.timeScale > GameSettings.defaultTimeScale)
        {
            timeIndicator.sprite = timeSkippingImage;
        }
        else if (Time.timeScale == 0)
        {
            timeIndicator.sprite = timePausedImage;
        }
        else
        {
            timeIndicator.sprite = timeSlowedImage;
        }
    }
    void SkipState()
    {
        switch (optionState)
        {
            case Options.None:
                Time.timeScale = SkippingTimeScale;
                break;
        }
    }
    void UpdateParallax()
    {
        Vector2 cameraPosition = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
        backgrounds[0].transform.localPosition = cameraPosition * backgroundMovementFactor;
        backgrounds[0].transform.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.rotation.z * backgroundRotationFactor);
        stationParent.localPosition = cameraPosition * stationsMovementFactor;
        stationParent.localRotation = Quaternion.Euler(0, 0, Camera.main.transform.rotation.z * stationsRotationFactor);
    }
    void UpdateMusic()
    {
        if (player && enemy)
        {
            intenseMusic = enemy.stats.GetDangerFactor() >= player.stats.GetDangerFactor() && state != BattleStates.Won;
        }
        else
        {
            intenseMusic = intenseMusic && state != BattleStates.Won;
        }
        if (musicPlaying)
        {
            musicSourceIntense.volume = Mathf.MoveTowards(musicSourceIntense.volume, (GameSettings.musicVolume * Convert.ToInt32(intenseMusic)), Time.unscaledDeltaTime * GameSettings.musicVolume);
            musicSourceCalm.volume = GameSettings.musicVolume - musicSourceIntense.volume;
            musicSourceIntense.pitch = Mathf.Clamp(Time.timeScale, 0, 1);
            musicSourceCalm.pitch = musicSourceIntense.pitch;
            musicSourceCalm.time = musicSourceIntense.time;
            if (paused && musicSourceIntense.isPlaying)
            {
                musicSourceIntense.Pause();
                musicSourceCalm.Pause();
            }
            else if (!paused && !musicSourceIntense.isPlaying)
            {
                musicSourceIntense.UnPause();
                musicSourceCalm.UnPause();
            }
        }
    }
    IEnumerator SetUpBattle()
    {
        state = BattleStates.Start;
        player = Instantiate(playerPrefab.gameObject, PlayerBattleStation).GetComponent<Unit>();
        playerHUD.SetHUD(player);
        player.audioOrganizer = audioOrganizer;
        InstantiateEnemy();

        yield return new WaitForSeconds(introDialogueWaitTime);
        StartCoroutine(PlayerTurn());
    }
    Vector2 NewTurn(Unit subject)
    {
        for (int i = 0; i < uiButtons.Count; i++)
        {
            if (uiButtons[i].uses == 0 && subject == player)
            {
                if (turnsSinceUse.Count < uiButtons.Count)
                {
                    turnsSinceUse.Clear();
                    for (int j = 0; j < uiButtons.Count; j++)
                    {
                        turnsSinceUse.Add(0);
                    }
                }
                turnsSinceUse[i]++;
                if (turnsSinceUse[i] % turnsBetweenButtonUse == 0)
                {
                    uiButtons[i].AddUse(1);
                }
            }
        }
        bool effect = (subject.poisoned + subject.burning > 0);
        bool poisonDeath = subject.ApplyEffects(Unit.StatusEffects.Poison);
        bool burningDeath = subject.ApplyEffects(Unit.StatusEffects.Burning);
        bool isDead = subject.currentHealth == 0;
        if (isDead)
        {
            PlayImpactEffect(2, 2);
            if (poisonDeath)
            {
                SetDialogueText(subject, "You died from poisoning", subject.unitName + " died from poisoning");
            }
            else if (burningDeath)
            {
                SetDialogueText(subject, "You died from burning", subject.unitName + " died from burning");
            }
            DetermineNextTurn(subject, false);
        }
        if (effect && !isDead)
        {
            PlayImpactEffect(1, 0.5f);
        }
        return new Vector2(Convert.ToInt32(isDead), Convert.ToInt32(effect));
    }
    IEnumerator PlayerTurn()
    {
        Vector2 newTurn = NewTurn(player);
        if (newTurn.x == 1) yield break;
        if (newTurn.y == 1) yield return new WaitForSeconds(aftermathTime);
        state = BattleStates.PlayerTurn;
        Time.timeScale = GameSettings.defaultTimeScale;
        optionState = Options.ActionSelection;
        SetDialogueText(player, "Select an action...", "");
    }
    IEnumerator EnemyTurn()
    {
        Vector2 newTurn = NewTurn(enemy);
        if (newTurn.x == 1) yield break;
        if (newTurn.y == 1) yield return new WaitForSeconds(aftermathTime);
        state = BattleStates.EnemyTurn;
        DetermineEnemyMove();
    }
    void DetermineEnemyMove()
    {
        float index = UnityEngine.Random.Range(0, 101);
        if (attacknext)
        {
            StartCoroutine(Attack(enemy, player));
            attacknext = false;
        }
        else if (index < 15)
        {
            StartCoroutine(Charge(enemy));
            attacknext = true;
        }
        else if (index < 40 && !enemy.blocking)
        {
            StartCoroutine(Block(enemy));
            attacknext = true;
        }
        else if (index < 60 && enemy.currentHealth / enemy.stats.maxHealth < 0.5f)
        {
            StartCoroutine(Heal(enemy));
            attacknext = true;
        }
        else
        {
            StartCoroutine(Attack(enemy, player));
        }
    }
    public void OnAttackButton(int buttonIndex)
    {
        OnButton(buttonIndex);
        if (state != BattleStates.PlayerTurn) return;
        StartCoroutine(Attack(player, enemy));
    }
    IEnumerator Attack(Unit actor, Unit target)
    {
        SetDialogueText(actor, "You attack ", enemy.unitName + " attacks");
        actor.StartAnimation("Attack");

        yield return new WaitForSeconds(actionTime);

        Vector2 isDead = actor.Attack(target);
        SetDialogueText(target, "You were hit!", target.unitName + " was hit!");
        if (isDead.x == 1)
            SetDialogueText(target, "You were slain", target.unitName + " was slain");
        if (isDead.x + isDead.y >= 1)
            PlayImpactEffect(2, 2);
        else
            PlayImpactEffect(1, 1);

        yield return new WaitForSeconds(aftermathTime);
        DetermineNextTurn(actor, true);
    }
    public void OnHealButton(int buttonIndex)
    {
        OnButton(buttonIndex);
        if (state != BattleStates.PlayerTurn) return;
        StartCoroutine(Heal(player));
    }
    IEnumerator Heal(Unit actor)
    {
        SetDialogueText(actor, "You cast a healing spell", actor.unitName + " cast a healing spell");
        actor.StartAnimation("Heal");

        yield return new WaitForSeconds(actionTime);

        actor.Heal(actor.stats.healStrength);
        SetDialogueText(actor, "You healed", actor.unitName + " healed");
        PlayImpactEffect(0, 0.5f);

        yield return new WaitForSeconds(aftermathTime);
        DetermineNextTurn(actor, true);
    }
    public void OnBlockButton(int buttonIndex)
    {
        OnButton(buttonIndex);
        if (state != BattleStates.PlayerTurn) return;
        StartCoroutine(Block(player));
    }
    IEnumerator Block(Unit actor)
    {
        SetDialogueText(actor, "You cast a blocking spell", actor.unitName + " cast a blocking spell");
        actor.StartAnimation("Block");

        yield return new WaitForSeconds(actionTime);
        actor.Block();
        PlayImpactEffect(0, 0.5f);

        SetDialogueText(actor, "You are blocking", actor.unitName + " is blocking");

        yield return new WaitForSeconds(aftermathTime);
        DetermineNextTurn(actor, true);
    }
    public void OnChargeButton(int buttonIndex)
    {
        OnButton(buttonIndex);
        if (state != BattleStates.PlayerTurn) return;
        StartCoroutine(Charge(player));
    }
    IEnumerator Charge(Unit actor)
    {
        SetDialogueText(actor, "You began charging", actor.unitName + " began charging");
        actor.StartAnimation("Charge");

        yield return new WaitForSeconds(actionTime);
        actor.Charge();
        PlayImpactEffect(0, 0.25f);

        SetDialogueText(actor, "Attack charged", actor.unitName + " charged an attack");

        yield return new WaitForSeconds(aftermathTime);
        DetermineNextTurn(actor, true);
    }
    void OnButton(int buttonIndex)
    {
        clicksUntilSkip = 2;
        uiButtons[buttonIndex].AddUse(-1);
        optionState = Options.None;
        lastSelected = buttonIndex;
    }
    void DetermineNextTurn(Unit actor, bool newTurn)
    {
        if (enemy.currentHealth == 0)
        {
            StartCoroutine(SpawnNextEnemy());
            return;
        }
        if (player.currentHealth == 0)
        {
            EndBattle(BattleStates.Lost);
            return;
        }
        if (newTurn)
        {
            if (actor == player)
                StartCoroutine(EnemyTurn());
            else
                StartCoroutine(PlayerTurn());
        }
    }
    void SetDialogueText(Unit actor, string playerText, string enemyText)
    {
        if (actor == player)
        {
            dialogueText.text = playerText;
        }
        else
        {
            dialogueText.text = enemyText;
        }
    }
    IEnumerator SpawnNextEnemy()
    {
        RemoveEffects(enemy);
        yield return new WaitForSeconds(aftermathTime);
        currentEnemy++;
        if (currentEnemy >= levelStats.enemies.Count)
        {
            EndBattle(BattleStates.Won);
            yield break;
        }
        InstantiateEnemy();
        yield return new WaitForSeconds(introDialogueWaitTime);
        StartCoroutine(PlayerTurn());
    }
    void RemoveEffects(Unit subject)
    {
        subject.poisoned = 0;
        subject.blocking = false;
        subject.charging = false;
        subject.burning = 0;
        subject.weakened = false;
    }
    void InstantiateEnemy()
    {
        var newEnemy = Instantiate(levelStats.enemies[currentEnemy].gameObject, EnemyBattleStation).GetComponent<Unit>();
        if (currentEnemy != 0)
            newEnemy.transform.localPosition = new Vector3(5, 0, 0);
        enemy = newEnemy;
        enemyHUD.SetHUD(enemy);
        enemy.audioOrganizer = audioOrganizer;
        SetDialogueText(player, "A wild " + enemy.unitName + " joined the brawl!", "");
    }
    void DropLoot()
    {
        if (levelStats.potentialLoot.Count > 0)
        {
            var droppedItem = Instantiate(itemHolder.gameObject, EnemyBattleStation);
            droppedItem.transform.parent = null;
            droppedItem.transform.position += new Vector3(0, 0.5f);
            ItemHolder droppedItemHolder = droppedItem.GetComponent<ItemHolder>();
            List<Item> possibleItems = new List<Item>();
            for (int i = 0; i < levelStats.potentialLoot.Count; i++)
            {
                if (levelStats.potentialLoot[i].itemType != player.inventory.lastItemType)
                {
                    possibleItems.Add(levelStats.potentialLoot[i]);
                }
            }
            if (possibleItems.Count > 0)
            {
                droppedItemHolder.item = (possibleItems[UnityEngine.Random.Range(0, possibleItems.Count)]);
            }
            else
            {
                droppedItemHolder.item = (player.inventory.items[UnityEngine.Random.Range(0, player.inventory.items.Count)]);
            }
            droppedItemHolder.battleSystem = this;
            itemHolder = droppedItemHolder;
        }
    }
    void EndBattle(BattleStates battleResult)
    {
        state = battleResult;
        optionState = Options.None;
        Time.timeScale = GameSettings.defaultTimeScale;
        switch (battleResult)
        {
            case BattleStates.Won:
                RemoveEffects(player);
                DropLoot();
                if (player.inventory.levelsCleared.Count > levelStats.GetLevelIndex())
                    player.inventory.levelsCleared[levelStats.GetLevelIndex()] = true;
                else
                    print("Game Completed");
                SetDialogueText(player, "You won the battle!", "");
                break;
            case BattleStates.Lost:
                RemoveEffects(player);
                InstantiateMenu(defeatMenu);
                SetDialogueText(player, "You were defeated", "");
                break;
            default:
                break;
        }
    }
    void InstantiateMenu(Menu menu)
    {
        var newMenu = Instantiate(menu.gameObject, transform);
        Menu menuComponent = newMenu.GetComponent<Menu>();
        menuComponent.instantiator = this;
    }
    public void PlayImpactEffect(int effectStrength, float shakeStrength)
    {
        effectsAnimator.SetInteger("Strength", effectStrength);
        effectsAnimator.SetTrigger("Impact");
        cameraController.shakeCamera(shakeStrength);
    }
}
