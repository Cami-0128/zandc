using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss基本設定")]
    public int maxHealth = 500;
    private int currentHealth;
    public bool isDead = false;

    [Header("瞬移系統")]
    [SerializeField] private Transform[] teleportPoints; // 使用SerializeField確保顯示
    public float teleportCooldown = 3f;
    private float lastTeleportTime = -999f;
    public GameObject teleportEffectPrefab;

    [Header("攻擊系統")]
    public float attackRange = 10f;
    public float attackCooldown = 2f;
    private float lastAttackTime = -999f;
    public int attackDamage = 20;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("偵測系統")]
    public Transform player;
    public float detectionRange = 15f;
    private bool playerInRange = false;

    [Header("視覺反饋")]
    public SpriteRenderer spriteRenderer;
    public Color damageColor = Color.red;
    public Color normalColor = Color.white;

    [Header("血條系統")]
    public GameObject healthBarPrefab; // EnemyHealthBar預製體
    private EnemyHealthBar healthBar;
    public Vector3 healthBarOffset = new Vector3(0, 2f, 0); // 血條偏移位置

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
        currentHealth = maxHealth;
        
        // 自動找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 驗證瞬移點
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogError("[Boss] 請在Inspector中設定瞬移點陣列！右鍵點擊 Teleport Points，選擇 Size 設定數量後拖入物件");
        }

        // 初始化血條
        InitializeHealthBar();

        StartCoroutine(BossAI());
    }

    void InitializeHealthBar()
    {
        if (healthBarPrefab != null)
        {
            // 在Boss上方生成血條
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<EnemyHealthBar>();
            
            if (healthBar != null)
            {
                // 將血條設為Boss的子物件
                healthBarObj.transform.SetParent(transform);
                healthBarObj.transform.localPosition = healthBarOffset;
                
                // 初始化血條
                healthBar.Initialize(transform);
                healthBar.UpdateHealthBar(currentHealth, maxHealth);
                
                Debug.Log("[Boss] 血條初始化成功");
            }
        }
        else
        {
            Debug.LogWarning("[Boss] 未設定 healthBarPrefab！請在Inspector中拖入EnemyHealthBar預製體");
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // 檢測玩家距離
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= detectionRange;

        // 面向玩家
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    // Boss AI主循環
    IEnumerator BossAI()
    {
        while (!isDead)
        {
            if (player == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 狀態機邏輯
            if (!playerInRange)
            {
                currentState = BossState.Idle;
                yield return new WaitForSeconds(0.5f);
            }
            else if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                currentState = BossState.Attacking;
                Attack();
                yield return new WaitForSeconds(attackCooldown);
            }
            else if (Time.time - lastTeleportTime >= teleportCooldown)
            {
                currentState = BossState.Teleporting;
                Teleport();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                currentState = BossState.Chasing;
                yield return new WaitForSeconds(0.2f);
            }

            yield return null;
        }
    }

    // 瞬移功能
    void Teleport()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("[Boss] 無法瞬移：瞬移點陣列為空！");
            return;
        }

        // 瞬移前特效
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }

        // 選擇隨機瞬移點
        int randomIndex = Random.Range(0, teleportPoints.Length);
        Transform targetPoint = teleportPoints[randomIndex];

        if (targetPoint != null)
        {
            // 執行瞬移
            transform.position = targetPoint.position;
            lastTeleportTime = Time.time;

            // 瞬移後特效
            if (teleportEffectPrefab != null)
            {
                Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            }

            Debug.Log($"[Boss] 瞬移到位置: {targetPoint.name}");
        }
        else
        {
            Debug.LogWarning("[Boss] 瞬移點為null！");
        }
    }

    // 攻擊功能
    void Attack()
    {
        if (player == null) return;

        lastAttackTime = Time.time;

        if (projectilePrefab != null && firePoint != null)
        {
            // 發射投射物
            Vector2 direction = (player.position - firePoint.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            BossProjectile projectileScript = projectile.GetComponent<BossProjectile>();
            if (projectileScript != null)
            {
                projectileScript.SetDirection(direction);
                projectileScript.damage = attackDamage;
            }

            Debug.Log("[Boss] 發射投射物攻擊玩家");
        }
        else
        {
            // 近戰攻擊（直接檢測距離）
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                PlayerController2D playerController = player.GetComponent<PlayerController2D>();
                if (playerController != null)
                {
                    playerController.TakeDamage(attackDamage);
                    Debug.Log("[Boss] 近戰攻擊玩家");
                }
            }
        }
    }

    // 受到傷害
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[Boss] 受到 {damage} 點傷害，剩餘血量: {currentHealth}/{maxHealth}");

        UpdateHealthBar();
        StartCoroutine(DamageFlash());

        // 受傷時有30%機會瞬移躲避
        if (Random.value < 0.3f && Time.time - lastTeleportTime >= teleportCooldown * 0.5f)
        {
            Teleport();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 傷害閃爍效果
    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = normalColor;
        }
    }

    // 死亡
    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[Boss] Boss已被擊敗！");

        // 停止所有協程
        StopAllCoroutines();

        // 隱藏血條
        if (healthBar != null)
        {
            healthBar.Hide();
        }

        // 死亡動畫或特效
        if (teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }

        // 可以在這裡觸發勝利事件
        BossDefeatedEvent();

        // 銷毀Boss物件
        Destroy(gameObject, 1f);
    }

    void BossDefeatedEvent()
    {
        // 觸發遊戲管理器的Boss擊敗事件
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // gm.OnBossDefeated(); // 如果你的GameManager有這個方法
        }
        Debug.Log("[Boss] Boss擊敗事件觸發");
    }

    // 更新血條
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    // 視覺化偵測範圍（編輯器中）
    void OnDrawGizmosSelected()
    {
        // 偵測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻擊範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 繪製瞬移點連線
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
    }
}