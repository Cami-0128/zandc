using UnityEngine;

public class ManaPotionPickup : MonoBehaviour
{
    [Header("藥水設定")]
    [Tooltip("回復的魔力量")]
    public int manaRestoreAmount = 30;

    [Header("視覺效果")]
    [Tooltip("藥水的顏色")]
    public Color potionColor = new Color(0.3f, 0.5f, 1f); // 藍色

    [Tooltip("拾取時的特效")]
    public GameObject pickupEffectPrefab;

    [Header("音效")]
    [Tooltip("拾取時的音效")]
    public AudioClip pickupSound;

    [Header("旋轉動畫")]
    [Tooltip("是否啟用旋轉動畫")]
    public bool enableRotation = true;

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 90f;

    [Header("漂浮動畫")]
    [Tooltip("是否啟用上下漂浮")]
    public bool enableFloating = true;

    [Tooltip("漂浮高度")]
    public float floatHeight = 0.3f;

    [Tooltip("漂浮速度")]
    public float floatSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        // 獲取 SpriteRenderer 並設定顏色
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = potionColor;
        }

        // 記錄初始位置
        startPosition = transform.position;

        // 隨機時間偏移，讓多個藥水不同步
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // 旋轉動畫
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // 漂浮動畫
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查是否是玩家
        PlayerAttack player = other.GetComponent<PlayerAttack>();

        if (player != null)
        {
            // 回復玩家魔力
            player.RestoreMana(manaRestoreAmount);

            Debug.Log($"[ManaPotionPickup] 玩家拾取魔力藥水，回復 {manaRestoreAmount} MP！");

            // 播放拾取音效
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 生成拾取特效
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // 銷毀藥水
            Destroy(gameObject);
        }
    }
}