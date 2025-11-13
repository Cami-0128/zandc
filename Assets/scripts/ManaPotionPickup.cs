using UnityEngine;

public class ManaPotionPickup : MonoBehaviour
{
    [Header("藥水設定")]
    public int manaRestoreAmount = 30;

    [Header("視覺效果")]
    public Color potionColor = new Color(0.3f, 0.5f, 1f);
    public GameObject pickupEffectPrefab;

    [Header("音效")]
    public AudioClip pickupSound;

    [Header("旋轉動畫")]
    public bool enableRotation = true;
    public float rotationSpeed = 90f;

    [Header("漂浮動畫")]
    public bool enableFloating = true;
    public float floatHeight = 0.15f;  // 改為較小的浮動幅度
    public float floatSpeed = 2f;

    [Header("自動吸取設定")]
    [Tooltip("是否自動吸取到玩家")]
    public bool autoAbsorb = true;
    [Tooltip("吸取速度")]
    public float absorbSpeed = 10f;
    [Tooltip("吸取範圍")]
    public float absorbRange = 8f;

    [Header("功能設定")]
    [Tooltip("是否啟用魔力滿判斷，若魔力滿則不拾取")]
    public bool useCheckManaFull = true;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float timeOffset;
    private bool collected = false;
    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = potionColor;
        }

        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (collected) return;

        // 旋轉動畫
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // 漂浮動畫 - 只改變 Y 軸
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, startPosition.z);
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
                    PlayerAttack player = playerTransform.GetComponent<PlayerAttack>();
                    if (player != null)
                    {
                        if (useCheckManaFull && player.GetCurrentMana() >= player.maxMana)
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
                PlayerAttack player = other.GetComponent<PlayerAttack>();
                if (player != null)
                {
                    if (useCheckManaFull && player.GetCurrentMana() >= player.maxMana)
                    {
                        return;
                    }
                    Collect(player);
                }
            }
        }
    }

    void Collect(PlayerAttack player)
    {
        if (collected) return;
        collected = true;

        player.RestoreMana(manaRestoreAmount);
        Debug.Log($"[ManaPotionPickup] 玩家拾取魔力藥水，回復 {manaRestoreAmount} MP！");

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}