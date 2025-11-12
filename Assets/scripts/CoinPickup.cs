using System.Collections;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("金幣設定")]
    [SerializeField]
    private int coinValue = 1;

    [Tooltip("拾取音效")]
    public AudioClip pickupSound;
    [Tooltip("是否自動吸取到玩家")]
    public bool autoAbsorb = true;
    [Tooltip("吸取速度")]
    public float absorbSpeed = 10f;

    private bool collected = false;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb;
    private Vector3 startPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        // 自動吸取
        if (autoAbsorb && playerTransform != null && !collected)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < 0.5f)
            {
                Collect();
            }
            else if (distanceToPlayer < 5f)
            {
                // 朝玩家移動
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                if (rb != null)
                    rb.velocity = direction * absorbSpeed;
            }
        }
    }

    void LateUpdate()
    {
        // 浮動和旋轉動畫（參考你的 HealthPickup 風格）
        if (!collected)
        {
            // 浮動
            float newY = startPosition.y + Mathf.Sin(Time.time * 2f) * 0.3f;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                playerTransform = other.transform;

                if (!autoAbsorb)
                {
                    Collect();
                }
            }
        }
    }

    void Collect()
    {
        if (collected)
            return;

        collected = true;

        // 透過 CoinManager 增加金幣
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddMoney(coinValue);
            Debug.Log($"[CoinPickup] 玩家拾取 {coinValue} 個金幣！");
        }
        else
        {
            Debug.LogWarning("[CoinPickup] CoinManager 未初始化！");
        }

        // 播放音效
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // 視覺效果 - 變色並消失
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設定金幣數值（敵人掉落時使用）
    /// </summary>
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}