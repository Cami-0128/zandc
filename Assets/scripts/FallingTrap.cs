using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    [Header("陷阱設定")]
    public int damage = 20;                    // 造成的傷害
    public float detectionRange = 2f;          // 偵測範圍（玩家在下方多遠會觸發）
    public float fallSpeed = 10f;              // 落下速度
    public float resetTime = 3f;               // 重置時間（秒）
    public bool canReset = true;               // 是否可以重置

    [Header("音效設定（可選）")]
    public AudioClip triggerSound;             // 觸發音效
    public AudioClip impactSound;              // 撞擊音效

    [Header("偵測設定")]
    public LayerMask playerLayer = -1;         // 玩家圖層（-1表示所有圖層）
    public bool showDebugRays = true;          // 是否顯示偵測射線

    private Vector3 originalPosition;          // 原始位置
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isTriggered = false;          // 是否已觸發
    private bool isFalling = false;            // 是否正在落下
    private bool hasHitPlayer = false;         // 是否已經打到玩家（避免重複傷害）

    // 組件引用
    private SpriteRenderer spriteRenderer;
    private Collider2D trapCollider;

    void Start()
    {
        // 記錄原始位置
        originalPosition = transform.position;

        // 獲取組件
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trapCollider = GetComponent<Collider2D>();

        // 設定 Rigidbody2D 為 Kinematic（不受重力影響，直到觸發）
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;
        rb.gravityScale = 0;

        // 確保有碰撞器
        if (trapCollider == null)
        {
            trapCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // 設定為觸發器，這樣才能偵測玩家
        trapCollider.isTrigger = false; // 落下時需要物理碰撞
    }

    void Update()
    {
        // 如果還沒觸發且不在落下狀態，檢測玩家
        if (!isTriggered && !isFalling)
        {
            DetectPlayer();
        }
    }

    /// <summary>
    /// 偵測玩家是否在下方
    /// </summary>
    void DetectPlayer()
    {
        // 方法1：使用射線檢測
        Vector2 rayStart = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, detectionRange, playerLayer);

        // 方法2：使用範圍檢測（更可靠）
        Collider2D[] playersInRange = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x, transform.position.y - detectionRange / 2),
            new Vector2(1f, detectionRange),
            0f,
            playerLayer
        );

        bool playerDetected = false;

        // 檢查射線檢測結果
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (hit.transform.position.y < transform.position.y)
            {
                playerDetected = true;
                Debug.Log($"[射線檢測] 偵測到玩家: {hit.collider.name}");
            }
        }

        // 檢查範圍檢測結果
        foreach (Collider2D col in playersInRange)
        {
            if (col.CompareTag("Player") && col.transform.position.y < transform.position.y)
            {
                playerDetected = true;
                Debug.Log($"[範圍檢測] 偵測到玩家: {col.name}");
                break;
            }
        }

        if (playerDetected)
        {
            TriggerTrap();
        }

        // 顯示偵測範圍（Debug用）
        if (showDebugRays)
        {
            Debug.DrawRay(rayStart, Vector2.down * detectionRange, Color.red);
            // 顯示偵測盒子的四個邊
            Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - detectionRange / 2);
            Vector2 boxSize = new Vector2(1f, detectionRange);

            Vector2 topLeft = boxCenter + new Vector2(-boxSize.x / 2, boxSize.y / 2);
            Vector2 topRight = boxCenter + new Vector2(boxSize.x / 2, boxSize.y / 2);
            Vector2 bottomLeft = boxCenter + new Vector2(-boxSize.x / 2, -boxSize.y / 2);
            Vector2 bottomRight = boxCenter + new Vector2(boxSize.x / 2, -boxSize.y / 2);

            Debug.DrawLine(topLeft, topRight, Color.green);
            Debug.DrawLine(topRight, bottomRight, Color.green);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green);
            Debug.DrawLine(bottomLeft, topLeft, Color.green);
        }
    }

    /// <summary>
    /// 觸發陷阱
    /// </summary>
    void TriggerTrap()
    {
        if (isTriggered) return;

        isTriggered = true;
        isFalling = true;
        hasHitPlayer = false;

        // 播放觸發音效
        if (audioSource && triggerSound)
        {
            audioSource.PlayOneShot(triggerSound);
        }

        // 啟用物理系統讓它落下
        rb.isKinematic = false;
        rb.gravityScale = 1;

        // 給予初始向下速度
        rb.velocity = new Vector2(0, -fallSpeed);

        Debug.Log($"{gameObject.name} 陷阱被觸發！");
    }

    /// <summary>
    /// 碰撞檢測
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果撞到玩家且還沒造成傷害
        if (collision.gameObject.CompareTag("Player") && !hasHitPlayer && isFalling)
        {
            hasHitPlayer = true;

            // 對玩家造成傷害
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"{gameObject.name} 對玩家造成 {damage} 點傷害！");
            }

            // 播放撞擊音效
            if (audioSource && impactSound)
            {
                audioSource.PlayOneShot(impactSound);
            }

            // 停止移動
            StopTrap();
        }
        // 如果撞到地面或其他物體
        else if (!collision.gameObject.CompareTag("Player"))
        {
            // 停止移動
            StopTrap();
        }
    }

    /// <summary>
    /// 停止陷阱移動
    /// </summary>
    void StopTrap()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            rb.gravityScale = 0;
        }

        isFalling = false;

        // 如果可以重置，開始重置計時
        if (canReset)
        {
            StartCoroutine(ResetTrap());
        }
    }

    /// <summary>
    /// 重置陷阱到原始位置
    /// </summary>
    IEnumerator ResetTrap()
    {
        yield return new WaitForSeconds(resetTime);

        // 重置位置和狀態
        transform.position = originalPosition;
        isTriggered = false;
        isFalling = false;
        hasHitPlayer = false;

        // 確保物理設定正確
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }

        Debug.Log($"{gameObject.name} 陷阱已重置");
    }

    /// <summary>
    /// 手動觸發陷阱（供其他腳本調用）
    /// </summary>
    public void ManualTrigger()
    {
        if (!isTriggered)
        {
            TriggerTrap();
        }
    }

    /// <summary>
    /// 手動重置陷阱
    /// </summary>
    public void ManualReset()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        isTriggered = false;
        isFalling = false;
        hasHitPlayer = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 在編輯器中顯示偵測範圍
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 顯示偵測範圍
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionRange);

        // 顯示偵測區域
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position + Vector3.down * (detectionRange / 2), new Vector3(1, detectionRange, 1));
    }
}