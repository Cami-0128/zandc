using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    // �즳�ܼ�
    private bool isDead = false;
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

    // === �ק�G�����d��q�t�� ===
    [Header("Health System ��q�t��")]
    public int maxHealth = 100;              // �̤j��q

    // === �s�W�G�R�A��q�ܼơ]������O���^ ===
    private static int persistentHealth = -1; // -1 ��ܩ|����l��
    private static bool isFirstTimePlay = true; // �O�_�Ĥ@���C��

    private int currentHealth;               // ��e��q
    public int enemyDamage = 15;             // Enemy�y�����ˮ`
    private float damageInvulnerabilityTime = 1f; // ���˵L�Įɶ��]����s�򦩦�^
    private float lastDamageTime = -999f;    // �W�����ˮɶ�

    // Wall Slide �����]�즳�^
    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;

    // Wall Jump �����]�즳�^
    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;

    // Wall Jump Hang Time �����]�즳�^
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // === �ק�G��q��l���޿� ===
        InitializeHealth();

        Time.timeScale = 1f;
        Debug.Log($"Game Start. �C���}�l - ��q: {currentHealth}/{maxHealth}");

        // === ��s���UI ===
        UpdateHealthUI();
    }

    // === �s�W�G��q��l�Ƥ�k ===
    void InitializeHealth()
    {
        // �p�G�O�Ĥ@���C���A�]�m����
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            isFirstTimePlay = false;
            Debug.Log("[��q�t��] �Ĥ@���C���A��q�]������");
        }
        // �_�h�ϥΫO�s����q
        else
        {
            currentHealth = persistentHealth;
            Debug.Log($"[��q�t��] ���J�O�s����q: {currentHealth}/{maxHealth}");
        }
    }

    // === �s�W�G�O�s��q��k ===
    void SaveHealth()
    {
        persistentHealth = currentHealth;
        Debug.Log($"[��q�t��] ��q�w�O�s: {persistentHealth}");
    }

    // === �s�W�G���m��q�t�Ρ]�Ω󭫷s�}�l�C���^ ===
    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[��q�t��] ��q�t�Τw���m");
    }

    void Update()
    {
        if (!canControl) return;

        // === �s�W�G���ի��� ===
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"[����] ��e��q: {currentHealth}/{maxHealth}, �O�s��q: {persistentHealth}");
        }

        // �즳���C���޿�
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

        // �������`�˴��]�즳�^
        if (transform.position.y < fall)
        {
            Fall();
        }
    }

    // === �s�W�G���������ɫO�s��q ===
    void OnDestroy()
    {
        if (!isDead && currentHealth > 0)
        {
            SaveHealth();
        }
    }

    // �����޿�]�즳�^
    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    // ���D��J�B�z�]�즳�^
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

    // ���D�]�즳�^
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }

    // ����]�즳�^
    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;

        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }

    // ��s�������ɶ��]�즳�^
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

    // �ˬd��ơ]�즳�^
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

    // === �I���˴��]�즳�޿�^ ===
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy1�G�������`
        if (collision.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log("�I��Enemy1�A�������`�I");
            TakeDamage(currentHealth); // �y�������e��q���ˮ`�A�������`
        }
        // Enemy�G����
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // �ˬd�L�Įɶ��A�קK�s�����
            if (Time.time - lastDamageTime >= damageInvulnerabilityTime)
            {
                Debug.Log("�I��Enemy�A����" + enemyDamage + "�I��q�I");
                TakeDamage(enemyDamage);
            }
        }

        // �a���˴��]�즳�^
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }

    // ���}�I���]�즳�^
    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // === �ק�G���˨t�� ===
    public void TakeDamage(int damage)
    {
        if (isDead) return; // �p�G�w�g���`�A���A����

        currentHealth -= damage;              // ������q
        currentHealth = Mathf.Max(currentHealth, 0); // �T�O��q���|�p��0
        lastDamageTime = Time.time;           // �O�����ˮɶ�

        // === �s�W�G�Y�ɫO�s��q ===
        SaveHealth();

        UpdateHealthUI();                     // ��s���UI

        Debug.Log($"���a���� {damage} �I�ˮ`�C��e��q: {currentHealth}/{maxHealth}");

        // ��q�k�s�ɦ��`
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // === �ק�G�v���t�� ===
    public void Heal(int healAmount)
    {
        if (isDead) return; // �p�G�w�g���`�A�L�k�v��

        currentHealth += healAmount;                          // �W�[��q
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // �T�O��q���|�W�L�̤j��

        // === �s�W�G�Y�ɫO�s��q ===
        SaveHealth();

        UpdateHealthUI();                                     // ��s���UI
        Debug.Log($"���a�^�_ {healAmount} �I��q�C��e��q: {currentHealth}/{maxHealth}");
    }

    // === ��s���UI�]�즳�^ ===
    void UpdateHealthUI()
    {
        // �M������������UI�ե�
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

    // === �����q��T�]�즳�^ ===
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    // === �ק�G�������` ===
    void Fall()
    {
        if (isDead) return;

        Debug.Log("���a�������`�I");

        // �������`�ɦ���k�s
        currentHealth = 0;
        SaveHealth(); // �O�s���`���A
        UpdateHealthUI();

        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === �Τ@���`�B�z�]�즳�^ ===
    void Die()
    {
        if (isDead) return;

        isDead = true;
        canControl = false;

        // === �s�W�G���`�ɫO�s��q���A ===
        SaveHealth();

        Debug.Log("���a���`�I");
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === �Ȱ��C���]�즳�^ ===
    public void PauseGame()
    {
        Die();
    }

    // === �s�W�G���}��k�Ѩ�L�}���ϥ� ===

    /// <summary>
    /// ��ʫO�s��e��q
    /// </summary>
    public void SaveCurrentHealth()
    {
        SaveHealth();
    }

    /// <summary>
    /// ����O�s����q
    /// </summary>
    public static int GetPersistentHealth()
    {
        return persistentHealth;
    }

    /// <summary>
    /// �]�m��q������]�Ω�s�C���^
    /// </summary>
    public void SetFullHealth()
    {
        currentHealth = maxHealth;
        SaveHealth();
        UpdateHealthUI();
        Debug.Log("[��q�t��] ��q�]������");
    }
}
