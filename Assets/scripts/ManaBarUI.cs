using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("魔力條UI組件")]
    public Image manaBarFill;
    public Image manaBarBackground;
    public TextMeshProUGUI manaText;

    [Header("視覺效果 - 魔力條顏色漸變")]
    public Color fullManaColor = new Color(0.2f, 0.5f, 1f);
    public Color highManaColor = new Color(0.5f, 0.3f, 0.8f);
    public Color mediumManaColor = new Color(0.4f, 0.7f, 1f);
    public Color lowManaColor = new Color(0.1f, 0.2f, 0.5f);

    [Header("動畫設定")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;
    public bool enableFlashOnUse = true;
    public Color useFlashColor = Color.cyan;
    public float flashDuration = 0.15f;

    [Header("顏色漸變設定")]
    public bool enableColorLerp = true;
    public float colorTransitionSpeed = 3f;

    private PlayerController2D player;
    private float targetFillAmount;
    private float currentFillAmount;
    private int lastKnownMana = -1;
    private Color currentColor;
    private Color targetColor;
    private bool isFlashing = false;
    private bool isInitialized = false;
    private Coroutine flashCoroutine;

    void OnValidate()
    {
        if (manaBarFill != null)
        {
            manaBarFill.type = Image.Type.Filled;
            manaBarFill.fillMethod = Image.FillMethod.Horizontal;
            manaBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            manaBarFill.fillAmount = 1f;
            manaBarFill.color = fullManaColor;
        }
        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Awake()
    {
        currentFillAmount = 1f;
        targetFillAmount = 1f;
        currentColor = fullManaColor;
        targetColor = fullManaColor;
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = 1f;
            manaBarFill.color = fullManaColor;
        }
        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Start()
    {
        StartCoroutine(InitializeManaBar());
    }

    IEnumerator InitializeManaBar()
    {
        yield return new WaitForEndOfFrame();
        player = FindObjectOfType<PlayerController2D>();
        int attempts = 0;
        while (player == null && attempts < 100)
        {
            yield return new WaitForEndOfFrame();
            player = FindObjectOfType<PlayerController2D>();
            attempts++;
        }

        if (player != null)
        {
            UpdateManaBar(player.currentMana, player.maxMana);
            lastKnownMana = player.currentMana;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("找不到 PlayerController2D 組件，魔力條無法初始化。");
            if (manaBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                manaBarFill.fillAmount = 1f;
                manaBarFill.color = fullManaColor;
            }
        }
    }

    void Update()
    {
        if (isInitialized)
        {
            AutoSyncWithPlayer();
        }
        if (enableSmoothTransition && manaBarFill != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
                manaBarFill.fillAmount = currentFillAmount;
                if (targetFillAmount <= 0f && currentFillAmount < 0.01f)
                {
                    currentFillAmount = 0f;
                    manaBarFill.fillAmount = 0f;
                }
            }
        }
        if (enableColorLerp && manaBarFill != null && !isFlashing)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            manaBarFill.color = currentColor;
        }
    }

    void AutoSyncWithPlayer()
    {
        if (player != null)
        {
            int currentMana = player.currentMana;
            if (currentMana != lastKnownMana)
            {
                UpdateManaBar(currentMana, player.maxMana);
                lastKnownMana = currentMana;
            }
        }
    }

    // 供玩家施放攻擊時呼叫，減少魔力並更新UI
    public void ReduceMana(int amount)
    {
        if (player == null) return;
        player.currentMana -= amount;
        player.currentMana = Mathf.Clamp(player.currentMana, 0, player.maxMana);
        UpdateManaBar(player.currentMana, player.maxMana);
    }

    public void UpdateManaBar(int currentMana, int maxMana)
    {
        if (manaBarFill == null)
            return;

        float manaPercentage = (float)currentMana / maxMana;
        bool usedMana = (lastKnownMana > 0 && currentMana < lastKnownMana);
        targetFillAmount = manaPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            manaBarFill.fillAmount = currentFillAmount;
        }
        if (currentMana <= 0)
        {
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            manaBarFill.fillAmount = 0f;
        }

        UpdateManaBarColor(manaPercentage);
        UpdateManaText(currentMana, maxMana);

        if (enableFlashOnUse && usedMana)
        {
            PlayManaFlash();
        }

        lastKnownMana = currentMana;

        if (!isInitialized)
        {
            isInitialized = true;
        }
    }

    void UpdateManaBarColor(float manaPercentage)
    {
        if (manaBarFill == null)
            return;

        Color newTargetColor;
        if (manaPercentage > 0.75f)
            newTargetColor = fullManaColor;
        else if (manaPercentage > 0.5f)
            newTargetColor = highManaColor;
        else if (manaPercentage > 0.25f)
            newTargetColor = mediumManaColor;
        else
            newTargetColor = lowManaColor;

        targetColor = newTargetColor;

        if (!enableColorLerp)
        {
            currentColor = targetColor;
            manaBarFill.color = targetColor;
        }
    }

    void UpdateManaText(int currentMana, int maxMana)
    {
        if (manaText != null)
            manaText.text = $"{currentMana}/{maxMana}";
    }

    void PlayManaFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (manaBarFill != null)
        {
            flashCoroutine = StartCoroutine(ManaFlashCoroutine());
        }
    }

    IEnumerator ManaFlashCoroutine()
    {
        if (manaBarFill == null)
            yield break;

        isFlashing = true;
        Color originalColor = currentColor;
        manaBarFill.color = useFlashColor;

        yield return new WaitForSeconds(flashDuration);

        manaBarFill.color = originalColor;
        isFlashing = false;
        flashCoroutine = null;
    }
}
