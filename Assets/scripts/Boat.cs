using UnityEngine;
using System.Collections;

public class Boat : MonoBehaviour
{
    [Header("═══ 船隻基礎設定 ═══")]
    public float boatSpeed = 3f;
    public float boatWidth = 2f;
    public bool canBeRidden = true;

    [Header("═══ 物理設定 ═══")]
    public float buoyancyMultiplier = 1.5f;
    public float dragMultiplier = 0.5f;
    public float maxTilt = 15f;

    [Header("═══ 搭乘設定 ═══")]
    public float playerGetOnDistance = 1f;
    public KeyCode getOnKey = KeyCode.E;  // 新增：上船按鍵

    [Header("═══ 船隻受損系統 ═══")]
    public int maxBoatHealth = 100;
    public int currentBoatHealth;
    public float sinkStartHealth = 0.2f;
    public float sinkSpeed = 0.5f;

    [Header("═══ 船隻修復系統 ═══")]
    public bool enableManaRepair = true;
    public int manaRepairCost = 10;
    public int manaRepairAmount = 5;
    public float repairCooldown = 1f;
    public KeyCode repairKey = KeyCode.B;
    private float lastRepairTime = -999f;

    [Header("═══ 自動修復系統 ═══")]
    public bool enableAutoRepair = true;
    public float autoRepairInterval = 5f;
    private float lastAutoRepairTime = 0f;

    [Header("═══ 血條系統 ═══")]
    public EnemyHealthBar healthBar;

    [Header("═══ 視覺效果 ═══")]
    public bool enableBoatTilt = true;
    public float tiltSmoothness = 5f;
    public Color damagedColor = new Color(0.8f, 0.4f, 0.4f, 1f);
    public Color repairingColor = new Color(0.4f, 0.8f, 0.4f, 1f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerController2D playerOnBoard = null;
    private Rigidbody2D playerRbOnBoard = null;
    private ImprovedWaveSystem waveSystem;

    private float originalBoatMass;
    private Vector3 originalLocalPosition;
    private bool isFollowingWave = true;
    private bool isSinking = false;
    private Color originalColor;
    private Vector3 playerOriginalOffset = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("[Boat] 需要 Rigidbody2D 組件");
            return;
        }

        originalBoatMass = rb.mass;
        originalLocalPosition = transform.localPosition;
        currentBoatHealth = maxBoatHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        waveSystem = FindObjectOfType<ImprovedWaveSystem>();
        if (waveSystem == null)
        {
            Debug.LogWarning("[Boat] 未找到 ImprovedWaveSystem");
            isFollowingWave = false;
        }

