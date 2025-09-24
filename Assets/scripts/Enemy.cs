using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("敵人屬性")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("移動設定")]
    public float jumpForce = 8f;        // 跳躍力度
    public float moveRange = 3f;        // 左右移動範圍 ⭐ 可自訂
    public float jumpHeight = 1f;       // 跳躍高度 ⭐ 可自訂 (降低預設值)
    public float jumpInterval = 2f;     // 跳躍間隔(秒)
    public bool useGravity = false;     // 是否使用重力（建議false）

    [Header("地板檢測")]
    public LayerMask groundLayerMask = 1;  // 地板圖層
    public float groundCheckDistance = 1.1f; // 地板檢測距離

    [Header("移動狀態")]
    private bool isJumping = false;
    private bool isGrounded = true;
    private float jumpTimer = 0f;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    [Header("視覺效果")]
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    [Header("殘影效果")]
    public GameObject trailPrefab; // 殘影預製體（可選）
    private TrailRenderer trailRenderer;

    void Start()
    {
        // 初始化血量
        currentHealth = maxHealth;

        // 獲取組件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();

        // 記錄起始位置
        startPosition = transform.position;

        // 設定物理屬性
        if (rb != null)
        {
            rb.gravityScale = useGravity ? 1f : 0f; // 控制重力
            rb.freezeRotation = true; // 防止旋轉
        }

        Debug.Log($"敵人初始化完成！起始位置: {startPosition}, 血量: {currentHealth}/{maxHealth}");
    }

    void FixedUpdate()
    {
        if (isDying) return;

        // 地板檢測 - 防止穿透
        CheckGrounded();

        // 跳躍計時器
        jumpTimer += Time.fixedDeltaTime;
        if (jumpTimer >= jumpInterval && !isJumping && isGrounded)
        {
            PerformJump();
            jumpTimer = 0f;
        }
    }

    void CheckGrounded()
    {
        // 向下射線檢測地板
        Vector2 rayOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayerMask);

        // 檢測是否在地面
        bool wasGrounded = isGrounded;
        isGrounded = (hit.collider != null);

        // 如果碰到地板且正在下降，進行反彈處理
        if (isGrounded && rb.velocity.y <= 0)
        {
            // 調整位置，防止穿透
            float groundY = hit.point.y + 0.5f; // 0.5f是敵人的半徑
            if (transform.position.y < groundY + 0.1f)
            {
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            }

            // 停止垂直移動
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }

            // 結束跳躍狀態
            if (isJumping && !wasGrounded)
            {
                isJumping = false;
                Debug.Log("敵人著陸！");
            }
        }

        // 繪製射線 (在Scene視圖中可見)
        Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void PerformJump()
    {
        if (isJumping || !isGrounded) return;

        // 隨機選擇跳躍方向和距離
        float direction = Random.Range(-1f, 1f) > 0 ? 1f : -1f;
        float jumpDistance = Random.Range(1f, moveRange);
        float targetX = startPosition.x + (direction * jumpDistance);

        // 限制在移動範圍內
        targetX = Mathf.Clamp(targetX, startPosition.x - moveRange, startPosition.x + moveRange);

        Debug.Log($"敵人準備跳躍！目標X: {targetX}, 跳躍距離: {jumpDistance}");

        // 執行跳躍
        if (!useGravity)
        {
            // 固定跳躍模式 - 完美弧線 ⭐ 推薦
            StartCoroutine(FixedJumpCoroutine(targetX));
        }
        else
        {
            // 重力跳躍模式 - 物理跳躍
            GravityJump(targetX);
        }

        isJumping = true;
    }

    IEnumerator FixedJumpCoroutine(float targetX)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z); // 目標Y軸保持和起始相同！

        float jumpDuration = 1f; // 跳躍持續時間
        float elapsedTime = 0f;

        // 啟用殘影效果
        if (trailRenderer != null)
            trailRenderer.enabled = true;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            // 水平移動 (線性)
            float currentX = Mathf.Lerp(startPos.x, targetPos.x, progress);

            // 垂直移動 (小弧線) - 只在地面附近跳躍！
            float arcHeight = jumpHeight * 0.3f; // 降低跳躍高度！
            float currentY = startPos.y + (arcHeight * 4f * progress * (1f - progress));

            transform.position = new Vector3(currentX, currentY, startPos.z);

            yield return null;
        }

        // 確保回到地面高度
        transform.position = new Vector3(targetPos.x, startPos.y, startPos.z);

        // 關閉殘影效果
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        isJumping = false;
        Debug.Log($"地面跳躍完成！從 {startPos.x} 跳到 {targetPos.x}");
    }

    void GravityJump(float targetX)
    {
        if (rb == null) return;

        Vector3 currentPos = transform.position;
        float distance = targetX - currentPos.x;

        // 計算跳躍力度
        Vector2 jumpVelocity = new Vector2(
            distance * jumpForce * 0.5f, // 水平力度
            jumpHeight * jumpForce        // 垂直力度
        );

        // 啟用殘影效果
        if (trailRenderer != null)
            trailRenderer.enabled = true;

        rb.velocity = jumpVelocity;
        Debug.Log($"重力跳躍！速度: {jumpVelocity}");
    }

    // 受傷處理
    public void TakeDamage(float damage)
    {
        if (isDying) return;

        currentHealth -= damage;
        Debug.Log($"敵人受傷！受到 {damage} 點傷害，剩餘血量: {currentHealth}/{maxHealth}");

        // 受傷閃爍效果
        StartCoroutine(HitFlashEffect());

        // 檢查死亡
        if (currentHealth <= 0)
        {
            Debug.Log("敵人血量歸零，準備死亡！");
            Die();
        }
    }

    IEnumerator HitFlashEffect()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red; // 受傷時變紅

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (isDying) return;

        isDying = true;
        Debug.Log("敵人死亡！開始瓦解特效...");

        // 停止移動
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        // 關閉殘影
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        // 死亡特效
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        // 閃爍效果
        for (int i = 0; i < 6; i++)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.2f);
        }

        // 瓦解效果 - 創建碎片
        CreateDeathParticles();

        // 銷毀敵人
        Destroy(gameObject);
    }

    void CreateDeathParticles()
    {
        // 創建多個小碎片
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.transform.position = transform.position;
            particle.transform.localScale = Vector3.one * 0.2f;

            // 隨機顏色 (橘色系)
            Renderer particleRenderer = particle.GetComponent<Renderer>();
            particleRenderer.material.color = new Color(1f, Random.Range(0.3f, 0.8f), 0f);

            // 隨機飛散方向
            Rigidbody2D particleRb = particle.AddComponent<Rigidbody2D>();
            Vector2 randomDirection = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f)
            ).normalized;

            particleRb.velocity = randomDirection * Random.Range(3f, 8f);
            particleRb.angularVelocity = Random.Range(-360f, 360f);

            // 3秒後銷毀碎片
            Destroy(particle, 3f);
        }
    }

    // 碰撞檢測 - 魔法攻擊
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        // 檢查是否為魔法子彈
        MagicBullet bullet = other.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            TakeDamage(50f); // 每發扣50血

            // 立即銷毀子彈，防止重複觸發
            Destroy(other.gameObject);
        }
    }

    // Gizmos繪製 - 顯示移動範圍
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // 繪製移動範圍
            Gizmos.color = Color.yellow;
            Vector3 leftBound = new Vector3(startPosition.x - moveRange, transform.position.y, transform.position.z);
            Vector3 rightBound = new Vector3(startPosition.x + moveRange, transform.position.y, transform.position.z);

            Gizmos.DrawLine(leftBound, rightBound);
            Gizmos.DrawWireSphere(leftBound, 0.2f);
            Gizmos.DrawWireSphere(rightBound, 0.2f);
        }
        else if (startPosition == Vector3.zero)
        {
            startPosition = transform.position;
        }
    }
}