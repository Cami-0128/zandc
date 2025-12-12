using UnityEngine;
using System.Collections;

/// <summary>
/// 暴風雪系統 - 管理整體暴風雪效果和傷害邏輯
/// 雪球從右側飄來，帶有風吹效果，會推開並傷害玩家
/// </summary>
public class BlizzardSystem : MonoBehaviour
{
    [Header("暴風雪範圍設置")]
    [Tooltip("雪球生成的右邊界")]
    public float spawnRight = 12f;
    [Tooltip("雪球生成的上邊界")]
    public float spawnTop = 8f;
    [Tooltip("雪球生成的下邊界")]
    public float spawnBottom = -2f;
    [Tooltip("雪球消失的左邊界")]
    public float destroyLeft = -12f;

    [Header("雪球設置")]
    [Tooltip("雪球預製物")]
    public GameObject snowballPrefab;
    [Tooltip("每秒生成的雪球數量")]
    public int snowballSpawnRate = 5;
    [Tooltip("雪球大小範圍（最小-最大）")]
    public Vector2 snowballSizeRange = new Vector2(0.3f, 0.8f);

    [Header("風力設置")]
    [Tooltip("風力強度（1-10，影響整體速度）")]
    [Range(1f, 10f)]
    public float windStrength = 5f;
    [Tooltip("基礎水平速度")]
    public float baseHorizontalSpeed = 3f;
    [Tooltip("基礎垂直速度（下飄速度）")]
    public float baseVerticalSpeed = 1f;
    [Tooltip("風力波動頻率（模擬陣風）")]
    public float windWaveFrequency = 2f;
    [Tooltip("風力波動強度")]
    public float windWaveAmplitude = 1.5f;

    [Header("飄動效果")]
    [Tooltip("上下飄動的振幅")]
    public float verticalDriftAmount = 0.5f;
    [Tooltip("左右飄動的振幅")]
    public float horizontalDriftAmount = 0.3f;
    [Tooltip("飄動速度")]
    public float driftSpeed = 1.5f;

    [Header("視覺效果")]
    [Tooltip("雪球顏色")]
    public Color snowballColor = new Color(1f, 1f, 1f, 0.9f);
    [Tooltip("啟用調試區域顯示")]
    public bool showDebugArea = true;

    [Header("傷害與推力設置")]
    [Tooltip("碰到雪球造成的血量傷害")]
    public int healthDamage = 3;
    [Tooltip("碰到雪球造成的魔力傷害")]
    public int manaDamage = 5;
    [Tooltip("推開玩家的力度（水平方向）")]
    public float pushForce = 10f;
    [Tooltip("推力持續時間（秒）")]
    public float pushDuration = 0.3f;
    [Tooltip("傷害冷卻時間（秒）")]
    public float damageCooldown = 1f;

    [Header("物理設置")]
    [Tooltip("地板的Tag名稱")]
    public string groundTag = "Ground";
    [Tooltip("雪球的物理材質（可選）")]
    public PhysicsMaterial2D snowballPhysicsMaterial;

    private Transform spawnParent;
    private float nextSpawnTime = 0f;
    private float currentWindMultiplier = 1f;

    void Start()
    {
        GameObject snowContainer = new GameObject("SnowballContainer");
        snowContainer.transform.parent = transform;
        spawnParent = snowContainer.transform;

        if (snowballPrefab == null)
        {
            snowballPrefab = CreateDefaultSnowball();
        }

        Debug.Log("[BlizzardSystem] 暴風雪系統已初始化");
    }

    void Update()
    {
        // 更新風力波動（模擬陣風）
        UpdateWindStrength();

        // 生成雪球
        if (Time.time >= nextSpawnTime)
        {
            SpawnSnowball();
            nextSpawnTime = Time.time + (1f / snowballSpawnRate);
        }

        if (showDebugArea)
        {
            DrawDebugArea();
        }
    }

