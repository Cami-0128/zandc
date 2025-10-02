using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("敵人屬性")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("血條設定")]
    public GameObject healthBarPrefab;  // 血條 Prefab
    private EnemyHealthBar healthBar;   // 血條腳本引用

    [Header("移動設定")]
    public float jumpForce = 8f;
    public float jumpHeight = 5f;
    public float jumpInterval = 2f;

    [Header("追蹤玩家設定")]
    public bool enablePlayerTracking = true;

    [Header("外觀設定")]
    public Color enemyColor = Color.red;

    [Header("死亡特效")]
    public int fragmentCount = 15;
    public float fragmentSpeed = 5f;
    public GameObject fragmentPrefab;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform playerTransform;
    private bool isJumping = false;
    private bool isDying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        currentHealth = maxHealth;

        SetupAppearance();

        // === 創建血條 ===
        CreateHealthBar();

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

        GradientColorKey[] colorKeys = new GradientColorKey[2]
        {
            new GradientColorKey(enemyColor, 0f),
            new GradientColorKey(enemyColor, 1f)
        };
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(0.8f, 0f),
            new GradientAlphaKey(0f, 1f)
        };

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        trailRenderer.colorGradient = gradient;
    }

    // === 新增：創建血條 ===
    void CreateHealthBar()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} 未設定 Health Bar Prefab！");
            return;
        }

        GameObject healthBarObj = Instantiate(healthBarPrefab);
        healthBar = healthBarObj.GetComponent<EnemyHealthBar>();

        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogError("[Enemy] Health Bar Prefab 缺少 EnemyHealthBar 組件！");
        }
    }

    IEnumerator JumpRoutine()
    {
        PerformJump();

        while (currentHealth > 0 && !isDying)
        {
            yield return new WaitForSeconds(jumpInterval);
            if (!isJumping && !isDying)
                PerformJump();
        }
    }

    void PerformJump()
    {
        if (rb == null || isDying) return;

        isJumping = true;

        Vector2 jumpDirection;
        if (enablePlayerTracking && playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            jumpDirection = new Vector2(dir.x, jumpHeight).normalized;
        }
        else
        {
            jumpDirection = new Vector2(0, jumpHeight).normalized;
        }

        rb.velocity = Vector2.zero;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void Update()
    {
        if (isDying) return;

        if (transform.position.y < -15f)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;

        currentHealth -= damage;

        // === 更新血條 ===
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
            StartCoroutine(FlashEffect());
        }
    }

    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Die()
    {
        if (isDying) return;

        isDying = true;

        StopAllCoroutines();

        // === 銷毀血條 ===
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            Destroy(trailRenderer.gameObject, 0.1f);
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        StartCoroutine(FragmentationEffect());
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

        yield return new WaitForSeconds(3f);

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

        if (other.CompareTag("Player"))
            return;

        MagicBullet bullet = other.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            TakeDamage(50f);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Enemy") || other.CompareTag("Enemy1"))
        {
            TakeDamage(50f);
            return;
        }
    }

    // === 清理血條 ===
    void OnDestroy()
    {
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
        }
    }
}