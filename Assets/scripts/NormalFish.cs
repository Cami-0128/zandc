using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通魚 - 會主動靠近玩家並攻擊
/// 修復版：確保正確靠近和發射子彈
/// </summary>
public class NormalFish : MonoBehaviour
{
    [Header("═══ 基礎設定 ═══")]
    public float swimSpeed = 3f;
    public float swimRadius = 8f;

    [Header("═══ AI 行為 ═══")]
    public bool enableAI = true;
    public float directionChangeInterval = 3f;

    [Header("═══ 敵人躲避 ═══")]
    public float enemyDetectDistance = 6f;
    public float enemyAvoidanceForce = 5f;

    [Header("═══ 血量 ═══")]
    public int maxFishHealth = 40;
    public int currentFishHealth;
    public EnemyHealthBar healthBar;

    [Header("═══ 攻擊設定 ═══")]
    public bool canAttack = true;
    public float attackDistance = 5f;
    public float approachDistance = 6f;  // 靠近玩家的距離
    public int attackDamage = 10;
    public GameObject fishBulletPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;

    [Header("═══ 掉落物品 ═══")]
    public GameObject coinPickupPrefab;
    public GameObject manaPickupPrefab;
    public GameObject healthPickupPrefab;

    [SerializeField]
    [Range(0f, 1f)]
    private float coinDropRate = 0.4f;
    [SerializeField]
    [Range(0f, 1f)]
    private float manaDropRate = 0.3f;
    [SerializeField]
    [Range(0f, 1f)]
    private float healthDropRate = 0.3f;

    public int coinDropValue = 10;
    public int manaRestoreAmount = 20;
    public int healthRestoreAmount = 15;

    [Header("═══ 視覺效果 ═══")]
    public bool flipBasedOnDirection = true;
    public Color damagedColor = new Color(1f, 0.5f, 0.5f, 1f);
    public Color normalColor = Color.white;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 swimDirection = Vector2.right;
    private float directionChangeTimer = 0f;
    private Vector3 spawnPosition;
    private bool isDead = false;

