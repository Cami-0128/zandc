using System.Collections;
using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    public GameObject fireballPrefab;      // ���yPrefab
    public float shootInterval = 3f;       // �o�g���j(��)
    public Transform shootPoint;           // �o�g��m (�i�H�O�o�g���U����m�Ū���)

    private void Start()
    {
        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        while (true)
        {
            ShootFireball();
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootFireball()
    {
        if (fireballPrefab != null && shootPoint != null)
        {
            // �ͦ����y
            GameObject fb = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);
            // ���o����}���A�}�l�o�g�欰
            FireballController fc = fb.GetComponent<FireballController>();
            if (fc != null)
            {
                fc.Shoot();
            }
        }
        else
        {
            Debug.LogWarning("�|���]�w���yPrefab�εo�g��m");
        }
    }
}
