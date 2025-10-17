using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("敵人屬性")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("血條設定")]
    public EnemyHealthBar healthBar;

    [Header("跳躍設定")]
    public float jumpForceVertical = 8f;          // 垂直跳躍力
    public float jumpForceHorizontalMax = 5f;     // 最大水平跳躍力
    public float jumpInterval = 2f;               // 跳躍間隔（秒）

    [Header("追蹤玩家設定")]
    public bool enablePlayerTracking = true;
    public float trackingRange = 40f;             // 追蹤觸發範圍
    public int maxJumpCount = 2;                  // 最多允許跳幾次（雙跳設2）
    public float groundCheckDistance = 0.6f;      // 地面偵測（Raycast）距離

    [Header("外觀設定")]
    public Color enemyColor = Color.red;

    [Header("死亡特效")]
    public int fragmentCount = 15;
    public float fragmentSpeed = 5f;
    public GameObject fragmentPrefab;

    [Header("金幣掉落設定")]
    public GameObject coinPrefab;
    public bool dropCoins = true;
    [Range(1, 20)]
    public int minCoins = 2;
    [Range(1, 20)]
    public int maxCoins = 5;
    public float coinDropRadius = 1f;

    [Header("Debug 設定")]
    public bool debugMode = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform playerTransform;
    private bool isJumping = false;
    private bool isDying = false;

    private int currentJumpCount = 0;
    private HashSet<GameObject> processedBullets = new HashSet<GameObject>();
    private float lastDamageTime = -999f;
    private float damageCooldown = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        currentHealth = maxHealth;

        SetupAppearance();

        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyHealthBar>();

        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        StartCoroutine(JumpRoutine());
    }

    void SetupAppearance()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = enemyColor;

        if (trailRenderer == null)
            trailRenderer = gameObject.AddComponent<TrailRenderer>();

        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailMaterial.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, 0.5f);
        trailRenderer.material = trailMaterial;
        trailRenderer.startWidth = 0.4f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.time = 0.3f;
        trailRenderer.sortingOrder = -1;
    }

    IEnumerator JumpRoutine()
    {
        while (currentHealth > 0 && !isDying)
        {
            float distanceToPlayer = (playerTransform != null) ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

            // 重點：無論地面與否，「只要跳躍次數還沒超過」且「玩家在追蹤距離內」都允許執行跳躍
            if ((IsGrounded() || currentJumpCount < maxJumpCount) && distanceToPlayer <= trackingRange)
            {
                PerformJump();
            }

            yield return new WaitForSeconds(jumpInterval);
        }
    }

    void PerformJump()
    {
        if (rb == null || isDying) return;
        isJumping = true;

        // 預設空中連跳時垂直力稍降以防過高（可調整或關掉）
        float verticalForce = jumpForceVertical * (currentJumpCount == 0 ? 1f : 0.9f);

        float horizontalForce = 0f;

        if (enablePlayerTracking && playerTransform != null)
        {
            float dx = playerTransform.position.x - transform.position.x;
            float normalizedHorizontal = Mathf.Clamp(dx / trackingRange, -1f, 1f);
            horizontalForce = normalizedHorizontal * jumpForceHorizontalMax;

            // 角色面向
            if (horizontalForce != 0 && spriteRenderer != null)
                spriteRenderer.flipX = horizontalForce < 0;
        }

        // 跳前歸零水平速度，避免飄移
        rb.velocity = new Vector2(0f, rb.velocity.y);

        rb.AddForce(new Vector2(horizontalForce, verticalForce), ForceMode2D.Impulse);

        currentJumpCount++;
        StartCoroutine(JumpEndCheck());
    }

    IEnumerator JumpEndCheck()
    {
        yield return new WaitForSeconds(0.1f);
        while (rb.velocity.y > 0.1f || !IsGrounded())
            yield return new WaitForFixedUpdate();
        isJumping = false;
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void Update()
    {
        if (isDying) return;

        if (IsGrounded()) currentJumpCount = 0;

        if (spriteRenderer != null && spriteRenderer.color != enemyColor && spriteRenderer.color != Color.white)
            spriteRenderer.color = enemyColor;
        if (transform.position.y < -15f) Die();
    }

    public void TakeDamage(float damage, string damageSource = "Unknown")
    {
        if (isDying) return;

        if (Time.time - lastDamageTime < damageCooldown)
        {
            if (debugMode)
            {
                Debug.Log($"[Enemy] {gameObject.name} 傷害冷卻中，忽略此次傷害");
            }
            return;
        }

        lastDamageTime = Time.time;

        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 受到 {damage} 點傷害 (來源: {damageSource})");
            Debug.Log($"[Enemy] 血量變化: {oldHealth} → {currentHealth} / {maxHealth}");
        }

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StopCoroutine("FlashEffect");
            spriteRenderer.color = enemyColor;
            StartCoroutine(FlashEffect());
        }
    }

    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = enemyColor;

        for (int i = 0; i < 3; i++)
        {
            if (isDying)
            {
                spriteRenderer.color = originalColor;
                yield break;
            }

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            if (isDying)
            {
                spriteRenderer.color = originalColor;
                yield break;
            }

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        if (!isDying && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        if (isDying) return;

        isDying = true;

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 死亡！");
        }

        StopAllCoroutines();

        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

        DropCoins();

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        StartCoroutine(FragmentationEffect());
    }

    void DropCoins()
    {
        if (!dropCoins || coinPrefab == null)
        {
            return;
        }

        int coinCount = Random.Range(minCoins, maxCoins + 1);
        Vector3 dropPosition = transform.position;

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * coinDropRadius;
            Vector3 spawnPos = dropPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            CoinPickup coinPickup = coin.GetComponent<CoinPickup>();
            if (coinPickup != null)
            {
                coinPickup.value = 1;
            }

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomForce = Random.Range(2f, 5f);
                coinRb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);

                float randomTorque = Random.Range(-10f, 10f);
                coinRb.AddTorque(randomTorque);
            }
        }

        if (debugMode)
        {
            Debug.Log($"[Enemy] 掉落 {coinCount} 個金幣");
        }
    }

    IEnumerator FragmentationEffect()
    {
        Vector3 centerPosition = transform.position;

        spriteRenderer.enabled = false;
        if (trailRenderer != null) trailRenderer.enabled = false;

        for (int i = 0; i < fragmentCount; i++)
        {
            CreateFragment(centerPosition, i);
        }

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    void CreateFragment(Vector3 center, int fragmentIndex)
    {
        GameObject fragment;

        if (fragmentPrefab != null)
            fragment = Instantiate(fragmentPrefab, center, Quaternion.identity);
        else
        {
            fragment = new GameObject($"Fragment_{fragmentIndex}");
            fragment.transform.position = center;

            SpriteRenderer fragmentSprite = fragment.AddComponent<SpriteRenderer>();
            fragmentSprite.sprite = spriteRenderer.sprite;
            fragmentSprite.color = enemyColor;

            float scale = Random.Range(0.1f, 0.3f);
            fragment.transform.localScale = Vector3.one * scale;
        }

        Rigidbody2D fragmentRb = fragment.GetComponent<Rigidbody2D>();
        if (fragmentRb == null)
            fragmentRb = fragment.AddComponent<Rigidbody2D>();

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomSpeed = Random.Range(fragmentSpeed * 0.5f, fragmentSpeed * 1.5f);

        fragmentRb.AddForce(randomDirection * randomSpeed, ForceMode2D.Impulse);
        fragmentRb.AddTorque(Random.Range(-10f, 10f));

        StartCoroutine(FadeOutFragment(fragment));
    }

    IEnumerator FadeOutFragment(GameObject fragment)
    {
        SpriteRenderer fragmentSprite = fragment.GetComponent<SpriteRenderer>();
        if (fragmentSprite == null) yield break;

        Color originalColor = fragmentSprite.color;
        float fadeTime = 2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            Color newColor = originalColor;
            newColor.a = alpha;
            fragmentSprite.color = newColor;

            yield return null;
        }

        Destroy(fragment);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 觸發碰撞: {other.gameObject.name} (Tag: {other.tag})");
        }

        if (other.CompareTag("Player"))
        {
            return;
        }

        MagicBullet bullet = other.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            if (processedBullets.Contains(other.gameObject))
            {
                return;
            }

            processedBullets.Add(other.gameObject);
            TakeDamage(bullet.damage, "MagicBullet");
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Enemy") || other.CompareTag("Enemy1"))
        {
            TakeDamage(30f, "EnemyCollision");
            return;
        }
    }

    void OnDestroy()
    {
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
        }

        processedBullets.Clear();
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void Heal(float healAmount)
    {
        if (isDying) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void ShowHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Show();
        }
    }

    public void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Hide();
        }
    }
}
