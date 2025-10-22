using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制器 - 簡化版（移除揮劍相關）
/// 只負責移動、跳躍、血量系統
/// </summary>
public class PlayerController2D : MonoBehaviour
{
    //
    public bool isDead { get; private set; } = false;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public int maxJumps = 2;
    public float fall = -15f;
    public int Methodofdeath = 0;
    public GameObject deathUI;
    private Rigidbody2D rb;
    private int jumpCount;
    private bool isGrounded;
    public bool canControl = true;

    // === 跨關卡血量系統 ===
    [Header("Health System 血量系統")]
    public int maxHealth = 100;
    private static int persistentHealth = -1;
    private static bool isFirstTimePlay = true;
    public int currentHealth;
    public int enemyDamage = 15;
    private float damageInvulnerabilityTime = 1f;
    private float lastDamageTime = -999f;

    // === 血包系統相關 ===
    [Header("Health Pickup System 血包系統")]
    public AudioClip healSound;
    private AudioSource audioSource;

    // === 魔力 ===
    public int maxMana = 100;
    public int currentMana = 100;

    // Wall Slide 相關
    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;

    // Wall Jump 相關
    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;

    // Wall Jump Hang Time 相關
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;

    // 沙漏倒數計時器參考
    public HourglassTimer hourglassTimer;
    public bool hasReachedEnd = false;

    // 攻擊發射方向（供技能系統使用）
    public int LastHorizontalDirection { get; private set; } = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // === 音效系統初始化 ===
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // === MP魔力條 ===
        currentMana = maxMana;
        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);

        // === 血量初始化邏輯 ===
        InitializeHealth();
        Time.timeScale = 1f;
        Debug.Log($"Game Start. 遊戲開始 - 血量: {currentHealth}/{maxHealth}");

        // === 更新血條UI ===
        UpdateHealthUI();

        // 自動尋找hourglassTimer
        if (hourglassTimer == null)
        {
            hourglassTimer = GetComponentInChildren<HourglassTimer>();
            if (hourglassTimer == null)
            {
                Debug.LogWarning("找不到 HourglassTimer 組件！");
            }
        }
    }

    // === 血量初始化方法 ===
    void InitializeHealth()
    {
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            isFirstTimePlay = false;
            Debug.Log("[血量系統] 第一次遊戲，血量設為滿血");
        }
        else
        {
            currentHealth = persistentHealth;
            Debug.Log($"[血量系統] 載入保存的血量: {currentHealth}/{maxHealth}");
        }
    }

    // === 保存血量方法 ===
    void SaveHealth()
    {
        persistentHealth = currentHealth;
        Debug.Log($"[血量系統] 血量已保存: {persistentHealth}");
    }

    // === 重置血量系統 ===
    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[血量系統] 血量系統已重置");
    }

    public void ManaHeal(int manaAmount)
    {
        if (isDead) return;
        currentMana += manaAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        Debug.Log($"魔力回復了 {manaAmount} 點，現在魔力: {currentMana}/{maxMana}");

        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);
    }

    void Update()
    {
        if (!canControl) return;

        // 原有的遊戲邏輯
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

        // 掉落死亡檢測
        if (transform.position.y < fall)
        {
            Fall();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EndPoint"))
        {
            hasReachedEnd = true;
            canControl = false;
            rb.velocity = Vector2.zero;
            Debug.Log("玩家已抵達終點！");
        }
    }

    // === 場景切換時保存血量 ===
    void OnDestroy()
    {
        if (!isDead && currentHealth > 0)
        {
            SaveHealth();
        }
    }

    // 移動邏輯
    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0)
        {
            LastHorizontalDirection = (int)Mathf.Sign(moveX);
        }
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    // 跳躍輸入處理
    void HandleJumpInput()
    {
        Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTouchingWall && !isGrounded && Time.time - lastWallJumpTime >= wallJumpCooldown)
            {
                WallJump();
            }
            else if (jumpCount < maxJumps)
            {
                Jump();
            }
        }
    }

    // 跳躍
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }

    // 牆跳
    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;
        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }

    // 更新牆跳掛牆時間
    void UpdateWallJumpHangTime()
    {
        if (wallJumping)
        {
            wallJumpHangCounter -= Time.deltaTime;
            if (wallJumpHangCounter <= 0f)
            {
                wallJumping = false;
            }
        }
    }

    // 檢查牆滑
    void CheckWallSliding()
    {
        Vector2 checkLeft = Vector2.left;
        Vector2 checkRight = Vector2.right;
        bool wallLeft = Physics2D.Raycast(wallCheck.position, checkLeft, wallCheckDistance, wallLayer);
        bool wallRight = Physics2D.Raycast(wallCheck.position, checkRight, wallCheckDistance, wallLayer);
        isTouchingWall = wallLeft || wallRight;
        if (wallJumping)
        {
            isWallSliding = false;
            return;
        }
        if (isTouchingWall && !isGrounded)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
        if (isWallSliding && rb.velocity.y < -wallSlideSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
        Debug.DrawRay(wallCheck.position, checkLeft * wallCheckDistance, Color.red);
        Debug.DrawRay(wallCheck.position, checkRight * wallCheckDistance, Color.red);
    }

    // === 碰撞檢測 ===
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log("碰到Enemy1，直接死亡！");
            TakeDamage(currentHealth);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (Time.time - lastDamageTime >= damageInvulnerabilityTime)
            {
                Debug.Log("碰到Enemy，扣除" + enemyDamage + "點血量！");
                TakeDamage(enemyDamage);
            }
        }
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        lastDamageTime = Time.time;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"玩家受到 {damage} 點傷害。當前血量: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        Debug.Log($"Heal函數被呼叫，healAmount = {healAmount}");
        if (isDead) return;
        int oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (healSound != null && audioSource != null)
            audioSource.PlayOneShot(healSound);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"回血 {currentHealth - oldHealth} 點，目前血量: {currentHealth}/{maxHealth}");
        StartCoroutine(HealFeedback());
    }

    IEnumerator HealFeedback()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }

    void UpdateHealthUI()
    {
        HealthBarUI healthBar = FindObjectOfType<HealthBarUI>();
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("找不到HealthBarUI組件！請確保場景中有血條UI。");
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool CanHeal() => !isDead && currentHealth < maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;

    void Fall()
    {
        if (isDead) return;
        Debug.Log("玩家掉落死亡！");
        currentHealth = 0;
        SaveHealth();
        UpdateHealthUI();
        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        canControl = false;
        SaveHealth();
        Debug.Log("玩家死亡！");
        FindObjectOfType<GameManager>().PlayerDied();
    }

    public void PauseGame()
    {
        Die();
    }

    public void SaveCurrentHealth()
    {
        SaveHealth();
    }

    public static int GetPersistentHealth()
    {
        return persistentHealth;
    }

    public void SetFullHealth()
    {
        currentHealth = maxHealth;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log("[血量系統] 血量設為滿血");
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"[血量系統] 血量設為: {currentHealth}/{maxHealth}");
    }
}