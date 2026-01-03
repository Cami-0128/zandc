using System.Collections;
using UnityEngine;

public class CaptureBubble : MonoBehaviour
{
    private GameObject capturedEnemy;
    private BossController2D capturedBoss;
    private NormalFish capturedNormalFish;      // ✅ 新增
    private SpecialFish capturedSpecialFish;    // ✅ 新增

    private float riseSpeed;
    private float riseHeight;
    private float captureDelay;
    private Vector3 startPosition;
    private bool isRising = false;
    private float captureTime;
    private bool isBoss = false;
    private bool isFish = false;              // ✅ 新增

    public void Initialize(GameObject enemy, float speed, float height, float delay)
    {
        capturedEnemy = enemy;
        capturedBoss = null;
        capturedNormalFish = null;
        capturedSpecialFish = null;
        isBoss = false;
        isFish = false;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[圓球] 初始化，捕捉敵人：{enemy.name}");
        if (capturedEnemy != null)
        {
            DisableEnemy();
            capturedEnemy.transform.SetParent(transform);
            capturedEnemy.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    // 【新增】Boss 初始化方法
    public void InitializeBoss(GameObject boss, float speed, float height, float delay)
    {
        capturedEnemy = null;
        capturedBoss = boss.GetComponent<BossController2D>();
        capturedNormalFish = null;
        capturedSpecialFish = null;
        isBoss = true;
        isFish = false;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[圓球] 初始化，捕捉Boss：{boss.name}");
        if (capturedBoss != null)
        {
            DisableBoss(boss);
            boss.transform.SetParent(transform);
            boss.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    // ✅ 新增：普通魚初始化方法
    public void InitializeNormalFish(GameObject fish, float speed, float height, float delay)
    {
        capturedEnemy = null;
        capturedBoss = null;
        capturedNormalFish = fish.GetComponent<NormalFish>();
        capturedSpecialFish = null;
        isBoss = false;
        isFish = true;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[圓球] 初始化，捕捉普通魚：{fish.name}");
        if (capturedNormalFish != null)
        {
            DisableFish(fish);
            fish.transform.SetParent(transform);
            fish.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    // ✅ 新增：特殊魚初始化方法
    public void InitializeSpecialFish(GameObject fish, float speed, float height, float delay)
    {
        capturedEnemy = null;
        capturedBoss = null;
        capturedNormalFish = null;
        capturedSpecialFish = fish.GetComponent<SpecialFish>();
        isBoss = false;
        isFish = true;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;
        startPosition = transform.position;
        captureTime = Time.time;
        Debug.Log($"[圓球] 初始化，捕捉特殊魚：{fish.name}");
        if (capturedSpecialFish != null)
        {
            DisableFish(fish);
            fish.transform.SetParent(transform);
            fish.transform.localPosition = Vector3.zero;
        }
        StartCoroutine(AppearAnimation());
    }

    void Update()
    {
        if (!isRising && Time.time - captureTime >= captureDelay)
        {
            isRising = true;
            Debug.Log("[圓球] 開始上升");
        }
        if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (transform.position.y >= startPosition.y + riseHeight)
            {
                Debug.Log("[圓球] 達到目標高度，準備銷毀");
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
        Debug.Log($"[捕捉系統] 已禁用敵人組件：{capturedEnemy.name}");
    }

    // 【新增】禁用Boss的方法
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
        Debug.Log($"[捕捉系統] 已禁用Boss組件：{boss.name}");
    }

    // ✅ 新增：禁用魚的方法
    void DisableFish(GameObject fish)
    {
        if (fish == null) return;
        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        Collider2D[] colliders = fish.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        MonoBehaviour[] scripts = fish.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && !script.GetType().Name.Equals("CaptureBubble"))
            {
                script.enabled = false;
            }
        }
        Debug.Log($"[捕捉系統] 已禁用魚的組件：{fish.name}");
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

        // ✅ 根據捕捉的目標類型分別處理
        if (capturedEnemy != null)
        {
            Debug.Log($"[捕捉系統] 敵人已被消除：{capturedEnemy.name}");
            Destroy(capturedEnemy);
        }
        else if (capturedBoss != null)
        {
            Debug.Log($"[捕捉系統] Boss已被消除");
            capturedBoss.OnCaptured();
        }
        else if (capturedNormalFish != null)
        {
            Debug.Log($"[捕捉系統] 普通魚已被捕捉");
            capturedNormalFish.OnCaptured();
        }
        else if (capturedSpecialFish != null)
        {
            Debug.Log($"[捕捉系統] 特殊魚已被捕捉");
            capturedSpecialFish.OnCaptured();
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
        else if (capturedNormalFish != null)
        {
            GameObject fishGO = capturedNormalFish.gameObject;
            if (fishGO != null)
            {
                Destroy(fishGO);
            }
        }
        else if (capturedSpecialFish != null)
        {
            GameObject fishGO = capturedSpecialFish.gameObject;
            if (fishGO != null)
            {
                Destroy(fishGO);
            }
        }
    }
}