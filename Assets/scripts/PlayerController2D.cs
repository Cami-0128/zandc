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

    // === �s�W�G��q�t�� ===
    [Header("Health System ��q�t��")]
    public int maxHealth = 100;              // �̤j��q
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

        // === �s�W�G��l�Ʀ�q ===
        currentHealth = maxHealth; // �]�m��q������

        Time.timeScale = 1f;
        Debug.Log("Game Start. �C���}�l");

        // === �s�W�G��s���UI ===
        UpdateHealthUI();
    }

    void Update()
    {
        if (!canControl) return;

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

    // === �ק�G�I���˴��]�[�J��q�t�Ρ^ ===
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

    // === �s�W�G���˨t�� ===
    public void TakeDamage(int damage)
    {
        if (isDead) return; // �p�G�w�g���`�A���A����

        currentHealth -= damage;              // ������q
        currentHealth = Mathf.Max(currentHealth, 0); // �T�O��q���|�p��0
        lastDamageTime = Time.time;           // �O�����ˮɶ�

        UpdateHealthUI();                     // ��s���UI

        Debug.Log($"���a���� {damage} �I�ˮ`�C��e��q: {currentHealth}/{maxHealth}");

        // ��q�k�s�ɦ��`
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // === �s�W�G�v���t�Ρ]�i��\��^ ===
    public void Heal(int healAmount)
    {
        if (isDead) return; // �p�G�w�g���`�A�L�k�v��

        currentHealth += healAmount;                          // �W�[��q
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // �T�O��q���|�W�L�̤j��

        UpdateHealthUI();                                     // ��s���UI
        Debug.Log($"���a�^�_ {healAmount} �I��q�C��e��q: {currentHealth}/{maxHealth}");
    }

    // === �s�W�G��s���UI ===
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

    // === �s�W�G�����q��T�]�Ѩ�L�}���ϥΡ^ ===
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
        UpdateHealthUI();

        isDead = true;
        canControl = false;
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === �s�W�G�Τ@���`�B�z ===
    void Die()
    {
        if (isDead) return;

        isDead = true;
        canControl = false;

        Debug.Log("���a���`�I");
        FindObjectOfType<GameManager>().PlayerDied();
    }

    // === �ק�G�Ȱ��C���]�ϥβΤ@���`��k�^ ===
    public void PauseGame()
    {
        Die();
    }
}