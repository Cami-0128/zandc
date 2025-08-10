using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    // 原有變數
    private bool isDead = false;
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

    // === 新增：血量系統 ===
    [Header("Health System 血量系統")]
    public int maxHealth = 100;              // 最大血量
    private int currentHealth;               // 當前血量
    public int enemyDamage = 15;             // Enemy造成的傷害
    private float damageInvulnerabilityTime = 1f; // 受傷無敵時間（防止連續扣血）
    private float lastDamageTime = -999f;    // 上次受傷時間

    // Wall Slide 相關（原有）
    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;

    // Wall Jump 相關（原有）
    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;

    // Wall Jump Hang Time 相關（原有）
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // === 新增：初始化血量 ===
        currentHealth = maxHealth; // 設置血量為滿血

        Time.timeScale = 1f;
        Debug.Log("Game Start. 遊戲開始");

        // === 新增：更新血條UI ===
        UpdateHealthUI();
    }

    void Update()
    {
        if (!canControl) return;

        // 原有的遊戲邏輯
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

        // 掉落死亡檢測（原有）
        if (transform.position.y < fall)
        {
            Fall();
        }
    }

    // 移動邏輯（原有）
    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    // 跳躍輸入處理（原有）
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

    // 跳躍（原有）
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }

    // 牆跳（原有）
    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;

        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }

    // 更新牆跳掛牆時間（原有）
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

    // 檢查牆滑（原有）
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

    // === 修改：碰撞檢測（加入血量系統） ===
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy1：直接死亡
        if (collision.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log("碰到Enemy1，直接死亡！");
            TakeDamage(currentHealth); // 造成等於當前血量的傷害，直接死亡
        }
        // Enemy：扣血
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // 檢查無敵時間，避免連續受傷
            if (Time.time - lastDamageTime >= damageInvulnerabilityTime)
            {
                Debug.Log("碰到Enemy，扣除" + enemyDamage + "點血量！");
                TakeDamage(enemyDamage);
            }
        }

        // 地面檢測（原有）
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }

    // 離開碰撞（原有）
    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // === 新增：受傷系統 ===
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 如果已經死亡，不再受傷

        currentHealth -= damage;              // 扣除血量
        currentHealth = Mathf.Max(currentHealth, 0); // 確保血量不會小於0
        lastDamageTime = Time.time;           // 記錄受傷時間

        UpdateHealthUI();                     // 更新血條UI

        Debug.Log($"玩家受到 {damage} 點傷害。當前血量: {currentHealth}/{maxHealth}");

        // 血量歸零時死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // === 新增：治療系統（可選功能） ===
    public void Heal(int healAmount)
    {
        if (isDead) return; // 如果已經死亡，無法治療

        currentHealth += healAmount;                          // 增加血量
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // 確保血量不會超過最大值

        UpdateHealthUI();                                     // 更新血條UI
        Debug.Log($"玩家回復 {healAmount} 點血量。當前血量: {currentHealth}/{maxHealth}");
    }

    // === 新增：更新血條UI ===
    void UpdateHealthUI()
    {
        // 尋找場景中的血條UI組件
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

    // === 新增：獲取血量資訊（供其他腳本使用） ===
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // === 修改：掉落死亡 ===
    void Fall()
    {
        if (isDead) return;

        Debug.Log("玩家掉落死亡！");

        // 掉落死亡時血條歸零
        currentHealth = 0;
        UpdateHealthUI();

        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === 新增：統一死亡處理 ===
    void Die()
    {
        if (isDead) return;

        isDead = true;
        canControl = false;

        Debug.Log("玩家死亡！");
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === 修改：暫停遊戲（使用統一死亡方法） ===
    public void PauseGame()
    {
        Die();
    }
}