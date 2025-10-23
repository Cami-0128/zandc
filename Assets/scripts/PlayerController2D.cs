using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public bool isDead { get; private set; } = false;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public int maxJumps = 2;
    public float fall = -15f;
    public int Methodofdeath = 0;
    public GameObject deathUI;
    private Rigidbody2D rb;
    private int jumpCount;
    private bool isGrounded;
    public bool canControl = true;

    [Header("Health System ��q�t��")]
    public int maxHealth = 100;
    private static int persistentHealth = -1;
    private static bool isFirstTimePlay = true;
    public int currentHealth;
    public int enemyDamage = 15;
    private float damageInvulnerabilityTime = 1f;
    private float lastDamageTime = -999f;

    [Header("Health Pickup System ��]�t��")]
    public AudioClip healSound;
    private AudioSource audioSource;

    // === �]�O ===
    public int maxMana = 100;
    public int currentMana = 100;

    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;

    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;

    public HourglassTimer hourglassTimer;
    public bool hasReachedEnd = false;

    public int LastHorizontalDirection { get; private set; } = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);

        InitializeHealth();
        Time.timeScale = 1f;
        Debug.Log($"Game Start. �C���}�l - ��q: {currentHealth}/{maxHealth}");
        UpdateHealthUI();

        if (hourglassTimer == null)
        {
            hourglassTimer = GetComponentInChildren<HourglassTimer>();
            if (hourglassTimer == null)
            {
                Debug.LogWarning("�䤣�� HourglassTimer �ե�I");
            }
        }
    }

    void InitializeHealth()
    {
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            currentMana = maxMana;
            isFirstTimePlay = false;
            Debug.Log("[��q�t��] �Ĥ@���C���A��q�]������A�]�O�]�]��");
        }
        else
        {
            currentHealth = persistentHealth;
            Debug.Log($"[��q�t��] ���J�O�s����q: {currentHealth}/{maxHealth}");
        }
    }

    void SaveHealth()
    {
        persistentHealth = currentHealth;
        Debug.Log($"[��q�t��] ��q�w�O�s: {persistentHealth}");
    }

    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[��q�t��] ��q�t�Τw���m");
    }

    // <<<<<<<<<<<<<<<< �o�O�A�D�n�n�ΨӨ��N���u�]�O�^�_�v�禡 <<<<<<<<<<<<<<<<<<<<<<
    public void ManaHeal(int manaAmount)
    {
        if (isDead) return;
        int oldMana = currentMana;
        currentMana += manaAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        Debug.Log($"ManaHeal called: �^�_ {manaAmount} �]�O�A�q {oldMana} �� {currentMana}");

        // ����G�P�B PlayerAttack �����]�O�]�o�|�v�TUI��ܡ^
        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.RestoreMana(manaAmount); // ��PlayerAttack���]�O�]�@�֥[��
            ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
            if (manaBar != null)
                manaBar.UpdateManaBar(attack.GetCurrentMana(), attack.maxMana);
            else
                Debug.LogWarning("�䤣�� ManaBarUI �ե�");
        }
        else
        {
            Debug.LogWarning("�䤣�� PlayerAttack �ե�");
        }
    }
    // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    void Update()
    {
        if (!canControl) return;
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

        if (transform.position.y < fall)
        {
            Fall();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EndPoint"))
        {
            hasReachedEnd = true;
            canControl = false;
            rb.velocity = Vector2.zero;
            Debug.Log("���a�w��F���I�I");
        }
    }

    void OnDestroy()
    {
        if (!isDead && currentHealth > 0)
        {
            SaveHealth();
        }
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0)
        {
            LastHorizontalDirection = (int)Mathf.Sign(moveX);
        }
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    void HandleJumpInput()
    {
        Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTouchingWall && !isGrounded && Time.time - lastWallJumpTime >= wallJumpCooldown)
            {
                WallJump();
            }
            else if (jumpCount < maxJumps)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }

    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;
        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }

    void UpdateWallJumpHangTime()
    {
        if (wallJumping)
        {
            wallJumpHangCounter -= Time.deltaTime;
            if (wallJumpHangCounter <= 0f)
            {
                wallJumping = false;
            }
        }
    }

    void CheckWallSliding()
    {
        Vector2 checkLeft = Vector2.left;
        Vector2 checkRight = Vector2.right;
        bool wallLeft = Physics2D.Raycast(wallCheck.position, checkLeft, wallCheckDistance, wallLayer);
        bool wallRight = Physics2D.Raycast(wallCheck.position, checkRight, wallCheckDistance, wallLayer);
        isTouchingWall = wallLeft || wallRight;
        if (wallJumping)
        {
            isWallSliding = false;
            return;
        }
        if (isTouchingWall && !isGrounded)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
        if (isWallSliding && rb.velocity.y < -wallSlideSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
        Debug.DrawRay(wallCheck.position, checkLeft * wallCheckDistance, Color.red);
        Debug.DrawRay(wallCheck.position, checkRight * wallCheckDistance, Color.red);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log("�I��Enemy1�A�������`�I");
            TakeDamage(currentHealth);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (Time.time - lastDamageTime >= damageInvulnerabilityTime)
            {
                Debug.Log("�I��Enemy�A����" + enemyDamage + "�I��q�I");
                TakeDamage(enemyDamage);
            }
        }
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        lastDamageTime = Time.time;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"���a���� {damage} �I�ˮ`�C��e��q: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        Debug.Log($"Heal��ƳQ�I�s�AhealAmount = {healAmount}");
        if (isDead) return;
        int oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        if (healSound != null && audioSource != null)
            audioSource.PlayOneShot(healSound);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"�^�� {currentHealth - oldHealth} �I�A�ثe��q: {currentHealth}/{maxHealth}");
        StartCoroutine(HealFeedback());
    }

    IEnumerator HealFeedback()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }

    void UpdateHealthUI()
    {
        HealthBarUI healthBar = FindObjectOfType<HealthBarUI>();
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("�䤣��HealthBarUI�ե�I�нT�O�����������UI�C");
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool CanHeal() => !isDead && currentHealth < maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;

    void Fall()
    {
        if (isDead) return;
        Debug.Log("���a�������`�I");
        currentHealth = 0;
        SaveHealth();
        UpdateHealthUI();
        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        canControl = false;
        SaveHealth();
        Debug.Log("���a���`�I");
        FindObjectOfType<GameManager>().PlayerDied();
    }

    public void PauseGame()
    {
        Die();
    }

    public void SaveCurrentHealth()
    {
        SaveHealth();
    }

    public static int GetPersistentHealth()
    {
        return persistentHealth;
    }

    public void SetFullHealth()
    {
        currentHealth = maxHealth;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log("[��q�t��] ��q�]������");
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"[��q�t��] ��q�]��: {currentHealth}/{maxHealth}");
    }
}
