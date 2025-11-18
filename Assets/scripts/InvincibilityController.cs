using UnityEngine;
using System.Collections;

/// <summary>
/// 無敵星星控制器 - 附加到玩家身上，與 PlayerController2D 和 PlayerAttack 共存
/// </summary>
public class InvincibilityController : MonoBehaviour
{
    [Header("跳躍設定")]
    [Tooltip("無敵狀態跳躍倍數")]
    public float invincibleJumpMultiplier = 2f;

    [Header("無敵效果設定")]
    [Tooltip("無敵持續時間（秒）")]
    public float invincibilityDuration = 10f;
    [Tooltip("無敵時的血量上限")]
    public int invincibleMaxHealth = 200;
    [Tooltip("無敵時的魔力上限")]
    public int invincibleMaxMana = 200;
    [Tooltip("護盾預製物")]
    public GameObject shieldPrefab;
    [Tooltip("護盾大小")]
    public float shieldSize = 1.5f;
    [Tooltip("發光強度")]
    public float glowIntensity = 1f;
    [Tooltip("發光顏色")]
    public Color glowColor = Color.yellow;
    [Tooltip("敵人消失淡出時間")]
    public float enemyFadeOutDuration = 0.5f;

    [Header("音效")]
    [Tooltip("收集星星的音效")]
    public AudioClip starCollectSound;

    // 私有變數
    private bool isInvincible = false;
    private GameObject shieldInstance;
    private SpriteRenderer playerRenderer;
    private Color originalColor;
    private PlayerController2D playerController;
    private PlayerAttack playerAttack;
    private AudioSource audioSource;

    private int originalMaxHealth;
    private int originalMaxMana;
    private float originalJumpForce;

    private void Start()
    {
        playerController = GetComponent<PlayerController2D>();
        playerAttack = GetComponent<PlayerAttack>();
        playerRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (playerRenderer != null)
        {
            originalColor = playerRenderer.color;
        }

        // 記錄原始數值
        if (playerController != null)
        {
            originalMaxHealth = playerController.maxHealth;
            originalJumpForce = playerController.jumpForce;
        }

        if (playerAttack != null)
        {
            originalMaxMana = playerAttack.maxMana;
        }
    }

    public void ActivateInvincibility()
    {
        if (!isInvincible)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // 播放收集音效
        if (starCollectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(starCollectSound);
        }

        Debug.Log("[無敵星星] 無敵狀態啟動！");

        // === 提升血量上限並補滿 ===
        if (playerController != null)
        {
            playerController.maxHealth = invincibleMaxHealth;
            playerController.SetHealth(invincibleMaxHealth);
            playerController.jumpForce = originalJumpForce * invincibleJumpMultiplier;
            Debug.Log($"[無敵星星] 血量提升至 {invincibleMaxHealth}/{invincibleMaxHealth}");
            Debug.Log($"[無敵星星] 跳躍力度提升至 {playerController.jumpForce}");
        }

        // === 提升魔力上限並補滿 ===
        if (playerAttack != null)
        {
            playerAttack.maxMana = invincibleMaxMana;
            playerAttack.RestoreMana(invincibleMaxMana);

            // 更新魔力條UI
            ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
            if (manaBar != null)
            {
                manaBar.UpdateManaBar(invincibleMaxMana, invincibleMaxMana);
            }
            Debug.Log($"[無敵星星] 魔力提升至 {invincibleMaxMana}/{invincibleMaxMana}");
        }

        // === 創建護盾 ===
        if (shieldPrefab != null)
        {
            shieldInstance = Instantiate(shieldPrefab, transform);
            shieldInstance.transform.localPosition = Vector3.zero;
            shieldInstance.transform.localScale = Vector3.one * shieldSize;
            Debug.Log("[無敵星星] 護盾已生成");
        }

        // === 啟動發光效果 ===
        StartCoroutine(GlowEffect());

        // === 等待持續時間 ===
        yield return new WaitForSeconds(invincibilityDuration);

        // === 結束無敵狀態 ===
        isInvincible = false;
        Debug.Log("[無敵星星] 無敵狀態結束");

        // 恢復原始上限
        if (playerController != null)
        {
            playerController.maxHealth = originalMaxHealth;
            playerController.jumpForce = originalJumpForce;

            // 如果當前血量超過原始上限，調整回上限
            if (playerController.GetCurrentHealth() > originalMaxHealth)
            {
                playerController.SetHealth(originalMaxHealth);
            }
            Debug.Log($"[無敵星星] 血量上限恢復至 {originalMaxHealth}");
        }

        if (playerAttack != null)
        {
            playerAttack.maxMana = originalMaxMana;

            // 如果當前魔力超過原始上限，調整回上限
            if (playerAttack.GetCurrentMana() > originalMaxMana)
            {
                int excess = playerAttack.GetCurrentMana() - originalMaxMana;
                playerAttack.RestoreMana(-excess);
            }

            // 更新魔力條UI
            ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
            if (manaBar != null)
            {
                manaBar.UpdateManaBar(playerAttack.GetCurrentMana(), originalMaxMana);
            }
            Debug.Log($"[無敵星星] 魔力上限恢復至 {originalMaxMana}");
        }

        // 移除護盾
        if (shieldInstance != null)
        {
            Destroy(shieldInstance);
        }

        // 恢復原始顏色
        if (playerRenderer != null)
        {
            playerRenderer.color = originalColor;
        }
    }

    private IEnumerator GlowEffect()
    {
        float elapsedTime = 0f;

        while (isInvincible && elapsedTime < invincibilityDuration)
        {
            if (playerRenderer != null)
            {
                // 創建閃爍效果
                float glow = Mathf.PingPong(Time.time * 2f, 1f) * glowIntensity;
                playerRenderer.color = Color.Lerp(originalColor, glowColor, glow);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 恢復原始顏色
        if (playerRenderer != null && !isInvincible)
        {
            playerRenderer.color = originalColor;
        }
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public float GetJumpForceMultiplier()
    {
        return isInvincible ? invincibleJumpMultiplier : 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 無敵狀態下碰到敵人或陷阱會讓它們消失
        if (isInvincible)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Enemy1") || other.CompareTag("Trap"))
            {
                Debug.Log($"[無敵星星] 消滅了 {other.gameObject.name}");
                StartCoroutine(FadeOutAndDestroy(other.gameObject));
            }
        }
    }

    private IEnumerator FadeOutAndDestroy(GameObject obj)
    {
        // 禁用碰撞
        Collider2D[] colliders = obj.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float elapsedTime = 0f;
            Color originalColor = renderer.color;

            while (elapsedTime < enemyFadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / enemyFadeOutDuration);
                renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(obj);
    }
}