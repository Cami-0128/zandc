using System.Collections;
using UnityEngine;

public class FrostTrapZone : MonoBehaviour
{
    [Header("冰凍設定")]
    public float slowdownMultiplier = 0.2f;  // 速度減為 20%
    public float slowdownDuration = 3f;      // 減速持續時間

    private Rigidbody2D playerRb;
    private PlayerController2D playerController;
    private InvincibilityController invincibilityController;
    private float slowdownEndTime = -999f;
    private bool isPlayerInZone = false;
    private Vector2 lastAppliedVelocity = Vector2.zero;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerRb = collision.GetComponent<Rigidbody2D>();
            playerController = collision.GetComponent<PlayerController2D>();
            invincibilityController = collision.GetComponent<InvincibilityController>();
            slowdownEndTime = Time.time + slowdownDuration;
            isPlayerInZone = true;
            Debug.Log("[FrostTrapZone] 玩家進入冰凍區域，移動嚴重減速！");
        }
    }

    void FixedUpdate()
    {
        if (!isPlayerInZone || playerRb == null) return;

        // 檢查是否還在減速時間內
        if (Time.time < slowdownEndTime)
        {
            // ========== 檢查玩家是否處於無敵狀態 ==========
            if (invincibilityController != null && invincibilityController.IsInvincible())
            {
                Debug.Log("[FrostTrapZone] 玩家無敵中，免除冰凍減速效果！");
                return;  // 無敵狀態下不應用冰凍減速
            }

            // 在 FixedUpdate 中應用減速，確保與物理引擎同步
            Vector2 currentVelocity = playerRb.velocity;

            // 應用減速
            playerRb.velocity = new Vector2(
                currentVelocity.x * slowdownMultiplier,
                currentVelocity.y * slowdownMultiplier
            );

            lastAppliedVelocity = playerRb.velocity;
            Debug.Log($"[FrostTrapZone] 應用減速 - 當前速度: {playerRb.velocity}");
        }
        else
        {
            // 減速時間結束，退出冰凍狀態
            isPlayerInZone = false;
            Debug.Log("[FrostTrapZone] 冰凍效果結束");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[FrostTrapZone] 玩家離開冰凍區域");
            isPlayerInZone = false;
            playerRb = null;
            playerController = null;
            invincibilityController = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col is BoxCollider2D boxCol)
        {
            Gizmos.DrawCube(transform.position + (Vector3)boxCol.offset,
                           new Vector3(boxCol.size.x, boxCol.size.y, 1));
        }
    }
}