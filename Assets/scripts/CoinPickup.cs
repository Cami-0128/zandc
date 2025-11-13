using System.Collections;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("金幣設定")]
    [SerializeField]
    private int coinValue = 1;

    [Header("視覺效果")]
    [Tooltip("浮動幅度")]
    public float floatHeight = 0.15f;
    [Tooltip("浮動速度")]
    public float floatSpeed = 2f;

    [Header("音效")]
    [Tooltip("拾取音效")]
    public AudioClip pickupSound;

    [Header("自動吸取設定")]
    [Tooltip("是否自動吸取到玩家")]
    public bool autoAbsorb = true;
    [Tooltip("吸取速度")]
    public float absorbSpeed = 10f;
    [Tooltip("吸取範圍")]
    public float absorbRange = 8f;

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
        if (collected) return;

        // 自動吸取 - 放在浮動前面，優先處理
        if (autoAbsorb && !collected)
        {
            // 尋找玩家
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                if (distanceToPlayer < 0.5f)
                {
                    // 接觸到玩家，立即拾取
                    Collect();
                    return;  // 提前返回，不執行浮動
                }
                else if (distanceToPlayer < absorbRange)
                {
                    // 朝玩家移動
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    if (rb != null)
                    {
                        rb.velocity = new Vector2(direction.x * absorbSpeed, rb.velocity.y);
                    }
                }
                else
                {
                    // 超出範圍，停止吸取
                    if (rb != null)
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                    playerTransform = null;
                }
            }
        }

        // 浮動動畫 - 只在沒有吸取時執行
        if (floatHeight > 0 && (playerTransform == null || Vector3.Distance(transform.position, playerTransform.position) > absorbRange))
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            // 直接拾取，不管自動吸取是否開啟
            // 只要碰到玩家就拾取
            Collect();
        }
    }

    void Collect()
    {
        if (collected) return;
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

        // 視覺效果 - 淡出消失
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            Destroy(gameObject);
        }
    }

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