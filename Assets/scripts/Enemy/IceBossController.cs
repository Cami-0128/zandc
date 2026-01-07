using UnityEngine;
using System.Collections;

/// <summary>
/// 冰元素Boss控制器 - 進階版
/// 特性：階段轉換、投射物型/範圍型冰凍攻擊、完整無敵星星支援
/// </summary>
public class IceBossController : MonoBehaviour
{
    [Header("遊戲開始控制")]
    [Tooltip("Boss是否能夠行動（由StartManager控制）")]
    public bool canMove = false;

    [Tooltip("是否忽略 Time.timeScale（通常不需要勾選）")]
    public bool ignoreTimeScale = false;

    [Header("基本屬性")]
    [Tooltip("Boss最大血量")]
    public float maxHealth = 1000f;
    private float currentHealth;

    [Tooltip("血條UI組件")]
    public EnemyHealthBar healthBar;

    [Tooltip("是否啟用玩家追蹤")]
    public bool enablePlayerTracking = true;

    [Header("移動與跳躍 - 加強版")]
    [Tooltip("垂直跳躍力")]
    public float jumpForceVertical = 10f;

    [Tooltip("水平跳躍力（朝玩家方向）")]
    public float jumpForceHorizontal = 8f;

    [Tooltip("跳躍冷卻時間")]
    public float jumpCooldown = 0.8f;

    [Tooltip("地面檢測半徑")]
    public float groundCheckRadius = 0.3f;

    [Tooltip("地面圖層")]
    public LayerMask groundLayer;

    [Header("冰球攻擊")]
    [Tooltip("冰球Prefab")]
    public GameObject iceBallPrefab;

    [Tooltip("發射點")]
    public Transform firePoint;

    [Tooltip("冰球速度")]
    public float iceBallSpeed = 14f;

    [Tooltip("冰球攻擊冷卻（快速）")]
    public float iceBallCooldown = 1.5f;

    [Tooltip("冰球傷害")]
    public int iceBallDamage = 18;

    [Header("階段1：投射物型冰凍球")]
    [Tooltip("冰凍球Prefab")]
    public GameObject freezeOrbPrefab;

    [Tooltip("冰凍球速度（較慢）")]
    public float freezeOrbSpeed = 6f;

    [Tooltip("冰凍持續時間")]
    public float freezeDuration = 3f;

    [Tooltip("投射物型冰凍攻擊冷卻")]
    public float freezeOrbCooldown = 5f;

    [Header("階段2：範圍型冰環爆發")]
    [Tooltip("冰環Prefab")]
    public GameObject iceRingPrefab;

    [Tooltip("冰環爆發半徑")]
    public float iceRingRadius = 5f;

    [Tooltip("冰環持續時間")]
    public float iceRingDuration = 2f;

    [Tooltip("範圍型冰凍攻擊冷卻")]
    public float iceRingCooldown = 6f;

    [Tooltip("階段2血量閾值（1/4血量）")]
    public float phase2Threshold = 0.25f;

    [Header("受傷設定")]
    [Tooltip("玩家近戰劍攻擊傷害")]
    public float swordDamage = 30f;

    [Tooltip("魔法子彈傷害")]
    public float magicBulletDamage = 45f;

    [Tooltip("重型子彈傷害")]
    public float heavyBulletDamage = 70f;

    [Header("視覺效果")]
    [Tooltip("Boss顏色（冰藍色）")]
    public Color iceColor = new Color(0.5f, 0.8f, 1f, 1f);

    [Tooltip("階段2憤怒顏色")]
    public Color phase2Color = new Color(0.3f, 0.6f, 1f, 1f);

    [Header("音效")]
    [Tooltip("冰凍攻擊音效")]
    public AudioClip freezeAttackSound;

    [Tooltip("階段轉換音效")]
    public AudioClip phaseChangeSound;

    // 私有變數
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private AudioSource audioSource;
    private bool isDying = false;
    private bool isGrounded = false;
    private bool isPhase2 = false;

    private float lastJumpTime = -999f;
    private float lastIceBallTime = -999f;
    private float lastFreezeAttackTime = -999f;

    private System.Collections.Generic.HashSet<GameObject> processedProjectiles = new System.Collections.Generic.HashSet<GameObject>();

    void Start()
    {
        // 初始化組件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 尋找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[IceBoss] 找不到Player標籤的物件！");
        }

        // 初始化血量
        currentHealth = maxHealth;

