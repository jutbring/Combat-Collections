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

    [Header("Self")]
    [SerializeField] TMP_Text levelText = null;

    BattleSystem battleSystem = null;
    MapSystem mapSystem = null;
    private void Start()
    {
        if (pixelPerfectPosition)
        {
            Vector3 desiredPosition = transform.localPosition - new Vector3(
                transform.localPosition.x % (1 / (float)GameSettings.pixelsPerUnit),
                transform.localPosition.y % (1 / (float)GameSettings.pixelsPerUnit))
            + new Vector3(
                1 / (float)(2 * GameSettings.pixelsPerUnit),
                1 / (float)(2 * GameSettings.pixelsPerUnit));
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
        StartCoroutine(WaitForFadeout());
    }
    IEnumerator WaitForFadeout()
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
            print(level);
            battleSystem.StartGame(level);
            Destroy(gameObject);
        }
        else
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(StartBattleSystem());
        }
        GameSettings.isFading = false;
    }
}
