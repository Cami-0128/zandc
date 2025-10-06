using UnityEngine;

/// <summary>
/// 金幣拾取腳本
/// 整合版 - 配合現有的 CoinFlip 和 CoinManager
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("金幣設定")]
    [Tooltip("錢幣價值")]
    public int value = 1;

    [Header("自動消失")]
    [Tooltip("金幣存在時間（秒），0 = 永不消失")]
    public float lifetime = 10f;

    [Header("飛向玩家設定（可選）")]
    [Tooltip("是否自動飛向玩家")]
    public bool flyToPlayer = false;

    [Tooltip("飛行速度")]
    public float flySpeed = 5f;

    [Tooltip("開始飛向玩家的延遲時間")]
    public float flyDelay = 0.5f;

    private Transform playerTransform;
    private bool isFlying = false;

    void Start()
    {
        // 自動消失
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }

        // 如果啟用飛向玩家功能
        if (flyToPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Invoke("StartFlyingToPlayer", flyDelay);
            }
        }
    }

    void Update()
    {
        // 飛向玩家
        if (isFlying && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * flySpeed * Time.deltaTime;
        }
    }

    void StartFlyingToPlayer()
    {
        isFlying = true;

        // 禁用重力和物理（如果有 Rigidbody2D）
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 給予金幣
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddMoney(value);
            }

            // 銷毀金幣
            Destroy(gameObject);
        }
    }
}