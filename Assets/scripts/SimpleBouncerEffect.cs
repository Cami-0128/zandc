using UnityEngine;

/// <summary>
/// 簡單彈簧效果 - 購買後直接生成在玩家右方
/// </summary>
public class SimpleBouncerEffect : IShopItemEffect
{
    private GameObject bouncerPrefab;
    private float spawnDistance = 2f; // 生成距離（玩家右方多遠）
    private LayerMask groundLayer;

    /// <summary>
    /// 構造函數
    /// </summary>
    /// <param name="prefab">彈簧預製體</param>
    /// <param name="distance">生成距離</param>
    public SimpleBouncerEffect(GameObject prefab, float distance = 2f)
    {
        this.bouncerPrefab = prefab;
        this.spawnDistance = distance;
        // 設定地面圖層（可根據你的專案調整）
        this.groundLayer = LayerMask.GetMask("Ground", "Default");
    }

    public void ApplyEffect(PlayerController2D player)
    {
        if (player == null)
        {
            Debug.LogError("[SimpleBouncerEffect] Player 為 null！");
            return;
        }

        if (bouncerPrefab == null)
        {
            Debug.LogError("[SimpleBouncerEffect] Bouncer Prefab 未設定！");
            return;
        }

        // 計算生成位置（玩家右方）
        Vector3 spawnPosition = CalculateSpawnPosition(player);

        // 生成彈簧
        GameObject newBouncer = GameObject.Instantiate(bouncerPrefab, spawnPosition, Quaternion.identity);
        newBouncer.name = "Bouncer_Purchased";

        Debug.Log($"[SimpleBouncerEffect] 彈簧已生成在 {spawnPosition}");
    }

    /// <summary>
    /// 計算生成位置
    /// </summary>
    Vector3 CalculateSpawnPosition(PlayerController2D player)
    {
        Vector3 playerPos = player.transform.position;

        // 1. 基礎位置：玩家右方
        Vector3 basePosition = playerPos + Vector3.right * spawnDistance;

        // 2. 檢測地面高度
        RaycastHit2D groundHit = Physics2D.Raycast(
            basePosition + Vector3.up * 5f,  // 從上方開始射線
            Vector2.down,
            10f,
            groundLayer
        );

        if (groundHit.collider != null)
        {
            // 有檢測到地面，放在地面上
            basePosition.y = groundHit.point.y + 0.5f; // 稍微高於地面
            Debug.Log($"[SimpleBouncerEffect] 檢測到地面高度: {groundHit.point.y}");
        }
        else
        {
            // 沒有檢測到地面，使用玩家高度
            basePosition.y = playerPos.y;
            Debug.LogWarning("[SimpleBouncerEffect] 未檢測到地面，使用玩家高度");
        }

        // 3. 檢查是否有重疊（避免生成在其他彈簧上）
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(basePosition, 0.5f);
        if (overlaps.Length > 0)
        {
            // 有重疊，嘗試往右偏移
            for (int i = 1; i <= 5; i++)
            {
                Vector3 offsetPosition = basePosition + Vector3.right * i;
                overlaps = Physics2D.OverlapCircleAll(offsetPosition, 0.5f);
                if (overlaps.Length == 0)
                {
                    basePosition = offsetPosition;
                    Debug.Log($"[SimpleBouncerEffect] 位置有重疊，偏移 {i} 單位");
                    break;
                }
            }
        }

        return basePosition;
    }
}

/// <summary>
/// 彈簧生成管理器 - 用於集中管理彈簧預製體
/// 掛載在場景中的空物件上
/// </summary>
public class BouncerSpawnManager : MonoBehaviour
{
    [Header("=== 彈簧預製體 ===")]
    public GameObject bouncerPrefab;

    [Header("=== 生成設定 ===")]
    public float spawnDistance = 2f; // 生成距離

    // Singleton 模式
    public static BouncerSpawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 創建彈簧效果
    /// </summary>
    public SimpleBouncerEffect CreateBouncerEffect()
    {
        if (bouncerPrefab == null)
        {
            Debug.LogError("[BouncerSpawnManager] Bouncer Prefab 未設定！");
            return null;
        }
        return new SimpleBouncerEffect(bouncerPrefab, spawnDistance);
    }

    /// <summary>
    /// 獲取彈簧預製體
    /// </summary>
    public GameObject GetBouncerPrefab()
    {
        return bouncerPrefab;
    }
}