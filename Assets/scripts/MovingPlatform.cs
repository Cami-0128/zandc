using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("平台移動的速度")]
    public float speed = 2f;

    [Tooltip("平台左右來回移動的距離（單邊）")]
    public float moveDistance = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 計算左右來回的偏移量（範圍：-moveDistance 到 +moveDistance）
        float offset = Mathf.PingPong(Time.time * speed, moveDistance * 2) - moveDistance;

        // 讓平台左右移動（X 軸）
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}