    void UpdateWindStrength()
    {
        // 使用正弦波模擬風力變化
        float wave = Mathf.Sin(Time.time * windWaveFrequency) * windWaveAmplitude;
        currentWindMultiplier = 1f + (wave * 0.1f); // 風力在 0.9-1.1 倍之間波動
    }

    void SpawnSnowball()
    {
        // 在右側邊界隨機高度生成
        float randomY = Random.Range(spawnBottom, spawnTop);
        Vector3 spawnPos = new Vector3(spawnRight, randomY, 0f);

        GameObject snowball = Instantiate(snowballPrefab, spawnPos, Quaternion.identity, spawnParent);
        Snowball snowballScript = snowball.GetComponent<Snowball>();

        if (snowballScript != null)
        {
            // 隨機大小
            float size = Random.Range(snowballSizeRange.x, snowballSizeRange.y);
            snowball.transform.localScale = Vector3.one * size;

            // 計算速度（受風力影響）
            float horizontalSpeed = baseHorizontalSpeed * windStrength * currentWindMultiplier;
            float verticalSpeed = baseVerticalSpeed * windStrength * 0.5f;

            // 隨機飄動相位
            float driftPhase = Random.Range(0f, Mathf.PI * 2f);

            snowballScript.Initialize(
                horizontalSpeed,
                verticalSpeed,
                destroyLeft,
                verticalDriftAmount,
                horizontalDriftAmount,
                driftSpeed,
                driftPhase
            );
        }
    }

    GameObject CreateDefaultSnowball()
    {
        GameObject prefab = new GameObject("Snowball");

        // 添加 SpriteRenderer
        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.color = snowballColor;
        sr.sprite = CreateSnowballSprite();
        sr.sortingOrder = 5;

        // 添加兩個 CircleCollider2D
        // 第一個：實體碰撞（與玩家互動）
        CircleCollider2D physicalCollider = prefab.AddComponent<CircleCollider2D>();
        physicalCollider.radius = 0.5f;
        physicalCollider.isTrigger = false;

        // 如果有物理材質，應用它
        if (snowballPhysicsMaterial != null)
        {
            physicalCollider.sharedMaterial = snowballPhysicsMaterial;
        }

        // 第二個：Trigger 檢測（檢測地板）
        CircleCollider2D triggerCollider = prefab.AddComponent<CircleCollider2D>();
        triggerCollider.radius = 0.55f; // 稍微大一點，確保能檢測到
        triggerCollider.isTrigger = true;

        // 添加 Rigidbody2D
        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 添加 Snowball 腳本
        prefab.AddComponent<Snowball>();

        return prefab;
    }

    Sprite CreateSnowballSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    // 創建漸變效果（中心亮，邊緣稍暗）
                    float normalizedDistance = distance / radius;
                    float brightness = 1f - (normalizedDistance * 0.3f);

                    Color pixelColor = snowballColor * brightness;
                    pixelColor.a = snowballColor.a * (1f - normalizedDistance * 0.5f);

                    pixels[y * size + x] = pixelColor;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;

        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
    }

    void DrawDebugArea()
    {
        // 繪製生成區域
        Debug.DrawLine(
            new Vector3(spawnRight, spawnTop, 0),
            new Vector3(spawnRight, spawnBottom, 0),
            Color.cyan
        );
        Debug.DrawLine(
            new Vector3(destroyLeft, spawnTop, 0),
            new Vector3(destroyLeft, spawnBottom, 0),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(destroyLeft, spawnTop, 0),
            new Vector3(spawnRight, spawnTop, 0),
            Color.yellow
        );
        Debug.DrawLine(
            new Vector3(destroyLeft, spawnBottom, 0),
            new Vector3(spawnRight, spawnBottom, 0),
            Color.yellow
        );
    }

    public int GetHealthDamage() => healthDamage;
    public int GetManaDamage() => manaDamage;
    public float GetPushForce() => pushForce;
    public float GetPushDuration() => pushDuration;
    public float GetDamageCooldown() => damageCooldown;
    public string GetGroundTag() => groundTag;
}

