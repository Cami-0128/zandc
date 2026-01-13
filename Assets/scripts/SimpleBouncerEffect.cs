using UnityEngine;

/// <summary>
/// 簡單彈簧效果 - 購買後直接生成在玩家右方
/// 這個類別實作 IShopItemEffect 介面，類似 HealEffect
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
        // 設定地面圖層 - 包含 Wall 和 Default
        this.groundLayer = LayerMask.GetMask("Wall", "Default");
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

        // 2. 檢測地面高度（從上方往下射線）
        RaycastHit2D groundHit = Physics2D.Raycast(
            basePosition + Vector3.up * 5f,  // 從上方 5 單位開始
            Vector2.down,
            10f,  // 往下檢測 10 單位
            groundLayer
        );

        if (groundHit.collider != null)
        {
            // 有檢測到地面，放在地面上方
            basePosition.y = groundHit.point.y + 0.5f; // 稍微高於地面 0.5 單位
            Debug.Log($"[SimpleBouncerEffect] 檢測到地面: {groundHit.collider.name}, 高度: {groundHit.point.y}");
        }
        else
        {
            // 沒有檢測到地面，使用玩家高度
            basePosition.y = playerPos.y;
            Debug.LogWarning("[SimpleBouncerEffect] 未檢測到地面，使用玩家高度");
        }

        // 3. 檢查是否有重疊（避免生成在其他彈簧上）
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(basePosition, 0.5f);
        bool hasOverlap = false;

        foreach (var col in overlaps)
        {
            // 忽略地面碰撞器
            if (col.gameObject.layer != LayerMask.NameToLayer("Wall") &&
                col.gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                hasOverlap = true;
                break;
            }
        }

        if (hasOverlap)
        {
            // 有重疊，嘗試往右偏移
            for (int i = 1; i <= 5; i++)
            {
                Vector3 offsetPosition = basePosition + Vector3.right * i * 0.5f;
                overlaps = Physics2D.OverlapCircleAll(offsetPosition, 0.5f);

                bool stillOverlap = false;
                foreach (var col in overlaps)
                {
                    if (col.gameObject.layer != LayerMask.NameToLayer("Wall") &&
                        col.gameObject.layer != LayerMask.NameToLayer("Default"))
                    {
                        stillOverlap = true;
                        break;
                    }
                }

                if (!stillOverlap)
                {
                    basePosition = offsetPosition;
                    Debug.Log($"[SimpleBouncerEffect] 位置有重疊，偏移 {i * 0.5f} 單位");
                    break;
                }
            }
        }

        return basePosition;
    }
}