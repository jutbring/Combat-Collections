using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpeechBubble : MonoBehaviour
{
    public string message = "";
    public bool enemy = false;
    [SerializeField] float typingTime = 0.1f;
    [SerializeField] TMP_Text text = null;
    [SerializeField] Image image = null;
    [SerializeField] float sizeUpdateSpeed = 1f;
    [SerializeField] float sizeOffset = 0.5f;
    [SerializeField] bool splash = false;
    [SerializeField] float maxTextSize = 350f;
    [SerializeField] float textStartSize = 50f;

    Animator animator = null;
    private void Start()
    {
        StartCoroutine(WriteMessage());
        animator = GetComponent<Animator>();
        animator.SetBool("Splash", splash);
        animator.SetBool("Enemy", enemy);
        text.fontSize = Mathf.Min(textStartSize, maxTextSize);
        if (splash)
        {
            transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(15 * Convert.ToInt32(enemy), -15 * Convert.ToInt32(!enemy))));
        }
        try
        {
            text.fontSize += Mathf.Clamp(float.Parse(message) / (GameSettings.damageScale / 2), 0, maxTextSize);
        }
        catch { }
    }
    void Update()
    {
        image.rectTransform.sizeDelta = Vector2.Lerp(image.rectTransform.sizeDelta, new Vector2((text.text.Length * text.fontSize / 200) + sizeOffset, image.rectTransform.sizeDelta.y), sizeUpdateSpeed * Time.deltaTime * 100);
    }
    IEnumerator WriteMessage()
    {
        if (splash)
        {
            text.text = message;
            yield break;
        }
        text.text = "";
        for (int i = 0; i < message.Length + 1; i++)
        {
            bool isBlank = message.Substring(Mathf.Max(i - 1, 0), 1) == " ";
            if (!isBlank)
            {
                yield return new WaitForSeconds(typingTime);
            }
            text.text = message.Substring(0, i);
        }
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
