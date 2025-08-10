using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMove : MonoBehaviour
{
    public float moveSpeed = 2f;      // 移動速度
    public float moveRange = 2f;      // 左右移動的範圍（從起點延伸的距離）

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
        // 水平翻轉角色
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
