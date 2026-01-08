// ========================================
// BossController.cs - Boss完整控制器（修正版）
// ========================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss基本設定")]
    public float maxHealth = 5000f;
    [SerializeField] private float currentHealth;
    public bool isDead = false;
    public bool canMove = false;

    [Header("移動與追蹤設定")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 3f;
    public bool enablePlayerTracking = true;

    [Header("瞬移系統")]
    [SerializeField] private Transform[] teleportPoints;
    public float teleportCooldown = 5f;
    private float lastTeleportTime = -999f;
    public GameObject teleportEffectPrefab;
    public float teleportChanceOnHit = 0.3f;

    [Header("玩家靠近瞬移設定")]
    [Tooltip("玩家靠近到此距離時，Boss會瞬移到遠處")]
    public float playerTooCloseDistance = 5f;
    [Tooltip("瞬移後與玩家的最小安全距離")]
    public float minSafeTeleportDistance = 15f;

    [Header("地面尖刺陷阱")]
    [Tooltip("場景中的所有地面尖刺")]
    public GroundSpikeTrap[] groundSpikes;
    [Tooltip("觸發尖刺的距離")]
    public float spikeTriggerDistance = 5f;
    [Tooltip("是否在瞬移前觸發尖刺")]
    public bool triggerSpikesBeforeTeleport = true;

    [Header("攻擊系統")]
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;

    [Header("Boss攻擊傷害設定")]
    [Tooltip("Boss單發火球的傷害")]
    public float bossFireballDamage = 20f;
    [Tooltip("Boss多重火球的單發傷害")]
    public float bossMultipleFireballDamage = 15f;
    [Tooltip("Boss地面陷阱的傷害")]
    public float bossHazardDamage = 25f;

    [Header("火球攻擊")]
    public GameObject fireballPrefab;
    public GameObject iceBulletPrefab; // 冰凍子彈
    public GameObject poisonBulletPrefab; // 毒彈（可選）
    public Transform firePoint;
    public float fireballSpeed = 15f;

    [Header("子彈類型權重")]
    [Range(0, 100)] public int normalBulletWeight = 50;
    [Range(0, 100)] public int iceBulletWeight = 30;
    [Range(0, 100)] public int poisonBulletWeight = 20;

    [Header("偵測系統")]
    public Transform player;
    public float detectionRange = 80f;
    private bool playerInRange = false;

    [Header("血條設定")]
    public EnemyHealthBar healthBar;

    [Header("視覺反饋")]
    public SpriteRenderer spriteRenderer;
    public Color bossColor = Color.magenta;
    public Color damageColor = Color.white;

    [Header("跳躍與地板判斷")]
    public float jumpForceVertical = 8f;
    public float jumpForceHorizontal = 8f;
    public float jumpCooldown = 0.7f;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;
    private float lastJumpTime = -999f;
    private Vector2 groundOffset = new Vector2(0, -0.5f);

    [Header("【新增】玩家傷害設定")]
    public float playerSwordDamage = 30f;
    public float playerMagicBulletDamage = 50f;
    public float playerHeavyBulletDamage = 75f;

    [Header("地面陷阱設定")]
    public GameObject hazardZonePrefab;
    public float hazardCooldown = 1.5f;
    private float lastHazardTime = -999f;

    [Header("Debug 設定")]
    public bool debugMode = true;
    public bool showDetailedDebug = false;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private HashSet<GameObject> processedBullets = new HashSet<GameObject>();
    private float lastDamageTime = -999f;
    private float damageCooldown = 0.1f;
    private bool isAttacking = false;
    private Transform effectsParent;

    private enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Teleporting
    }
    private BossState currentState = BossState.Idle;

    void Start()
    {
        Debug.Log("=== [Boss] 開始初始化 ===");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[Boss] 找不到Rigidbody2D組件！");
        }
        else
        {
            Debug.Log($"[Boss] Rigidbody2D已找到 - Body Type: {rb.bodyType}");
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning("[Boss] Rigidbody2D的Body Type應該設為Dynamic才能移動！");
            }
        }

        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = bossColor;

        // 創建特效父物件
        GameObject effectsObj = new GameObject("BossEffects");
        effectsParent = effectsObj.transform;
        effectsParent.SetParent(transform);
        effectsParent.localPosition = Vector3.zero;

        // 自動找玩家
        if (player == null)
        {
            Debug.Log("[Boss] 開始搜尋玩家...");
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerTransform = player;
                Debug.Log($"[Boss] 找到玩家: {playerObj.name}");
            }
            else
            {
                Debug.LogError("[Boss] 找不到標籤為'Player'的物件！");
            }
        }
        else
        {
            playerTransform = player;
            Debug.Log($"[Boss] 玩家已設定: {player.name}");
        }

        // 驗證瞬移點
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("[Boss] 瞬移點陣列為空，瞬移功能將無法使用");
        }
        else
        {
            Debug.Log($"[Boss] 已設定 {teleportPoints.Length} 個瞬移點");
        }

        // 驗證火球預製體
        if (fireballPrefab == null)
        {
            Debug.LogError("[Boss] fireballPrefab 未設定！Boss將無法攻擊");
        }

        if (firePoint == null)
        {
            Debug.LogError("[Boss] firePoint 未設定！Boss將無法攻擊");
        }

        // 初始化血條
        InitializeHealthBar();

        Debug.Log($"[Boss] Boss初始化完成 - 血量: {currentHealth}/{maxHealth}");
        Debug.Log($"[Boss] canMove初始狀態: {canMove}");
        Debug.Log("=== [Boss] 初始化結束 ===");

        // 開始AI
        StartCoroutine(BossAI());
    }

    void InitializeHealthBar()
    {
        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyHealthBar>();

        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
            Debug.Log("[Boss] 血條初始化成功");
        }
        else
        {
            Debug.LogWarning("[Boss] 未找到EnemyHealthBar組件！");
        }
    }

    void Update()
    {
        if (isDead) return;

        if (showDetailedDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Boss Update] canMove: {canMove}, State: {currentState}, isDead: {isDead}");
        }

        if (!canMove || playerTransform == null) return;

        // 檢測玩家距離
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distanceToPlayer <= detectionRange;

        if (showDetailedDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Boss Update] 距離玩家: {distanceToPlayer:F2}, playerInRange: {playerInRange}");
        }

        // 面向玩家
        if (playerTransform.position.x < transform.position.x)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = true;
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        if (isDead || !canMove) return;

        // 即使在攻擊時也允許移動（但速度較慢）
        if (currentState == BossState.Chasing && playerTransform != null)
        {
            MoveTowardsPlayer();
        }
        else if (currentState == BossState.Attacking && playerTransform != null)
        {
            // 攻擊時緩慢移動
            SlowMoveTowardsPlayer();
        }
    }

    // Boss AI主循環 - 【關鍵修改】
    IEnumerator BossAI()
    {
        Debug.Log("[Boss AI] 協程已啟動");

        while (!isDead)
        {
            // 等待可以移動
            if (!canMove)
            {
                if (showDetailedDebug && Time.frameCount % 120 == 0)
                {
                    Debug.Log("[Boss AI] 等待 canMove = true");
                }
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("[Boss AI] playerTransform 為 null");
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (debugMode)
            {
                Debug.Log($"[Boss AI] 距離玩家: {distanceToPlayer:F2}, 攻擊範圍: {attackRange}, playerInRange: {playerInRange}");
            }

            // ===== 【新增】玩家太靠近時自動瞬移到遠處 =====
            if (distanceToPlayer <= playerTooCloseDistance && Time.time - lastTeleportTime >= teleportCooldown * 0.3f)
            {
                currentState = BossState.Teleporting;
                Debug.Log($"[Boss AI] 玩家太靠近({distanceToPlayer:F2}m)，觸發尖刺並瞬移！");

                // 觸發附近的尖刺
                if (triggerSpikesBeforeTeleport)
                {
                    TriggerNearbySpikes();
                    yield return new WaitForSeconds(0.5f); // 等待尖刺警告
                }

                TeleportAwayFromPlayer();
                yield return new WaitForSeconds(1f);
                continue;
            }

            // 狀態機邏輯
            if (!playerInRange)
            {
                // 玩家不在偵測範圍內
                currentState = BossState.Idle;
                if (debugMode)
                    Debug.Log("[Boss AI] 狀態: Idle (玩家太遠)");
                yield return new WaitForSeconds(0.5f);
            }
            else if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 可以攻擊了
                currentState = BossState.Attacking;
                if (debugMode)
                    Debug.Log($"[Boss AI] 狀態: Attacking (距離: {distanceToPlayer:F2})");
                yield return StartCoroutine(AttackSequence());
            }
            else if (Time.time - lastTeleportTime >= teleportCooldown && Random.value < 0.2f)
            {
                // 隨機瞬移
                currentState = BossState.Teleporting;
                if (debugMode)
                    Debug.Log("[Boss AI] 狀態: Teleporting (隨機)");
                TeleportRandomly();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                // 追蹤玩家
                currentState = BossState.Chasing;
                if (showDetailedDebug && Time.frameCount % 60 == 0)
                    Debug.Log("[Boss AI] 狀態: Chasing");
                yield return new WaitForSeconds(0.2f);
            }

            yield return null;
        }

        Debug.Log("[Boss AI] 協程已結束（Boss已死亡）");
    }

    // 移動向玩家
    void MoveTowardsPlayer()
    {
        if (playerTransform == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 如果距離太近就減速
        if (distanceToPlayer <= stoppingDistance)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // 判斷是否需要跳躍
        if (IsGrounded() && Time.time - lastJumpTime >= jumpCooldown)
        {
            if (playerTransform.position.y > transform.position.y + 1f)
            {
                Jump(direction);
            }
        }

        // 水平移動
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);

        if (showDetailedDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Boss Move] 向玩家移動 - velocity: {rb.velocity}, direction: {direction}");
        }
    }

    // 攻擊時緩慢移動
    void SlowMoveTowardsPlayer()
    {
        if (playerTransform == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= stoppingDistance)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * (chaseSpeed * 0.5f), rb.velocity.y);
    }

    // 跳躍
    void Jump(Vector2 direction)
    {
        if (rb == null) return;

        rb.velocity = new Vector2(direction.x * jumpForceHorizontal, jumpForceVertical);
        lastJumpTime = Time.time;

        if (debugMode)
        {
            Debug.Log("[Boss] 執行跳躍");
        }
    }

    // 檢查是否在地面
    bool IsGrounded()
    {
        Vector2 groundPos = (Vector2)transform.position + groundOffset;
        return Physics2D.OverlapCircle(groundPos, groundCheckRadius, groundLayer) != null;
    }

    // 攻擊序列
    IEnumerator AttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Debug.Log("[Boss Attack] 開始攻擊序列");

        // 隨機選擇攻擊方式
        int attackType = Random.Range(0, 3);
        Debug.Log($"[Boss Attack] 攻擊類型: {attackType}");

        switch (attackType)
        {
            case 0:
                FireballAttack();
                break;
            case 1:
                if (hazardZonePrefab != null)
                    SpawnHazardZone();
                else
                    FireballAttack();
                break;
            case 2:
                MultipleFireballAttack();
                break;
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
        Debug.Log("[Boss Attack] 攻擊序列結束");
    }

    // 火球攻擊 - 【新增】隨機子彈類型
    void FireballAttack()
    {
        if (firePoint == null)
        {
            Debug.LogError("[Boss] firePoint 為 null！無法發射火球");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("[Boss] playerTransform 為 null！無法瞄準");
            return;
        }

        // 選擇隨機子彈類型
        GameObject bulletPrefab = GetRandomBulletPrefab();

        if (bulletPrefab == null)
        {
            Debug.LogError("[Boss] 沒有可用的子彈預製體！");
            return;
        }

        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        GameObject fireball = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        string bulletType = GetBulletTypeName(bulletPrefab);
        Debug.Log($"[Boss] 發射{bulletType}於位置: {firePoint.position}");

        BossProjectile projectile = fireball.GetComponent<BossProjectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.damage = (int)bossFireballDamage;
            projectile.speed = fireballSpeed;
            Debug.Log($"[Boss] {bulletType}設定完成 - 傷害: {bossFireballDamage}");
        }
        else
        {
            Debug.LogError("[Boss] 子彈上找不到 BossProjectile 腳本！");
        }
    }

    /// <summary>
    /// 根據權重隨機選擇子彈類型
    /// </summary>
    GameObject GetRandomBulletPrefab()
    {
        // 計算總權重
        int totalWeight = normalBulletWeight + iceBulletWeight + poisonBulletWeight;

        if (totalWeight == 0)
        {
            Debug.LogWarning("[Boss] 所有子彈權重為0，使用普通火球");
            return fireballPrefab;
        }

        // 隨機選擇
        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < normalBulletWeight && fireballPrefab != null)
        {
            return fireballPrefab; // 普通火球
        }

        randomValue -= normalBulletWeight;

        if (randomValue < iceBulletWeight && iceBulletPrefab != null)
        {
            return iceBulletPrefab; // 冰凍子彈
        }

        randomValue -= iceBulletWeight;

        if (randomValue < poisonBulletWeight && poisonBulletPrefab != null)
        {
            return poisonBulletPrefab; // 毒彈
        }

        // 預設返回普通火球
        return fireballPrefab != null ? fireballPrefab :
               (iceBulletPrefab != null ? iceBulletPrefab : poisonBulletPrefab);
    }

    /// <summary>
    /// 獲取子彈類型名稱（用於Debug）
    /// </summary>
    string GetBulletTypeName(GameObject bulletPrefab)
    {
        if (bulletPrefab == fireballPrefab) return "普通火球";
        if (bulletPrefab == iceBulletPrefab) return "冰凍子彈";
        if (bulletPrefab == poisonBulletPrefab) return "毒彈";
        return "未知子彈";
    }

    // 多重火球攻擊 - 【新增】隨機子彈類型
    void MultipleFireballAttack()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("[Boss] 多重火球攻擊失敗：firePoint未設定");
            return;
        }

        Debug.Log("[Boss] 發射多重火球攻擊");

        float[] angles = { -20f, 0f, 20f };

        foreach (float angle in angles)
        {
            // 每發子彈隨機選擇類型
            GameObject bulletPrefab = GetRandomBulletPrefab();

            if (bulletPrefab == null) continue;

            Vector2 baseDirection = playerTransform != null ?
                (playerTransform.position - firePoint.position).normalized :
                Vector2.right;

            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(
                baseDirection.x * Mathf.Cos(rad) - baseDirection.y * Mathf.Sin(rad),
                baseDirection.x * Mathf.Sin(rad) + baseDirection.y * Mathf.Cos(rad)
            );

            GameObject fireball = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            BossProjectile projectile = fireball.GetComponent<BossProjectile>();
            if (projectile != null)
            {
                projectile.SetDirection(direction);
                projectile.damage = (int)bossMultipleFireballDamage;
                projectile.speed = fireballSpeed;
            }
        }

        Debug.Log($"[Boss] 多重火球發射完成 - 每發傷害: {bossMultipleFireballDamage}");
    }

    // 生成地面陷阱
    void SpawnHazardZone()
    {
        if (hazardZonePrefab == null || playerTransform == null) return;
        if (Time.time - lastHazardTime < hazardCooldown) return;

        Vector3 spawnPos = playerTransform.position;
        GameObject hazard = Instantiate(hazardZonePrefab, spawnPos, Quaternion.identity);
        lastHazardTime = Time.time;

        if (debugMode)
        {
            Debug.Log("[Boss] 生成地面陷阱");
        }
    }

    // ===== 【新增】觸發附近的尖刺 =====
    void TriggerNearbySpikes()
    {
        if (groundSpikes == null || groundSpikes.Length == 0)
        {
            if (debugMode)
                Debug.LogWarning("[Boss] 沒有設定地面尖刺");
            return;
        }

        if (playerTransform == null) return;

        int triggeredCount = 0;

        // 觸發玩家附近的所有尖刺
        foreach (GroundSpikeTrap spike in groundSpikes)
        {
            if (spike == null) continue;

            float distanceToPlayer = Vector2.Distance(spike.transform.position, playerTransform.position);

            if (distanceToPlayer <= spikeTriggerDistance)
            {
                spike.Trigger();
                triggeredCount++;
            }
        }

        if (debugMode)
        {
            Debug.Log($"[Boss] 觸發了 {triggeredCount} 個尖刺陷阱（玩家附近{spikeTriggerDistance}m範圍內）");
        }
    }

    // ===== 【✅ 保留】瞬移到離玩家最遠的點 =====
    void TeleportAwayFromPlayer()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("[Boss] 無法瞬移：瞬移點陣列為空");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[Boss] 玩家Transform為空，使用隨機瞬移");
            TeleportRandomly();
            return;
        }

        // 瞬移前特效
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.SetParent(effectsParent);
        }

        // 找到離玩家最遠的瞬移點
        Transform farthestPoint = null;
        float maxDistance = 0f;

        foreach (Transform point in teleportPoints)
        {
            if (point == null) continue;

            float distanceToPlayer = Vector2.Distance(point.position, playerTransform.position);

            // 確保瞬移點距離玩家足夠遠
            if (distanceToPlayer > maxDistance && distanceToPlayer >= minSafeTeleportDistance)
            {
                maxDistance = distanceToPlayer;
                farthestPoint = point;
            }
        }

        // 如果沒找到足夠遠的點，就選最遠的那個
        if (farthestPoint == null)
        {
            foreach (Transform point in teleportPoints)
            {
                if (point == null) continue;
                float distanceToPlayer = Vector2.Distance(point.position, playerTransform.position);
                if (distanceToPlayer > maxDistance)
                {
                    maxDistance = distanceToPlayer;
                    farthestPoint = point;
                }
            }
        }

        if (farthestPoint != null)
        {
            transform.position = farthestPoint.position;
            lastTeleportTime = Time.time;

            // 瞬移後特效
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.SetParent(effectsParent);
            }

            if (debugMode)
            {
                Debug.Log($"[Boss] 瞬移到最遠點: {farthestPoint.name} (距離玩家: {maxDistance:F2}m)");
            }
        }
        else
        {
            Debug.LogWarning("[Boss] 找不到有效的瞬移點！");
        }
    }

    // ===== 【✅ 保留】隨機瞬移功能 =====
    void TeleportRandomly()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("[Boss] 無法瞬移：瞬移點陣列為空");
            return;
        }

        // 瞬移前特效
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.SetParent(effectsParent);
        }

        // 選擇隨機瞬移點
        int randomIndex = Random.Range(0, teleportPoints.Length);
        Transform targetPoint = teleportPoints[randomIndex];

        if (targetPoint != null)
        {
            transform.position = targetPoint.position;
            lastTeleportTime = Time.time;

            // 瞬移後特效
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.SetParent(effectsParent);
            }

            if (debugMode)
            {
                Debug.Log($"[Boss] 隨機瞬移到: {targetPoint.name}");
            }
        }
    }

    // 受到傷害
    public void TakeDamage(float damage, string damageSource = "Unknown")
    {
        if (isDead) return;

        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        lastDamageTime = Time.time;

        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (debugMode)
        {
            Debug.Log($"[Boss] 受到 {damage} 點傷害 (來源: {damageSource})");
            Debug.Log($"[Boss] 血量: {oldHealth} → {currentHealth} / {maxHealth}");
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        StartCoroutine(DamageFlash());

        if (Random.value < teleportChanceOnHit && Time.time - lastTeleportTime >= teleportCooldown * 0.5f)
        {
            TeleportRandomly();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 傷害閃爍效果
    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = bossColor;

        for (int i = 0; i < 3; i++)
        {
            if (isDead) break;

            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 死亡
    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("[Boss] Boss已被擊敗！");

        StopAllCoroutines();

        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (debugMode)
        {
            Debug.Log($"[Boss] 觸發碰撞: {other.gameObject.name} (Tag: {other.tag})");
        }

        if (other.CompareTag("Player"))
        {
            PlayerController2D playerController = other.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                InvincibilityController invincibilityController = playerController.GetComponent<InvincibilityController>();
                if (invincibilityController != null && invincibilityController.IsInvincible())
                {
                    if (debugMode)
                        Debug.Log("[Boss] 玩家處於無敵狀態");
                    return;
                }
            }
            return;
        }

        MagicBullet magicBullet = other.GetComponent<MagicBullet>();
        if (magicBullet != null)
        {
            if (processedBullets.Contains(other.gameObject))
                return;

            processedBullets.Add(other.gameObject);
            TakeDamage(playerMagicBulletDamage, "Player_MagicBullet");
            Destroy(other.gameObject);
            return;
        }

        HeavyBullet heavyBullet = other.GetComponent<HeavyBullet>();
        if (heavyBullet != null)
        {
            if (processedBullets.Contains(other.gameObject))
                return;

            processedBullets.Add(other.gameObject);
            TakeDamage(playerHeavyBulletDamage, "Player_HeavyBullet");
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("PlayerAttack"))
        {
            TakeDamage(playerSwordDamage, "Player_Sword");
            return;
        }
    }

    void OnDestroy()
    {
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
        }
        processedBullets.Clear();
    }

    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetCurrentHealth() => currentHealth;

    void OnDrawGizmosSelected()
    {
        // 偵測範圍（黃色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻擊範圍（紅色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 停止距離（藍色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // 【新增】玩家太靠近距離（橘色）
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, playerTooCloseDistance);

        // 地面檢測（綠色）
        Gizmos.color = Color.green;
        Vector2 groundPos = (Vector2)transform.position + groundOffset;
        Gizmos.DrawWireSphere(groundPos, groundCheckRadius);

        // 瞬移點（青色）
        if (teleportPoints != null && teleportPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in teleportPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawLine(transform.position, point.position);
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }

        // 尖刺觸發範圍（紫色）
        if (groundSpikes != null && groundSpikes.Length > 0)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            foreach (GroundSpikeTrap spike in groundSpikes)
            {
                if (spike != null)
                {
                    Gizmos.DrawWireSphere(spike.transform.position, spikeTriggerDistance);
                }
            }
        }
    }
}