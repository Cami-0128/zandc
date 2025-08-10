using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMove : MonoBehaviour
{
    public float moveSpeed = 2f;      // ���ʳt��
    public float moveRange = 2f;      // ���k���ʪ��d��]�q�_�I�������Z���^

    private float leftLimit;
    private float rightLimit;
    private bool movingRight = true;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        leftLimit = startPos.x - moveRange;
        rightLimit = startPos.x + moveRange;
    }

    void Update()
    {
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
        // ����½�ਤ��
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
