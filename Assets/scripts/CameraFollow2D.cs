using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraFollow2D : MonoBehaviour  //Main Camera
{
    public Transform target; public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 2, -10); // 預設向上偏移 2 單位
    void LateUpdate() {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
