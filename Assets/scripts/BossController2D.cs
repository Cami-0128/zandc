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
    public float jumpCooldown = 0.8f;
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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isDying = false;

    private float lastJumpTime = -999f;
    private float lastAttackTime = -999f;

    private bool wasGroundedLastFrame = false;
    private HashSet<GameObject> processedProjectiles = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.Initialize(transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isDying || playerTransform == null)
            return;

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

        // Boss只要在地面且冷卻到就持續朝玩家跳
        if (isGroundedNow && !isDying && enablePlayerTracking && Time.time - lastJumpTime >= jumpCooldown)
        {
            PerformJumpTowardsPlayer();
            lastJumpTime = Time.time;
        }

        // 落地時生成HazardZone（僅剛落地時）
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
        if (fireballPrefab == null || firePoint == null) return;

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

        if ((other.CompareTag("MagicBullet") || other.CompareTag("HeavyBullet")) && !processedProjectiles.Contains(other.gameObject))
        {
            processedProjectiles.Add(other.gameObject);

            float damage = 0f;
            var magicBullet = other.GetComponent<MagicBullet>();
            if (magicBullet != null) damage = magicBullet.damage;

            var heavyBullet = other.GetComponent<HeavyBullet>();
            if (heavyBullet != null) damage = heavyBullet.damage;

            if (damage > 0)
                TakeDamage(damage, "PlayerBullet");

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

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;
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
}
