using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool hasCollided = false;
    private ArrowSpawner spawner;

    void OnEnable()
    {
        hasCollided = false;  // 每次啟用時重設碰撞狀態
    }

    public void Initialize(ArrowSpawner arrowSpawner)
    {
        spawner = arrowSpawner;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCollided)
        {
            HandleCollision();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasCollided)
        {
            HandleCollision();
        }
    }

    private void HandleCollision()
    {
        hasCollided = true;

        // 停止箭矢移動
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 通知 spawner 處理回收（立即消失）
        if (spawner != null)
        {
            spawner.OnArrowCollision(gameObject);
        }
        else
        {
            // 如果沒有 spawner 引用，直接銷毀或停用
            gameObject.SetActive(false);
        }
    }
}