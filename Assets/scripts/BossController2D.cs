using UnityEngine;
using System.Collections.Generic;

public class BossController2D : MonoBehaviour
{
    [Header("基本屬性")]
    public bool enablePlayerTracking = true;
    public float maxHealth = 10000f;
    private float currentHealth;
    public EnemyHealthBar healthBar;

    [Header("跳躍與地板判斷")]
    public float jumpForceVertical = 8f;
    public float jumpForceHorizontalMax = 8f;
    public float jumpCooldown = 0.7f;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("攻擊")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballSpeed = 15f;
    public float attackCooldown = 3f;

    [Header("地面陷阱")]
    public GameObject hazardZonePrefab;
    public float hazardCooldown = 1.5f;
    private float lastHazardTime = -999f;

    [Header("【新增】玩家傷害設置")]
    [Tooltip("劍造成的傷害")]
    public float swordDamage = 30f;
    [Tooltip("魔法子彈造成的傷害")]
    public float magicBulletDamage = 50f;
    [Tooltip("重型子彈造成的傷害")]
    public float heavyBulletDamage = 75f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isDying = false;

    private float lastJumpTime = -999f;
    private float lastAttackTime = -999f;
    private bool wasGroundedLastFrame = false;

    // 傷害檢查緩衝 - 防止同一次攻擊多次造成傷害
    private HashSet<GameObject> processedProjectiles = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.Initialize(transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        Debug.Log($"[Boss] 初始化完成，最大血量：{maxHealth}");
    }

    void Update()
    {
        if (isDying || playerTransform == null)
            return;

        // 持續攻擊射擊
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            ShootFireball();
            lastAttackTime = Time.time;
        }
        UpdateOrientation();
    }

    void FixedUpdate()
    {
        bool isGroundedNow = IsGrounded();

        // Boss只要在地面且冷卻到，就持續朝玩家跳
        if (isGroundedNow && !isDying && enablePlayerTracking && Time.time - lastJumpTime >= jumpCooldown)
        {
            PerformJumpTowardsPlayer();
            lastJumpTime = Time.time;
        }

        // 落地就生成HazardZone（僅剛落地時）
        if (!wasGroundedLastFrame && isGroundedNow && Time.time - lastHazardTime >= hazardCooldown)
        {
            Instantiate(hazardZonePrefab, transform.position + Vector3.down, Quaternion.identity);
            lastHazardTime = Time.time;
            Debug.Log("[Boss] 落地生成HazardZone");
        }
        wasGroundedLastFrame = isGroundedNow;
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius, groundLayer);
        return hit.collider != null;
    }

    void PerformJumpTowardsPlayer()
    {
        if (rb == null || isDying || playerTransform == null) return;

        // 水平追蹤方向
        float xDir = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector2 jumpVector = new Vector2(xDir * jumpForceHorizontalMax, jumpForceVertical);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.AddForce(jumpVector, ForceMode2D.Impulse);
    }

    private void ShootFireball()
    {
        if (fireballPrefab == null || firePoint == null || playerTransform == null) return;

        GameObject fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (playerTransform.position - firePoint.position).normalized;
        fb.transform.right = dir;
        Rigidbody2D fbRb = fb.GetComponent<Rigidbody2D>();
        if (fbRb != null)
        {
            fbRb.velocity = dir * fireballSpeed;
            fbRb.gravityScale = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;
        if (other.CompareTag("Player")) return;

        // 【簡化】只處理子彈，劍攻擊由OverlapCircle檢測
        if ((other.CompareTag("MagicBullet") || other.CompareTag("HeavyBullet")) && !processedProjectiles.Contains(other.gameObject))
        {
            processedProjectiles.Add(other.gameObject);
            float damage = 0f;

            if (other.CompareTag("MagicBullet"))
            {
                damage = magicBulletDamage;
            }
            else if (other.CompareTag("HeavyBullet"))
            {
                damage = heavyBulletDamage;
            }

            if (damage > 0)
            {
                TakeDamage(damage, "PlayerBullet");
                Debug.Log($"[Boss] 被子彈擊中！造成 {damage} 點傷害");
            }

            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (isDying) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        if (healthBar != null)
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        Debug.Log($"[Boss] 受到來自 {source} 的 {damage} 點傷害。剩餘血量: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;
        Debug.Log("[Boss] Boss 已死亡！");
        if (healthBar != null)
            Destroy(healthBar.gameObject);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        foreach (var collider in GetComponents<Collider2D>())
            collider.enabled = false;
        Destroy(gameObject, 2f);
    }

    private void UpdateOrientation()
    {
        if (playerTransform == null) return;
        float dirX = playerTransform.position.x - transform.position.x;
        if (spriteRenderer != null)
            spriteRenderer.flipX = dirX < 0;
    }

    // 【新增】外部接口 - 用於捕捉技能
    public void OnCaptured()
    {
        Debug.Log("[Boss] Boss 被捕捉了！");
        Die();
    }

    // 【新增】獲取當前血量百分比
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}