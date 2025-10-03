using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 敵人系統 - 修正版
/// 防止重複扣血和血條跟隨問題
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("敵人屬性")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("血條設定")]
    [Tooltip("血條腳本（從子物件自動獲取）")]
    public EnemyHealthBar healthBar;

    [Tooltip("血條位置偏移")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

    [Header("移動設定")]
    public float jumpForce = 8f;
    public float jumpHeight = 5f;
    public float jumpInterval = 2f;

    [Header("追蹤玩家設定")]
    public bool enablePlayerTracking = true;

    [Header("外觀設定")]
    public Color enemyColor = Color.red;

    [Header("死亡特效")]
    public int fragmentCount = 15;
    public float fragmentSpeed = 5f;
    public GameObject fragmentPrefab;

    [Header("傷害設定")]
    [Tooltip("接觸玩家時造成的傷害")]
    public float contactDamage = 10f;
    [Tooltip("接觸傷害冷卻時間")]
    public float contactDamageCooldown = 1f;
    private float lastContactDamageTime = -999f;

    [Header("Debug 設定")]
    public bool debugMode = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform playerTransform;
    private bool isJumping = false;
    private bool isDying = false;

    // 防止重複傷害的機制
    private HashSet<GameObject> processedBullets = new HashSet<GameObject>();
    private float lastDamageTime = -999f;
    private float damageCooldown = 0.1f;  // 傷害冷卻時間

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        currentHealth = maxHealth;

        SetupAppearance();

        // 從子物件獲取血條（不動態創建）
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<EnemyHealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);

            if (debugMode)
            {
                Debug.Log($"[Enemy] {gameObject.name} 找到血條組件");
            }
        }
        else
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} 找不到 EnemyHealthBar 組件！請在子物件中添加");
        }

        StartCoroutine(JumpRoutine());

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 初始化完成，血量: {currentHealth}/{maxHealth}");
        }
    }

    void SetupAppearance()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;

        if (trailRenderer == null)
            trailRenderer = gameObject.AddComponent<TrailRenderer>();

        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailMaterial.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, 0.5f);
        trailRenderer.material = trailMaterial;
        trailRenderer.startWidth = 0.4f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.time = 0.3f;
        trailRenderer.sortingOrder = -1;

        GradientColorKey[] colorKeys = new GradientColorKey[2]
        {
            new GradientColorKey(enemyColor, 0f),
            new GradientColorKey(enemyColor, 1f)
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(0.8f, 0f),
            new GradientAlphaKey(0f, 1f)
        };

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        trailRenderer.colorGradient = gradient;
    }

    /// <summary>
    /// 動態創建血條（作為Enemy的子物件）
    /// </summary>
    void CreateHealthBar()
    {
        // 創建血條物件
        //healthBarObject = new GameObject($"HealthBar");

        // 添加 EnemyHealthBar 組件
        //healthBar = healthBarObject.AddComponent<EnemyHealthBar>();

        // 設定血條參數
        //healthBar.offset = healthBarOffset;

        // 初始化血條（會自動設為子物件）
        healthBar.Initialize(this.transform);
        healthBar.UpdateHealthBar(currentHealth, maxHealth);

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 血條已創建為子物件");
        }
    }

    IEnumerator JumpRoutine()
    {
        PerformJump();

        while (currentHealth > 0 && !isDying)
        {
            yield return new WaitForSeconds(jumpInterval);
            if (!isJumping && !isDying)
                PerformJump();
        }
    }

    void PerformJump()
    {
        if (rb == null || isDying) return;

        isJumping = true;

        Vector2 jumpDirection;
        if (enablePlayerTracking && playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            jumpDirection = new Vector2(dir.x, jumpHeight).normalized;
        }
        else
        {
            jumpDirection = new Vector2(0, jumpHeight).normalized;
        }

        rb.velocity = Vector2.zero;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        StartCoroutine(JumpEndCheck());
    }

    IEnumerator JumpEndCheck()
    {
        yield return new WaitForSeconds(0.1f);
        while (rb.velocity.y > 0.1f || !IsGrounded())
            yield return new WaitForFixedUpdate();

        isJumping = false;
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void Update()
    {
        if (isDying) return;

        if (transform.position.y < -15f)
        {
            Die();
        }
    }

    // ============================================
    // 核心：接收傷害系統（修正版 - 防止重複觸發）
    // ============================================

    /// <summary>
    /// 敵人受到傷害（帶冷卻機制）
    /// </summary>
    public void TakeDamage(float damage, string damageSource = "Unknown")
    {
        if (isDying) return;

        // 防止短時間內重複扣血
        if (Time.time - lastDamageTime < damageCooldown)
        {
            if (debugMode)
            {
                Debug.Log($"[Enemy] {gameObject.name} 傷害冷卻中，忽略此次傷害");
            }
            return;
        }

        lastDamageTime = Time.time;

        // 扣血
        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 受到 {damage} 點傷害 (來源: {damageSource})");
            Debug.Log($"[Enemy] 血量變化: {oldHealth} → {currentHealth} / {maxHealth}");
        }

        // 更新血條
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} 血條不存在！");
        }

        // 檢查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashEffect());
        }
    }

    /// <summary>
    /// 受傷閃爍效果
    /// </summary>
    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Die()
    {
        if (isDying) return;

        isDying = true;

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 死亡！");
        }

        StopAllCoroutines();

        // 血條會自動隨著 Enemy 銷毀（因為是子物件）

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        StartCoroutine(FragmentationEffect());
    }

    IEnumerator FragmentationEffect()
    {
        Vector3 centerPosition = transform.position;

        spriteRenderer.enabled = false;
        if (trailRenderer != null) trailRenderer.enabled = false;

        for (int i = 0; i < fragmentCount; i++)
        {
            CreateFragment(centerPosition, i);
        }

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    void CreateFragment(Vector3 center, int fragmentIndex)
    {
        GameObject fragment;

        if (fragmentPrefab != null)
            fragment = Instantiate(fragmentPrefab, center, Quaternion.identity);
        else
        {
            fragment = new GameObject($"Fragment_{fragmentIndex}");
            fragment.transform.position = center;

            SpriteRenderer fragmentSprite = fragment.AddComponent<SpriteRenderer>();
            fragmentSprite.sprite = spriteRenderer.sprite;
            fragmentSprite.color = enemyColor;

            float scale = Random.Range(0.1f, 0.3f);
            fragment.transform.localScale = Vector3.one * scale;
        }

        Rigidbody2D fragmentRb = fragment.GetComponent<Rigidbody2D>();
        if (fragmentRb == null)
            fragmentRb = fragment.AddComponent<Rigidbody2D>();

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomSpeed = Random.Range(fragmentSpeed * 0.5f, fragmentSpeed * 1.5f);

        fragmentRb.AddForce(randomDirection * randomSpeed, ForceMode2D.Impulse);
        fragmentRb.AddTorque(Random.Range(-10f, 10f));

        StartCoroutine(FadeOutFragment(fragment));
    }

    IEnumerator FadeOutFragment(GameObject fragment)
    {
        SpriteRenderer fragmentSprite = fragment.GetComponent<SpriteRenderer>();
        if (fragmentSprite == null) yield break;

        Color originalColor = fragmentSprite.color;
        float fadeTime = 2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            Color newColor = originalColor;
            newColor.a = alpha;
            fragmentSprite.color = newColor;

            yield return null;
        }

        Destroy(fragment);
    }

    // ============================================
    // 碰撞檢測（修正版 - 防止重複觸發）
    // ============================================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 觸發碰撞: {other.gameObject.name} (Tag: {other.tag})");
        }

        // === 1. 與玩家接觸造成傷害 ===
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastContactDamageTime >= contactDamageCooldown)
            {
                PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
                if (playerAttack != null)
                {
                    playerAttack.TakeDamage(contactDamage);
                    lastContactDamageTime = Time.time;

                    if (debugMode)
                    {
                        Debug.Log($"[Enemy] 對玩家造成 {contactDamage} 點接觸傷害");
                    }
                }
            }
            return;
        }

        // === 2. 魔法子彈傷害（防止重複處理）===
        MagicBullet bullet = other.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            // 檢查是否已經處理過這個子彈
            if (processedBullets.Contains(other.gameObject))
            {
                if (debugMode)
                {
                    Debug.Log($"[Enemy] 子彈已處理過，忽略: {other.gameObject.name}");
                }
                return;
            }

            // 標記為已處理
            processedBullets.Add(other.gameObject);

            // 造成傷害
            TakeDamage(bullet.damage, "MagicBullet");

            // 銷毀子彈
            Destroy(other.gameObject);
            return;
        }

        // === 3. 與其他敵人碰撞的傷害 ===
        if (other.CompareTag("Enemy") || other.CompareTag("Enemy1"))
        {
            TakeDamage(30f, "EnemyCollision");
            return;
        }
    }

    void OnDestroy()
    {
        // 血條會自動隨著 Enemy 銷毀
        processedBullets.Clear();
    }

    // ============================================
    // 公開方法供外部調用
    // ============================================

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = GetHealthPercentage();
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthPercentage;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void Heal(float healAmount)
    {
        if (isDying) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 回復 {healAmount} 血量，當前: {currentHealth}/{maxHealth}");
        }
    }

    public void ShowHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Show();
        }
    }

    public void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Hide();
        }
    }
}