        // 尋找或創建血條
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<EnemyHealthBar>();
        }

        if (healthBar == null)
        {
            CreateHealthBar();
        }
        else
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
            Debug.Log("[Boat] 血條已初始化");
        }

        lastAutoRepairTime = autoRepairInterval;

        Debug.Log("[Boat] 初始化完成 - 血量: " + currentBoatHealth);
    }

    void CreateHealthBar()
    {
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = new Vector3(0, 2f, 0);
        healthBarObj.transform.localScale = Vector3.one;

        healthBar = healthBarObj.AddComponent<EnemyHealthBar>();
        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
            Debug.Log("[Boat] 血條已自動創建");
        }
    }

    void FixedUpdate()
    {
        if (!isFollowingWave || waveSystem == null) return;

        UpdateBoatFloating();

        if (enableBoatTilt)
        {
            UpdateBoatTilt();
        }

        CheckSinking();

        if (enableAutoRepair)
        {
            UpdateAutoRepair();
        }

        // ✅ 修復：每幀都鎖定玩家
        if (playerOnBoard != null && playerRbOnBoard != null)
        {
            LockPlayerOnBoat();
        }
    }

    void Update()
    {
        if (playerOnBoard != null)
        {
            CheckPlayerJump();
            CheckRepairInput();
        }
        else
        {
            // ✅ 修復：檢查靠近的玩家是否按了上船鍵
            CheckPlayerNearby();
        }
    }

    // ========== 檢查靠近的玩家 ==========
    void CheckPlayerNearby()
    {
        PlayerController2D nearbyPlayer = FindObjectOfType<PlayerController2D>();
        if (nearbyPlayer == null) return;

        float distance = Vector2.Distance(transform.position, nearbyPlayer.transform.position);

        if (distance <= playerGetOnDistance)
        {
            // 玩家靠近船隻範圍內
            if (Input.GetKeyDown(getOnKey))
            {
                Debug.Log($"[Boat] 玩家按下 {getOnKey} 鍵，嘗試上船");
                PlayerGetOn(nearbyPlayer);
            }
        }
    }

    // ========== 船隻浮動邏輯 ==========
    void UpdateBoatFloating()
    {
        if (waveSystem == null) return;

        float centerWaveHeight = waveSystem.GetWaveHeightAtPosition(transform.position.x);

        Vector3 newPosition = transform.position;
        newPosition.y = originalLocalPosition.y + centerWaveHeight;
        transform.position = newPosition;

        float waveVelocity = waveSystem.GetWaveVelocityAtPosition(transform.position.x);
        rb.velocity = new Vector2(rb.velocity.x * boatSpeed, waveVelocity * buoyancyMultiplier);
    }

    // ========== 船隻傾斜邏輯 ==========
    void UpdateBoatTilt()
    {
        float leftWaveHeight = waveSystem.GetWaveHeightAtPosition(transform.position.x - boatWidth * 0.5f);
        float rightWaveHeight = waveSystem.GetWaveHeightAtPosition(transform.position.x + boatWidth * 0.5f);

        float heightDifference = rightWaveHeight - leftWaveHeight;
        float targetTilt = Mathf.Clamp(heightDifference * 45f, -maxTilt, maxTilt);

        float currentTilt = transform.eulerAngles.z;
        if (currentTilt > 180f) currentTilt -= 360f;

        float smoothTilt = Mathf.Lerp(currentTilt, targetTilt, Time.fixedDeltaTime * tiltSmoothness);
        transform.rotation = Quaternion.Euler(0, 0, smoothTilt);
    }

    // ========== 玩家固定在船上 ==========
    void LockPlayerOnBoat()
    {
        if (playerOnBoard == null || playerRbOnBoard == null) return;

        // 1️⃣ 禁用玩家控制
        playerOnBoard.canControl = false;

        // 2️⃣ 玩家位置固定在船的中心
        Vector3 targetPos = transform.position;
        targetPos.z = playerOnBoard.transform.position.z;
        playerOnBoard.transform.position = targetPos;

        // 3️⃣ 玩家速度設為零
        playerRbOnBoard.velocity = Vector2.zero;

        // 4️⃣ 玩家 Rigidbody 設為 Kinematic
        playerRbOnBoard.isKinematic = true;

        // Debug 日誌
        // Debug.Log($"[Boat] 玩家被鎖定在船上");
    }

    // ========== 船隻受損系統 ==========
    public void TakeDamage(int damage)
    {
        if (isSinking) return;

        currentBoatHealth -= damage;
        currentBoatHealth = Mathf.Max(currentBoatHealth, 0);

        Debug.Log($"[Boat] 🔥 船受到 {damage} 點傷害。當前血量: {currentBoatHealth}/{maxBoatHealth}");

        WaterSplashEffect splashEffect = FindObjectOfType<WaterSplashEffect>();
        if (splashEffect != null)
        {
            splashEffect.CreateBoatDamageSplash(transform.position);
        }

        UpdateBoatAppearance();

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
        }

        if (currentBoatHealth <= 0)
        {
            StartSinking();
        }
    }

    void UpdateBoatAppearance()
    {
        if (spriteRenderer == null) return;

        float healthRatio = (float)currentBoatHealth / maxBoatHealth;
        Color lerpColor = Color.Lerp(damagedColor, originalColor, healthRatio);
        spriteRenderer.color = lerpColor;
    }

    void CheckSinking()
    {
        if (isSinking && currentBoatHealth > 0)
        {
            Vector3 newPos = transform.position;
            newPos.y -= sinkSpeed * Time.fixedDeltaTime;
            transform.position = newPos;
        }
    }

    void StartSinking()
    {
        isSinking = true;
        Debug.Log("[Boat] ⛵ 船隻開始沉沒！");

        if (playerOnBoard != null)
        {
            PlayerGetOff();
        }
    }

    // ========== 船隻修復系統 ==========
    void CheckRepairInput()
    {
        bool repairPressed = Input.GetKeyDown(repairKey);

        if (repairPressed)
        {
            AttemptRepairBoat();
        }
    }

    void AttemptRepairBoat()
    {
        if (!enableManaRepair || isSinking) return;
        if (currentBoatHealth >= maxBoatHealth) return;
        if (Time.time - lastRepairTime < repairCooldown) return;

        PlayerAttack playerAttack = playerOnBoard.GetComponent<PlayerAttack>();
        if (playerAttack == null) return;

        if (playerAttack.GetCurrentMana() < manaRepairCost)
        {
            Debug.Log("[Boat] 魔力不足，無法修復！");
            return;
        }

        playerAttack.ConsumeMana(manaRepairCost);
        RepairBoat(manaRepairAmount);
    }

    public void RepairBoat(int repairAmount)
    {
        if (isSinking) return;

        currentBoatHealth += repairAmount;
        currentBoatHealth = Mathf.Min(currentBoatHealth, maxBoatHealth);
        lastRepairTime = Time.time;

        Debug.Log($"[Boat] 🔧 船被修復 {repairAmount} 點。當前血量: {currentBoatHealth}/{maxBoatHealth}");

        StartCoroutine(RepairFlashEffect());
        UpdateBoatAppearance();

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
        }
    }

    IEnumerator RepairFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalSpriteColor = spriteRenderer.color;
        spriteRenderer.color = repairingColor;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = originalSpriteColor;
    }

    // ========== 自動修復系統 ==========
    void UpdateAutoRepair()
    {
        if (currentBoatHealth >= maxBoatHealth) return;

        lastAutoRepairTime -= Time.fixedDeltaTime;

        if (lastAutoRepairTime <= 0)
        {
            currentBoatHealth = Mathf.Min(currentBoatHealth + 1, maxBoatHealth);
            lastAutoRepairTime = autoRepairInterval;

            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
            }

            Debug.Log($"[Boat] ⚡ 船隻自動修復 1 點。當前血量: {currentBoatHealth}/{maxBoatHealth}");
        }
    }

    // ========== 玩家跳躍下船 ==========
    void CheckPlayerJump()
    {
        if (playerOnBoard == null) return;

        bool jumpPressed = false;
        KeyBindingManager keyManager = KeyBindingManager.Instance;

        if (keyManager != null)
        {
            jumpPressed = keyManager.GetKeyDown(KeyBindingManager.ActionType.Jump);
        }
        else
        {
            jumpPressed = Input.GetKeyDown(KeyCode.W) ||
                         Input.GetKeyDown(KeyCode.Space) ||
                         Input.GetKeyDown(KeyCode.UpArrow);
        }

        if (jumpPressed)
        {
            PlayerGetOff();
        }
    }

    // ========== 玩家上/下船邏輯 ==========
    public bool CanPlayerGetOn(PlayerController2D player)
    {
        if (!canBeRidden || playerOnBoard != null || isSinking) return false;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= playerGetOnDistance;
    }

    public void PlayerGetOn(PlayerController2D player)
    {
        if (playerOnBoard != null) return;

        playerOnBoard = player;
        playerRbOnBoard = player.GetComponent<Rigidbody2D>();

        Debug.Log($"[Boat] ⛵ 玩家已上船，現在固定玩家");

        // 立即鎖定玩家
        LockPlayerOnBoat();
    }

    public void PlayerGetOff()
    {
        if (playerOnBoard == null) return;

        Debug.Log("[Boat] 🏊 玩家正在下船");

        if (playerRbOnBoard != null)
        {
            // 恢復 Rigidbody 為 Dynamic
            playerRbOnBoard.isKinematic = false;

            // 給玩家一個向上的速度以脫離船隻
            playerRbOnBoard.velocity = new Vector2(playerRbOnBoard.velocity.x, 5f);

            Debug.Log($"[Boat] 恢復玩家 Rigidbody");
        }

        if (playerOnBoard != null)
        {
            playerOnBoard.canControl = true;
        }

        Debug.Log("[Boat] 玩家已下船");
        playerOnBoard = null;
        playerRbOnBoard = null;
    }

    public PlayerController2D GetPlayerOnBoard() => playerOnBoard;
    public bool HasPlayerOnBoard() => playerOnBoard != null;

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null && CanPlayerGetOn(player))
            {
                // 可添加上船提示
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerGetOnDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - boatWidth * 0.5f, transform.position.y, 0),
            new Vector3(transform.position.x + boatWidth * 0.5f, transform.position.y, 0)
        );
    }

    public int GetBoatHealth() => currentBoatHealth;
    public int GetMaxBoatHealth() => maxBoatHealth;
    public float GetBoatHealthPercentage() => (float)currentBoatHealth / maxBoatHealth;
    public bool IsSinking() => isSinking;
}