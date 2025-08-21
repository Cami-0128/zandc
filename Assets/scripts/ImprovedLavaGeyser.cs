// === 1. 改進版岩漿噴泉 (結合你的 LavaGeyser 概念) ===
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ImprovedLavaGeyser : MonoBehaviour
{
    [Header("熔岩噴泉基本設定")]
    [Tooltip("噴發間隔（秒）")]
    public float eruptionInterval = 4f;

    [Tooltip("噴發持續時間（秒）")]
    public float eruptionDuration = 2f;

    [Tooltip("噴發高度")]
    public float eruptionHeight = 6f;

    [Tooltip("噴泉寬度")]
    [Range(1f, 4f)]
    public float geyserWidth = 2f;

    [Tooltip("對玩家造成的傷害")]
    public int damageAmount = 25;

    [Tooltip("推力強度（將玩家向上推）")]
    public float pushForce = 10f;

    [Header("預警系統")]
    [Tooltip("噴發前預警時間")]
    public float warningTime = 1f;

    [Tooltip("預警閃爍頻率")]
    public float warningBlinkRate = 5f;

    [Header("視覺與音效")]
    [Tooltip("熔岩顏色")]
    public Color baseColor = new Color(0.6f, 0.1f, 0f, 1f);

    [Tooltip("預警顏色")]
    public Color warningColor = new Color(1f, 0.8f, 0f, 0.8f);

    [Tooltip("氣泡顏色")]
    public Color bubbleColor = new Color(1f, 0.9f, 0.6f, 0.8f);

    [Tooltip("啟用粒子效果")]
    public bool enableParticles = true;

    [Header("平台設定")]
    [Tooltip("是否有可站立的平台")]
    public bool hasPlatform = false;

    [Tooltip("平台上升速度")]
    public float platformRiseSpeed = 3f;

    [Header("音效")]
    public AudioClip warningSound;
    public AudioClip eruptionSound;
    public AudioClip bubbleSound;

    // 私有變數
    private bool isErupting = false;
    private bool isWarning = false;
    private GameObject geyserVisual;
    private GameObject warningIndicator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer warningRenderer;
    private BoxCollider2D triggerCollider;
    private ParticleSystem bubbleParticles;
    private ParticleSystem steamParticles;
    private AudioSource audioSource;
    private Coroutine eruptionCoroutine;
    private GameObject platform;

    void Start()
    {
        SetupGeyser();
        StartEruptionLoop();
    }

    void Update()
    {
        // 預警閃爍效果
        if (isWarning && warningRenderer != null)
        {
            float alpha = 0.3f + 0.7f * Mathf.Abs(Mathf.Sin(Time.time * warningBlinkRate));
            Color color = warningColor;
            color.a = alpha;
            warningRenderer.color = color;
        }

        // 平台上升邏輯
        if (isErupting && platform != null && hasPlatform)
        {
            Vector3 targetPos = Vector3.up * (eruptionHeight * 0.8f);
            platform.transform.localPosition = Vector3.MoveTowards(
                platform.transform.localPosition,
                targetPos,
                platformRiseSpeed * Time.deltaTime
            );
        }
    }

    void SetupGeyser()
    {
        // 創建主要的熔岩噴泉視覺
        SetupVisuals();

        // 設置碰撞器
        SetupCollider();

        // 創建材質和貼圖
        CreateGeyserTexture();

        // 設置音效
        SetupAudio();

        // 創建粒子效果
        CreateParticleEffects();

        // 創建平台（如果需要）
        if (hasPlatform)
        {
            CreatePlatform();
        }
    }

    void SetupVisuals()
    {
        // 主要噴泉視覺
        geyserVisual = new GameObject("GeyserVisual");
        geyserVisual.transform.SetParent(transform);
        geyserVisual.transform.localPosition = Vector3.zero;
        spriteRenderer = geyserVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 1;
        geyserVisual.SetActive(false);

        // 預警指示器
        warningIndicator = new GameObject("WarningIndicator");
        warningIndicator.transform.SetParent(transform);
        warningIndicator.transform.localPosition = Vector3.zero;
        warningRenderer = warningIndicator.AddComponent<SpriteRenderer>();
        warningRenderer.sortingOrder = 0;
        warningIndicator.SetActive(false);
    }

    void SetupCollider()
    {
        triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(geyserWidth, eruptionHeight);
        triggerCollider.offset = new Vector2(0, eruptionHeight * 0.5f);
        triggerCollider.enabled = false;
    }

    void CreateGeyserTexture()
    {
        int textureWidth = Mathf.RoundToInt(32 * (geyserWidth / 2f));
        int textureHeight = 128;

        // 主要噴泉貼圖
        Texture2D mainTexture = CreateLavaTexture(textureWidth, textureHeight, false);
        spriteRenderer.sprite = Sprite.Create(mainTexture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0f));

        // 預警指示器貼圖
        Texture2D warningTexture = CreateLavaTexture(textureWidth, textureHeight / 4, true);
        warningRenderer.sprite = Sprite.Create(warningTexture,
            new Rect(0, 0, textureWidth, textureHeight / 4),
            new Vector2(0.5f, 0.5f));
    }

    Texture2D CreateLavaTexture(int width, int height, bool isWarning)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                if (isWarning)
                {
                    // 預警指示器：簡單的橙黃色圓形
                    float centerX = width * 0.5f;
                    float centerY = height * 0.5f;
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    float maxDistance = Mathf.Min(width, height) * 0.4f;

                    if (distance < maxDistance)
                    {
                        pixels[index] = warningColor;
                    }
                    else
                    {
                        pixels[index] = Color.clear;
                    }
                }
                else
                {
                    // 主要噴泉：漸變色
                    float heightRatio = (float)y / height;
                    Color color;

                    if (heightRatio < 0.2f)
                        color = new Color(0.6f, 0.1f, 0f, 1f); // 深紅
                    else if (heightRatio < 0.5f)
                        color = new Color(1f, 0.3f, 0f, 1f);   // 橘紅
                    else if (heightRatio < 0.8f)
                        color = new Color(1f, 0.6f, 0f, 1f);   // 橘黃
                    else
                        color = new Color(1f, 0.9f, 0.6f, 0.9f); // 淺黃

                    pixels[index] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void CreateParticleEffects()
    {
        if (!enableParticles) return;

        // 氣泡粒子系統
        CreateBubbleParticles();

        // 蒸汽粒子系統
        CreateSteamParticles();
    }

    void CreateBubbleParticles()
    {
        GameObject bubbleObj = new GameObject("BubbleParticles");
        bubbleObj.transform.SetParent(transform);
        bubbleObj.transform.localPosition = Vector3.up * (eruptionHeight * 0.9f);
        bubbleParticles = bubbleObj.AddComponent<ParticleSystem>();

        var main = bubbleParticles.main;
        main.startColor = bubbleColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.maxParticles = 50;
        main.startLifetime = 1.5f;

        var emission = bubbleParticles.emission;
        emission.rateOverTime = 25;

        var shape = bubbleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = geyserWidth * 0.3f;

        bubbleParticles.Stop();
    }

    void CreateSteamParticles()
    {
        GameObject steamObj = new GameObject("SteamParticles");
        steamObj.transform.SetParent(transform);
        steamObj.transform.localPosition = Vector3.zero;
        steamParticles = steamObj.AddComponent<ParticleSystem>();

        var main = steamParticles.main;
        main.startColor = new Color(1f, 1f, 1f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.maxParticles = 20;
        main.startLifetime = 2f;

        var emission = steamParticles.emission;
        emission.rateOverTime = 10;

        steamParticles.Play(); // 蒸汽一直存在
    }

    void CreatePlatform()
    {
        platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.SetParent(transform);
        platform.transform.localScale = new Vector3(geyserWidth * 0.8f, 0.2f, 1f);
        platform.transform.localPosition = Vector3.zero;

        // 設置平台碰撞
        BoxCollider2D platformCollider = platform.GetComponent<BoxCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = platform.AddComponent<BoxCollider2D>();
        }

        // 設置平台材質
        SpriteRenderer platformRenderer = platform.GetComponent<SpriteRenderer>();
        if (platformRenderer != null)
        {
            platformRenderer.color = new Color(0.4f, 0.2f, 0.1f, 1f); // 深褐色岩石
        }

        platform.tag = "Platform";
    }

    void StartEruptionLoop()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
        }
        eruptionCoroutine = StartCoroutine(EruptionLoop());
    }

    IEnumerator EruptionLoop()
    {
        while (true)
        {
            // 等待間隔
            yield return new WaitForSeconds(eruptionInterval - warningTime);

            // 開始預警
            yield return StartCoroutine(ShowWarning());

            // 開始噴發
            yield return StartCoroutine(Erupt());
        }
    }

    IEnumerator ShowWarning()
    {
        isWarning = true;
        warningIndicator.SetActive(true);

        // 播放預警音效
        if (audioSource != null && warningSound != null)
        {
            audioSource.clip = warningSound;
            audioSource.Play();
        }

        yield return new WaitForSeconds(warningTime);

        isWarning = false;
        warningIndicator.SetActive(false);
    }

    IEnumerator Erupt()
    {
        isErupting = true;

        // 啟動視覺效果
        geyserVisual.SetActive(true);
        geyserVisual.transform.localScale = new Vector3(geyserWidth, eruptionHeight, 1f);

        // 啟動碰撞器
        triggerCollider.enabled = true;

        // 播放噴發音效
        if (audioSource != null && eruptionSound != null)
        {
            audioSource.clip = eruptionSound;
            audioSource.Play();
        }

        // 啟動粒子效果
        if (enableParticles && bubbleParticles != null)
        {
            bubbleParticles.Play();
        }

        yield return new WaitForSeconds(eruptionDuration);

        // 結束噴發
        isErupting = false;
        geyserVisual.SetActive(false);
        triggerCollider.enabled = false;

        if (enableParticles && bubbleParticles != null)
        {
            bubbleParticles.Stop();
        }

        // 重置平台位置
        if (platform != null)
        {
            platform.transform.localPosition = Vector3.zero;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isErupting && other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                // 造成傷害
                player.TakeDamage(damageAmount);

                // 向上推力
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, pushForce);
                }

                Debug.Log($"玩家觸碰熔岩噴泉！造成 {damageAmount} 點傷害並獲得向上推力");
            }
        }
    }

    // 公開方法
    public void ForceErupt()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
        }
        StartCoroutine(Erupt());
        StartEruptionLoop();
    }

    public void StopEruption()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
            eruptionCoroutine = null;
        }

        isErupting = false;
        isWarning = false;
        geyserVisual.SetActive(false);
        warningIndicator.SetActive(false);
        triggerCollider.enabled = false;

        if (bubbleParticles != null)
            bubbleParticles.Stop();
    }
}