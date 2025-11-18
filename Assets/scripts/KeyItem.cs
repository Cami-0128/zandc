using UnityEngine;
using System.Collections.Generic;

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
    public bool destroyOnPickup = false;

    [Header("背包系統")]
    [Tooltip("是否使用背包系統（如果沒有勾選，鑰匙會跟隨玩家）")]
    public bool useInventorySystem = false;

    [Header("視覺效果 - 拾取前")]
    [Tooltip("旋轉速度")]
    public float rotationSpeed = 100f;

    [Tooltip("浮動速度")]
    public float floatSpeed = 2f;

    [Tooltip("浮動幅度")]
    public float floatAmount = 0.3f;

    [Tooltip("發光顏色")]
    public Color glowColor = Color.yellow;

    [Header("跟隨玩家設定")]
    [Tooltip("跟隨玩家時的縮放比例")]
    [Range(0.1f, 1f)]
    public float followScale = 0.3f;

    [Tooltip("跟隨玩家時在玩家上方的高度")]
    public float followHeight = 1.5f;

    [Tooltip("跟隨玩家時的水平偏移")]
    public float followOffsetX = 0.5f;

    [Tooltip("跟隨移動的平滑速度")]
    public float followSmoothSpeed = 5f;

    [Tooltip("跟隨時的浮動速度")]
    public float followFloatSpeed = 3f;

    [Tooltip("跟隨時的浮動幅度")]
    public float followFloatAmount = 0.15f;

    [Header("音效")]
    public AudioClip pickupSound;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private bool isFollowingPlayer = false;
    private Transform playerTransform;
    private Vector3 originalScale;
    private float followFloatTimer = 0f;

    // 靜態字典來追蹤玩家持有的鑰匙
    private static Dictionary<string, KeyItem> playerKeys = new Dictionary<string, KeyItem>();

    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (keyIcon == null && spriteRenderer != null)
        {
            keyIcon = spriteRenderer.sprite;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = glowColor;
        }
    }

    void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            FollowPlayer();
        }
        else
        {
            FloatAndRotate();
        }
    }

    void FloatAndRotate()
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

    void FollowPlayer()
    {
        // 計算目標位置（玩家上方）
        followFloatTimer += Time.deltaTime;
        float floatOffset = Mathf.Sin(followFloatTimer * followFloatSpeed) * followFloatAmount;
        Vector3 targetPosition = playerTransform.position +
                                new Vector3(followOffsetX, followHeight + floatOffset, 0);

        // 平滑移動到目標位置
        transform.position = Vector3.Lerp(transform.position, targetPosition,
                                         followSmoothSpeed * Time.deltaTime);

        // 持續旋轉
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 保持發光效果
        if (spriteRenderer != null)
        {
            float alpha = 0.8f + Mathf.Sin(followFloatTimer * 4f) * 0.2f;
            spriteRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isFollowingPlayer)
        {
            if (useInventorySystem)
            {
                // 使用背包系統
                PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddKey(keyID, keyName, keyIcon);
                    Debug.Log($"[KeyItem] 玩家獲得 {keyName} (ID: {keyID}) - 已加入背包");

                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }

                    CreatePickupEffect();

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
            else
            {
                // 不使用背包系統，鑰匙跟隨玩家
                playerTransform = collision.transform;
                isFollowingPlayer = true;

                // 縮小鑰匙
                transform.localScale = originalScale * followScale;

                // 禁用碰撞器避免重複觸發
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false;
                }

                // 將鑰匙加入玩家持有列表
                if (!playerKeys.ContainsKey(keyID))
                {
                    playerKeys.Add(keyID, this);
                }
                else
                {
                    playerKeys[keyID] = this;
                }

                Debug.Log($"[KeyItem] 玩家獲得 {keyName} (ID: {keyID}) - 鑰匙跟隨玩家");

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                CreatePickupEffect();
            }
        }
    }

    void CreatePickupEffect()
    {
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

    // 公開方法：檢查玩家是否持有某個鑰匙
    public static bool PlayerHasKey(string keyID)
    {
        return playerKeys.ContainsKey(keyID) && playerKeys[keyID] != null;
    }

    // 公開方法：使用鑰匙（讓鑰匙消失）
    public static void UseKey(string keyID)
    {
        if (playerKeys.ContainsKey(keyID) && playerKeys[keyID] != null)
        {
            KeyItem key = playerKeys[keyID];
            Debug.Log($"[KeyItem] 使用鑰匙: {key.keyName}");

            // 創建消失效果
            key.CreateDisappearEffect();

            // 移除並銷毀
            playerKeys.Remove(keyID);
            Destroy(key.gameObject);
        }
    }

    void CreateDisappearEffect()
    {
        // 鑰匙消失時的閃光效果
        GameObject effectObj = new GameObject("DisappearEffect");
        effectObj.transform.position = transform.position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = glowColor;
        main.startSize = 0.8f;
        main.startSpeed = 5f;
        main.startLifetime = 0.7f;
        main.maxParticles = 30;
        main.duration = 0.7f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        ps.Play();
        Destroy(effectObj, 1.5f);
    }

    // 清空所有鑰匙（用於重置或死亡）
    public static void ClearAllKeys()
    {
        foreach (var key in playerKeys.Values)
        {
            if (key != null)
            {
                Destroy(key.gameObject);
            }
        }
        playerKeys.Clear();
        Debug.Log("[KeyItem] 所有鑰匙已清空");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"{keyName}\nID: {keyID}");
#endif
    }
}