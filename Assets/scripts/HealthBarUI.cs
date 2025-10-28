using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("血條UI組件")]
    public Image healthBarFill;
    public Image healthBarBackground;
    public TextMeshProUGUI healthText;

    [Header("動畫設定")]
    public float animationSpeed = 2f;
    public bool smoothTransition = true;

    [Header("視覺效果")]
    public Color fullHealthColor = Color.green;
    public Color highHealthColor = Color.yellow;
    public Color mediumHealthColor = new Color(1f, 0.5f, 0f);
    public Color lowHealthColor = Color.red;

    [Header("受傷閃爍效果")]
    public bool enableDamageFlash = true;
    public Color damageFlashColor = Color.white;
    public float flashDuration = 0.1f;

    private float targetFillAmount;
    private float currentFillAmount;
    private Coroutine flashCoroutine;
    private int lastKnownHealth = -1;
    private bool isInitialized = false;

    void Start()
    {
        // 儘早根據玩家初始血量設置
        StartCoroutine(LateInitializeHealthBar());
    }

    IEnumerator LateInitializeHealthBar()
    {
        yield return new WaitForEndOfFrame();
        var player = FindObjectOfType<PlayerController2D>();
        int attempts = 0;

        while (player == null && attempts < 100)
        {
            yield return new WaitForEndOfFrame();
            player = FindObjectOfType<PlayerController2D>();
            attempts++;
        }

        int cur = 1, max = 1;
        if (player != null)
        {
            cur = player.GetCurrentHealth();
            max = player.GetMaxHealth();
        }
        InitializeHealthBarDisplay(cur, max);

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
            stats.onHealthChanged += UpdateHealthBar;
    }

    void InitializeHealthBarDisplay(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        currentFillAmount = healthPercentage;
        targetFillAmount = healthPercentage;
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercentage;
            UpdateHealthBarColor(healthPercentage);
        }
        UpdateHealthText(currentHealth, maxHealth);
        lastKnownHealth = currentHealth;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
        // 平滑動畫
        if (smoothTransition && healthBarFill != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
                healthBarFill.fillAmount = currentFillAmount;
            }
        }
    }

    public void UpdateHealthBar(int current, int max)
    {
        float percent = (float)current / max;
        bool tookDamage = (lastKnownHealth > 0 && current < lastKnownHealth);

        targetFillAmount = percent;
        // 強制設置顏色
        UpdateHealthBarColor(percent);
        UpdateHealthText(current, max);

        if (enableDamageFlash && tookDamage)
        {
            PlayDamageFlash();
        }
        lastKnownHealth = current;
    }

    void UpdateHealthBarColor(float percent)
    {
        if (healthBarFill == null) return;
        if (percent > 0.75f)
            healthBarFill.color = fullHealthColor;
        else if (percent > 0.5f)
            healthBarFill.color = highHealthColor;
        else if (percent > 0.25f)
            healthBarFill.color = mediumHealthColor;
        else
            healthBarFill.color = lowHealthColor;
    }

    void UpdateHealthText(int cur, int max)
    {
        if (healthText != null)
            healthText.text = $"{cur}/{max}";
    }

    void PlayDamageFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DamageFlashCoroutine());
    }

    IEnumerator DamageFlashCoroutine()
    {
        if (healthBarFill == null) yield break;
        Color originalColor = healthBarFill.color;
        healthBarFill.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        healthBarFill.color = originalColor;
        flashCoroutine = null;
    }
}
