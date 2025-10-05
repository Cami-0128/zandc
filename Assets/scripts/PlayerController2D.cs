using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController2D : MonoBehaviour
{
    // 
    public bool isDead { get; private set; } = false;  // 改成公開只讀屬性，供外部檢查
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
    public int maxHealth = 100;              // 最大血量
    // === 靜態血量變數（跨場景保持） ===
    private static int persistentHealth = -1; // -1 表示尚未初始化
    private static bool isFirstTimePlay = true; // 是否第一次遊戲
    public int currentHealth;               // 當前血量
    public int enemyDamage = 15;             // Enemy造成的傷害
    private float damageInvulnerabilityTime = 1f; // 受傷無敵時間（防止連續扣血）
    private float lastDamageTime = -999f;    // 上次受傷時間
    // === 血包系統相關 ===
    [Header("Health Pickup System 血包系統")]
    public AudioClip healSound;              // 治療音效
    private AudioSource audioSource;
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
    // 攻擊發射方向
    public int LastHorizontalDirection { get; private set; } = 1; // 初始預設向右

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // === 新增：音效系統初始化 ===
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
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
        // 如果是第一次遊戲，設置滿血
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            isFirstTimePlay = false;
            Debug.Log("[血量系統] 第一次遊戲，血量設為滿血");
        }
        // 否則使用保存的血量
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
    // === 重置血量系統（用於重新開始遊戲） ===
    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[血量系統] 血量系統已重置");
    }
    void Update()
    {
        if (!canControl) return;
        //// 以下示範用空白鍵啟動沙漏計時（可改成對應遊戲事件觸發）
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    HourglassTimer hourglassTimer = GetComponentInChildren<HourglassTimer>();
        //    if (hourglassTimer != null)
        //    {
        //        hourglassTimer.StartTimer();
        //    }
        //}
        // === 測試按鍵 ===
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Debug.Log($"[測試] 當前血量: {currentHealth}/{maxHealth}, 保存血量: {persistentHealth}");
        //}
        //// === 測試受傷按鍵 ===
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    TakeDamage(10);
        //    Debug.Log("[測試] 玩家受到 10 點測試傷害");
        //}
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
        // 地面檢測
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }
    // 離開碰撞
    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
    // === 修改：受傷系統 ===
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 如果已經死亡，不再受傷
        currentHealth -= damage;              // 扣除血量
        currentHealth = Mathf.Max(currentHealth, 0); // 確保血量不會小於0
        lastDamageTime = Time.time;           // 記錄受傷時間
        // === 新增：即時保存血量 ===
        SaveHealth();
        UpdateHealthUI();                     // 更新血條UI
        Debug.Log($"玩家受到 {damage} 點傷害。當前血量: {currentHealth}/{maxHealth}");
        // 血量歸零時死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // === 修改：治療系統（增強版） ===
    public void Heal(int healAmount)
    {
        Debug.Log($"Heal函數被呼叫，healAmount = {healAmount}");
        if (isDead) return; // 如果已經死亡，無法治療

        int oldHealth = currentHealth;
        currentHealth += healAmount;                          // 增加血量
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // 確保血量不會超過最大值

        if (healSound != null && audioSource != null)
            audioSource.PlayOneShot(healSound);

        SaveHealth();
        UpdateHealthUI();

        Debug.Log($"回血 {currentHealth - oldHealth} 點，目前血量: {currentHealth}/{maxHealth}");

        StartCoroutine(HealFeedback());
    }
    // === 新增：治療反饋效果協程 ===
    IEnumerator HealFeedback()
    {
        // 可以在這裡添加視覺反饋，比如改變玩家顏色
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.green; // 短暫變綠表示治療
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }
    // === 更新血條UI ===
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
    // === 獲取血量資訊 ===
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    // === 新增：檢查是否可以治療 ===
    public bool CanHeal()
    {
        return !isDead && currentHealth < maxHealth;
    }
    // === 新增：獲取血量百分比 ===
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    // === 修改：掉落死亡 ===
    void Fall()
    {
        if (isDead) return;
        Debug.Log("玩家掉落死亡！");
        // 掉落死亡時血條歸零
        currentHealth = 0;
        SaveHealth(); // 保存死亡狀態
        UpdateHealthUI();
        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }
    // === 統一死亡處理 ===
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        canControl = false;
        // === 新增：死亡時保存血量狀態 ===
        SaveHealth();
        Debug.Log("玩家死亡！");
        FindObjectOfType<GameManager>().PlayerDied();
    }
    // === 暫停遊戲 ===
    public void PauseGame()
    {
        Die();
    }
    // === 新增：公開方法供其他腳本使用 ===
    /// <summary>
    /// 手動保存當前血量
    /// </summary>
    public void SaveCurrentHealth()
    {
        SaveHealth();
    }
    /// <summary>
    /// 獲取保存的血量
    /// </summary>
    public static int GetPersistentHealth()
    {
        return persistentHealth;
    }
    /// <summary>
    /// 設置血量為滿血（用於新遊戲）
    /// </summary>
    public void SetFullHealth()
    {
        currentHealth = maxHealth;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log("[血量系統] 血量設為滿血");
    }
    /// <summary>
    /// 設置特定血量值
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"[血量系統] 血量設為: {currentHealth}/{maxHealth}");
    }
}