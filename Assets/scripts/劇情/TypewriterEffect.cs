using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI textDisplay;
    private string fullText;
    private float charDelay = 0.05f;
    private bool isTyping = false;

    private void OnEnable()
    {
        // 在 OnEnable 時就尋找，比 Start 更早
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
            if (textDisplay == null)
            {
                Debug.LogError("[TypewriterEffect] 找不到 TextMeshProUGUI 組件！", gameObject);
            }
        }
    }

    private void Start()
    {
        // 再檢查一次
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
            if (textDisplay == null)
            {
                Debug.LogError("[TypewriterEffect] Start() 中找不到 TextMeshProUGUI！", gameObject);
            }
        }
    }

    public void StartTyping(string text, float speed = 0.05f)
    {
        // 在執行前再檢查一次
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
        }

        if (textDisplay == null)
        {
            Debug.LogError("[TypewriterEffect] textDisplay 仍然為 null！無法執行 StartTyping");
            return;
        }

        fullText = text;
        charDelay = speed;
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        if (textDisplay == null)
        {
            Debug.LogError("[TypewriterEffect] TypeText 中 textDisplay 為 null");
            yield break;
        }

        isTyping = true;
        textDisplay.text = "";

        foreach (char c in fullText)
        {
            if (textDisplay == null) yield break; // 防止物件被銷毀

            textDisplay.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    public void ShowAll()
    {
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
        }

        if (textDisplay != null)
        {
            StopAllCoroutines();
            textDisplay.text = fullText;
            isTyping = false;
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
        isTyping = false;
    }

    public bool IsTyping() => isTyping;
}