// ============= Bouncer.cs =============
// 彈簧物件主要腳本 - 改進版本，含視覺效果和隨機力度
using System.Collections;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [Header("=== 彈簧基本設定 ===")]
    public float bounceForce = 20f;
    public float bounceUpForce = 25f;

    [Header("=== 隨機力度設定 ===")]
    public bool enableRandomForce = false;
    public float minForce = 15f;
    public float maxForce = 35f;

    [Header("=== 使用次數設定 ===")]
    public bool unlimitedUses = true;
    public int maxUses = 5;
    private int currentUses = 0;

    [Header("=== 視覺效果設定 ===")]
    public float compressScale = 0.5f;  // 壓縮時的縮放
    public float expandScale = 1.3f;     // 反彈時的膨脹
    public float animationDuration = 0.4f;
    public bool enableColorChange = true;
    public Color compressColor = new Color(1f, 0f, 0f, 1f); // 紅色 - 被壓縮時
    public Color expandColor = new Color(0f, 1f, 0f, 1f);   // 綠色 - 反彈時
    public Color normalColor = Color.white;                  // 白色 - 正常

    [Header("=== 外觀設定 ===")]
    public Color activeColor = Color.white;
    public Color disabledColor = Color.gray;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Collider2D bounceCollider;
    private bool isAnimating = false;
    private bool isEnabled = true;

    // 追蹤在彈簧上的物體（防止連續觸發）
    private Rigidbody2D lastBouncedObject = null;
    private float lastBounceTime = -999f;
    private float bounceCooldown = 0.1f;

    // 記錄上次的力度
    private float lastBounceForce = 0f;

    void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        bounceCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            Debug.LogError($"[Bouncer] {gameObject.name} 缺少 SpriteRenderer 組件！");
        if (bounceCollider == null)
            Debug.LogError($"[Bouncer] {gameObject.name} 缺少 Collider2D 組件！");

        currentUses = 0;
        UpdateVisuals();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 檢查碰撞物體是否有 Rigidbody2D
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // 防止連續彈跳同一物體
        if (rb == lastBouncedObject && Time.time - lastBounceTime < bounceCooldown)
            return;

        // 檢查是否從上方踩到（法線指向下方表示從上方碰撞）
        bool isFromAbove = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isFromAbove = true;
                break;
            }
        }

        if (!isFromAbove) return;

        // 執行彈跳邏輯
        if (isEnabled)
        {
            PerformBounce(rb);
        }
    }

    void PerformBounce(Rigidbody2D rb)
    {
        // 記錄彈跳信息
        lastBouncedObject = rb;
        lastBounceTime = Time.time;

        // 計算實際彈跳力度
        float actualForce = GetBounceForce();
        lastBounceForce = actualForce;

        // 應用彈跳力
        rb.velocity = new Vector2(rb.velocity.x, 0); // 重置 Y 速度
        rb.velocity += Vector2.up * actualForce;

        Debug.Log($"[Bouncer] {rb.gameObject.name} 被彈起！力度: {actualForce:F1}");

        // 使用次數邏輯
        if (!unlimitedUses)
        {
            currentUses++;
            if (currentUses >= maxUses)
            {
                isEnabled = false;
                Debug.Log($"[Bouncer] 彈簧已用盡 ({currentUses}/{maxUses})");
            }
        }

        // 播放動畫和視覺效果
        StartCoroutine(BounceAnimation());
        UpdateVisuals();
    }

    // 計算彈跳力度
    float GetBounceForce()
    {
        if (enableRandomForce)
        {
            return Random.Range(minForce, maxForce);
        }
        else
        {
            return bounceUpForce;
        }
    }

    IEnumerator BounceAnimation()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;

        // ========== 壓縮階段 (下去) ==========
        while (elapsedTime < animationDuration * 0.4f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration * 0.4f);

            // 縮放：正常 → 壓縮
            transform.localScale = Vector3.Lerp(originalScale, originalScale * compressScale, progress);

            // 顏色：正常 → 紅色（壓縮色）
            if (enableColorChange && spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(normalColor, compressColor, progress);
            }

            yield return null;
        }

        elapsedTime = 0f;

        // ========== 反彈階段 (上來 + 膨脹) ==========
        while (elapsedTime < animationDuration * 0.4f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration * 0.4f);
            float easeOutProgress = 1f - Mathf.Pow(1f - progress, 2f); // 緩動曲線

            // 縮放：壓縮 → 膨脹超調
            transform.localScale = Vector3.Lerp(originalScale * compressScale, originalScale * expandScale, easeOutProgress);

            // 顏色：紅色 → 綠色（反彈色）
            if (enableColorChange && spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(compressColor, expandColor, easeOutProgress);
            }

            yield return null;
        }

        elapsedTime = 0f;

        // ========== 回到原始大小 ==========
        while (elapsedTime < animationDuration * 0.2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (animationDuration * 0.2f);

            // 縮放：膨脹 → 正常
            transform.localScale = Vector3.Lerp(originalScale * expandScale, originalScale, progress);

            // 顏色：綠色 → 正常
            if (enableColorChange && spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(expandColor, normalColor, progress);
            }

            yield return null;
        }

        // 確保回到初始狀態
        transform.localScale = originalScale;
        if (enableColorChange && spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        isAnimating = false;
    }

    void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isEnabled ? activeColor : disabledColor;
        }

        if (bounceCollider != null)
        {
            bounceCollider.enabled = isEnabled;
        }
    }

    // 公開方法：重置彈簧
    public void ResetBouncer()
    {
        currentUses = 0;
        isEnabled = true;
        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        UpdateVisuals();
        Debug.Log($"[Bouncer] {gameObject.name} 已重置");
    }

    // 公開方法：取得使用次數信息
    public int GetCurrentUses() => currentUses;
    public int GetMaxUses() => maxUses;
    public bool IsEnabled() => isEnabled;
    public float GetUsagePercentage() => unlimitedUses ? 1f : (float)currentUses / maxUses;
    public float GetLastBounceForce() => lastBounceForce;

    // Gizmo 視覺化（方便調試）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale * 1.2f);

        // 如果啟用隨機力度，顯示力度範圍
        if (enableRandomForce)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * (minForce / 10f));
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * (maxForce / 10f));
        }
    }
}