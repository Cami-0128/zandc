using UnityEngine;

/// <summary>
/// 無敵星星商店效果 - 購買後生成星星在玩家附近
/// </summary>
public class InvincibilityStarEffect : IShopItemEffect
{
    private GameObject starPrefab;
    private float spawnDistance = 1.5f; // 生成距離
    private LayerMask groundLayer;

    /// <summary>
    /// 構造函數
    /// </summary>
    /// <param name="prefab">星星預製體</param>
    /// <param name="distance">生成距離</param>
    public InvincibilityStarEffect(GameObject prefab, float distance = 1.5f)
    {
        this.starPrefab = prefab;
        this.spawnDistance = distance;
        // 設定地面圖層
        this.groundLayer = LayerMask.GetMask("Wall", "Default");
    }

    public void ApplyEffect(PlayerController2D player)
    {
        if (player == null)
        {
            Debug.LogError("[InvincibilityStarEffect] Player 為 null！");
            return;
        }

        if (starPrefab == null)
        {
            Debug.LogError("[InvincibilityStarEffect] Star Prefab 未設定！");
            return;
        }

        // 計算生成位置
        Vector3 spawnPosition = CalculateSpawnPosition(player);

        // 生成星星
        GameObject newStar = GameObject.Instantiate(starPrefab, spawnPosition, Quaternion.identity);
        newStar.name = "InvincibilityStar_Purchased";

        Debug.Log($"[InvincibilityStarEffect] 無敵星星已生成在 {spawnPosition}");
    }

    /// <summary>
    /// 計算生成位置（玩家上方）
    /// </summary>
    Vector3 CalculateSpawnPosition(PlayerController2D player)
    {
        Vector3 playerPos = player.transform.position;

        // 方案1: 生成在玩家上方
        Vector3 basePosition = playerPos + Vector3.up * spawnDistance;

        // 方案2: 如果你想生成在玩家右方，改用這個
        // Vector3 basePosition = playerPos + Vector3.right * spawnDistance;

        // 檢查是否有障礙物
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(basePosition, 0.3f);
        if (overlaps.Length > 0)
        {
            // 有重疊，嘗試往上偏移
            for (int i = 1; i <= 3; i++)
            {
                Vector3 offsetPosition = basePosition + Vector3.up * i * 0.5f;
                overlaps = Physics2D.OverlapCircleAll(offsetPosition, 0.3f);

                bool hasNonGroundCollision = false;
                foreach (var col in overlaps)
                {
                    if (col.gameObject != player.gameObject &&
                        col.gameObject.layer != LayerMask.NameToLayer("Wall") &&
                        col.gameObject.layer != LayerMask.NameToLayer("Default"))
                    {
                        hasNonGroundCollision = true;
                        break;
                    }
                }

                if (!hasNonGroundCollision)
                {
                    basePosition = offsetPosition;
                    Debug.Log($"[InvincibilityStarEffect] 位置有重疊，偏移 {i * 0.5f} 單位");
                    break;
                }
            }
        }

        return basePosition;
    }
}