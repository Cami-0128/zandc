using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab;             // 箭矢預製體 (確保 scale = (1,1,1))
    public float delayBetweenShots = 1.75f;   // 每次發射的間隔
    public float arrowSpeed = 5f;             // 箭矢往左的速度
    public int repeatCount = -1;              // -1 表示無限次；>0 表示固定次數

    // 物件池
    private Queue<GameObject> arrowPool = new Queue<GameObject>();
    public int initialPoolSize = 10;          // 初始池子大小
    public int maxPoolSize = 20;              // 池子最大大小
    private int currentPoolSize;
    public float recycleTime = 5.3f;          // 箭矢回收時間 / 消失時間

    // 你原本箭矢的大小（記得用箭矢 prefab scale，確保是正的）
    private Vector3 arrowOriginalScale = new Vector3(0.7897f, 1.4733f, 1f);

    // 用來追蹤正在進行回收的箭矢
    private Dictionary<GameObject, Coroutine> recycleCoroutines = new Dictionary<GameObject, Coroutine>();

    void Start()
    {
        currentPoolSize = initialPoolSize;
        InitializePool();
        StartCoroutine(FireArrowsRoutine());
    }

    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            arrow.SetActive(false);
            arrow.transform.localScale = arrowOriginalScale;  // 強制固定大小

            // 確保箭矢有 Arrow 腳本並初始化
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript == null)
            {
                arrowScript = arrow.AddComponent<Arrow>();
            }
            arrowScript.Initialize(this);

            arrowPool.Enqueue(arrow);
        }
    }

    IEnumerator FireArrowsRoutine()
    {
        int count = 0;
        while (repeatCount < 0 || count < repeatCount)
        {
            SpawnArrowFromPool();
            count++;
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    void SpawnArrowFromPool()
    {
        if (arrowPool.Count == 0)
        {
            if (currentPoolSize < maxPoolSize)
            {
                GameObject newArrow = Instantiate(arrowPrefab);
                newArrow.SetActive(false);
                newArrow.transform.localScale = arrowOriginalScale;

                // 確保新箭矢有 Arrow 腳本並初始化
                Arrow arrowScript = newArrow.GetComponent<Arrow>();
                if (arrowScript == null)
                {
                    arrowScript = newArrow.AddComponent<Arrow>();
                }
                arrowScript.Initialize(this);

                arrowPool.Enqueue(newArrow);
                currentPoolSize++;
            }
            else
            {
                Debug.LogWarning("箭矢池已滿");
                return;
            }
        }

        GameObject arrow = arrowPool.Dequeue();
        // 重設位置、旋轉與大小
        arrow.transform.SetParent(null);
        arrow.transform.position = transform.position;
        arrow.transform.rotation = Quaternion.Euler(0, 0, 90); // 讓箭圖從「朝上」變成「朝左」
        arrow.transform.localScale = arrowOriginalScale;

        // 啟用物件
        arrow.SetActive(true);

        // 確保 Rigidbody2D 存在
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.left * arrowSpeed;  // 正確給速度
            rb.angularVelocity = 0f;                  // 停止旋轉
        }
        else
        {
            Debug.LogError("箭矢 prefab 上沒有 Rigidbody2D！");  //debug
        }

        // 開始正常的回收計時
        Coroutine recycleCoroutine = StartCoroutine(RecycleArrowAfterSeconds(arrow, recycleTime));
        recycleCoroutines[arrow] = recycleCoroutine;
    }

    // 當箭矢碰撞時被調用（由 Arrow 腳本調用）
    public void OnArrowCollision(GameObject arrow)
    {
        // 停止原本的回收協程
        if (recycleCoroutines.ContainsKey(arrow))
        {
            StopCoroutine(recycleCoroutines[arrow]);
            recycleCoroutines.Remove(arrow);
        }

        // 立即回收箭矢到池中
        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }

    IEnumerator RecycleArrowAfterSeconds(GameObject arrow, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 清理記錄
        if (recycleCoroutines.ContainsKey(arrow))
        {
            recycleCoroutines.Remove(arrow);
        }

        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }
}