using System.Collections;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    public float shootUpDistance = 3f;    // ���y�V�W�o�g�Z��
    public float moveSpeed = 5f;           // ���y���ʳt��
    public float pauseTime = 0.5f;         // ���y�b�̰��I���d�ɶ�

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * shootUpDistance;
    }

    // �~���I�s����k�}�l�o�g���y
    public void Shoot()
    {
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        // ���y�V�W����
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // ���y���d�b�̰��I
        yield return new WaitForSeconds(pauseTime);

        // ���y�U���ɰ��Ϥ��W�U�A�ˡ]����½��^
        Vector3 scale = transform.localScale;
        scale.y = -Mathf.Abs(scale.y);
        transform.localScale = scale;

        // ���y�V�U�^��_�l��m
        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // �^��_�I���_�Ϥ����`���
        scale.y = Mathf.Abs(scale.y);
        transform.localScale = scale;

        // �P�����y����A�קK������n
        Destroy(gameObject);
    }
}
