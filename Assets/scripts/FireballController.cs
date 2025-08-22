using System.Collections;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    public float shootUpDistance = 3f;    // 火球向上發射距離
    public float moveSpeed = 5f;           // 火球移動速度
    public float pauseTime = 0.5f;         // 火球在最高點停留時間

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * shootUpDistance;
    }

    // 外部呼叫此方法開始發射火球
    public void Shoot()
    {
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        // 火球向上飛行
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 火球停留在最高點
        yield return new WaitForSeconds(pauseTime);

        // 火球下落時做圖片上下顛倒（垂直翻轉）
        Vector3 scale = transform.localScale;
        scale.y = -Mathf.Abs(scale.y);
        transform.localScale = scale;

        // 火球向下回到起始位置
        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 回到起點後恢復圖片正常顯示
        scale.y = Mathf.Abs(scale.y);
        transform.localScale = scale;

        // 銷毀火球物件，避免場景堆積
        Destroy(gameObject);
    }
}
