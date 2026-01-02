using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特殊魚 V3 - 使用 FishBullet 替代 HeavyBullet
/// </summary>
public class SpecialFish : MonoBehaviour
{
    [Header("═══ 基礎設定 ═══")]
    public float swimSpeed = 2f;
    public float swimRadius = 10f;

    [Header("═══ AI 行為 ═══")]
    public bool enableAI = true;
    public float directionChangeInterval = 2f;
    public float evadeDistance = 4f;
    public float evadeSpeed = 3.5f;

    [Header("═══ 敵人躲避 ═══")]
    public float enemyDetectDistance = 6f;
    public float enemyAvoidanceForce = 5f;

    [Header("═══ 血量 ═══")]
    public int maxFishHealth = 60;
    public int currentFishHealth;
    public EnemyHealthBar healthBar;

    [Header("═══ 攻擊 ═══")]
    public bool canAttack = true;
    public float attackDistance = 1.5f;
    public int attackDamage = 15;
    public GameObject fishBulletPrefab;  // 改為 FishBullet
    public Transform firePoint;
    public float attackCooldown = 3f;
    private float lastAttackTime = -999f;

    [Header("═══ 掉落物品（獎勵較高）═══")]
    public GameObject coinPickupPrefab;
    public GameObject manaPickupPrefab;
    public GameObject healthPickupPrefab;

    [SerializeField]
    [Range(0f, 1f)]
    private float coinDropRate = 0.3f;
    [SerializeField]
    [Range(0f, 1f)]
    private float manaDropRate = 0.35f;
    [SerializeField]
    [Range(0f, 1f)]
    private float healthDropRate = 0.35f;

    public int coinDropValue = 20;
    public int manaRestoreAmount = 40;
    public int healthRestoreAmount = 30;

    [Header("═══ 視覺效果 ═══")]
    public bool flipBasedOnDirection = true;
    public Color damagedColor = new Color(1f, 0.5f, 0.5f, 1f);
    public Color specialColor = new Color(0.7f, 0.3f, 1f, 1f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 swimDirection = Vector2.right;
    private float directionChangeTimer = 0f;
    private Vector3 spawnPosition;
    private bool isDead = false;
    private bool isEvading = false;
    private float evadeTimer = 0f;

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
            Debug.LogError("[SpecialFish] 需要 Rigidbody2D 組件");
            return;
        }

        spawnPosition = transform.position;
        directionChangeTimer = directionChangeInterval;
        currentFishHealth = maxFishHealth;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = specialColor;
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

        // 檢查 FishBullet 預製物
        if (fishBulletPrefab == null)
        {
            Debug.LogWarning("[SpecialFish] fishBulletPrefab 未設定！請在 Inspector 中指定 FishBullet 預製物");
        }

        Debug.Log($"[SpecialFish] 初始化完成 - 血量: {currentFishHealth}，攻擊傷害: {attackDamage}");
    }

    void FixedUpdate()
    {
        if (isDead || !enableAI) return;

        ApplySwimming();
    }

    void Update()
    {
        if (isDead) return;

        if (!isEvading)
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                ChangeSwimmingDirection();
                directionChangeTimer = directionChangeInterval;
            }
        }

        if (isEvading)
        {
            evadeTimer -= Time.deltaTime;
            if (evadeTimer <= 0f)
            {
                isEvading = false;
            }
        }

        CheckAttackTarget();
        CheckPlayerAndEnemies();
        UpdateVisuals();
    }

    void ChangeSwimmingDirection()
    {
        swimDirection = Random.insideUnitCircle.normalized;
    }

    // ========== 玩家和敵人檢測 ==========
    void CheckPlayerAndEnemies()
    {
        nearestPlayer = FindObjectOfType<PlayerController2D>();
        nearestBoat = FindObjectOfType<Boat>();

        // 玩家接近時躲避
        if (nearestPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);
            if (distanceToPlayer < evadeDistance)
            {
                if (!isEvading)
                {
                    isEvading = true;
                    evadeTimer = 2f;
                    // 計算躲避方向（遠離玩家）
                    swimDirection = (transform.position - nearestPlayer.transform.position).normalized;
                }
            }
        }

        // 敵人躲避
        CheckEnemiesAndAvoid();
    }

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

        // 如果有敵人附近，優先躲避敵人
        if (nearbyEnemies.Count > 0)
        {
            swimDirection = avoidanceForce.normalized;
        }
    }

    // ========== 攻擊系統 ==========
    void CheckAttackTarget()
    {
        if (!canAttack || Time.time - lastAttackTime < attackCooldown) return;

        // 只有極靠近時才攻擊
        if (nearestPlayer != null && !nearestPlayer.isDead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);
            if (distanceToPlayer < attackDistance)
            {
                AttackTarget(nearestPlayer.transform.position);
                return;
            }
        }

        // 船上有玩家時也攻擊
        if (nearestBoat != null && nearestBoat.HasPlayerOnBoard())
        {
            float distanceToBoat = Vector2.Distance(transform.position, nearestBoat.transform.position);
            if (distanceToBoat < attackDistance)
            {
                AttackTarget(nearestBoat.transform.position);
            }
        }
    }

    void AttackTarget(Vector3 targetPos)
    {
        if (fishBulletPrefab == null)
        {
            Debug.LogError("[SpecialFish] fishBulletPrefab 未設定");
            return;
        }

        // 計算朝向目標的方向
        Vector3 directionToTarget = (targetPos - firePoint.position).normalized;

        // 從射擊點發射 FishBullet
        GameObject bullet = Instantiate(fishBulletPrefab, firePoint.position, Quaternion.identity);

        // 設定子彈的方向和速度
        FishBullet bulletScript = bullet.GetComponent<FishBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(directionToTarget, 11f);
            bulletScript.SetDamage(attackDamage);  // 設定傷害值
        }

        lastAttackTime = Time.time;
        Debug.Log($"[SpecialFish] 朝向 {targetPos} 發射魚子彈，方向: {directionToTarget}，傷害: {attackDamage}");
    }

    // ========== 受傷系統 ==========
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentFishHealth -= damage;
        currentFishHealth = Mathf.Max(currentFishHealth, 0);

        Debug.Log($"[SpecialFish] 受到 {damage} 點傷害。當前血量: {currentFishHealth}/{maxFishHealth}");

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
            spriteRenderer.color = specialColor;
        }
    }

    // ========== 死亡系統 ==========
    void Die()
    {
        isDead = true;
        Debug.Log($"[SpecialFish] 特殊魚已死亡");

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
        float currentSpeed = isEvading ? evadeSpeed : swimSpeed;

        Vector2 currentVelocity = rb.velocity;
        currentVelocity += swimDirection * currentSpeed * Time.fixedDeltaTime;

        if (currentVelocity.magnitude > currentSpeed)
        {
            currentVelocity = currentVelocity.normalized * currentSpeed;
        }

        rb.velocity = currentVelocity;
    }

    // ========== 視覺更新 ==========
    void UpdateVisuals()
    {
        if (flipBasedOnDirection && spriteRenderer != null)
        {
            if (swimDirection.x < -0.1f)
                spriteRenderer.flipX = true;
            else if (swimDirection.x > 0.1f)
                spriteRenderer.flipX = false;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", rb.velocity.magnitude);
            animator.SetBool("IsEvading", isEvading);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPosition, swimRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyDetectDistance);
    }

    public int GetFishHealth() => currentFishHealth;
    public int GetMaxFishHealth() => maxFishHealth;
}