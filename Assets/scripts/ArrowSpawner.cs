using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab;             // �b�ڹw�s�� (�T�O scale = (1,1,1))
    public float delayBetweenShots = 1.75f;   // �C���o�g�����j
    public float arrowSpeed = 5f;             // �b�ک������t��
    public int repeatCount = -1;              // -1 ��ܵL�����F>0 ��ܩT�w����

    // �����
    private Queue<GameObject> arrowPool = new Queue<GameObject>();
    public int initialPoolSize = 10;          // ��l���l�j�p
    public int maxPoolSize = 20;              // ���l�̤j�j�p
    private int currentPoolSize;
    public float recycleTime = 5.3f;          // �b�ڦ^���ɶ� / �����ɶ�

    // �A�쥻�b�ڪ��j�p�]�O�o�νb�� prefab scale�A�T�O�O�����^
    private Vector3 arrowOriginalScale = new Vector3(0.7897f, 1.4733f, 1f);

    // �ΨӰl�ܥ��b�i��^�����b��
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
            arrow.transform.localScale = arrowOriginalScale;  // �j��T�w�j�p

            // �T�O�b�ڦ� Arrow �}���ê�l��
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

                // �T�O�s�b�ڦ� Arrow �}���ê�l��
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
                Debug.LogWarning("�b�ڦ��w��");
                return;
            }
        }

        GameObject arrow = arrowPool.Dequeue();
        // ���]��m�B����P�j�p
        arrow.transform.SetParent(null);
        arrow.transform.position = transform.position;
        arrow.transform.rotation = Quaternion.Euler(0, 0, 90); // ���b�ϱq�u�¤W�v�ܦ��u�¥��v
        arrow.transform.localScale = arrowOriginalScale;

        // �ҥΪ���
        arrow.SetActive(true);

        // �T�O Rigidbody2D �s�b
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.left * arrowSpeed;  // ���T���t��
            rb.angularVelocity = 0f;                  // �������
        }
        else
        {
            Debug.LogError("�b�� prefab �W�S�� Rigidbody2D�I");  //debug
        }

        // �}�l���`���^���p��
        Coroutine recycleCoroutine = StartCoroutine(RecycleArrowAfterSeconds(arrow, recycleTime));
        recycleCoroutines[arrow] = recycleCoroutine;
    }

    // ��b�ڸI���ɳQ�եΡ]�� Arrow �}���եΡ^
    public void OnArrowCollision(GameObject arrow)
    {
        // ����쥻���^����{
        if (recycleCoroutines.ContainsKey(arrow))
        {
            StopCoroutine(recycleCoroutines[arrow]);
            recycleCoroutines.Remove(arrow);
        }

        // �ߧY�^���b�ڨ����
        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }

    IEnumerator RecycleArrowAfterSeconds(GameObject arrow, float delay)
    {
        yield return new WaitForSeconds(delay);

        // �M�z�O��
        if (recycleCoroutines.ContainsKey(arrow))
        {
            recycleCoroutines.Remove(arrow);
        }

        arrow.SetActive(false);
        arrowPool.Enqueue(arrow);
    }
}