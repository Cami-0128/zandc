using System.Collections;
using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    public GameObject fireballPrefab;      // 火球Prefab
    public float shootInterval = 3f;       // 發射間隔(秒)
    public Transform shootPoint;           // 發射位置 (可以是發射器下面位置空物件)

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
            // 生成火球
            GameObject fb = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);
            // 取得控制腳本，開始發射行為
            FireballController fc = fb.GetComponent<FireballController>();
            if (fc != null)
            {
                fc.Shoot();
            }
        }
        else
        {
            Debug.LogWarning("尚未設定火球Prefab或發射位置");
        }
    }
}
