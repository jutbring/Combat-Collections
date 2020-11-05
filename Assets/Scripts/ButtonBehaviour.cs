using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField] TMP_Text usesText = null;
    [SerializeField] Button button = null;
    [SerializeField] Image highlightCover = null;

    public int uses = 1000;
    int maxUses = 100;
    void Update()
    {
        UpdateUsesText();
    }
    void UpdateUsesText()
    {
        usesText.text = uses.ToString();
        bool textVisible = uses < maxUses;
        usesText.enabled = textVisible;
        highlightCover.enabled = !textVisible;
        button.interactable = uses != 0;
    }
    public void AddUse(int amount)
    {
        if (uses < maxUses)
        {
            uses = Mathf.Clamp(uses + amount, 0, maxUses - 1);
        }
    }
}
