using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("���ʳ]�w")]
    [Tooltip("���x���ʪ��t��")]
    public float speed = 2f;

    [Tooltip("���x���k�Ӧ^���ʪ��Z���]����^")]
    public float moveDistance = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // �p�⥪�k�Ӧ^�������q�]�d��G-moveDistance �� +moveDistance�^
        float offset = Mathf.PingPong(Time.time * speed, moveDistance * 2) - moveDistance;

        // �����x���k���ʡ]X �b�^
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}
