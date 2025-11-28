using UnityEngine;
using System.Collections;

/// <summary>
/// 下雨伤害系统 - 管理整体下雨效果和伤害逻辑
/// 雨丝会斜斜地下落，颜色可在Inspector中调整
/// </summary>
public class RainSystem : MonoBehaviour
{
    [Header("下雨范围设置")]
    [Tooltip("下雨范围的左边界")]
    public float rainAreaLeft = -10f;
    [Tooltip("下雨范围的右边界")]
    public float rainAreaRight = 10f;
    [Tooltip("雨开始下落的高度")]
    public float rainStartHeight = 10f;
    [Tooltip("雨停止的高度")]
    public float rainEndHeight = -5f;

    [Header("雨滴设置")]
    [Tooltip("雨滴预制物")]
    public GameObject raindropPrefab;
    [Tooltip("每秒生成的雨滴数量")]
    public int raindropSpawnRate = 20;
    [Tooltip("单个雨滴下落速度")]
    public float raindropSpeed = 8f;
    [Tooltip("雨丝宽度")]
    public float raindropWidth = 0.08f;
    [Tooltip("雨丝长度")]
    public float raindropLength = 0.5f;

    [Header("雨的倾斜设置")]
    [Tooltip("雨的倾斜角度（度数，0=垂直下落，负数=左倾，正数=右倾）")]
    public float rainTiltAngle = -15f;
    [Tooltip("水平移动速度（控制斜向下落的强度）")]
    public float horizontalSpeed = 2f;

    [Header("视觉效果")]
    [Tooltip("雨丝颜色")]
    public Color raindropColor = new Color(0.3f, 0.7f, 1f, 0.8f);
    [Tooltip("启用下雨范围可视化（调试用）")]
    public bool showDebugArea = true;

    [Header("伤害设置")]
    [Tooltip("碰到雨滴造成的血量伤害")]
    public int healthDamage = 5;
    [Tooltip("碰到雨滴造成的魔力伤害")]
    public int manaDamage = 10;
    [Tooltip("伤害冷却时间（秒）")]
    public float damageCooldown = 0.5f;

    private Transform spawnParent;
    private float nextSpawnTime = 0f;
    private float tiltRadian; // 预计算的角度（弧度）

    void Start()
    {
        // 创建一个父物体来放置所有雨滴
        GameObject rainContainer = new GameObject("RainContainer");
        rainContainer.transform.parent = transform;
        spawnParent = rainContainer.transform;

        // 预计算倾斜角度（弧度）
        tiltRadian = rainTiltAngle * Mathf.Deg2Rad;

        // 如果没有雨滴预制物，创建一个默认的
        if (raindropPrefab == null)
        {
            raindropPrefab = CreateDefaultRaindrop();
        }

        Debug.Log("[RainSystem] 下雨系统已初始化");
    }

    void Update()
    {
        // 定期生成雨滴
        if (Time.time >= nextSpawnTime)
        {
            SpawnRaindrop();
            nextSpawnTime = Time.time + (1f / raindropSpawnRate);
        }

        // 绘制调试范围
        if (showDebugArea)
        {
            DrawDebugArea();
        }
    }

    void SpawnRaindrop()
    {
        // 随机生成位置在范围内
        float randomX = Random.Range(rainAreaLeft, rainAreaRight);
        Vector3 spawnPos = new Vector3(randomX, rainStartHeight, 0f);

        GameObject raindrop = Instantiate(raindropPrefab, spawnPos, Quaternion.identity, spawnParent);
        Raindrop raindropScript = raindrop.GetComponent<Raindrop>();

        if (raindropScript != null)
        {
            raindropScript.Initialize(raindropSpeed, rainEndHeight, horizontalSpeed, tiltRadian);
        }
    }

    GameObject CreateDefaultRaindrop()
    {
        GameObject prefab = new GameObject("Raindrop");

        // 添加SpriteRenderer
        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.color = raindropColor;

        // 创建雨丝精灵
        Sprite sprite = CreateRaindropSprite();
        sr.sprite = sprite;

        // 添加BoxCollider2D
        BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(raindropWidth, raindropLength);
        collider.isTrigger = true;

        // 添加Raindrop脚本
        prefab.AddComponent<Raindrop>();

        // 设置大小
        prefab.transform.localScale = new Vector3(raindropWidth, raindropLength, 1f);

        return prefab;
    }

