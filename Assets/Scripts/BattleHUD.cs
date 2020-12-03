using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TMP_Text nameText = null;
    [SerializeField] TMP_Text dangerFactorText = null;
    [SerializeField] Slider hpSlider = null;
    [SerializeField] Slider hpSliderSlow = null;
    [SerializeField] float fillSpeed = 1f;
    [SerializeField] Image fillImage = null;
    [SerializeField] Image fillSlowImage = null;
    [SerializeField] Gradient healthGradient = null;
    [SerializeField] Gradient healthSlowGradient = null;
    [SerializeField] Animator blockingIndicator = null;
    [SerializeField] Animator chargingIndicator = null;
    [SerializeField] Animator poisonedIndicator = null;
    [SerializeField] Animator burningIndicator = null;
    [SerializeField] Animator weakenedIndicator = null;
    [SerializeField] Animator blindedIndicator = null;
    [SerializeField] float iconPosX = 0f;
    [SerializeField] float iconDistance = 1f;
    [SerializeField] float indicatorMoveSpeed = 1f;

    List<int> effectIndex = new List<int>();
    Unit unit = null;
    int lastPoisonAmount = 0;
    int lastBurningAmount = 0;

    public void SetHUD(Unit newUnit)
    {
        unit = newUnit;
        dangerFactorText.text = "lvl " + unit.stats.GetDangerFactor();
        nameText.text = unit.unitName;
        hpSlider.maxValue = unit.maxHealth;
        hpSliderSlow.maxValue = unit.maxHealth;
    }
    void Update()
    {
        if (!unit)
        {
            return;
        }
        SetHP();
        effectIndex.RemoveRange(0, effectIndex.Count);
        UpdateIndicator(unit.blocking, blockingIndicator, 0);
        UpdateIndicator(unit.charging, chargingIndicator, 1);
        UpdateIndicator(unit.poisoned > 0, poisonedIndicator, 2);
        UpdateIndicator(unit.burning > 0, burningIndicator, 3);
        UpdateIndicator(unit.weakened, weakenedIndicator, 4);
        UpdateIndicator(unit.blinded, blindedIndicator, 5);
        if (lastPoisonAmount != unit.poisoned)
        {
            lastPoisonAmount = unit.poisoned;
            UseIndicator(poisonedIndicator);
        }
        if (lastBurningAmount != unit.burning)
        {
            lastBurningAmount = unit.burning;
            UseIndicator(burningIndicator);
        }
        if (unit.currentHealth == 0)
        {
            hpSliderSlow.value = 0;
        }
    }
    void SetHP()
    {
        bool hpReducing = hpSlider.value >= unit.currentHealth;
        if (hpReducing)
        {
            hpSlider.value = unit.currentHealth;
            hpSliderSlow.value = Mathf.Lerp(hpSliderSlow.value, unit.currentHealth, fillSpeed * Time.deltaTime * 100 /* (Mathf.Abs(unit.currentHealth - hpSliderSlow.value) / hpSliderSlow.maxValue)*/);
        }
        else
        {
            hpSlider.value = Mathf.Lerp(hpSlider.value, unit.currentHealth, fillSpeed * Time.deltaTime * 100 /* (Mathf.Abs(unit.currentHealth - hpSlider.value) / hpSlider.maxValue)*/);
            hpSliderSlow.value = unit.currentHealth;
        }
        fillImage.color = healthGradient.Evaluate(hpSliderSlow.value / Mathf.Max(hpSliderSlow.maxValue, 10));
        fillSlowImage.color = healthSlowGradient.Evaluate(hpSlider.value / Mathf.Max(hpSlider.maxValue, 10));
    }
    void UpdateIndicator(bool unitActivity, Animator indicatorAnimator, int lastStateIndex)
    {
        bool isActive = unitActivity;
        if (unit.indicatorLastStates.Count <= lastStateIndex)
        {
            unit.indicatorLastStates.Add(unitActivity);
        }
        if (unit.indicatorLastStates[lastStateIndex] != unitActivity)
        {
            unit.indicatorLastStates[lastStateIndex] = unitActivity;
            indicatorAnimator.SetBool("Visible", isActive);
            indicatorAnimator.SetTrigger("Update");
        }
        if (isActive)
        {
            effectIndex.Add(effectIndex.Count);
            int index = -1;
            for (int i = 0; i < effectIndex.Count; i++)
            {
                index++;
            }
            Vector3 desiredPosition = new Vector3(
                iconPosX + (iconDistance * effectIndex[index] % 7),
                indicatorAnimator.transform.localPosition.y,
                indicatorAnimator.transform.localPosition.z);
            Vector2 pos = indicatorAnimator.transform.localPosition = Vector3.Lerp(
                  indicatorAnimator.transform.localPosition,
                  desiredPosition,
                  indicatorMoveSpeed * Time.deltaTime * 100);
            indicatorAnimator.transform.localPosition = pos;
        }
    }
    void UseIndicator(Animator indicatorAnimator)
    {
        indicatorAnimator.SetTrigger("Apply");
    }
}
