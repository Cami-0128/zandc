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

    private void Awake()
    {
        // 確保 GameObject 和 Script 都是啟用的
        gameObject.SetActive(true);
        enabled = true;
    }

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
            textDisplay.text = "";
        }
    }

    public void StartTyping(string text, float speed = 0.05f)
    {
        Debug.Log("[TypewriterEffect] StartTyping 被調用，文字: " + text);
        Debug.Log("[TypewriterEffect] GameObject Active: " + gameObject.activeInHierarchy + ", Script Enabled: " + enabled);

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

        // 確保在啟動協程前，GameObject 是啟用的
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[TypewriterEffect] GameObject 未啟用，無法啟動協程！");
            gameObject.SetActive(true);
        }

        if (!enabled)
        {
            Debug.LogError("[TypewriterEffect] Script 未啟用，無法啟動協程！");
            enabled = true;
        }

        typingCoroutine = StartCoroutine(TypeText());
        Debug.Log("[TypewriterEffect] 協程已啟動");
    }

    private IEnumerator TypeText()
    {
        Debug.Log("[TypewriterEffect] 協程開始執行");
        isTyping = true;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (textDisplay == null)
            {
                Debug.LogWarning("[TypewriterEffect] textDisplay 在打字過程中變為 null");
                isTyping = false;
                yield break;
            }

            textDisplay.text = fullText.Substring(0, i + 1);
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = null;
        Debug.Log("[TypewriterEffect] 協程執行完成");
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
        Debug.Log("[TypewriterEffect] Stop 已執行");
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