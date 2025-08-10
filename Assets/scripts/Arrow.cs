using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool hasCollided = false;
    private ArrowSpawner spawner;

    void OnEnable()
    {
        hasCollided = false;  // �C���ҥήɭ��]�I�����A
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

        // ����b�ڲ���
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // �q�� spawner �B�z�^���]�ߧY�����^
        if (spawner != null)
        {
            spawner.OnArrowCollision(gameObject);
        }
        else
        {
            // �p�G�S�� spawner �ޥΡA�����P���ΰ���
            gameObject.SetActive(false);
        }
    }
}