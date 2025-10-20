using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    //
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

    // === �����d��q�t�� ===
    [Header("Health System ��q�t��")]
    public int maxHealth = 100;
    private static int persistentHealth = -1;
    private static bool isFirstTimePlay = true;
    public int currentHealth;
    public int enemyDamage = 15;
    private float damageInvulnerabilityTime = 1f;
    private float lastDamageTime = -999f;

    // === ��]�t�ά��� ===
    [Header("Health Pickup System ��]�t��")]
    public AudioClip healSound;
    private AudioSource audioSource;

    // === �]�O ===
    public int maxMana = 100;
    public int currentMana = 100;

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
    public int LastHorizontalDirection { get; private set; } = 1;

    // === �s�W�G���C�t�� ===
    [Header("Sword Slash System ���C�t��")]
    public GameObject swordObject; // �C��GameObject
    public float slashDuration = 0.3f; // ���C����ɶ�
    public float startAngle = 60f; // �_�l����
    public float endAngle = -60f; // ��������
    public int trailCount = 8; // �ݼv�ƶq
    public float trailFadeDuration = 0.5f; // �ݼv�����ɶ�
    public Color trailColor = new Color(0.7f, 0.7f, 0.7f, 0.5f); // �ݼv�C��

    private bool isSlashing = false;
    private float slashTimer = 0f;
    private Vector3 originalSwordRotation;
    private Vector3 originalSwordLocalPosition; // �O���C����l�۹��m
    private SpriteRenderer swordSpriteRenderer;
    private float lastTrailTime = 0f; // �O���W���Ыشݼv���ɶ�

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // === ���Ĩt�Ϊ�l�� ===
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // === MP�]�O�� ===
        currentMana = maxMana;
        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);

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

        // === ��l�ƼC�t�� ===
        InitializeSword();
    }

    // === ��l�ƼC ===
    void InitializeSword()
    {
        if (swordObject != null)
        {
            originalSwordRotation = swordObject.transform.localEulerAngles;
            originalSwordLocalPosition = swordObject.transform.localPosition;

            // ����C��SpriteRenderer
            swordSpriteRenderer = swordObject.GetComponent<SpriteRenderer>();
            if (swordSpriteRenderer == null)
            {
                Debug.LogWarning("�C����S��SpriteRenderer�ե�I");
            }
        }
        else
        {
            Debug.LogWarning("���]�w�C����I�ЦbInspector�����wswordObject");
        }
    }

    // === ��q��l�Ƥ�k ===
    void InitializeHealth()
    {
        if (isFirstTimePlay || persistentHealth <= 0)
        {
            currentHealth = maxHealth;
            persistentHealth = maxHealth;
            isFirstTimePlay = false;
            Debug.Log("[��q�t��] �Ĥ@���C���A��q�]������");
        }
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

    // === ���m��q�t�� ===
    public static void ResetHealthSystem()
    {
        persistentHealth = -1;
        isFirstTimePlay = true;
        Debug.Log("[��q�t��] ��q�t�Τw���m");
    }

    public void ManaHeal(int manaAmount)
    {
        if (isDead) return;
        currentMana += manaAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        Debug.Log($"�]�O�^�_�F {manaAmount} �I�A�{�b�]�O: {currentMana}/{maxMana}");

        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
            manaBar.UpdateManaBar(currentMana, maxMana);
    }

    void Update()
    {
        if (!canControl) return;

        // �즳���C���޿�
        Move();
        HandleJumpInput();
        UpdateWallJumpHangTime();
        CheckWallSliding();

        // === �s�W�G��s�C����m�M��V ===
        UpdateSwordPosition();

        // === �s�W�G���C��J�˴� ===
        HandleSwordSlash();

        // �������`�˴�
        if (transform.position.y < fall)
        {
            Fall();
        }
    }

    // === �s�W�G��s�C����m�]���H���a��V�^ ===
    void UpdateSwordPosition()
    {
        if (swordObject == null) return;

        // �ھڪ��a�¦V�վ�C����m
        Vector3 newLocalPosition = originalSwordLocalPosition;
        if (LastHorizontalDirection == -1) // �¥�
        {
            newLocalPosition.x = -Mathf.Abs(originalSwordLocalPosition.x);
        }
        else // �¥k
        {
            newLocalPosition.x = Mathf.Abs(originalSwordLocalPosition.x);
        }

        swordObject.transform.localPosition = newLocalPosition;
    }

    // === �B�z���C ===
    void HandleSwordSlash()
    {
        // ��W��o�ʴ��C
        if (Input.GetKeyDown(KeyCode.W) && !isSlashing && swordObject != null)
        {
            StartSlash();
        }

        // �B�z���C�ʵe
        if (isSlashing)
        {
            slashTimer += Time.deltaTime;
            float progress = slashTimer / slashDuration;

            if (progress >= 1f)
            {
                // ���C����
                isSlashing = false;
                slashTimer = 0f;
                lastTrailTime = 0f;
                swordObject.transform.localEulerAngles = originalSwordRotation;
            }
            else
            {
                // �p���e���ס]�ϥ�easeInOutQuad���ʧ@��y�Z�^
                float easedProgress = progress < 0.5f
                    ? 2f * progress * progress
                    : 1f - Mathf.Pow(-2f * progress + 2f, 2f) / 2f;

                float currentAngle = Mathf.Lerp(startAngle, endAngle, easedProgress);

                // �ھڪ��a�¦V�վ�C������
                Vector3 newRotation = originalSwordRotation;
                if (LastHorizontalDirection == -1) // �¥����蹳����
                {
                    newRotation.z = -currentAngle + 180f; // �[180��½��C
                }
                else // �¥k
                {
                    newRotation.z = currentAngle;
                }
                swordObject.transform.localEulerAngles = newRotation;

                // �Ыشݼv�]�ϥΩT�w�ɶ����j�Ӥ��O�i�צʤ���^
                float trailInterval = slashDuration / trailCount;
                if (slashTimer - lastTrailTime >= trailInterval)
                {
                    CreateSwordTrail();
                    lastTrailTime = slashTimer;
                }
            }
        }
    }

    // === �}�l���C ===
    void StartSlash()
    {
        isSlashing = true;
        slashTimer = 0f;
        lastTrailTime = 0f;
        Debug.Log("���C�I");
    }

    // === �ЫؼC���ݼv ===
    void CreateSwordTrail()
    {
        if (swordObject == null || swordSpriteRenderer == null) return;

        // �ЫشݼvGameObject
        GameObject trail = new GameObject("SwordTrail");
        trail.transform.position = swordObject.transform.position;
        trail.transform.rotation = swordObject.transform.rotation;
        trail.transform.localScale = swordObject.transform.lossyScale; // �ϥΥ@���Y��

        // �K�[SpriteRenderer����ܴݼv
        SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = swordSpriteRenderer.sprite;
        trailRenderer.color = trailColor;
        trailRenderer.sortingLayerName = swordSpriteRenderer.sortingLayerName;
        trailRenderer.sortingOrder = swordSpriteRenderer.sortingOrder - 1; // �b�C�᭱

        // �K�[�H�X�}��
        SwordTrailFade fade = trail.AddComponent<SwordTrailFade>();
        fade.fadeDuration = trailFadeDuration;
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

// === �ݼv�H�X�}�� ===
public class SwordTrailFade : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);

        Color newColor = startColor;
        newColor.a = alpha;
        spriteRenderer.color = newColor;

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}