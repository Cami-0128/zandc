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
        var playerAttack = FindObjectOfType<PlayerAttack>();
        int attempts = 0;
        while (playerAttack == null && attempts < 100)
        {
            yield return new WaitForEndOfFrame();
            playerAttack = FindObjectOfType<PlayerAttack>();
            attempts++;
        }

        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();
            int maxMana = playerAttack.maxMana;

            float manaPercentage = (float)currentMana / maxMana;
            currentFillAmount = manaPercentage;
            targetFillAmount = manaPercentage;

            if (manaBarFill != null)
            {
                manaBarFill.fillAmount = manaPercentage;
                UpdateManaBarColor(manaPercentage);
            }
            UpdateManaText(currentMana, maxMana);

            lastKnownMana = currentMana;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("找不到 PlayerAttack 組件！魔力條無法初始化");
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
        var playerAttack = FindObjectOfType<PlayerAttack>();
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();
            if (currentMana != lastKnownMana)
            {
                int maxMana = playerAttack.maxMana;
                UpdateManaBar(currentMana, maxMana);
                lastKnownMana = currentMana;
            }
        }
    }

    public void UpdateManaBar(int currentMana, int maxMana)
    {
        if (manaBarFill == null)
            return;
        float manaPercentage = (float)currentMana / maxMana;

        // 只要魔力有變化就觸發更新
        bool manaChanged = (lastKnownMana != currentMana);

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

        // 任何魔力變化都觸發閃光
        if (enableFlashOnUse && manaChanged)
        {
            PlayManaFlash();
        }

        lastKnownMana = currentMana;

        if (!isInitialized)
            isInitialized = true;
    }

    void UpdateManaBarColor(float manaPercentage)
    {
        if (manaBarFill == null) return;
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
