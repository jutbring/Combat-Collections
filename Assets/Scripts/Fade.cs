using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    [SerializeField] Animator fadeAnimator = null;
    BattleSystem battleSystem = null;
    MapSystem mapSystem = null;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (!battleSystem && !mapSystem)
        {
            battleSystem = GameObject.FindWithTag("GameController").GetComponent<BattleSystem>();
            mapSystem = GameObject.FindWithTag("GameController").GetComponent<MapSystem>();
        }
        if (battleSystem)
        {
            battleSystem.fade = this;
        }
        if (mapSystem)
        {
            mapSystem.fade = this;
        }
    }
    public void FadeIn()
    {
        fadeAnimator.SetTrigger("FadeIn");
    }
    public void FadeOut()
    {
        fadeAnimator.SetTrigger("FadeOut");
    }
}
