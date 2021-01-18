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
    public bool isBoss = false;

    [Header("Self")]
    [SerializeField] TMP_Text levelText = null;

    BattleSystem battleSystem = null;
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
        FindObjectOfType<Fade>().FadeIn();
        StartCoroutine(WaitForFadeIn());
    }
    IEnumerator WaitForFadeIn()
    {
        yield return new WaitForSeconds(GameSettings.fadeInTime);
        DontDestroyOnLoad(gameObject);
        if (!isBoss)
            SceneManager.LoadScene((int)GameSettings.Scenes.Battle);
        else
            SceneManager.LoadScene((int)GameSettings.Scenes.Boss);
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
        StartCoroutine(StartBattleSystem());
    }
}
