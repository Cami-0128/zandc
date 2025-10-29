using UnityEngine;

/// <summary>
/// 劍碰撞系統診斷工具 - 修正版
/// </summary>
public class SwordCollisionDebugger : MonoBehaviour   //可不用
{
    void Start()
    {
        Debug.Log("====== 【劍碰撞系統診斷】 ======");

        // 檢查玩家
        PlayerController2D player = GetComponent<PlayerController2D>();
        if (player == null)
        {
            Debug.LogError("❌ 找不到PlayerController2D組件！");
        }
        else
        {
            Debug.Log("✅ 找到PlayerController2D組件");
        }

        // 檢查SwordSlashSkill
        SwordSlashSkill swordSkill = GetComponent<SwordSlashSkill>();
        if (swordSkill == null)
        {
            Debug.LogError("❌ 找不到SwordSlashSkill組件！");
        }
        else
        {
            Debug.Log("✅ 找到SwordSlashSkill組件");
            if (swordSkill.swordObject == null)
            {
                Debug.LogError("❌ SwordSlashSkill的swordObject未設定！");
            }
            else
            {
                Debug.Log($"✅ 劍物件已設定：{swordSkill.swordObject.name}");

                // 檢查劍的Collider
                Collider2D[] colliders = swordSkill.swordObject.GetComponentsInChildren<Collider2D>();
                if (colliders.Length == 0)
                {
                    Debug.LogError("❌ 劍物件上沒有Collider2D！");
                }
                else
                {
                    foreach (Collider2D col in colliders)
                    {
                        Debug.Log($"✅ 找到Collider2D：{col.gameObject.name}");
                        Debug.Log($"   - IsTrigger: {col.isTrigger}");
                        Debug.Log($"   - 是否啟用: {col.enabled}");
                        Debug.Log($"   - Layer: {LayerMask.LayerToName(col.gameObject.layer)}");

                        // 檢查Rigidbody2D
                        Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                        if (rb == null)
                        {
                            Debug.LogWarning("⚠️  Collider上沒有Rigidbody2D！【如果劍是子物件可能不需要】");
                        }
                        else
                        {
                            Debug.Log($"✅ 找到Rigidbody2D");
                            Debug.Log($"   - BodyType: {rb.bodyType}");
                            Debug.Log($"   - GravityScale: {rb.gravityScale}");
                        }
                    }
                }
            }
        }

        // 檢查Boss
        BossController2D[] bosses = FindObjectsOfType<BossController2D>();
        if (bosses.Length == 0)
        {
            Debug.LogWarning("⚠️  場景中找不到任何Boss！");
        }
        else
        {
            foreach (BossController2D boss in bosses)
            {
                Debug.Log($"✅ 找到Boss：{boss.gameObject.name}");
                Debug.Log($"   - Layer: {LayerMask.LayerToName(boss.gameObject.layer)}");
                Debug.Log($"   - Tag: {boss.gameObject.tag}");

                // 檢查Boss的Collider
                Collider2D[] bossColliders = boss.GetComponents<Collider2D>();
                if (bossColliders.Length == 0)
                {
                    Debug.LogError($"❌ Boss {boss.gameObject.name} 上沒有Collider2D！");
                }
                else
                {
                    foreach (Collider2D col in bossColliders)
                    {
                        Debug.Log($"✅ Boss找到Collider2D: {col.GetType().Name}");
                        Debug.Log($"   - IsTrigger: {col.isTrigger}");
                        Debug.Log($"   - 是否啟用: {col.enabled}");
                    }
                }

                // 檢查Boss的Rigidbody2D
                Rigidbody2D bossRb = boss.GetComponent<Rigidbody2D>();
                if (bossRb == null)
                {
                    Debug.LogError($"❌ Boss {boss.gameObject.name} 上沒有Rigidbody2D！【重要】");
                }
                else
                {
                    Debug.Log($"✅ Boss找到Rigidbody2D");
                    Debug.Log($"   - BodyType: {bossRb.bodyType}");
                }
            }
        }

        // 檢查碰撞矩陣
        CheckCollisionMatrix();

        Debug.Log("====== 【診斷完成】 ======");
    }

    /// <summary>
    /// 檢查碰撞矩陣設定
    /// </summary>
    void CheckCollisionMatrix()
    {
        Debug.Log("\n===== 【碰撞矩陣檢查】 =====");

        // 取得常用 Layer
        int defaultLayer = LayerMask.NameToLayer("Default");
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        // 檢查 Default 與 Enemy 的碰撞
        if (!Physics2D.GetIgnoreLayerCollision(defaultLayer, enemyLayer))
        {
            Debug.Log("✅ Default 與 Enemy 可以碰撞");
        }
        else
        {
            Debug.LogWarning("⚠️  Default 與 Enemy 碰撞被忽略！");
        }

        // 檢查 Player 與 Enemy 的碰撞
        if (!Physics2D.GetIgnoreLayerCollision(playerLayer, enemyLayer))
        {
            Debug.Log("✅ Player 與 Enemy 可以碰撞");
        }
        else
        {
            Debug.LogWarning("⚠️  Player 與 Enemy 碰撞被忽略！");
        }
    }
}