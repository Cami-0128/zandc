using UnityEngine;
using System.Collections;

/// <summary>
/// 史萊姆捕食者敵人 - 會追蹤玩家並包裹吞噬
/// 【修改】改善全方位視角檢測 + 修復右側檢測問題
/// </summary>
public class SlimePredatorEnemy : MonoBehaviour
{
    [Header("血量系統")]
    [Tooltip("是否有血量系統")]
    public bool hasHealthSystem = false;
    [Tooltip("最大血量")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [Tooltip("血條UI")]
    public EnemyHealthBar healthBar;
    [Tooltip("是否受到玩家攻擊影響")]
    public bool canTakeDamage = true;
    [Tooltip("是否會被無敵星星殺死")]
    public bool vulnerableToInvincibility = true;

    [Header("巡邏設定")]
    [Tooltip("巡邏移動速度")]
    public float patrolSpeed = 2f;
    [Tooltip("巡邏範圍（左右各多少單位）")]
    public float patrolRange = 5f;
    [Tooltip("是否啟用巡邏")]
    public bool enablePatrol = true;
    [Tooltip("碰到障礙物時自動折返")]
    public bool autoTurnOnCollision = true;

    [Header("追蹤設定")]
    [Tooltip("視線範圍")]
    public float visionRange = 10f;
    [Tooltip("視線模式")]
    public VisionMode visionMode = VisionMode.Omnidirectional;
    [Tooltip("視線角度（僅在方向性模式下使用）")]
    public float visionAngle = 60f;
    [Tooltip("追蹤移動速度")]
    public float chaseSpeed = 3f;
    [Tooltip("進入追蹤狀態所需停留時間")]
    public float detectionTime = 3f;
    [Tooltip("視線檢測層級")]
    public LayerMask obstacleLayer;

    public enum VisionMode
    {
        Omnidirectional,  // 全方位（360度，左右都能看到）
        Directional       // 方向性（只能看前方，需要轉身）
    }

    [Header("包裹吞噬設定")]
    [Tooltip("觸發包裹的距離")]
    public float captureDistance = 1.5f;
    [Tooltip("包裹完成時間")]
    public float captureTime = 2f;
    [Tooltip("包裹特效預製物")]
    public GameObject captureEffectPrefab;
    [Tooltip("包裹粒子特效顏色")]
    public Color captureColor = new Color(0.2f, 0.8f, 0.3f, 0.7f);
    [Tooltip("逃脫機率（0-1，越小越難逃）")]
    [Range(0f, 1f)]
    public float escapeChance = 0.1f;
    [Tooltip("玩家溶解時間")]
    public float dissolveTime = 1.5f;

    [Header("外觀設定")]
    [Tooltip("史萊姆顏色")]
    public Color slimeColor = new Color(0.2f, 0.8f, 0.3f);
    [Tooltip("偵測到玩家時的顏色")]
    public Color alertColor = new Color(1f, 0.3f, 0.2f);
    [Tooltip("警戒提示符號預製物（驚嘆號）")]
    public GameObject alertIconPrefab;

    [Header("音效")]
    [Tooltip("偵測到玩家音效")]
    public AudioClip detectSound;
    [Tooltip("包裹音效")]
    public AudioClip captureSound;
    [Tooltip("吞噬音效")]
    public AudioClip devourSound;

    [Header("Debug設定")]
    [Tooltip("顯示視線範圍")]
    public bool showVisionRange = true;
    [Tooltip("顯示偵測計時")]
    public bool showDetectionTimer = true;
    [Tooltip("顯示詳細調試信息")]
    public bool showDetailedDebug = true;

    // 私有變數
    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private Vector3 patrolStartPosition;
    private bool movingRight = true;
    private float detectionTimer = 0f;
    private bool isChasing = false;
    private bool isCapturing = false;
    private bool isDying = false;
    private GameObject alertIcon;
    private GameObject captureEffect;

    private enum State
    {
        Patrol,      // 巡邏狀態
        Detecting,   // 偵測玩家中
        Chasing,     // 追蹤玩家
        Capturing    // 包裹玩家
    }

    private State currentState = State.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            if (showDetailedDebug)
                Debug.Log($"[SlimePredator] 找到玩家: {playerTransform.name}");
        }
        else
        {
            Debug.LogError("[SlimePredator] 找不到標籤為 'Player' 的物件！");
        }

