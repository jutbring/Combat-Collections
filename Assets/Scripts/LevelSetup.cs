using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSetup : MonoBehaviour
{
    [Header("Assigned")]
    public Level level = null;
    [SerializeField] bool loadInstantly = true;
    [SerializeField] bool pixelPerfectPosition = true;
    [SerializeField] SpriteRenderer positionReference = null;

    [Header("Self")]
    [SerializeField] TMP_Text levelText = null;

    BattleSystem battleSystem = null;
    MapSystem mapSystem = null;
    private void Awake()
    {
        if (pixelPerfectPosition)
        {
            float pixelsPerUnit = positionReference.sprite.pixelsPerUnit;
            Vector3 desiredPosition = transform.localPosition - new Vector3(
                transform.localPosition.x % (1 / pixelsPerUnit),
                transform.localPosition.y % (1 / pixelsPerUnit))
            + new Vector3(
                1 / (2 * pixelsPerUnit),
                1 / (2 * pixelsPerUnit),
                0);
            transform.localPosition = desiredPosition;
        }
        if (loadInstantly)
        {
            LoadBattle();
        }
        StartCoroutine(SetLevelText());
    }
    IEnumerator SetLevelText()
    {
        yield return new WaitForEndOfFrame();
        levelText.text = level.GetLevelName();
    }
    public void LoadBattle()
    {
        transform.parent = null;
        transform.localScale = new Vector3(1, 1, 1);
        if (GameSettings.isFading)
            return;
        GameSettings.isFading = true;
        battleSystem = GameObject.FindWithTag("GameController").GetComponent<BattleSystem>();
        if (battleSystem)
        {
            battleSystem.fade.FadeIn();
        }
        mapSystem = GameObject.FindWithTag("GameController").GetComponent<MapSystem>();
        if (mapSystem)
        {
            mapSystem.fade.FadeIn();
        }
        StartCoroutine(WaitForFadeIn());
    }
    IEnumerator WaitForFadeIn()
    {
        yield return new WaitForSeconds(GameSettings.fadeInTime);
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene((int)GameSettings.Scenes.Battle);
        StartCoroutine(StartBattleSystem());
    }
    IEnumerator StartBattleSystem()
    {
        battleSystem = GameObject.FindWithTag("GameController").GetComponent<BattleSystem>();
        if (battleSystem)
        {
            if (!battleSystem.started)
            {
                battleSystem.StartGame(level);
                FindObjectOfType<Fade>().FadeOut();
                Destroy(gameObject);
            }
        }
        yield return new WaitForEndOfFrame();
        GameSettings.isFading = false;
        StartCoroutine(StartBattleSystem());
    }
}