        // 設定Boss顏色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = iceColor;
        }

        // 初始化血條
        if (healthBar != null)
        {
            healthBar.Initialize(transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("[IceBoss] 血條組件未設定！");
        }

        // 自動創建發射點
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = fp.transform;
        }

        Debug.Log($"[IceBoss] 冰元素Boss初始化完成，血量：{maxHealth}");
    }

    void Update()
    {
        if (isDying || playerTransform == null) return;

        // 更新朝向
        UpdateOrientation();

        // 攻擊邏輯
        HandleAttacks();
    }

    void FixedUpdate()
    {
        if (isDying) return;

        // 地面檢測
        isGrounded = IsGrounded();

        // 跳躍移動
        if (isGrounded && enablePlayerTracking && Time.time - lastJumpTime >= jumpCooldown)
        {
            PerformJumpTowardsPlayer();
            lastJumpTime = Time.time;
        }
    }

    /// <summary>
    /// 地面檢測
    /// </summary>
    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius, groundLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// 朝玩家跳躍
    /// </summary>
    void PerformJumpTowardsPlayer()
    {
        if (rb == null || playerTransform == null) return;

        float xDir = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector2 jumpVector = new Vector2(xDir * jumpForceHorizontal, jumpForceVertical);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.AddForce(jumpVector, ForceMode2D.Impulse);

        Debug.Log("[IceBoss] 跳躍朝向玩家");
    }

    /// <summary>
    /// 處理攻擊邏輯
    /// </summary>
    void HandleAttacks()
    {
        // 冰球攻擊（高頻率）
        if (Time.time - lastIceBallTime >= iceBallCooldown)
        {
            ShootIceBall();
            lastIceBallTime = Time.time;
        }

        // 階段判定
        float healthPercentage = currentHealth / maxHealth;

        if (!isPhase2 && healthPercentage <= phase2Threshold)
        {
            // 進入階段2
            EnterPhase2();
        }

        // 根據階段使用不同的冰凍攻擊
        if (isPhase2)
        {
            // 階段2：範圍型冰環
            if (Time.time - lastFreezeAttackTime >= iceRingCooldown)
            {
                StartCoroutine(CastIceRing());
                lastFreezeAttackTime = Time.time;
            }
        }
        else
        {
            // 階段1：投射物型冰凍球
            if (Time.time - lastFreezeAttackTime >= freezeOrbCooldown)
            {
                ShootFreezeOrb();
                lastFreezeAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 發射冰球（普通攻擊）
    /// </summary>
    void ShootIceBall()
    {
        if (iceBallPrefab == null || firePoint == null || playerTransform == null) return;

        GameObject iceBall = Instantiate(iceBallPrefab, firePoint.position, Quaternion.identity);

        // 計算朝向玩家的方向
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        iceBall.transform.right = direction;

        // 設定速度
        Rigidbody2D iceBallRb = iceBall.GetComponent<Rigidbody2D>();
        if (iceBallRb != null)
        {
            iceBallRb.velocity = direction * iceBallSpeed;
            iceBallRb.gravityScale = 0;
        }

        // 設定冰球組件
        IceBall iceBallScript = iceBall.GetComponent<IceBall>();
        if (iceBallScript != null)
        {
            iceBallScript.damage = iceBallDamage;
        }

        Debug.Log("[IceBoss] 發射冰球");
    }

    /// <summary>
    /// 發射冰凍球（階段1：投射物型）
    /// </summary>
    void ShootFreezeOrb()
    {
        if (freezeOrbPrefab == null || firePoint == null || playerTransform == null) return;

        GameObject freezeOrb = Instantiate(freezeOrbPrefab, firePoint.position, Quaternion.identity);

        // 計算朝向玩家的方向
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        freezeOrb.transform.right = direction;

        // 設定速度（較慢）
        Rigidbody2D orbRb = freezeOrb.GetComponent<Rigidbody2D>();
        if (orbRb != null)
        {
            orbRb.velocity = direction * freezeOrbSpeed;
            orbRb.gravityScale = 0;
        }

        // 設定冰凍球組件
        FreezeOrb orbScript = freezeOrb.GetComponent<FreezeOrb>();
        if (orbScript != null)
        {
            orbScript.freezeDuration = freezeDuration;
        }

        // 播放音效
        if (freezeAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(freezeAttackSound);
        }

        Debug.Log("[IceBoss] 發射冰凍球（階段1）");
    }

    /// <summary>
    /// 施放冰環（階段2：範圍型）
    /// </summary>
    IEnumerator CastIceRing()
    {
        if (playerTransform == null) yield break;

        Debug.Log("[IceBoss] 施放冰環爆發（階段2）");

        // 播放音效
        if (freezeAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(freezeAttackSound);
        }

        // 生成冰環特效
        GameObject iceRing = null;
        if (iceRingPrefab != null)
        {
            iceRing = Instantiate(iceRingPrefab, transform.position, Quaternion.identity);
            iceRing.transform.localScale = Vector3.one * iceRingRadius * 2f;
        }

        // 持續檢測範圍內的玩家
        float elapsed = 0f;
        while (elapsed < iceRingDuration)
        {
            // 檢測範圍內的玩家
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, iceRingRadius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // 檢查玩家是否無敵
                    InvincibilityController invincibility = hit.GetComponent<InvincibilityController>();
                    if (invincibility != null && invincibility.IsInvincible())
                    {
                        Debug.Log("[IceBoss] 玩家無敵，冰環無效！");
                        continue;
                    }

                    // 凍結玩家（只凍結一次）
                    PlayerFreezeEffect freezeEffect = hit.GetComponent<PlayerFreezeEffect>();
                    if (freezeEffect != null && !freezeEffect.IsFrozen())
                    {
                        freezeEffect.Freeze(freezeDuration);
                        Debug.Log($"[IceBoss] 冰環擊中玩家，凍結 {freezeDuration} 秒");
                        break; // 凍結後跳出
                    }
                }
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // 移除冰環特效
        if (iceRing != null)
        {
            Destroy(iceRing, 0.5f);
        }

        Debug.Log("[IceBoss] 冰環結束");
    }

    /// <summary>
    /// 進入階段2
    /// </summary>
    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("[IceBoss] ⚠️ 進入階段2 - 憤怒模式！");

        // 改變顏色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = phase2Color;
        }

        // 播放音效
        if (phaseChangeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(phaseChangeSound);
        }

        // 提升移動速度（可選）
        jumpForceVertical *= 1.2f;
        jumpForceHorizontal *= 1.2f;
        jumpCooldown *= 0.8f;
    }

    /// <summary>
    /// 碰撞檢測 - 處理玩家子彈傷害
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        // 忽略玩家本體碰撞
        if (other.CompareTag("Player")) return;

        // 處理子彈傷害
        if ((other.CompareTag("MagicBullet") || other.CompareTag("HeavyBullet"))
            && !processedProjectiles.Contains(other.gameObject))
        {
            processedProjectiles.Add(other.gameObject);

            float damage = 0f;
            if (other.CompareTag("MagicBullet"))
            {
                damage = magicBulletDamage;
            }
            else if (other.CompareTag("HeavyBullet"))
            {
                damage = heavyBulletDamage;
            }

            if (damage > 0)
            {
                TakeDamage(damage, "PlayerBullet");
            }

            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// 受到傷害
    /// </summary>
    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (isDying) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // 更新血條
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        Debug.Log($"[IceBoss] 受到 {damage} 點傷害（來源：{source}），剩餘血量：{currentHealth}/{maxHealth}");

        // 受傷閃爍效果
        StartCoroutine(DamageFlash());

        // 死亡檢測
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 受傷閃爍效果
    /// </summary>
    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        if (!isDying)
        {
            spriteRenderer.color = original;
        }
    }

    /// <summary>
    /// Boss死亡
    /// </summary>
    void Die()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log("[IceBoss] Boss死亡！");

        // 停止所有協程
        StopAllCoroutines();

        // 移除血條
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        // 停止物理
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 禁用碰撞
        foreach (var collider in GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }

        // 2秒後銷毀
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 更新Boss朝向
    /// </summary>
    void UpdateOrientation()
    {
        if (playerTransform == null || spriteRenderer == null) return;

        float dirX = playerTransform.position.x - transform.position.x;
        spriteRenderer.flipX = dirX < 0;
    }

    /// <summary>
    /// 獲取當前血量百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// 外部接口 - 用於捕捉技能
    /// </summary>
    public void OnCaptured()
    {
        Debug.Log("[IceBoss] Boss被捕捉！");
        Die();
    }

    /// <summary>
    /// Debug繪製
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 繪製地面檢測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckRadius, groundCheckRadius);

        // 繪製冰環範圍
        if (isPhase2)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, iceRingRadius);
        }
    }
}