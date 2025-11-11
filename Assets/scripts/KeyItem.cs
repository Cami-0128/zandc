using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("鑰匙設定")]
    [Tooltip("鑰匙唯一 ID（必須與傳送門的 Required Key ID 一致）")]
    public string keyID = "Key_1";

    [Tooltip("鑰匙顯示名稱")]
    public string keyName = "金鑰匙";

    [Tooltip("鑰匙圖標")]
    public Sprite keyIcon;

    [Tooltip("拾取後是否銷毀物件")]
    public bool destroyOnPickup = true;

    [Header("視覺效果")]
    [Tooltip("旋轉速度")]
    public float rotationSpeed = 100f;

    [Tooltip("浮動速度")]
    public float floatSpeed = 2f;

    [Tooltip("浮動幅度")]
    public float floatAmount = 0.3f;

    [Tooltip("發光顏色")]
    public Color glowColor = Color.yellow;

    [Header("音效")]
    public AudioClip pickupSound;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 如果沒有設定圖標，使用物件自己的 Sprite
        if (keyIcon == null && spriteRenderer != null)
        {
            keyIcon = spriteRenderer.sprite;
        }

        // 設定發光效果
        if (spriteRenderer != null)
        {
            spriteRenderer.color = glowColor;
        }
    }

    void Update()
    {
        // 旋轉效果
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 上下浮動效果
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 脈衝發光效果
        if (spriteRenderer != null)
        {
            float alpha = 0.7f + Mathf.Sin(Time.time * 3f) * 0.3f;
            spriteRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // 添加鑰匙到背包
                inventory.AddKey(keyID, keyName, keyIcon);
                Debug.Log($"[KeyItem] 玩家獲得 {keyName} (ID: {keyID})");

                // 播放音效
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 拾取特效（可選）
                CreatePickupEffect();

                // 銷毀或隱藏物件
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    void CreatePickupEffect()
    {
        // 簡單的拾取粒子效果
        GameObject effectObj = new GameObject("PickupEffect");
        effectObj.transform.position = transform.position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = glowColor;
        main.startSize = 0.5f;
        main.startSpeed = 3f;
        main.startLifetime = 0.5f;
        main.maxParticles = 20;
        main.duration = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        ps.Play();
        Destroy(effectObj, 1f);
    }

    void OnDrawGizmosSelected()
    {
        // 在編輯器中顯示鑰匙資訊
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"{keyName}\nID: {keyID}");
#endif
    }
}