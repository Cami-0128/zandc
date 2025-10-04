using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 敵人系統 - 完全修正版
/// 防止重複扣血和血條跟隨問題
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("敵人屬性")]
    [Tooltip("最大血量")]
    public float maxHealth = 100f;

    [Tooltip("當前血量（只讀，僅供查看）")]
    [SerializeField] private float currentHealth;

    [Header("敵人類型設定")]
    [Tooltip("敵人類型名稱（例如：史萊姆、哥布林、Boss）")]
    public string enemyType = "普通敵人";

    [Tooltip("敵人等級")]
    [Range(1, 100)]
    public int enemyLevel = 1;

    [Tooltip("是否為 Boss")]
    public bool isBoss = false;

    [Header("獎勵設定")]
    [Tooltip("擊殺後獲得的經驗值")]
    public int expReward = 10;

    [Tooltip("擊殺後掉落的金幣")]
    public int goldReward = 5;

    [Tooltip("掉落物品機率 (0-1)")]
    [Range(0f, 1f)]
    public float dropChance = 0.1f;
    [Tooltip("血條腳本（從子物件自動獲取或手動拖入）")]
    public EnemyHealthBar healthBar;

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

    [Header("傷害設定")]
    [Tooltip("接觸玩家時造成的傷害")]
    public float contactDamage = 10f;
    [Tooltip("接觸傷害冷卻時間")]
    public float contactDamageCooldown = 1f;
    private float lastContactDamageTime = -999f;

    [Header("Debug 設定")]
    public bool debugMode = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Transform playerTransform;
    private bool isJumping = false;
    private bool isDying = false;

    // 防止重複傷害的機制
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

        // 可選：自動根據等級調整血量（如果你想啟用這個功能）
        // ApplyLevelScaling();

        // 從子物件獲取血條
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<EnemyHealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.Initialize(this.transform);
            healthBar.UpdateHealthBar(currentHealth, maxHealth);

            if (debugMode)
            {
                Debug.Log($"[Enemy] {gameObject.name} 找到血條組件");
            }
        }
        else
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} 找不到 EnemyHealthBar 組件！");
        }

        StartCoroutine(JumpRoutine());

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} ({enemyType} Lv.{enemyLevel}) 初始化完成");
            Debug.Log($"[Enemy] 血量: {currentHealth}/{maxHealth}");
            Debug.Log($"[Enemy] 獎勵: {expReward} EXP, {goldReward} Gold");
        }
    }

    void SetupAppearance()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

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

    /// <summary>
    /// 確保敵人顏色正確（修復用）
    /// </summary>
    void EnsureCorrectColor()
    {
        if (spriteRenderer != null && !isDying)
        {
            spriteRenderer.color = enemyColor;
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

        // 定期檢查並修正顏色（防止卡在白色）
        if (spriteRenderer != null && spriteRenderer.color != enemyColor && spriteRenderer.color != Color.white)
        {
            spriteRenderer.color = enemyColor;
        }

        if (transform.position.y < -15f)
        {
            Die();
        }
    }

    /// <summary>
    /// 敵人受到傷害
    /// </summary>
    public void TakeDamage(float damage, string damageSource = "Unknown")
    {
        if (isDying) return;

        // 防止短時間內重複扣血
        if (Time.time - lastDamageTime < damageCooldown)
        {
            if (debugMode)
            {
                Debug.Log($"[Enemy] {gameObject.name} 傷害冷卻中，忽略此次傷害");
            }
            return;
        }

        lastDamageTime = Time.time;

        // 扣血
        float oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 受到 {damage} 點傷害 (來源: {damageSource})");
            Debug.Log($"[Enemy] 血量變化: {oldHealth} → {currentHealth} / {maxHealth}");
        }

        // 更新血條
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        // 檢查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 停止之前的閃爍效果（如果有）
            StopCoroutine("FlashEffect");
            // 確保顏色正確後開始新的閃爍
            spriteRenderer.color = enemyColor;
            StartCoroutine(FlashEffect());
        }
    }

    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        // 確保使用正確的敵人顏色
        Color originalColor = enemyColor;

        for (int i = 0; i < 3; i++)
        {
            if (isDying)
            {
                // 如果死亡，恢復顏色後退出
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

        // 確保最後恢復原色
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
            Debug.Log($"[Enemy] {gameObject.name} ({enemyType} Lv.{enemyLevel}) 死亡！");
            Debug.Log($"[Enemy] 獲得獎勵: {expReward} EXP, {goldReward} Gold");
        }

        // 給予玩家獎勵（未來可以連接到玩家系統）
        GiveRewards();

        StopAllCoroutines();

        // 立即銷毀血條
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

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

        // 禁用所有 Collider
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        StartCoroutine(FragmentationEffect());
    }

    /// <summary>
    /// 給予玩家獎勵
    /// </summary>
    void GiveRewards()
    {
        // TODO: 連接到玩家的經驗值和金幣系統
        // 例如：GameManager.Instance.AddExp(expReward);
        //      GameManager.Instance.AddGold(goldReward);

        // 掉落物品判定
        if (Random.value <= dropChance)
        {
            Debug.Log($"[Enemy] {enemyType} 掉落了物品！");
            // TODO: 生成掉落物品
            // Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator FragmentationEffect()
    {
        Vector3 centerPosition = transform.position;

        // 立即隱藏敵人本體
        spriteRenderer.enabled = false;
        if (trailRenderer != null) trailRenderer.enabled = false;

        // 生成碎片
        for (int i = 0; i < fragmentCount; i++)
        {
            CreateFragment(centerPosition, i);
        }

        // 縮短等待時間
        yield return new WaitForSeconds(2f);

        // 立即銷毀敵人物件
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

        // 與玩家接觸造成傷害
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastContactDamageTime >= contactDamageCooldown)
            {
                PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();
                if (playerAttack != null)
                {
                    playerAttack.TakeDamage(contactDamage);
                    lastContactDamageTime = Time.time;

                    if (debugMode)
                    {
                        Debug.Log($"[Enemy] 對玩家造成 {contactDamage} 點接觸傷害");
                    }
                }
            }
            return;
        }

        // 魔法子彈傷害
        MagicBullet bullet = other.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            // 檢查是否已經處理過這個子彈
            if (processedBullets.Contains(other.gameObject))
            {
                if (debugMode)
                {
                    Debug.Log($"[Enemy] 子彈已處理過，忽略: {other.gameObject.name}");
                }
                return;
            }

            // 標記為已處理
            processedBullets.Add(other.gameObject);

            // 造成傷害
            TakeDamage(bullet.damage, "MagicBullet");

            // 銷毀子彈
            Destroy(other.gameObject);
            return;
        }

        // 與其他敵人碰撞的傷害
        if (other.CompareTag("Enemy") || other.CompareTag("Enemy1"))
        {
            TakeDamage(30f, "EnemyCollision");
            return;
        }
    }

    void OnDestroy()
    {
        // 確保血條被銷毀
        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
        }

        processedBullets.Clear();
    }

    // 公開方法

    /// <summary>
    /// 獲取當前血量百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// 獲取當前血量
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 獲取最大血量
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 設定最大血量（保持當前血量百分比）
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        if (newMaxHealth <= 0)
        {
            Debug.LogWarning("[Enemy] 最大血量必須大於 0！");
            return;
        }

        float healthPercentage = GetHealthPercentage();
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthPercentage;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (debugMode)
        {
            Debug.Log($"[Enemy] 最大血量設為 {maxHealth}，當前血量: {currentHealth}");
        }
    }

    /// <summary>
    /// 設定當前血量（直接設定）
    /// </summary>
    public void SetCurrentHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 根據等級自動調整血量
    /// </summary>
    public void ApplyLevelScaling()
    {
        // 每級增加 10% 基礎血量
        float scaledHealth = maxHealth * (1 + (enemyLevel - 1) * 0.1f);

        // Boss 額外增加 50% 血量
        if (isBoss)
        {
            scaledHealth *= 1.5f;
        }

        maxHealth = scaledHealth;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (debugMode)
        {
            Debug.Log($"[Enemy] {enemyType} Lv.{enemyLevel} 血量調整為: {maxHealth}");
        }
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

        if (debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 回復 {healAmount} 血量，當前: {currentHealth}/{maxHealth}");
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