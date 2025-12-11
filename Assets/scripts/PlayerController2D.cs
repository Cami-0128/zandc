using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
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

    [Header("Health System 血量系統")]
    public int maxHealth = 100;
    private static int persistentHealth = -1;
    private static bool isFirstTimePlay = true;
    public int currentHealth;
    public int enemyDamage = 15;
    private float damageInvulnerabilityTime = 1f;
    private float lastDamageTime = -999f;

    [Header("Health Pickup System 血包系統")]
    public AudioClip healSound;
    private AudioSource audioSource;

    // === 魔力 ===
    public int maxMana = 100;
    public int currentMana = 100;

    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;

    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;

    public HourglassTimer hourglassTimer;
    public bool hasReachedEnd = false;

    public int LastHorizontalDirection { get; private set; } = 1;

    // ========== 按鍵綁定管理器 ==========
    private KeyBindingManager keyManager;  //自訂義按鍵功能未完成

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // ========== 初始化按鍵管理器 ==========
        keyManager = KeyBindingManager.Instance;
        if (keyManager == null)
        {
            Debug.LogWarning("[PlayerController2D] KeyBindingManager 未找到，使用傳統按鍵");
        }

        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);

        InitializeHealth();
        Time.timeScale = 1f;
        Debug.Log($"Game Start. 遊戲開始 - 血量: {currentHealth}/{maxHealth}");
        UpdateHealthUI();

        if (hourglassTimer == null)
        {
            hourglassTimer = GetComponentInChildren<HourglassTimer>();
            if (hourglassTimer == null)
            {
                Debug.LogWarning("找不到 HourglassTimer 組件！");
            }
        }
    }

    void InitializeHealth()
    {
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            currentMana = maxMana;
            isFirstTimePlay = false;
            Debug.Log("[血量系統] 第一次遊戲，血量設為滿血，魔力也設滿");
        }
        else
        {
            currentHealth = persistentHealth;
            Debug.Log($"[血量系統] 載入保存的血量: {currentHealth}/{maxHealth}");
        }
    }

    void SaveHealth()
    {
        persistentHealth = currentHealth;
        Debug.Log($"[血量系統] 血量已保存: {persistentHealth}");
    }

    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[血量系統] 血量系統已重置");
    }

    public void ManaHeal(int manaAmount)
    {
        if (isDead) return;
        int oldMana = currentMana;
        currentMana += manaAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        Debug.Log($"ManaHeal called: 回復 {manaAmount} 魔力，從 {oldMana} 到 {currentMana}");

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.RestoreMana(manaAmount);
            ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
            if (manaBar != null)
                manaBar.UpdateManaBar(attack.GetCurrentMana(), attack.maxMana);
            else
                Debug.LogWarning("找不到 ManaBarUI 組件");
        }
        else
        {
            Debug.LogWarning("找不到 PlayerAttack 組件");
        }
    }

    void Update()
    {
        if (!canControl) return;
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

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

    void OnDestroy()
    {
        if (!isDead && currentHealth > 0)
        {
            SaveHealth();
        }
    }

    // ========== 整合自定義按鍵移動 + 完整回退 ==========
    void Move()
    {
        float moveX = 0f;

        // 優先使用自定義按鍵
        if (keyManager != null)
        {
            if (keyManager.GetKeyPressed(KeyBindingManager.ActionType.MoveLeft))
            {
                moveX = -1f;
            }
            else if (keyManager.GetKeyPressed(KeyBindingManager.ActionType.MoveRight))
            {
                moveX = 1f;
            }
        }
        else
        {
            // 完整回退到傳統按鍵 (支援 WASD + 方向鍵)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                moveX = -1f;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                moveX = 1f;
            }
        }

        if (moveX != 0)
        {
            LastHorizontalDirection = (int)Mathf.Sign(moveX);
        }
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    // ========== 整合自定義按鍵跳躍 + 完整回退 ==========
    void HandleJumpInput()
    {
        Time.timeScale = 1f;

        bool jumpPressed = false;

        // 優先使用自定義按鍵
        if (keyManager != null)
        {
            jumpPressed = keyManager.GetKeyDown(KeyBindingManager.ActionType.Jump);
        }
        else
        {
            // 完整回退到傳統按鍵 (支援 W + Space + 上方向鍵)
            jumpPressed = Input.GetKeyDown(KeyCode.W) ||
                         Input.GetKeyDown(KeyCode.Space) ||
                         Input.GetKeyDown(KeyCode.UpArrow);
        }

        if (jumpPressed)
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

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }

    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;
        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }

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

    // ========== 修改：添加彈簧邏輯 ==========
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 檢查是否踩到彈簧
        Bouncer bouncer = collision.gameObject.GetComponent<Bouncer>();
        if (bouncer != null)
        {
            Debug.Log("[PlayerController2D] 踩到彈簧！");
            // 彈簧會自己處理彈跳邏輯，玩家這邊只需要記錄
            return;
        }

        // 原有的敵人碰撞邏輯
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

        // 地面碰撞邏輯
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