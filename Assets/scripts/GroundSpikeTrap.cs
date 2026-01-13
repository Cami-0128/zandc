using System.Collections;
using UnityEngine;

/// <summary>
/// 地面尖刺陷阱 - 平時隱藏，觸發時升起
/// 正式版本 - 由Boss控制觸發
/// </summary>
public class GroundSpikeTrap : MonoBehaviour
{
    [Header("尖刺設定")]
    [Tooltip("尖刺的Sprite Renderer")]
    public SpriteRenderer spikeRenderer;

    [Tooltip("尖刺傷害")]
    public int damage = 30;

    [Header("動畫設定")]
    [Tooltip("升起需要的時間")]
    public float riseTime = 0.3f;

    [Tooltip("停留時間（完全升起後）")]
    public float stayTime = 2f;

    [Tooltip("下降需要的時間")]
    public float fallTime = 0.3f;

    [Tooltip("升起高度")]
    public float riseHeight = 1f;

    [Header("視覺效果")]
    public Color warningColor = new Color(1f, 0.5f, 0f, 0.5f);
    public Color activeColor = Color.red;
    public float warningTime = 0.5f;

    [Header("音效")]
    public AudioClip riseSound;
    public AudioClip hitSound;

    [Header("傷害設定")]
    [Tooltip("是否造成持續傷害")]
    public bool enableContinuousDamage = false;
    [Tooltip("持續傷害間隔")]
    public float damageInterval = 0.5f;

    [Header("Debug設定")]
    public bool showDebugInfo = true;
    public Color gizmoColor = Color.yellow;

    private Vector3 hiddenPosition;
    private Vector3 activePosition;
    private Collider2D spikeCollider;
    private AudioSource audioSource;
    private bool isActive = false;
    private bool isTriggering = false;
    private float lastDamageTime = -999f;

    void Awake()
    {
        InitializeComponents();
        SetupPositions();
        Hide();

        if (showDebugInfo)
        {
            Debug.Log($"[Spike:{gameObject.name}] 初始化完成 - 位置: {transform.position}");
        }
    }

