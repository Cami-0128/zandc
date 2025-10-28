using UnityEngine;
using System.Collections.Generic;

public class BossController2D : MonoBehaviour
{
    public bool enablePlayerTracking = true;

    public float maxHealth = 10000f;
    private float currentHealth;

    public EnemyHealthBar healthBar;

    public float jumpForceVertical = 8f;
    public float jumpForceHorizontalMax = 8f;
    public float jumpCooldown = 0.35f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballSpeed = 15f;
    public float attackCooldown = 3f;

    public GameObject hazardZonePrefab;
    public float hazardCooldown = 1.5f;
    private float lastHazardTime = -999f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private bool isDying = false;

    private int currentJumpCount = 0;
    private float lastJumpTime = -999f;
    private float lastAttackTime = -999f;
    private Vector2 groundOffset = new Vector2(0, -0.5f);

    private bool wasGroundedLastFrame = false;

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

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 只有當Boss落地且冷卻時間到才執行跳躍
        if (enablePlayerTracking && distanceToPlayer <= 80f &&
            IsGrounded() && Time.time - lastJumpTime >= jumpCooldown &&
            currentJumpCount < 2)
        {
            PerformJump();
        }

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

        // 掉落至地面立刻重置跳躍次數
        if (!wasGroundedLastFrame && isGroundedNow)
        {
            currentJumpCount = 0;

            // 生成HazardZone
            if (Time.time - lastHazardTime >= hazardCooldown)
            {
                Instantiate(hazardZonePrefab, transform.position + Vector3.down, Quaternion.identity);
                lastHazardTime = Time.time;
                Debug.Log("[Boss] 落地生成HazardZone");
            }
        }
        wasGroundedLastFrame = isGroundedNow;
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius, groundLayer);
        return hit.collider != null;
    }

    void PerformJump()
    {
        if (rb == null || isDying) return;

        Vector2 toPlayer = playerTransform.position - transform.position;
        toPlayer.Normalize();

        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.AddForce(new Vector2(toPlayer.x * jumpForceHorizontalMax, jumpForceVertical * (currentJumpCount == 0 ? 1f : 0.8f)), ForceMode2D.Impulse);

        currentJumpCount++;
        lastJumpTime = Time.time;
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
