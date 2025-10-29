using System.Collections;
using UnityEngine;

public class CaptureBubble : MonoBehaviour
{
    private GameObject capturedEnemy;
    private BossController2D capturedBoss;
    private float riseSpeed;
    private float riseHeight;
    private float captureDelay;
    private Vector3 startPosition;
    private bool isRising = false;
    private float captureTime;
    private bool isBoss = false;

    public void Initialize(GameObject enemy, float speed, float height, float delay)
    {
        capturedEnemy = enemy;
        capturedBoss = null;
        isBoss = false;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[��y] ��l�ơA�����ĤH�G{enemy.name}");
        if (capturedEnemy != null)
        {
            DisableEnemy();
            capturedEnemy.transform.SetParent(transform);
            capturedEnemy.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    // �i�s�W�jBoss ��l�Ƥ�k
    public void InitializeBoss(GameObject boss, float speed, float height, float delay)
    {
        capturedEnemy = null;
        capturedBoss = boss.GetComponent<BossController2D>();
        isBoss = true;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[��y] ��l�ơA����Boss�G{boss.name}");
        if (capturedBoss != null)
        {
            DisableBoss(boss);
            boss.transform.SetParent(transform);
            boss.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    void Update()
    {
        if (!isRising && Time.time - captureTime >= captureDelay)
        {
            isRising = true;
            Debug.Log("[��y] �}�l�W��");
        }
        if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (transform.position.y >= startPosition.y + riseHeight)
            {
                Debug.Log("[��y] �F��ؼа��סA�ǳƾP��");
                DestroyBubbleAndTarget();
            }
        }
    }

    void DisableEnemy()
    {
        if (capturedEnemy == null) return;
        Rigidbody2D rb = capturedEnemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        Collider2D[] colliders = capturedEnemy.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        MonoBehaviour[] scripts = capturedEnemy.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script.GetType().Name.Contains("Enemy") ||
                script.GetType().Name.Contains("AI") ||
                script.GetType().Name.Contains("Controller"))
            {
                script.enabled = false;
            }
        }
        Debug.Log($"[�����t��] �w�T�μĤH�ե�G{capturedEnemy.name}");
    }

    // �i�s�W�j�T��Boss����k
    void DisableBoss(GameObject boss)
    {
        if (boss == null) return;
        Rigidbody2D rb = boss.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        Collider2D[] colliders = boss.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        MonoBehaviour[] scripts = boss.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && !script.GetType().Name.Equals("CaptureBubble"))
            {
                script.enabled = false;
            }
        }
        Debug.Log($"[�����t��] �w�T��Boss�ե�G{boss.name}");
    }

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

    void DestroyBubbleAndTarget()
    {
        StartCoroutine(DisappearAnimation());
    }

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
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }
            yield return null;
        }
        // �i�ק�j�ھڮ������O�ĤH�٬OBoss���O�B�z
        if (capturedEnemy != null)
        {
            Debug.Log($"[�����t��] �ĤH�w�Q�����G{capturedEnemy.name}");
            Destroy(capturedEnemy);
        }
        else if (capturedBoss != null)
        {
            Debug.Log($"[�����t��] Boss�w�Q����");
            capturedBoss.OnCaptured(); // �ե�Boss�������ƥ�
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (capturedEnemy != null)
        {
            Destroy(capturedEnemy);
        }
        else if (capturedBoss != null)
        {
            GameObject bossGO = capturedBoss.gameObject;
            if (bossGO != null)
            {
                Destroy(bossGO);
            }
        }
    }
}