    Sprite CreateRaindropSprite()
    {
        // 创建一个细长的雨丝纹理
        int width = Mathf.Max(1, Mathf.RoundToInt(raindropWidth * 100));
        int height = Mathf.Max(1, Mathf.RoundToInt(raindropLength * 100));

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        // 创建从透明到不透明再到透明的渐变效果
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedY = (float)y / height;
                // 顶部和底部渐淡
                float alpha = Mathf.Sin(normalizedY * Mathf.PI) * raindropColor.a;

                int pixelIndex = y * width + x;
                pixels[pixelIndex] = new Color(raindropColor.r, raindropColor.g, raindropColor.b, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;

        return Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 100f);
    }

    void DrawDebugArea()
    {
        // 绘制下雨范围
        Debug.DrawLine(
            new Vector3(rainAreaLeft, rainStartHeight, 0),
            new Vector3(rainAreaRight, rainStartHeight, 0),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(rainAreaLeft, rainEndHeight, 0),
            new Vector3(rainAreaRight, rainEndHeight, 0),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(rainAreaLeft, rainStartHeight, 0),
            new Vector3(rainAreaLeft, rainEndHeight, 0),
            Color.cyan
        );
        Debug.DrawLine(
            new Vector3(rainAreaRight, rainStartHeight, 0),
            new Vector3(rainAreaRight, rainEndHeight, 0),
            Color.cyan
        );
    }

    public int GetHealthDamage() => healthDamage;
    public int GetManaDamage() => manaDamage;
    public float GetDamageCooldown() => damageCooldown;
}

/// <summary>
/// 单个雨滴脚本 - 处理雨滴的移动和碰撞
/// </summary>
public class Raindrop : MonoBehaviour
{
    private float fallSpeed;
    private float destroyHeight;
    private float horizontalSpeed;
    private float tiltRadian;
    private Rigidbody2D rb;
    private RainSystem rainSystem;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        rainSystem = FindObjectOfType<RainSystem>();
    }

    public void Initialize(float speed, float endHeight, float hSpeed, float angle)
    {
        fallSpeed = speed;
        destroyHeight = endHeight;
        horizontalSpeed = hSpeed;
        tiltRadian = angle;
    }

    void Update()
    {
        // 斜向下落：水平速度 = 下落速度 * tan(角度)
        float verticalVelocity = -fallSpeed;
        float horizontalVelocity = -fallSpeed * Mathf.Tan(tiltRadian);

        if (rb != null)
        {
            rb.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        }

        // 超过销毁高度时删除雨滴
        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 检查玩家是否处于无敌状态
            InvincibilityController invincibility = collision.GetComponent<InvincibilityController>();
            if (invincibility != null && invincibility.IsInvincible())
            {
                // 无敌状态下，雨滴只是消失
                Debug.Log("[Raindrop] 玩家处于无敌状态，雨滴无法造成伤害");
                Destroy(gameObject);
                return;
            }

            // 造成伤害
            PlayerController2D playerController = collision.GetComponent<PlayerController2D>();
            if (playerController != null && rainSystem != null)
            {
                // 检查冷却时间
                if (!HasDamageOnCooldown(collision.gameObject))
                {
                    // 扣除血量
                    playerController.TakeDamage(rainSystem.GetHealthDamage());

                    // 扣除魔力
                    PlayerAttack playerAttack = collision.GetComponent<PlayerAttack>();
                    if (playerAttack != null)
                    {
                        int currentMana = playerAttack.GetCurrentMana();
                        int newMana = Mathf.Max(0, currentMana - rainSystem.GetManaDamage());
                        int manaDamageAmount = currentMana - newMana;
                        playerAttack.RestoreMana(-manaDamageAmount);

                        Debug.Log($"[Raindrop] 玩家碰到雨滴！扣除{rainSystem.GetHealthDamage()}血量，{manaDamageAmount}魔力");
                    }

                    // 标记伤害冷却时间
                    SetDamageOnCooldown(collision.gameObject);
                }
            }

            Destroy(gameObject);
        }
    }

    private bool HasDamageOnCooldown(GameObject target)
    {
        if (target.TryGetComponent<PlayerController2D>(out var pc))
        {
            if (!target.TryGetComponent<DamageTracker>(out var tracker))
            {
                tracker = target.AddComponent<DamageTracker>();
            }
            return tracker.IsOnCooldown();
        }
        return false;
    }

    private void SetDamageOnCooldown(GameObject target)
    {
        if (!target.TryGetComponent<DamageTracker>(out var tracker))
        {
            tracker = target.AddComponent<DamageTracker>();
        }
        tracker.SetCooldown(FindObjectOfType<RainSystem>().GetDamageCooldown());
    }
}

/// <summary>
/// 伤害冷却追踪器 - 防止在冷却期间重复伤害
/// </summary>
public class DamageTracker : MonoBehaviour
{
    private float lastDamageTime = -999f;
    private float cooldownDuration = 0.5f;

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