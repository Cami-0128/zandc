using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI textDisplay;
    private string fullText;
    private float charDelay = 0.05f;
    private bool isTyping = false;

    private void Start()
    {
        textDisplay = GetComponent<TextMeshProUGUI>();
    }

    public void StartTyping(string text, float speed = 0.05f)
    {
        fullText = text;
        charDelay = speed;

        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        textDisplay.text = "";

        foreach (char c in fullText)
        {
            textDisplay.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    public void ShowAll()
    {
        StopAllCoroutines();
        textDisplay.text = fullText;
        isTyping = false;
    }

    public bool IsTyping() => isTyping;
}
