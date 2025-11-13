using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBlock : MonoBehaviour
{
    [Header("方塊基本設定")]
    [Tooltip("觸發一次後銷毀")]
    public bool destroyOnTrigger = true;
    [Tooltip("可以多次觸發")]
    public bool canTriggerMultipleTimes = false;
    [Tooltip("多次觸發的冷卻時間")]
    public float triggerCooldown = 2f;

    [Header("效果類型選擇")]
    public bool dropCoins = true;
    public bool dropHealthPickup = false;
    public bool dropManaPickup = false;
    public bool fireAttack = false;
    public bool spawnEnemy = false;

    [Header("掉落金幣設定")]
    public int coinCount = 5;
    public GameObject coinPrefab;
    [Tooltip("金幣的拋出速度")]
    public float coinThrowForce = 3f;

    [Header("掉落血包設定")]
    public int healthPickupCount = 1;
    public GameObject healthPickupPrefab;
    [Tooltip("血包的拋出速度")]
    public float healthThrowForce = 2f;

    [Header("掉落魔力瓶設定")]
    public int manaPickupCount = 1;
    public GameObject manaPickupPrefab;
    [Tooltip("魔力瓶的拋出速度")]
    public float manaThrowForce = 2f;

    [Header("發射攻擊設定")]
    public GameObject bulletPrefab;
    public int bulletCount = 3;
    public float bulletSpeedMultiplier = 1f;
    public float spreadAngle = 30f;
    [Tooltip("子彈發射延遲時間（秒）- 建議 0.3 ~ 1.0")]
    public float bulletFireDelay = 0.5f;
    [Tooltip("每發子彈之間的時間間隔（秒）- 建議 0.2 ~ 0.5")]
    public float bulletFireInterval = 0.3f;
    [Tooltip("發射完成後是否自動銷毀方塊")]
    public bool destroyAfterFire = true;

    [Header("生成敵人設定")]
    public GameObject enemyPrefab;
    public int enemyCount = 1;
    public float spawnRadius = 2f;

    [Header("自訂掉落物設定")]
    [Tooltip("自訂掉落物數量")]
    public int customDropCount = 0;
    [Tooltip("自訂掉落物 Prefab")]
    public GameObject customDropPrefab;
    [Tooltip("自訂掉落物的拋出速度")]
    public float customDropThrowForce = 2f;

    [Header("視覺效果")]
    [Tooltip("方塊觸發後的顏色")]
    public Color activatedColor = Color.gray;
    [Tooltip("搖晃強度")]
    public float shakeIntensity = 0.2f;
    [Tooltip("搖晃持續時間")]
    public float shakeDuration = 0.3f;
    [Tooltip("是否啟用縮放動畫")]
    public bool enableScaleAnimation = true;

    [Header("音效")]
    public AudioClip activationSound;
    [Tooltip("音效音量")]
    public float soundVolume = 1f;

    private bool hasTriggered = false;
    private float lastTriggerTime = -999f;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // 檢查必要的 Prefab
        if (dropCoins && coinPrefab == null)
            Debug.LogWarning("[InteractiveBlock] 未設定金幣 Prefab");
        if (dropHealthPickup && healthPickupPrefab == null)
            Debug.LogWarning("[InteractiveBlock] 未設定血包 Prefab");
        if (dropManaPickup && manaPickupPrefab == null)
            Debug.LogWarning("[InteractiveBlock] 未設定魔力瓶 Prefab");
        if (fireAttack && bulletPrefab == null)
            Debug.LogWarning("[InteractiveBlock] 未設定子彈 Prefab");
        if (spawnEnemy && enemyPrefab == null)
            Debug.LogWarning("[InteractiveBlock] 未設定敵人 Prefab");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TriggerEffect();
        }
    }

    void TriggerEffect()
    {
        // 檢查是否已觸發
        if (hasTriggered && !canTriggerMultipleTimes)
            return;

        // 檢查冷卻時間
        if (Time.time - lastTriggerTime < triggerCooldown)
            return;

        lastTriggerTime = Time.time;

        // 播放音效
        if (activationSound != null && audioSource != null)
        {
            audioSource.clip = activationSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }

        // 視覺反饋
        StartCoroutine(ShakeEffect());
        if (enableScaleAnimation)
            StartCoroutine(ScaleEffect());
        if (spriteRenderer != null)
            StartCoroutine(FlashColor());

        // 執行各種效果
        if (dropCoins)
            DropCoins();

        if (dropHealthPickup)
            DropHealthPickup();

        if (dropManaPickup)
            DropManaPickup();

        if (fireAttack)
            FireAttack();

        if (spawnEnemy)
            SpawnEnemies();

        // 執行自訂掉落物
        if (customDropCount > 0 && customDropPrefab != null)
            DropCustomItems();

        hasTriggered = true;

        Debug.Log("[InteractiveBlock] 方塊已觸發！");

        // 銷毀或禁用方塊
        if (destroyOnTrigger && !canTriggerMultipleTimes)
        {
            StartCoroutine(DestroyAfterDelay(0.5f));
        }
    }

    void DropCoins()
    {
        if (coinPrefab == null)
            return;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.5f;
            spawnPos.z = 0;

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            // 給金幣隨機速度
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * coinThrowForce;
                coinRb.velocity = randomForce;
            }
        }

        Debug.Log($"[InteractiveBlock] 掉落 {coinCount} 個金幣");
    }

    void DropHealthPickup()
    {
        if (healthPickupPrefab == null)
            return;

        for (int i = 0; i < healthPickupCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0);
            spawnPos.z = 0;

            GameObject pickup = Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D pickupRb = pickup.GetComponent<Rigidbody2D>();
            if (pickupRb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * healthThrowForce;
                pickupRb.velocity = randomForce;
            }
        }

        Debug.Log($"[InteractiveBlock] 掉落 {healthPickupCount} 個血包");
    }

    void DropManaPickup()
    {
        if (manaPickupPrefab == null)
            return;

        for (int i = 0; i < manaPickupCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0);
            spawnPos.z = 0;

            GameObject pickup = Instantiate(manaPickupPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D pickupRb = pickup.GetComponent<Rigidbody2D>();
            if (pickupRb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * manaThrowForce;
                pickupRb.velocity = randomForce;
            }
        }

        Debug.Log($"[InteractiveBlock] 掉落 {manaPickupCount} 個魔力瓶");
    }

    void FireAttack()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("[InteractiveBlock] 【重大錯誤】子彈 Prefab 未設定！");
            return;
        }

        Debug.Log($"[InteractiveBlock] ========== 開始發射攻擊 ==========");
        Debug.Log($"[InteractiveBlock] 延遲: {bulletFireDelay}秒, 間隔: {bulletFireInterval}秒, 數量: {bulletCount}");

        // 直接發射所有子彈，不用延遲
        for (int i = 0; i < bulletCount; i++)
        {
            float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0;
            float angle = (angleStep * i) - (spreadAngle / 2f);

            Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            Debug.Log($"[InteractiveBlock] 【立即】發射第 {i + 1} 發子彈 - 角度: {angle}°");

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                float radians = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                bulletRb.velocity = direction * 5f * bulletSpeedMultiplier;

                Debug.Log($"[InteractiveBlock] 子彈 {i + 1} 速度: {bulletRb.velocity}");
            }
            else
            {
                Debug.LogError($"[InteractiveBlock] 子彈 Prefab 沒有 Rigidbody2D 組件！");
            }
        }

        Debug.Log($"[InteractiveBlock] 完成發射 {bulletCount} 發子彈");
        Debug.Log($"[InteractiveBlock] ========== 發射完成 ==========");

        // 立即銷毀方塊
        if (destroyOnTrigger && !canTriggerMultipleTimes)
        {
            Destroy(gameObject);
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[InteractiveBlock] 敵人 Prefab 未設定！");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.z = 0;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            if (enemy != null)
            {
                Debug.Log($"[InteractiveBlock] 成功生成敵人 {i + 1} / {enemyCount}，位置: {spawnPos}");
            }
            else
            {
                Debug.LogError("[InteractiveBlock] 敵人實例化失敗！");
            }
        }

        Debug.Log($"[InteractiveBlock] 生成 {enemyCount} 個敵人完成");
    }

    IEnumerator ShakeEffect()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float shake = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPosition + new Vector3(shake, shake, 0);
            yield return null;
        }

        transform.position = originalPosition;
    }

    IEnumerator ScaleEffect()
    {
        float elapsed = 0f;
        float duration = 0.2f;

        // 縮小
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0.8f, elapsed / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        elapsed = 0f;

        // 放大回原狀
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.8f, 1f, elapsed / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    IEnumerator FlashColor()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = activatedColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // 編輯器中顯示方塊資訊
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

    /// <summary>
    /// 掉落自訂物品（通用方法，可用於任何自訂掉落物）
    /// </summary>
    void DropCustomItems()
    {
        if (customDropPrefab == null)
        {
            Debug.LogWarning("[InteractiveBlock] 自訂掉落物 Prefab 未設定！");
            return;
        }

        for (int i = 0; i < customDropCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0);
            spawnPos.z = 0;

            GameObject customItem = Instantiate(customDropPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D itemRb = customItem.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle.normalized * customDropThrowForce;
                itemRb.velocity = randomForce;
            }
        }

        Debug.Log($"[InteractiveBlock] 掉落 {customDropCount} 個自訂物品");
    }
}