/// <summary>
/// 單個雪球腳本 - 處理雪球的移動、碰撞和推力
/// </summary>
public class Snowball : MonoBehaviour
{
    private float horizontalSpeed;
    private float verticalSpeed;
    private float destroyBoundary;
    private float verticalDrift;
    private float horizontalDrift;
    private float driftSpeed;
    private float driftPhase;

    private Rigidbody2D rb;
    private BlizzardSystem blizzardSystem;
    private Vector2 baseVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        blizzardSystem = FindObjectOfType<BlizzardSystem>();
    }

    public void Initialize(float hSpeed, float vSpeed, float boundary,
                          float vDrift, float hDrift, float drift, float phase)
    {
        horizontalSpeed = hSpeed;
        verticalSpeed = vSpeed;
        destroyBoundary = boundary;
        verticalDrift = vDrift;
        horizontalDrift = hDrift;
        driftSpeed = drift;
        driftPhase = phase;

        baseVelocity = new Vector2(-horizontalSpeed, -verticalSpeed);
    }

    void Update()
    {
        if (rb == null) return;

        // 計算飄動效果
        float time = Time.time * driftSpeed + driftPhase;
        float verticalOffset = Mathf.Sin(time) * verticalDrift;
        float horizontalOffset = Mathf.Cos(time * 0.7f) * horizontalDrift;

        // 應用速度（基礎速度 + 飄動）
        Vector2 driftVelocity = new Vector2(horizontalOffset, verticalOffset);
        rb.velocity = baseVelocity + driftVelocity;

        // 超出邊界時銷毀
        if (transform.position.x < destroyBoundary)
        {
            Destroy(gameObject);
        }

        // 額外檢測：使用 Raycast 檢測地板（備用方案）
        CheckGroundWithRaycast();
    }

    void CheckGroundWithRaycast()
    {
        if (blizzardSystem == null) return;

        // 向下發射射線檢測地板
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);

        if (hit.collider != null && hit.collider.CompareTag(blizzardSystem.GetGroundTag()))
        {
            Debug.Log("[Snowball] Raycast 檢測到地板，雪球消失");
            Destroy(gameObject);
        }
    }

    // Trigger 檢測 - 專門用來檢測地板
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 只檢測地板
        if (blizzardSystem != null && collision.CompareTag(blizzardSystem.GetGroundTag()))
        {
            Debug.Log($"[Snowball] Trigger檢測到地板：{collision.gameObject.name}，雪球消失");
            Destroy(gameObject);
            return;
        }
    }

    // 實體碰撞檢測 - 用來與玩家互動
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Snowball] 實體碰撞！碰到：{collision.gameObject.name}，Tag：{collision.gameObject.tag}");

        // 再次檢查地板（雙重保險）
        if (blizzardSystem != null && collision.gameObject.CompareTag(blizzardSystem.GetGroundTag()))
        {
            Debug.Log("[Snowball] 實體碰撞檢測到地板，雪球消失");
            Destroy(gameObject);
            return;
        }

        // 碰到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[Snowball] 碰到玩家，處理碰撞");
            HandlePlayerCollision(collision);
        }
    }

    void HandlePlayerCollision(Collision2D collision)
    {
        // 檢查無敵狀態
        InvincibilityController invincibility = collision.gameObject.GetComponent<InvincibilityController>();
        if (invincibility != null && invincibility.IsInvincible())
        {
            Debug.Log("[Snowball] 玩家處於無敵狀態");
            return; // 不銷毀雪球，讓它繼續飛
        }

        // 檢查傷害冷卻
        if (!HasDamageOnCooldown(collision.gameObject))
        {
            // 造成傷害
            ApplyDamage(collision.gameObject);

            // 推開玩家
            PushPlayer(collision.gameObject);

            // 設置冷卻
            SetDamageOnCooldown(collision.gameObject);
        }
        else
        {
            // 冷卻期間只推開，不造成傷害
            PushPlayer(collision.gameObject);
        }
    }

    void ApplyDamage(GameObject player)
    {
        if (blizzardSystem == null) return;

        // 扣除血量
        PlayerController2D playerController = player.GetComponent<PlayerController2D>();
        if (playerController != null)
        {
            playerController.TakeDamage(blizzardSystem.GetHealthDamage());
        }

        // 扣除魔力
        PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();
            int manaDamage = blizzardSystem.GetManaDamage();
            int newMana = Mathf.Max(0, currentMana - manaDamage);
            int actualDamage = currentMana - newMana;

            // 使用負數來扣除魔力
            playerAttack.RestoreMana(-actualDamage);

            Debug.Log($"[Snowball] 雪球擊中玩家！扣除 {blizzardSystem.GetHealthDamage()} 血量，{actualDamage} 魔力");
        }
    }

    void PushPlayer(GameObject player)
    {
        if (blizzardSystem == null) return;

        // 獲取或添加推力組件
        SnowballPushEffect pushEffect = player.GetComponent<SnowballPushEffect>();
        if (pushEffect == null)
        {
            pushEffect = player.AddComponent<SnowballPushEffect>();
        }

        // 計算推力方向（主要往左推，但保留一些碰撞方向）
        Vector2 pushDirection = Vector2.left;

        // 應用推力
        pushEffect.ApplyPush(pushDirection, blizzardSystem.GetPushForce(), blizzardSystem.GetPushDuration());

        Debug.Log($"[Snowball] 推開玩家，方向：{pushDirection}，力度：{blizzardSystem.GetPushForce()}");
    }

    private bool HasDamageOnCooldown(GameObject target)
    {
        if (!target.TryGetComponent<SnowballDamageTracker>(out var tracker))
        {
            return false;
        }
        return tracker.IsOnCooldown();
    }

    private void SetDamageOnCooldown(GameObject target)
    {
        if (!target.TryGetComponent<SnowballDamageTracker>(out var tracker))
        {
            tracker = target.AddComponent<SnowballDamageTracker>();
        }
        tracker.SetCooldown(blizzardSystem.GetDamageCooldown());
    }
}

