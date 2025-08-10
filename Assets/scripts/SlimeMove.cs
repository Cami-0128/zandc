using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveRange = 2f;

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;
    private Vector3 startPos;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        leftLimit = startPos.x - moveRange;
        rightLimit = startPos.x + moveRange;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 1000f;
            rb.drag = 10f;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.y = startPos.y;
        transform.position = pos;

        if (movingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
                Flip();
            }
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 pos = transform.position;
            pos.y = startPos.y;
            transform.position = pos;
        }
    }
}