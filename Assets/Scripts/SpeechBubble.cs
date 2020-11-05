using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public string message = "";
    [SerializeField] float typingTime = 0.1f;
    [SerializeField] TMP_Text text = null;
    [SerializeField] Image image = null;
    [SerializeField] float sizeUpdateSpeed = 1f;
    [SerializeField] float sizeOffset = 0.5f;
    private void Start()
    {
        StartCoroutine(WriteMessage());
    }
    void Update()
    {
        image.rectTransform.sizeDelta = Vector2.Lerp(image.rectTransform.sizeDelta, new Vector2((text.text.Length * text.fontSize / 200) + sizeOffset, image.rectTransform.sizeDelta.y), sizeUpdateSpeed * Time.deltaTime * 100);
    }
    IEnumerator WriteMessage()
    {
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
}
