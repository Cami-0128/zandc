using System.Collections;
using UnityEngine;

/// <summary>
/// ������y - �]�q�ĤH�äW��
/// </summary>
public class CaptureBubble : MonoBehaviour
{
    private GameObject capturedEnemy;
    private float riseSpeed;
    private float riseHeight;
    private float captureDelay;

    private Vector3 startPosition;
    private bool isRising = false;
    private float captureTime;

    /// <summary>
    /// ��l�ƪw�w
    /// </summary>
    public void Initialize(GameObject enemy, float speed, float height, float delay)
    {
        capturedEnemy = enemy;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;

        startPosition = transform.position;
        captureTime = Time.time;

        // ���ĤH���H�w�w
        if (capturedEnemy != null)
        {
            // �T�μĤH�����z�MAI
            DisableEnemy();

            // �]�m�ĤH���w�w���l����
            capturedEnemy.transform.SetParent(transform);
            capturedEnemy.transform.localPosition = Vector3.zero;
        }

        // �Y��ʵe
        StartCoroutine(AppearAnimation());
    }

    void Update()
    {
        // ���ݩ����}�l�W��
        if (!isRising && Time.time - captureTime >= captureDelay)
        {
            isRising = true;
        }

        // �W��
        if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // �F��ؼа��׫�P��
            if (transform.position.y >= startPosition.y + riseHeight)
            {
                DestroyBubbleAndEnemy();
            }
        }
    }

    /// <summary>
    /// �T�μĤH���ե�
    /// </summary>
    void DisableEnemy()
    {
        if (capturedEnemy == null) return;

        // �T�� Rigidbody2D
        Rigidbody2D rb = capturedEnemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // �T�ΩҦ��I����
        Collider2D[] colliders = capturedEnemy.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // �T�μĤHAI�}���]�p�G�����ܡ^
        MonoBehaviour[] scripts = capturedEnemy.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // �u�T�Υi��OAI����}��
            if (script.GetType().Name.Contains("Enemy") ||
                script.GetType().Name.Contains("AI") ||
                script.GetType().Name.Contains("Controller"))
            {
                script.enabled = false;
            }
        }

        Debug.Log($"[�����t��] �w�T�μĤH�ե�G{capturedEnemy.name}");
    }

    /// <summary>
    /// �X�{�ʵe
    /// </summary>
    IEnumerator AppearAnimation()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    /// <summary>
    /// �P���w�w�M�ĤH
    /// </summary>
    void DestroyBubbleAndEnemy()
    {
        // ��������S�ġ]�p�G�ݭn���ܡ^
        StartCoroutine(DisappearAnimation());
    }

    /// <summary>
    /// �����ʵe
    /// </summary>
    IEnumerator DisappearAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // �H�X�ĪG
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }

            yield return null;
        }

        // �P���ĤH
        if (capturedEnemy != null)
        {
            Debug.Log($"[�����t��] �ĤH�w�Q�����G{capturedEnemy.name}");
            Destroy(capturedEnemy);
        }

        // �P���w�w
        Destroy(gameObject);
    }

    /// <summary>
    /// �p�G�w�w�Q�}�a�A�T�O�ĤH�]�Q�P��
    /// </summary>
    void OnDestroy()
    {
        if (capturedEnemy != null)
        {
            Destroy(capturedEnemy);
        }
    }
}