        patrolStartPosition = transform.position;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = slimeColor;
        }

        // 初始化血量系統
        if (hasHealthSystem)
        {
            currentHealth = maxHealth;

            if (healthBar == null)
            {
                healthBar = GetComponentInChildren<EnemyHealthBar>();
            }

            if (healthBar != null)
            {
                healthBar.Initialize(this.transform);
                healthBar.UpdateHealthBar(currentHealth, maxHealth);
            }
        }

        Debug.Log("[SlimePredator] 史萊姆捕食者已生成");
    }

    void Update()
    {
        if (playerTransform == null || isCapturing || isDying) return;

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                CheckForPlayer();
                break;

            case State.Detecting:
                UpdateDetection();
                break;

            case State.Chasing:
                UpdateChase();
                break;
        }
    }

    void UpdatePatrol()
    {
        if (!enablePatrol) return;

        float distanceFromStart = transform.position.x - patrolStartPosition.x;

        if (movingRight)
        {
            rb.velocity = new Vector2(patrolSpeed, rb.velocity.y);

            if (distanceFromStart >= patrolRange)
            {
                movingRight = false;
                FlipSprite();
            }
        }
        else
        {
            rb.velocity = new Vector2(-patrolSpeed, rb.velocity.y);

            if (distanceFromStart <= -patrolRange)
            {
                movingRight = true;
                FlipSprite();
            }
        }
    }

    // 【修改】改善 CheckForPlayer 方法 - 修復全方位視角檢測
    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (showDetailedDebug)
            Debug.Log($"[SlimePredator] 當前狀態: {currentState}, 玩家距離: {distanceToPlayer:F2}, 視線範圍: {visionRange}");

        // 檢查是否在視線範圍內
        if (distanceToPlayer <= visionRange)
        {
            bool inAngle = true;

            // 【重點】全方位模式下，不檢查角度，直接通過
            if (visionMode == VisionMode.Directional)
            {
                // 方向性模式：只能看前方
                Vector3 forward = spriteRenderer.flipX ? Vector3.left : Vector3.right;
                float angle = Vector3.Angle(forward, directionToPlayer.normalized);
                inAngle = (angle <= visionAngle / 2f);
                if (showDetailedDebug)
                    Debug.Log($"[SlimePredator] 方向模式 - 角度: {angle:F1}°, 允許: {visionAngle / 2f}°");
            }
            else
            {
                // 【修改】Omnidirectional 模式：全方位檢測（360度）
                inAngle = true;
                if (showDetailedDebug)
                    Debug.Log("[SlimePredator] 全方位模式 - 範圍內自動通過");
            }

            if (inAngle)
            {
                // 檢查是否有障礙物遮擋
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayer);

                if (hit.collider == null || hit.collider.CompareTag("Player"))
                {
                    // 看到玩家了！
                    if (currentState == State.Patrol)
                    {
                        currentState = State.Detecting;
                        detectionTimer = 0f;
                        ShowAlertIcon();
                        Debug.Log("[SlimePredator] ✅ 偵測到玩家！");
                    }
                }
                else
                {
                    if (showDetailedDebug)
                        Debug.Log($"[SlimePredator] 玩家被遮擋，遮擋物: {hit.collider.name}");
                }
            }
        }
    }

    void UpdateDetection()
    {
        if (playerTransform == null)
        {
            ResetToPatrol();
            return;
        }

        // 停止移動，盯著玩家
        rb.velocity = new Vector2(0, rb.velocity.y);

        // 面向玩家
        if (playerTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

        // 檢查玩家是否還在視線內
        if (!IsPlayerInVision())
        {
            ResetToPatrol();
            return;
        }

        // 計時
        detectionTimer += Time.deltaTime;

        // 改變顏色提示
        if (spriteRenderer != null)
        {
            float t = detectionTimer / detectionTime;
            spriteRenderer.color = Color.Lerp(slimeColor, alertColor, t);
        }

        // 達到偵測時間，開始追蹤
        if (detectionTimer >= detectionTime)
        {
            currentState = State.Chasing;
            isChasing = true;
            HideAlertIcon();

            if (detectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(detectSound);
            }

            Debug.Log("[SlimePredator] 開始追蹤玩家！");
        }
    }

    void UpdateChase()
    {
        if (playerTransform == null)
        {
            ResetToPatrol();
            return;
        }

        // 追蹤玩家
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);

        // 面向玩家
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

        // 檢查是否接近玩家
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= captureDistance)
        {
            StartCapture();
        }
    }

    void StartCapture()
    {
        if (isCapturing) return;

        currentState = State.Capturing;
        isCapturing = true;
        rb.velocity = Vector2.zero;

        Debug.Log("[SlimePredator] 開始包裹玩家！");

        // 播放音效
        if (captureSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(captureSound);
        }

        StartCoroutine(CaptureSequence());
    }

    IEnumerator CaptureSequence()
    {
        // 檢查玩家是否無敵
        InvincibilityController invincibility = playerTransform.GetComponent<InvincibilityController>();
        if (invincibility != null && invincibility.IsInvincible())
        {
            Debug.Log("[SlimePredator] 玩家處於無敵狀態，無法捕獲！");
            ResetToPatrol();
            yield break;
        }

        // 禁用玩家控制
        PlayerController2D playerController = playerTransform.GetComponent<PlayerController2D>();
        if (playerController != null)
        {
            playerController.canControl = false;
        }

        // 創建包裹特效
        if (captureEffectPrefab != null)
        {
            captureEffect = Instantiate(captureEffectPrefab, playerTransform.position, Quaternion.identity);
            captureEffect.transform.SetParent(playerTransform);
            captureEffect.transform.localPosition = Vector3.zero;
        }
        else
        {
            // 使用簡單的粒子效果
            CreateDefaultCaptureEffect();
        }

        // 檢查逃脫機率
        float escapeRoll = Random.Range(0f, 1f);

        if (escapeRoll < escapeChance)
        {
            // 玩家逃脫了！
            Debug.Log("[SlimePredator] 玩家逃脫了！");
            yield return new WaitForSeconds(captureTime * 0.5f);

            if (captureEffect != null)
            {
                Destroy(captureEffect);
            }

            if (playerController != null)
            {
                playerController.canControl = true;
            }

            ResetToPatrol();
            yield break;
        }

        // 包裹過程
        float elapsed = 0f;
        Vector3 startScale = playerTransform.localScale;

        while (elapsed < captureTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / captureTime;

            // 玩家逐漸縮小
            if (playerTransform != null)
            {
                playerTransform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, t);
            }

            yield return null;
        }

        // 播放吞噬音效
        if (devourSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(devourSound);
        }

        // 玩家溶解動畫
        yield return StartCoroutine(DissolvePlayer());

        // 玩家死亡
        if (playerController != null)
        {
            playerController.Die();
        }

        // 清理
        if (captureEffect != null)
        {
            Destroy(captureEffect);
        }

        ResetToPatrol();
    }

    IEnumerator DissolvePlayer()
    {
        SpriteRenderer playerRenderer = playerTransform.GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
        {
            float elapsed = 0f;
            Color originalColor = playerRenderer.color;

            while (elapsed < dissolveTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dissolveTime;

                // 淡出並改變顏色
                Color dissolveColor = Color.Lerp(originalColor, captureColor, t);
                dissolveColor.a = Mathf.Lerp(1f, 0f, t);
                playerRenderer.color = dissolveColor;

                // 縮小
                if (playerTransform != null)
                {
                    float scale = Mathf.Lerp(0.3f, 0f, t);
                    playerTransform.localScale = Vector3.one * scale;
                }

                yield return null;
            }
        }
    }

    void CreateDefaultCaptureEffect()
    {
        GameObject effectObj = new GameObject("CaptureEffect");
        effectObj.transform.position = playerTransform.position;
        effectObj.transform.SetParent(playerTransform);

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = captureColor;
        main.startSize = 2f;
        main.startLifetime = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1.5f;

        captureEffect = effectObj;
    }

    // 【修改】改善 IsPlayerInVision - 修復全方位視角檢測
    bool IsPlayerInVision()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > visionRange) return false;

        // 根據視線模式檢查角度
        if (visionMode == VisionMode.Directional)
        {
            Vector3 forward = spriteRenderer.flipX ? Vector3.left : Vector3.right;
            float angle = Vector3.Angle(forward, directionToPlayer.normalized);

            if (angle > visionAngle / 2f) return false;
        }
        // 【修改】Omnidirectional 模式：跳過角度檢查，全方位有效

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayer);

        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    void ResetToPatrol()
    {
        currentState = State.Patrol;
        isChasing = false;
        isCapturing = false;
        detectionTimer = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = slimeColor;
        }

        HideAlertIcon();

        Debug.Log("[SlimePredator] 返回巡邏狀態");
    }

    void ShowAlertIcon()
    {
        if (alertIcon != null) return;

        if (alertIconPrefab != null)
        {
            alertIcon = Instantiate(alertIconPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            alertIcon.transform.SetParent(transform);
        }
    }

    void HideAlertIcon()
    {
        if (alertIcon != null)
        {
            Destroy(alertIcon);
            alertIcon = null;
        }
    }

    void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    // ===== 血量系統相關方法 =====

    public void TakeDamage(float damage, string damageSource = "Unknown")
    {
        if (!hasHealthSystem || !canTakeDamage || isDying) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        Debug.Log($"[SlimePredator] 受到 {damage} 點傷害 (來源: {damageSource})，剩餘血量: {currentHealth}/{maxHealth}");

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

        Color originalColor = currentState == State.Detecting
            ? Color.Lerp(slimeColor, alertColor, detectionTimer / detectionTime)
            : (currentState == State.Chasing ? alertColor : slimeColor);

        for (int i = 0; i < 3; i++)
        {
            if (isDying) yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            if (isDying) yield break;

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Die()
    {
        if (isDying) return;

        isDying = true;

        Debug.Log("[SlimePredator] 史萊姆死亡！");

        StopAllCoroutines();

        if (healthBar != null && healthBar.gameObject != null)
        {
            Destroy(healthBar.gameObject);
            healthBar = null;
        }

        HideAlertIcon();

        if (captureEffect != null)
        {
            Destroy(captureEffect);
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

        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            float fadeTime = 1f;
            Color originalColor = spriteRenderer.color;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                float scale = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying || isCapturing) return;

        // 玩家碰撞 - 不折返
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // 碰到其他物件就折返（牆壁、地面、平台等）
        Debug.Log($"[SlimePredator] 碰到障礙物: {collision.gameObject.name}，折返");

        if (currentState == State.Patrol && enablePatrol)
        {
            movingRight = !movingRight;
            FlipSprite();
        }
        else if (currentState == State.Chasing)
        {
            // 追蹤中碰到牆壁時，也轉身去追玩家
            if (playerTransform != null)
            {
                if (playerTransform.position.x > transform.position.x)
                {
                    spriteRenderer.flipX = false;
                    movingRight = true;
                }
                else
                {
                    spriteRenderer.flipX = true;
                    movingRight = false;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        // 處理玩家子彈傷害
        if (hasHealthSystem && canTakeDamage)
        {
            MagicBullet magicBullet = other.GetComponent<MagicBullet>();
            if (magicBullet != null)
            {
                TakeDamage(magicBullet.damage, "MagicBullet");
                Destroy(other.gameObject);
                return;
            }

            HeavyBullet heavyBullet = other.GetComponent<HeavyBullet>();
            if (heavyBullet != null)
            {
                TakeDamage(heavyBullet.damage, "HeavyBullet");
                Destroy(other.gameObject);
                return;
            }
        }

        // 處理無敵星星
        if (vulnerableToInvincibility && other.CompareTag("Player"))
        {
            InvincibilityController invincibility = other.GetComponent<InvincibilityController>();
            if (invincibility != null && invincibility.IsInvincible())
            {
                Debug.Log("[SlimePredator] 被無敵玩家消滅！");
                Die();
            }
        }
    }

    public float GetHealthPercentage()
    {
        if (!hasHealthSystem) return 1f;
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // ===== Gizmos 繪製 =====

    void OnDrawGizmos()
    {
        if (!showVisionRange) return;

        // 繪製視線範圍
        Gizmos.color = Color.yellow;

        if (visionMode == VisionMode.Omnidirectional)
        {
            // 全方位視角，繪製圓形
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
        else
        {
            // 方向性視角，繪製扇形
            Vector3 forward = spriteRenderer != null && spriteRenderer.flipX ? Vector3.left : Vector3.right;

            Vector3 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2f) * forward * visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2f) * forward * visionRange;

            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
            Gizmos.DrawLine(transform.position, transform.position + forward * visionRange);
        }

        // 繪製捕獲範圍
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureDistance);

        // 繪製巡邏範圍
        if (enablePatrol && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(patrolStartPosition + Vector3.left * patrolRange, patrolStartPosition + Vector3.right * patrolRange);
        }
    }

    void OnGUI()
    {
        if (!showDetectionTimer || currentState != State.Detecting) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        screenPos.y = Screen.height - screenPos.y;

        float progress = detectionTimer / detectionTime;
        string timerText = $"偵測中: {progress * 100:F0}%";

        GUI.Label(new Rect(screenPos.x - 50, screenPos.y, 100, 20), timerText);
    }
}