using UnityEngine;

/// <summary>
/// 浮動物體系統 - 讓敵人或其他物體浮在水面上
/// 附加到需要浮動的物體上（如特殊敵人）
/// </summary>
public class FloatingObject : MonoBehaviour
{
    [Header("═══ 浮動設定 ═══")]
    [Tooltip("在水中時是否浮動")]
    public bool enableFloating = true;

    [Tooltip("浮動高度（距離水面的距離）")]
    public float floatHeight = 1f;

    [Tooltip("浮力強度")]
    public float buoyancyForce = 15f;

    [Tooltip("浮動平滑度")]
    public float floatSmoothness = 0.1f;

    [Header("═══ 水中行為設定 ═══")]
    [Tooltip("在水中時的水平移動減速倍數")]
    public float waterDragMultiplier = 0.7f;

    [Tooltip("在水中時的下沉速度")]
    public float waterSinkForce = 1f;

    // 內部變數
    private Rigidbody2D rb;
    private WaterZone currentWaterZone;
    private bool isInWater = false;
    private Vector3 waterSurfacePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Debug.Log($"[FloatingObject] {gameObject.name} 浮動系統已初始化");
    }

    void FixedUpdate()
    {
        if (!enableFloating || rb == null) return;

        // 檢查是否在水中
        if (isInWater && currentWaterZone != null)
        {
            ApplyFloatingBehavior();
        }
    }

    void ApplyFloatingBehavior()
    {
        // 計算水面位置
        BoxCollider2D waterCollider = currentWaterZone.GetComponent<BoxCollider2D>();
        if (waterCollider == null) return;

        float waterSurfaceY = currentWaterZone.transform.position.y + waterCollider.bounds.extents.y;
        float targetY = waterSurfaceY + floatHeight;

        // 計算當前與目標高度的差距
        float heightDifference = targetY - transform.position.y;

        // 應用浮力
        float buoyancy = heightDifference * buoyancyForce;
        rb.velocity = new Vector2(
            rb.velocity.x * waterDragMultiplier,
            Mathf.Lerp(rb.velocity.y, buoyancy, floatSmoothness)
        );

        // 如果物體下沉太快，應用上升力
        if (rb.velocity.y < -waterSinkForce)
        {
            rb.velocity = new Vector2(rb.velocity.x, -waterSinkForce);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 檢查是否進入水域
        WaterZone waterZone = collision.GetComponent<WaterZone>();
        if (waterZone != null)
        {
            isInWater = true;
            currentWaterZone = waterZone;
            Debug.Log($"[FloatingObject] {gameObject.name} 進入水域，開始浮動");
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // 保持水域參考
        WaterZone waterZone = collision.GetComponent<WaterZone>();
        if (waterZone != null && isInWater == false)
        {
            isInWater = true;
            currentWaterZone = waterZone;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // 離開水域
        WaterZone waterZone = collision.GetComponent<WaterZone>();
        if (waterZone != null && waterZone == currentWaterZone)
        {
            isInWater = false;
            currentWaterZone = null;
            Debug.Log($"[FloatingObject] {gameObject.name} 離開水域");
        }
    }

    // 公開方法供外部調用
    public void SetFloatingEnabled(bool enabled)
    {
        enableFloating = enabled;
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    public void SetFloatHeight(float newHeight)
    {
        floatHeight = newHeight;
    }

    public void SetBuoyancyForce(float newForce)
    {
        buoyancyForce = newForce;
    }
}