using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float groundCheckDistance = 1f; // �����a�O�e�t���Z��

    private bool movingRight = false; // ��l�V������
    private Rigidbody2D rb;
    private Vector3 localScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        localScale = transform.localScale;
    }

    void Update()
    {
        Move();
        CheckGroundAhead();
    }

    void Move()
    {
        float direction = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);
    }

    void CheckGroundAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(movingRight ? 0.5f : -0.5f, -0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance);

        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);

        if (hit.collider == null)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemy1"))
        {
            Flip();
        }
    }
}
