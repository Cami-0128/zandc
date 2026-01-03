using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家冰凍效果腳本 - 掛在玩家物件上
/// 處理冰凍狀態和視覺效果
/// 支援無敵星星免疫效果
/// </summary>
public class PlayerFreezeEffect : MonoBehaviour
{
    [Header("視覺效果")]
    [Tooltip("冰凍時的顏色")]
    public Color frozenColor = new Color(0.5f, 0.8f, 1f, 1f); // 冰藍色

    [Tooltip("冰凍特效預製體（可選）")]
    public GameObject frozenEffectPrefab;

    [Tooltip("是否使用閃爍效果")]
    public bool useFlickerEffect = true;

    [Tooltip("閃爍頻率（次/秒）")]
    public float flickerSpeed = 10f;

    [Header("音效")]
    [Tooltip("解凍音效")]
    public AudioClip unfreezeSound;

    private bool isFrozen = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private PlayerController2D playerController;
    private Rigidbody2D rb;
    private GameObject frozenEffectInstance;
    private AudioSource audioSource;
    private InvincibilityController invincibility;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();
        invincibility = GetComponent<InvincibilityController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && unfreezeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (playerController == null)
        {
            Debug.LogError("找不到 PlayerController2D 組件！");
        }
    }

    /// <summary>
    /// 凍住玩家
    /// </summary>
    /// <param name="duration">冰凍持續時間</param>
    public void Freeze(float duration)
    {
        // ========== 檢查無敵狀態 ==========
        if (invincibility != null && invincibility.IsInvincible())
        {
            Debug.Log("[冰凍效果] 玩家處於無敵狀態，免疫冰凍！");
            return;
        }

        if (isFrozen) return;

        StartCoroutine(FreezeCoroutine(duration));
    }

    /// <summary>
    /// 檢查玩家是否被凍住
    /// </summary>
    public bool IsFrozen()
    {
        return isFrozen;
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        // ========== 再次檢查無敵狀態（雙重保險）==========
        if (invincibility != null && invincibility.IsInvincible())
        {
            Debug.Log("[冰凍效果] 協程中檢測到無敵狀態，取消冰凍");
            yield break;
        }

        isFrozen = true;

        // 禁用玩家控制
        if (playerController != null)
        {
            playerController.canControl = false;
        }

        // 凍結物理速度
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            // 可選：暫時增加重力縮放或設為 Kinematic
            // rb.gravityScale = 0f;
        }

        Debug.Log($"[冰凍效果] 玩家被凍住 {duration} 秒");

        // 生成冰凍特效
        if (frozenEffectPrefab != null)
        {
            frozenEffectInstance = Instantiate(frozenEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        // 視覺效果
        if (useFlickerEffect)
        {
            StartCoroutine(FlickerEffect(duration));
        }
        else
        {
            // 簡單變色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = frozenColor;
            }
        }

        // 等待冰凍持續時間
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // ========== 在等待期間持續檢查無敵狀態 ==========
            if (invincibility != null && invincibility.IsInvincible())
            {
                Debug.Log("[冰凍效果] 玩家獲得無敵狀態，立即解凍！");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 解除冰凍
        Unfreeze();
    }

    private void Unfreeze()
    {
        isFrozen = false;

        // 恢復玩家控制
        if (playerController != null && !playerController.isDead)
        {
            playerController.canControl = true;
        }

        // 恢復物理設定
        if (rb != null)
        {
            // 如果之前修改了 gravityScale，這裡恢復
            // rb.gravityScale = 1f;
        }

        // 恢復原本顏色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // 移除冰凍特效
        if (frozenEffectInstance != null)
        {
            Destroy(frozenEffectInstance);
        }

        // 播放解凍音效
        if (unfreezeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unfreezeSound);
        }

        Debug.Log("[冰凍效果] 玩家解凍");
    }

    /// <summary>
    /// 閃爍效果協程
    /// </summary>
    private IEnumerator FlickerEffect(float duration)
    {
        if (spriteRenderer == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration && isFrozen)
        {
            // ========== 檢查無敵狀態 ==========
            if (invincibility != null && invincibility.IsInvincible())
            {
                break;
            }

            // 在原色和冰凍色之間切換
            float t = Mathf.PingPong(elapsed * flickerSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, frozenColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 確保最後恢復原色
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// 強制解凍（例如玩家死亡或獲得無敵時）
    /// </summary>
    public void ForceUnfreeze()
    {
        StopAllCoroutines();
        Unfreeze();
    }

    void OnDestroy()
    {
        // 清理特效
        if (frozenEffectInstance != null)
        {
            Destroy(frozenEffectInstance);
        }
    }

    // ========== 當玩家獲得無敵時自動解凍 ==========
    void Update()
    {
        if (isFrozen && invincibility != null && invincibility.IsInvincible())
        {
            Debug.Log("[冰凍效果] 偵測到無敵狀態，強制解凍");
            ForceUnfreeze();
        }
    }
}