/// <summary>
/// 雪球推力效果 - 處理玩家被推開的邏輯
/// 與 PlayerController2D 的移動系統整合
/// </summary>
public class SnowballPushEffect : MonoBehaviour
{
    private Vector2 pushVelocity = Vector2.zero;
    private float pushEndTime = 0f;
    private Rigidbody2D rb;
    private PlayerController2D playerController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        // 推力持續期間
        if (Time.time < pushEndTime)
        {
            // 持續應用推力（覆蓋玩家的移動輸入）
            if (rb != null)
            {
                // 保持垂直速度，只修改水平速度
                rb.velocity = new Vector2(pushVelocity.x, rb.velocity.y);
            }
        }
        else
        {
            // 推力結束，重置
            pushVelocity = Vector2.zero;
        }
    }

    public void ApplyPush(Vector2 direction, float force, float duration)
    {
        // 計算推力速度
        pushVelocity = direction.normalized * force;
        pushEndTime = Time.time + duration;

        Debug.Log($"[SnowballPushEffect] 應用推力：{pushVelocity}，持續 {duration} 秒");
    }

    public bool IsPushing()
    {
        return Time.time < pushEndTime;
    }
}

/// <summary>
/// 雪球傷害冷卻追蹤器 - 防止在冷卻期間重複傷害
/// </summary>
public class SnowballDamageTracker : MonoBehaviour
{
    private float lastDamageTime = -999f;
    private float cooldownDuration = 1f;

    public bool IsOnCooldown()
    {
        return Time.time - lastDamageTime < cooldownDuration;
    }

    public void SetCooldown(float duration)
    {
        cooldownDuration = duration;
        lastDamageTime = Time.time;
    }
}