using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController2D : MonoBehaviour
{
    // 
    public bool isDead { get; private set; } = false;  // �令���}�uŪ�ݩʡA�ѥ~���ˬd
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
    // === �����d��q�t�� ===
    [Header("Health System ��q�t��")]
    public int maxHealth = 100;              // �̤j��q
    // === �R�A��q�ܼơ]������O���^ ===
    private static int persistentHealth = -1; // -1 ��ܩ|����l��
    private static bool isFirstTimePlay = true; // �O�_�Ĥ@���C��
    public int currentHealth;               // ��e��q
    public int enemyDamage = 15;             // Enemy�y�����ˮ`
    private float damageInvulnerabilityTime = 1f; // ���˵L�Įɶ��]����s�򦩦�^
    private float lastDamageTime = -999f;    // �W�����ˮɶ�
    // === ��]�t�ά��� ===
    [Header("Health Pickup System ��]�t��")]
    public AudioClip healSound;              // �v������
    private AudioSource audioSource;
    // Wall Slide ����
    public float wallSlideSpeed = 1f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    private bool isWallSliding = false;
    private bool isTouchingWall = false;
    // Wall Jump ����
    public float wallJumpForceY = 8f;
    public float wallJumpCooldown = 0.5f;
    private float lastWallJumpTime = -999f;
    // Wall Jump Hang Time ����
    private bool wallJumping = false;
    public float wallJumpHangTime = 0.2f;
    private float wallJumpHangCounter = 0f;
    // �F�|�˼ƭp�ɾ��Ѧ�
    public HourglassTimer hourglassTimer;
    public bool hasReachedEnd = false;
    // �����o�g��V
    public int LastHorizontalDirection { get; private set; } = 1; // ��l�w�]�V�k

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // === �s�W�G���Ĩt�Ϊ�l�� ===
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        // === ��q��l���޿� ===
        InitializeHealth();
        Time.timeScale = 1f;
        Debug.Log($"Game Start. �C���}�l - ��q: {currentHealth}/{maxHealth}");
        // === ��s���UI ===
        UpdateHealthUI();

        // �۰ʴM��hourglassTimer
        if (hourglassTimer == null)
        {
            hourglassTimer = GetComponentInChildren<HourglassTimer>();
            if (hourglassTimer == null)
            {
                Debug.LogWarning("�䤣�� HourglassTimer �ե�I");
            }
        }
    }
    // === ��q��l�Ƥ�k ===
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
    // === �O�s��q��k ===
    void SaveHealth()
    {
        persistentHealth = currentHealth;
        Debug.Log($"[��q�t��] ��q�w�O�s: {persistentHealth}");
    }
    // === ���m��q�t�Ρ]�Ω󭫷s�}�l�C���^ ===
    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[��q�t��] ��q�t�Τw���m");
    }
    void Update()
    {
        if (!canControl) return;
        //// �H�U�ܽd�Ϊť���ҰʨF�|�p�ɡ]�i�令�����C���ƥ�Ĳ�o�^
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    HourglassTimer hourglassTimer = GetComponentInChildren<HourglassTimer>();
        //    if (hourglassTimer != null)
        //    {
        //        hourglassTimer.StartTimer();
        //    }
        //}
        // === ���ի��� ===
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Debug.Log($"[����] ��e��q: {currentHealth}/{maxHealth}, �O�s��q: {persistentHealth}");
        //}
        //// === ���ը��˫��� ===
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    TakeDamage(10);
        //    Debug.Log("[����] ���a���� 10 �I���նˮ`");
        //}
        // �즳���C���޿�
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();
        // �������`�˴�
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
    // === ���������ɫO�s��q ===
    void OnDestroy()
    {
        if (!isDead && currentHealth > 0)
        {
            SaveHealth();
        }
    }
    // �����޿�
    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX != 0)
        {
            LastHorizontalDirection = (int)Mathf.Sign(moveX);
        }
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);
    }

    // ���D��J�B�z
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
    // ���D
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount++;
    }
    // ���
    void WallJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, wallJumpForceY);
        lastWallJumpTime = Time.time;
        wallJumping = true;
        wallJumpHangCounter = wallJumpHangTime;
    }
    // ��s�������ɶ�
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
    // �ˬd���
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
    // === �I���˴� ===
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
        // �a���˴�
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0;
            }
        }
    }
    // ���}�I��
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
    // === �ק�G�v���t�Ρ]�W�j���^ ===
    public void Heal(int healAmount)
    {
        Debug.Log($"Heal��ƳQ�I�s�AhealAmount = {healAmount}");
        if (isDead) return; // �p�G�w�g���`�A�L�k�v��

        int oldHealth = currentHealth;
        currentHealth += healAmount;                          // �W�[��q
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // �T�O��q���|�W�L�̤j��

        if (healSound != null && audioSource != null)
            audioSource.PlayOneShot(healSound);

        SaveHealth();
        UpdateHealthUI();

        Debug.Log($"�^�� {currentHealth - oldHealth} �I�A�ثe��q: {currentHealth}/{maxHealth}");

        StartCoroutine(HealFeedback());
    }
    // === �s�W�G�v�����X�ĪG��{ ===
    IEnumerator HealFeedback()
    {
        // �i�H�b�o�̲K�[��ı���X�A��p���ܪ��a�C��
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.green; // �u���ܺ��ܪv��
            yield return new WaitForSeconds(0.2f);
            sr.color = originalColor;
        }
    }
    // === ��s���UI ===
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
    // === �����q��T ===
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    // === �s�W�G�ˬd�O�_�i�H�v�� ===
    public bool CanHeal()
    {
        return !isDead && currentHealth < maxHealth;
    }
    // === �s�W�G�����q�ʤ��� ===
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
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
    // === �Τ@���`�B�z ===
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        canControl = false;
        // === �s�W�G���`�ɫO�s��q���A ===
        SaveHealth();
        Debug.Log("���a���`�I");
        FindObjectOfType<GameManager>().PlayerDied();
    }
    // === �Ȱ��C�� ===
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
    /// <summary>
    /// �]�m�S�w��q��
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SaveHealth();
        UpdateHealthUI();
        Debug.Log($"[��q�t��] ��q�]��: {currentHealth}/{maxHealth}");
    }
}