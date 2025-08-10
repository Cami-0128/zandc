using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalMovingPlatform : MonoBehaviour
{       //�������
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }
}

