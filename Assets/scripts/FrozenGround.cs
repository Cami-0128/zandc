using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冰凍地形腳本 - 掛在冰地物件上
/// 玩家接觸後開始計時，停留足夠時間會被凍住
/// 支援無敵星星免疫效果
/// </summary>
public class FrozenGround : MonoBehaviour
{
    [Header("冰凍設定")]
    [Tooltip("玩家需要停留多久才會被凍住（秒）")]
    public float timeToFreeze = 3f;

    [Tooltip("冰凍持續時間（秒）")]
    public float freezeDuration = 2f;

    [Tooltip("是否顯示計時器（調試用）")]
    public bool showDebugTimer = true;

    [Header("視覺效果")]
    [Tooltip("冰地的顏色（可選）")]
    public Color iceColor = new Color(0.7f, 0.9f, 1f, 1f); // 淺藍色

    [Header("音效")]
    [Tooltip("冰凍警告音效（接近被凍住時播放）")]
    public AudioClip freezeWarningSound;

    [Tooltip("冰凍音效")]
    public AudioClip freezeSound;

    private Dictionary<GameObject, float> playerTimers = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> hasPlayedWarning = new Dictionary<GameObject, bool>();
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Start()
    {
        // 獲取 SpriteRenderer 並設置冰地顏色
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = iceColor;
        }

        // 設置音效組件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (freezeWarningSound != null || freezeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // 檢查是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject player = collision.gameObject;

            // ========== 檢查無敵狀態 ==========
            InvincibilityController invincibility = player.GetComponent<InvincibilityController>();
            if (invincibility != null && invincibility.IsInvincible())
            {
                // 玩家處於無敵狀態，不會被凍住
                if (showDebugTimer)
                {
                    Debug.Log("[冰凍地形] 玩家處於無敵狀態，免疫冰凍效果！");
                }

                // 清除計時器（如果有的話）
                if (playerTimers.ContainsKey(player))
                {
                    playerTimers.Remove(player);
                    hasPlayedWarning.Remove(player);
                }
                return;
            }

            PlayerFreezeEffect freezeEffect = player.GetComponent<PlayerFreezeEffect>();

            if (freezeEffect == null)
            {
                Debug.LogWarning("玩家物件上沒有 PlayerFreezeEffect 組件！");
                return;
            }

            // 如果玩家已經被凍住，不繼續計時
            if (freezeEffect.IsFrozen())
            {
                return;
            }

            // 初始化或更新計時器
            if (!playerTimers.ContainsKey(player))
            {
                playerTimers[player] = 0f;
                hasPlayedWarning[player] = false;
            }

            playerTimers[player] += Time.deltaTime;

            // ========== 警告音效 ==========
            // 當剩餘時間小於1秒時播放警告音效
            if (!hasPlayedWarning[player] && playerTimers[player] >= (timeToFreeze - 1f) && freezeWarningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(freezeWarningSound);
                hasPlayedWarning[player] = true;
                Debug.Log("[冰凍地形] 播放冰凍警告音效");
            }

            if (showDebugTimer)
            {
                Debug.Log($"[冰凍地形] 玩家停留: {playerTimers[player]:F2} / {timeToFreeze:F2} 秒");
            }

            // 檢查是否達到冰凍時間
            if (playerTimers[player] >= timeToFreeze)
            {
                Debug.Log("[冰凍地形] 玩家被冰凍！");

                // 播放冰凍音效
                if (freezeSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(freezeSound);
                }

                freezeEffect.Freeze(freezeDuration);
                playerTimers[player] = 0f; // 重置計時器
                hasPlayedWarning[player] = false; // 重置警告標記
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 玩家離開冰地，重置計時器
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerTimers.ContainsKey(collision.gameObject))
            {
                playerTimers.Remove(collision.gameObject);
                hasPlayedWarning.Remove(collision.gameObject);
                Debug.Log("[冰凍地形] 玩家離開冰地，計時器已重置");
            }
        }
    }
}