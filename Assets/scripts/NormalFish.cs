using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通魚 - 使用 FishBullet 替代 MagicBullet
/// 修復版：支持捕捉技能整合
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
    public float approachDistance = 6f;
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

        UpdateTargetTracking();

        if (nearestPlayer != null && !nearestPlayer.isDead)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, nearestPlayer.transform.position);

            if (distanceToPlayer > approachDistance)
            {
                Vector2 directionToPlayer = (nearestPlayer.transform.position - transform.position).normalized;
                swimDirection = directionToPlayer;
            }
            else if (distanceToPlayer <= attackDistance)
            {
                swimDirection = Vector2.zero;
                CheckAttackTarget();
            }
        }
        else if (nearestBoat != null && nearestBoat.HasPlayerOnBoard())
        {
            float distanceToBoat = Vector2.Distance(transform.position, nearestBoat.transform.position);

            if (distanceToBoat > approachDistance)
            {
                Vector2 directionToBoat = (nearestBoat.transform.position - transform.position).normalized;
                swimDirection = directionToBoat;
            }
            else if (distanceToBoat <= attackDistance)
            {
                swimDirection = Vector2.zero;
                CheckAttackTarget();
            }
        }
        else
        {
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

                Vector2 awayFromEnemy = (transform.position - enemy.transform.position).normalized;
                avoidanceForce += awayFromEnemy * enemyAvoidanceForce;
            }
        }

        if (nearbyEnemies.Count > 0)
        {
            if (!(nearestPlayer != null || nearestBoat != null))
            {
                swimDirection = avoidanceForce.normalized;
            }
        }
    }

    void CheckAttackTarget()
    {
        if (!canAttack || Time.time - lastAttackTime < attackCooldown) return;

        if (nearestBoat != null && nearestBoat.HasPlayerOnBoard())
        {
            float distanceToBoat = Vector2.Distance(transform.position, nearestBoat.transform.position);
            if (distanceToBoat < attackDistance)
            {
                AttackTarget(nearestBoat.transform.position);
                return;
            }
        }

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

        Vector3 directionToTarget = (targetPos - firePoint.position).normalized;

        GameObject bullet = Instantiate(fishBulletPrefab, firePoint.position, Quaternion.identity);

        FishBullet bulletScript = bullet.GetComponent<FishBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(directionToTarget, 10f);
            bulletScript.SetDamage(attackDamage);
            Debug.Log($"[NormalFish] ✅ 朝向 {targetPos} 發射魚子彈，傷害: {attackDamage}");
        }

        lastAttackTime = Time.time;
    }

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

    // ✅ 新增：捕捉技能整合
    public void OnCaptured()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"[NormalFish] 普通魚被捕捉");

        WaterSplashEffect splashEffect = FindObjectOfType<WaterSplashEffect>();
        if (splashEffect != null)
        {
            splashEffect.CreateBloodSplash(transform.position);
        }

        DropLoot();
        // gameObject 由 CaptureBubble 銷毀
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
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
        }
    }

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, approachDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectDistance);
    }

    public int GetFishHealth() => currentFishHealth;
    public int GetMaxFishHealth() => maxFishHealth;
}