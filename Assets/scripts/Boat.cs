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
    public KeyCode getOnKey = KeyCode.E;

    [Header("═══ 船隻受損系統 ═══")]
    public int maxBoatHealth = 100;
    public int currentBoatHealth;
    public float sinkStartHealth = 0.2f;
    public float sinkSpeed = 3f; // ✅ 修正：加快沉沒速度

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
    private float autoRepairTimer = 0f;

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
    private float sinkStartTime = -999f;

    private float boatMoveX = 0f;

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

        autoRepairTimer = 0f;

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

    void Update()
    {
        // ✅ 修復：沉沒時不執行任何其他邏輯
        if (isSinking)
        {
            return;
        }

        if (playerOnBoard != null)
        {
            CheckPlayerJump();
            CheckRepairInput();
            CheckBoatMovementInput();
        }
        else
        {
            CheckPlayerNearby();
            boatMoveX = 0f;
        }
    }

    void FixedUpdate()
    {
        // ✅ 沉沒時完全禁用所有其他邏輯
        if (isSinking)
        {
            HandleSinking();
            return; // ⭐ 必須立即返回，不執行任何其他邏輯
        }

        if (!isFollowingWave || waveSystem == null)
            return;

        UpdateBoatFloating();

        if (enableBoatTilt)
        {
            UpdateBoatTilt();
        }

        if (enableAutoRepair)
        {
            UpdateAutoRepair();
        }

        if (playerOnBoard != null && playerRbOnBoard != null)
        {
            LockPlayerOnBoat();
        }
    }

    // ✅ 沉沒處理（在 FixedUpdate 中執行）
    void HandleSinking()
    {
        // 第一次進入沉沒狀態時初始化
        if (sinkStartTime < 0)
        {
            sinkStartTime = Time.time;
            Debug.Log("[Boat] ⛵ 船隻開始沉沒！");

            // ✅ 禁用波浪跟隨
            isFollowingWave = false;

            // ✅ 完全禁用 Rigidbody2D 的物理影響
            if (rb != null)
            {
                rb.isKinematic = true; // ⭐ 改為 true，禁用所有物理影響
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
                Debug.Log("[Boat] ✓ Rigidbody 已配置為純運動學模式");
            }

            // 強制玩家下船
            if (playerOnBoard != null)
            {
                Debug.Log("[Boat] ✓ 玩家強制下船");
                PlayerGetOff();
            }
        }

        // ✅ 每幀直接修改位置，不受任何外力影響
        Vector3 newPos = transform.position;
        newPos.y -= sinkSpeed * Time.fixedDeltaTime;
        transform.position = newPos;

        // 每30幀列印日誌，追蹤沉沒進度
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[Boat] ⬇️ 沉沒中... Y: {transform.position.y:F2}，時間: {Time.time - sinkStartTime:F2}秒");
        }
    }

    void CheckPlayerNearby()
    {
        PlayerController2D nearbyPlayer = FindObjectOfType<PlayerController2D>();
        if (nearbyPlayer == null) return;

        float distance = Vector2.Distance(transform.position, nearbyPlayer.transform.position);

        if (distance <= playerGetOnDistance)
        {
            if (Input.GetKeyDown(getOnKey))
            {
                Debug.Log($"[Boat] 玩家按下 {getOnKey} 鍵，嘗試上船");
                PlayerGetOn(nearbyPlayer);
            }
        }
    }

    void CheckBoatMovementInput()
    {
        boatMoveX = 0f;

        KeyBindingManager keyManager = KeyBindingManager.Instance;
        if (keyManager != null)
        {
            if (keyManager.GetKeyPressed(KeyBindingManager.ActionType.MoveLeft))
            {
                boatMoveX = -1f;
            }
            else if (keyManager.GetKeyPressed(KeyBindingManager.ActionType.MoveRight))
            {
                boatMoveX = 1f;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                boatMoveX = -1f;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                boatMoveX = 1f;
            }
        }
    }

    void UpdateBoatFloating()
    {
        if (waveSystem == null) return;

        float centerWaveHeight = waveSystem.GetWaveHeightAtPosition(transform.position.x);

        Vector3 newPosition = transform.position;
        newPosition.y = originalLocalPosition.y + centerWaveHeight;
        transform.position = newPosition;

        float waveVelocity = waveSystem.GetWaveVelocityAtPosition(transform.position.x);
        float horizontalVelocity = boatMoveX * boatSpeed;
        rb.velocity = new Vector2(horizontalVelocity, waveVelocity * buoyancyMultiplier);
    }

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

    void LockPlayerOnBoat()
    {
        // ✅ 沉沒時不鎖定玩家
        if (isSinking) return;

        if (playerOnBoard == null || playerRbOnBoard == null) return;

        Vector3 targetPos = transform.position;
        targetPos.y += 0.5f;
        targetPos.z = playerOnBoard.transform.position.z;
        playerOnBoard.transform.position = targetPos;

        playerRbOnBoard.velocity = Vector2.zero;
        playerRbOnBoard.isKinematic = true;
    }

    public void TakeDamage(int damage)
    {
        if (isSinking)
        {
            Debug.Log("[Boat] 船隻已在沉沒中，忽略傷害");
            return;
        }

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

        // ✅ 修復：血量≤0時立即開始沉沒
        if (currentBoatHealth <= 0)
        {
            Debug.Log("[Boat] ⚠️ 血量歸零，調用 StartSinking()");
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

    void StartSinking()
    {
        if (isSinking) return; // ✅ 防止重複調用

        isSinking = true;
        sinkStartTime = -999f; // 重置，讓 HandleSinking 進行初始化

        Debug.Log("[Boat] ⛵ 開始沉沒流程");
    }

    void CheckRepairInput()
    {
        if (isSinking) return;

        bool repairPressed = Input.GetKeyDown(repairKey);

        if (repairPressed)
        {
            Debug.Log($"[Boat] 玩家按下 {repairKey} 鍵，嘗試修復");
            AttemptRepairBoat();
        }
    }

    void AttemptRepairBoat()
    {
        if (!enableManaRepair || isSinking) return;
        if (currentBoatHealth >= maxBoatHealth) return;
        if (Time.time - lastRepairTime < repairCooldown)
        {
            Debug.Log($"[Boat] 修復冷卻中... {(repairCooldown - (Time.time - lastRepairTime)):F1}秒");
            return;
        }

        PlayerAttack playerAttack = playerOnBoard.GetComponent<PlayerAttack>();
        if (playerAttack == null)
        {
            Debug.LogError("[Boat] 找不到 PlayerAttack 組件");
            return;
        }

        if (playerAttack.GetCurrentMana() < manaRepairCost)
        {
            Debug.Log($"[Boat] 魔力不足！需要 {manaRepairCost} 但只有 {playerAttack.GetCurrentMana()}");
            return;
        }

        Debug.Log($"[Boat] 🔧 修復船隻，消耗 {manaRepairCost} 魔力");
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

    void UpdateAutoRepair()
    {
        if (currentBoatHealth >= maxBoatHealth) return;

        autoRepairTimer += Time.fixedDeltaTime;

        if (autoRepairTimer >= autoRepairInterval)
        {
            currentBoatHealth = Mathf.Min(currentBoatHealth + 1, maxBoatHealth);
            autoRepairTimer = 0f;

            if (healthBar != null)
            {
                healthBar.UpdateHealthBar(currentBoatHealth, maxBoatHealth);
            }

            Debug.Log($"[Boat] ⚡ 自動修復 1 點。當前血量: {currentBoatHealth}/{maxBoatHealth}");
        }
    }

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

        Debug.Log($"[Boat] ⛵ 玩家已上船");

        LockPlayerOnBoat();
    }

    public void PlayerGetOff()
    {
        if (playerOnBoard == null) return;

        Debug.Log("[Boat] 🏊 玩家下船");

        if (playerRbOnBoard != null)
        {
            playerRbOnBoard.isKinematic = false;
            playerRbOnBoard.velocity = new Vector2(playerRbOnBoard.velocity.x, 5f);
        }

        if (playerOnBoard != null)
        {
            playerOnBoard.canControl = true;
        }

        playerOnBoard = null;
        playerRbOnBoard = null;
        boatMoveX = 0f;
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