    void InitializeComponents()
    {
        // 獲取 Sprite Renderer（嘗試多種方式）
        if (spikeRenderer == null)
        {
            // 先檢查自己
            spikeRenderer = GetComponent<SpriteRenderer>();

            // 如果沒有，檢查子物件
            if (spikeRenderer == null)
            {
                spikeRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spikeRenderer == null)
            {
                Debug.LogError($"[Spike:{gameObject.name}] ❌ 找不到 SpriteRenderer！");
            }
            else if (showDebugInfo)
            {
                Debug.Log($"[Spike:{gameObject.name}] ✅ 找到 SpriteRenderer: {spikeRenderer.gameObject.name}");
            }
        }

        // 獲取 Collider
        spikeCollider = GetComponent<Collider2D>();
        if (spikeCollider == null)
        {
            Debug.LogError($"[Spike:{gameObject.name}] ❌ 找不到 Collider2D！");
        }
        else
        {
            if (!spikeCollider.isTrigger)
            {
                Debug.LogWarning($"[Spike:{gameObject.name}] ⚠️ Collider 的 Is Trigger 未勾選！自動設定為 Trigger");
                spikeCollider.isTrigger = true;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[Spike:{gameObject.name}] ✅ Collider 設定完成");
            }
        }

        // 獲取或創建 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (riseSound != null || hitSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            if (showDebugInfo)
            {
                Debug.Log($"[Spike:{gameObject.name}] ✅ 自動添加 AudioSource");
            }
        }
    }

    void SetupPositions()
    {
        // 設定初始和目標位置
        if (spikeRenderer != null)
        {
            hiddenPosition = spikeRenderer.transform.localPosition;
        }
        else
        {
            hiddenPosition = transform.localPosition;
        }

        activePosition = hiddenPosition + Vector3.up * riseHeight;

        if (showDebugInfo)
        {
            Debug.Log($"[Spike:{gameObject.name}] 位置設定 - 隱藏: {hiddenPosition}, 活動: {activePosition}");
        }
    }

    public void Hide()
    {
        if (spikeRenderer != null)
        {
            Color color = spikeRenderer.color;
            color.a = 0f;
            spikeRenderer.color = color;
            spikeRenderer.transform.localPosition = hiddenPosition;
        }

        if (spikeCollider != null)
            spikeCollider.enabled = false;

        isActive = false;
    }

    /// <summary>
    /// 觸發尖刺升起（由Boss調用）
    /// </summary>
    public void Trigger()
    {
        if (isTriggering)
        {
            if (showDebugInfo)
                Debug.Log($"[Spike:{gameObject.name}] 已在觸發中，忽略");
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"[Spike:{gameObject.name}] 🔥 開始觸發！");
        }

        StartCoroutine(TriggerSequence());
    }

    IEnumerator TriggerSequence()
    {
        isTriggering = true;

        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] === 開始序列 ===");

        // 1. 警告階段
        yield return StartCoroutine(WarningPhase());

        // 2. 升起階段
        yield return StartCoroutine(RisePhase());

        // 3. 停留階段
        yield return StartCoroutine(ActivePhase());

        // 4. 下降階段
        yield return StartCoroutine(FallPhase());

        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] === 序列結束 ===");

        isTriggering = false;
    }

    IEnumerator WarningPhase()
    {
        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] ⚠️ 警告階段");

        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            float alpha = Mathf.PingPong(elapsed * 8f, 1f) * 0.5f;

            if (spikeRenderer != null)
            {
                Color color = warningColor;
                color.a = alpha;
                spikeRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator RisePhase()
    {
        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] ⬆️ 升起階段");

        if (riseSound != null && audioSource != null)
            audioSource.PlayOneShot(riseSound);

        float elapsed = 0f;

        while (elapsed < riseTime)
        {
            float t = elapsed / riseTime;
            float easedT = EaseOutBack(t);

            if (spikeRenderer != null)
            {
                spikeRenderer.transform.localPosition = Vector3.Lerp(hiddenPosition, activePosition, easedT);

                Color color = Color.Lerp(warningColor, activeColor, t);
                color.a = Mathf.Lerp(0.5f, 1f, t);
                spikeRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spikeRenderer != null)
        {
            spikeRenderer.transform.localPosition = activePosition;
            spikeRenderer.color = activeColor;
        }
    }

    IEnumerator ActivePhase()
    {
        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] 💥 活動階段（可造成傷害）");

        isActive = true;

        if (spikeCollider != null)
            spikeCollider.enabled = true;

        yield return new WaitForSeconds(stayTime);

        isActive = false;

        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] 活動階段結束");
    }

    IEnumerator FallPhase()
    {
        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] ⬇️ 下降階段");

        if (spikeCollider != null)
            spikeCollider.enabled = false;

        float elapsed = 0f;

        while (elapsed < fallTime)
        {
            float t = elapsed / fallTime;

            if (spikeRenderer != null)
            {
                spikeRenderer.transform.localPosition = Vector3.Lerp(activePosition, hiddenPosition, t);

                Color color = activeColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                spikeRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Hide();
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (showDebugInfo)
            Debug.Log($"[Spike:{gameObject.name}] 碰撞檢測: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive || !enableContinuousDamage) return;

        if (other.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                DealDamageToPlayer(other.gameObject);
            }
        }
    }

    void DealDamageToPlayer(GameObject playerObject)
    {
        PlayerController2D player = playerObject.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.TakeDamage(damage);
            lastDamageTime = Time.time;

            if (showDebugInfo)
                Debug.Log($"[Spike:{gameObject.name}] ⚔️ 對玩家造成 {damage} 點傷害");

            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    public bool IsTriggering() => isTriggering;
    public bool IsActive() => isActive;

    // Gizmos 繪製
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (Application.isPlaying && isActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 顯示升起範圍
        Gizmos.color = Color.cyan;
        Vector3 topPos = transform.position + Vector3.up * riseHeight;
        Gizmos.DrawLine(transform.position, topPos);
        Gizmos.DrawWireSphere(topPos, 0.3f);
    }
}