using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI textDisplay;
    private string fullText = "";
    private float charDelay = 0.05f;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    public void Initialize(TextMeshProUGUI tmpText)
    {
        textDisplay = tmpText;
        if (textDisplay == null)
        {
            Debug.LogError("[TypewriterEffect] Initialize 傳入的 TextMeshProUGUI 為 null！");
        }
        else
        {
            Debug.Log("[TypewriterEffect] 初始化成功，TextMeshProUGUI: " + tmpText.gameObject.name);
            // 清空初始文字
            textDisplay.text = "";
        }
    }

    private void Awake()
    {
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
            if (textDisplay != null)
            {
                Debug.Log("[TypewriterEffect] Awake 中自動獲取 TextMeshProUGUI 成功");
                textDisplay.text = "";
            }
        }
    }

    public void StartTyping(string text, float speed = 0.05f)
    {
        Debug.Log("[TypewriterEffect] StartTyping 被調用，文字: " + text);

        // 停止之前的協程
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (textDisplay == null)
        {
            Debug.LogError("[TypewriterEffect] textDisplay 為 null！無法執行 StartTyping");
            return;
        }

        fullText = text;
        charDelay = speed;
        textDisplay.text = "";

        // 立即啟動協程
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        Debug.Log("[TypewriterEffect] 協程開始");
        isTyping = true;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (textDisplay == null)
            {
                Debug.LogWarning("[TypewriterEffect] textDisplay 在打字過程中變為 null");
                isTyping = false;
                yield break;
            }

            char c = fullText[i];
            textDisplay.text = fullText.Substring(0, i + 1);

            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;
        Debug.Log("[TypewriterEffect] 協程結束");
    }

    public void ShowAll()
    {
        if (textDisplay == null)
        {
            Debug.LogError("[TypewriterEffect] ShowAll 中 textDisplay 為 null");
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textDisplay.text = fullText;
        isTyping = false;
        Debug.Log("[TypewriterEffect] ShowAll 已執行");
    }

    public void Stop()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    public void Clear()
    {
        Stop();
        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
        fullText = "";
    }

    public bool IsTyping() => isTyping;
}