    private PlayerController2D nearestPlayer;
    private Boat nearestBoat;
    private WaterZone waterZone;
    private List<Enemy> nearbyEnemies = new List<Enemy>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("[NormalFish] 需要 Rigidbody2D 組件");
            return;
        }

        spawnPosition = transform.position;
        directionChangeTimer = directionChangeInterval;
        currentFishHealth = maxFishHealth;

        if (spriteRenderer != null)
        {
            normalColor = spriteRenderer.color;
        }

        if (enableAI)
        {
            swimDirection = Random.insideUnitCircle.normalized;
        }

        // 初始化血條
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<EnemyHealthBar>();
        }
        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentFishHealth, maxFishHealth);
        }

        // 創建射擊點（位於魚的中心）
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.zero;
            firePoint = firePointObj.transform;
        }

        waterZone = FindObjectOfType<WaterZone>();

        if (fishBulletPrefab == null)
        {
            Debug.LogWarning("[NormalFish] fishBulletPrefab 未設定！");
        }

        Debug.Log($"[NormalFish] 初始化完成 - 血量: {currentFishHealth}，攻擊傷害: {attackDamage}");
    }

    void FixedUpdate()
    {
        if (isDead || !enableAI) return;

        ApplySwimming();
    }

    void Update()
    {
        if (isDead) return;

        // 尋找最近的玩家
        UpdateTargetTracking();

        // 如果偵測到玩家，主動靠近
        if (nearestPlayer != null && !nearestPlayer.isDead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);

            if (distanceToPlayer > approachDistance)
            {
                // 主動靠近玩家
                Vector2 directionToPlayer = (nearestPlayer.transform.position - transform.position).normalized;
                swimDirection = directionToPlayer;
                Debug.Log($"[NormalFish] 靠近玩家，距離: {distanceToPlayer:F1}");
            }
            else if (distanceToPlayer <= attackDistance)
            {
                // 靠近到攻擊距離，停止移動並攻擊
                swimDirection = Vector2.zero;
                CheckAttackTarget();
            }
        }
        // 船上有玩家時，也靠近並攻擊
        else if (nearestBoat != null && nearestBoat.HasPlayerOnBoard())
        {
            float distanceToBoat = Vector2.Distance(transform.position, nearestBoat.transform.position);

            if (distanceToBoat > approachDistance)
            {
                Vector2 directionToBoat = (nearestBoat.transform.position - transform.position).normalized;
                swimDirection = directionToBoat;
                Debug.Log($"[NormalFish] 靠近有玩家的船，距離: {distanceToBoat:F1}");
            }
            else if (distanceToBoat <= attackDistance)
            {
                swimDirection = Vector2.zero;
                CheckAttackTarget();
            }
        }
        else
        {
            // 沒有玩家時，正常游動
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                ChangeSwimmingDirection();
                directionChangeTimer = directionChangeInterval;
            }
        }

        CheckEnemiesAndAvoid();
        UpdateVisuals();
    }

    void UpdateTargetTracking()
    {
        nearestPlayer = FindObjectOfType<PlayerController2D>();
        nearestBoat = FindObjectOfType<Boat>();
    }

    void ChangeSwimmingDirection()
    {
        swimDirection = Random.insideUnitCircle.normalized;
    }

    // ========== 敵人偵測和躲避 ==========
    void CheckEnemiesAndAvoid()
    {
        nearbyEnemies.Clear();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        Vector2 avoidanceForce = Vector2.zero;

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < enemyDetectDistance)
            {
                nearbyEnemies.Add(enemy);

                // 計算躲避方向
                Vector2 awayFromEnemy = (transform.position - enemy.transform.position).normalized;
                avoidanceForce += awayFromEnemy * enemyAvoidanceForce;
            }
        }

        // 如果有敵人且沒有在攻擊，改變方向躲避
        if (nearbyEnemies.Count > 0)
        {
            // 只有沒有在追擊玩家時才躲避敵人
            if (!(nearestPlayer != null || nearestBoat != null))
            {
                swimDirection = avoidanceForce.normalized;
                Debug.Log($"[NormalFish] 偵測到 {nearbyEnemies.Count} 個敵人，躲避");
            }
        }
    }

    // ========== 攻擊系統 ==========
    void CheckAttackTarget()
    {
        if (!canAttack || Time.time - lastAttackTime < attackCooldown) return;

        // 優先攻擊船上的玩家
        if (nearestBoat != null && nearestBoat.HasPlayerOnBoard())
        {
            float distanceToBoat = Vector2.Distance(transform.position, nearestBoat.transform.position);
            if (distanceToBoat < attackDistance)
            {
                AttackTarget(nearestBoat.transform.position);
                return;
            }
        }

        // 其次攻擊玩家
        if (nearestPlayer != null && !nearestPlayer.isDead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);
            if (distanceToPlayer < attackDistance)
            {
                AttackTarget(nearestPlayer.transform.position);
            }
        }
    }

    void AttackTarget(Vector3 targetPos)
    {
        if (fishBulletPrefab == null)
        {
            Debug.LogError("[NormalFish] fishBulletPrefab 未設定");
            return;
        }

        // 計算朝向目標的方向
        Vector3 directionToTarget = (targetPos - firePoint.position).normalized;

        // 從射擊點發射 FishBullet
        GameObject bullet = Instantiate(fishBulletPrefab, firePoint.position, Quaternion.identity);

        // 立即設定方向（在第一幀執行）
        FishBullet bulletScript = bullet.GetComponent<FishBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(directionToTarget, 10f);
            bulletScript.SetDamage(attackDamage);
            Debug.Log($"[NormalFish] ✅ 朝向 {targetPos} 發射魚子彈，方向: {directionToTarget}, 傷害: {attackDamage}");
        }
        else
        {
            Debug.LogError("[NormalFish] 子彈沒有 FishBullet 腳本");
        }

        lastAttackTime = Time.time;
    }

    // ========== 受傷系統 ==========
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentFishHealth -= damage;
        currentFishHealth = Mathf.Max(currentFishHealth, 0);

        Debug.Log($"[NormalFish] 受到 {damage} 點傷害。當前血量: {currentFishHealth}/{maxFishHealth}");

        StartCoroutine(DamagedEffect());

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentFishHealth, maxFishHealth);
        }

        if (currentFishHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamagedEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damagedColor;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = normalColor;
        }
    }

    // ========== 死亡系統 ==========
    void Die()
    {
        isDead = true;
        Debug.Log($"[NormalFish] 普通魚已死亡");

        WaterSplashEffect splashEffect = FindObjectOfType<WaterSplashEffect>();
        if (splashEffect != null)
        {
            splashEffect.CreateBloodSplash(transform.position);
        }

        DropLoot();
        Destroy(gameObject);
    }

    void DropLoot()
    {
        float randomValue = Random.value;

        if (randomValue < coinDropRate)
        {
            DropCoin();
        }
        else if (randomValue < coinDropRate + manaDropRate)
        {
            DropManaPotionPickup();
        }
        else if (randomValue < coinDropRate + manaDropRate + healthDropRate)
        {
            DropHealthPickup();
        }
    }

    void DropCoin()
    {
        if (coinPickupPrefab == null) return;

        GameObject coinObj = Instantiate(coinPickupPrefab, transform.position, Quaternion.identity);
        CoinPickup coinPickup = coinObj.GetComponent<CoinPickup>();
        if (coinPickup != null)
        {
            coinPickup.SetCoinValue(coinDropValue);
        }
    }

    void DropManaPotionPickup()
    {
        if (manaPickupPrefab == null) return;

        GameObject manaObj = Instantiate(manaPickupPrefab, transform.position, Quaternion.identity);
        ManaPotionPickup manaPickup = manaObj.GetComponent<ManaPotionPickup>();
        if (manaPickup != null)
        {
            manaPickup.manaRestoreAmount = manaRestoreAmount;
        }
    }

    void DropHealthPickup()
    {
        if (healthPickupPrefab == null) return;

        GameObject healthObj = Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        HealthPickup healthPickup = healthObj.GetComponent<HealthPickup>();
        if (healthPickup != null)
        {
            healthPickup.healAmount = healthRestoreAmount;
        }
    }

    // ========== 游泳移動 ==========
    void ApplySwimming()
    {
        if (swimDirection.magnitude > 0.1f)
        {
            Vector2 currentVelocity = rb.velocity;
            currentVelocity += swimDirection * swimSpeed * Time.fixedDeltaTime;

            if (currentVelocity.magnitude > swimSpeed)
            {
                currentVelocity = currentVelocity.normalized * swimSpeed;
            }

            rb.velocity = currentVelocity;
        }
        else
        {
            // 停止移動
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
        }
    }

    // ========== 視覺更新 ==========
    void UpdateVisuals()
    {
        if (flipBasedOnDirection && spriteRenderer != null && swimDirection.magnitude > 0.1f)
        {
            if (swimDirection.x < -0.1f)
                spriteRenderer.flipX = true;
            else if (swimDirection.x > 0.1f)
                spriteRenderer.flipX = false;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", rb.velocity.magnitude);
        }
    }

    // ========== 碰撞檢測 ==========
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        MagicBullet magicBullet = other.GetComponent<MagicBullet>();
        if (magicBullet != null)
        {
            TakeDamage((int)magicBullet.damage);
            return;
        }

        HeavyBullet heavyBullet = other.GetComponent<HeavyBullet>();
        if (heavyBullet != null)
        {
            TakeDamage((int)heavyBullet.damage);
            return;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 活動範圍
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPosition, swimRadius);

        // 靠近距離
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, approachDistance);

        // 攻擊距離
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // 敵人偵測距離
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectDistance);
    }

    public int GetFishHealth() => currentFishHealth;
    public int GetMaxFishHealth() => maxFishHealth;
}