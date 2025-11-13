using System.Collections;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("血包設定")]
    public int healAmount = 25;
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    [Header("動畫設定")]
    public bool enableFloating = true;
    public float floatSpeed = 2f;
    public float floatHeight = 0.15f;  // 改為較小的浮動幅度

    [Header("自動吸取設定")]
    [Tooltip("是否自動吸取到玩家")]
    public bool autoAbsorb = true;
    [Tooltip("吸取速度")]
    public float absorbSpeed = 10f;
    [Tooltip("吸取範圍")]
    public float absorbRange = 8f;

    [Header("功能設定")]
    [Tooltip("是否啟用滿血判斷，若滿血則不拾取血包")]
    public bool useCheckHealthFull = true;

    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool collected = false;
    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (collected) return;

        // 浮動動畫 - 只改變 Y 軸
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // 自動吸取
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

                if (distanceToPlayer < 0.3f)
                {
                    // 接觸到玩家，立即拾取
                    PlayerController2D player = playerTransform.GetComponent<PlayerController2D>();
                    if (player != null)
                    {
                        if (useCheckHealthFull && player.GetCurrentHealth() >= player.GetMaxHealth())
                        {
                            playerTransform = null;
                            return;
                        }

                        Collect(player);
                    }
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            // 如果沒啟用自動吸取，立即拾取
            if (!autoAbsorb)
            {
                PlayerController2D player = other.GetComponent<PlayerController2D>();
                if (player != null)
                {
                    if (useCheckHealthFull && player.GetCurrentHealth() >= player.GetMaxHealth())
                    {
                        return;
                    }
                    Collect(player);
                }
            }
        }
    }

    void Collect(PlayerController2D player)
    {
        if (collected) return;
        collected = true;

        player.Heal(healAmount);
        PlayPickupSound();
        PlayPickupEffect();
        Debug.Log($"[HealthPickup] 玩家拾取血包，回復 {healAmount} 點血量！");

        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }
    }

    void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }
    }

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
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

    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}