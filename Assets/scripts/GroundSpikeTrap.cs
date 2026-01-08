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
    public Color warningColor = new Color(1f, 0.5f, 0f, 0.5f); // 橘色警告
    public Color activeColor = Color.red;
    public float warningTime = 0.5f; // 警告閃爍時間

    [Header("音效")]
    public AudioClip riseSound;
    public AudioClip hitSound;

    [Header("傷害設定")]
    [Tooltip("是否造成持續傷害")]
    public bool enableContinuousDamage = false;
    [Tooltip("持續傷害間隔")]
    public float damageInterval = 0.5f;

    private Vector3 hiddenPosition;
    private Vector3 activePosition;
    private Collider2D spikeCollider;
    private AudioSource audioSource;
    private bool isActive = false;
    private bool isTriggering = false;
    private float lastDamageTime = -999f;

    void Awake()
    {
        // 獲取組件
        if (spikeRenderer == null)
            spikeRenderer = GetComponentInChildren<SpriteRenderer>();

        spikeCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        // 如果沒有AudioSource，自動添加
        if (audioSource == null && (riseSound != null || hitSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 設定初始位置
        hiddenPosition = transform.localPosition;
        activePosition = hiddenPosition + Vector3.up * riseHeight;

        // 初始狀態：隱藏
        Hide();
    }

    /// <summary>
    /// 隱藏尖刺
    /// </summary>
    public void Hide()
    {
        if (spikeRenderer != null)
        {
            Color color = spikeRenderer.color;
            color.a = 0f;
            spikeRenderer.color = color;
        }

        if (spikeCollider != null)
            spikeCollider.enabled = false;

        transform.localPosition = hiddenPosition;
        isActive = false;
    }

    /// <summary>
    /// 觸發尖刺升起（由Boss調用）
    /// </summary>
    public void Trigger()
    {
        if (isTriggering)
        {
            return;
        }

        StartCoroutine(TriggerSequence());
    }

    /// <summary>
    /// 尖刺觸發序列
    /// </summary>
    IEnumerator TriggerSequence()
    {
        isTriggering = true;

        // 1. 警告階段（閃爍）
        yield return StartCoroutine(WarningPhase());

        // 2. 升起階段
        yield return StartCoroutine(RisePhase());

        // 3. 停留階段（造成傷害）
        yield return StartCoroutine(ActivePhase());

        // 4. 下降階段
        yield return StartCoroutine(FallPhase());

        isTriggering = false;
    }

    /// <summary>
    /// 警告階段
    /// </summary>
    IEnumerator WarningPhase()
    {
        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            // 閃爍效果
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

    /// <summary>
    /// 升起階段
    /// </summary>
    IEnumerator RisePhase()
    {
        // 播放音效
        if (riseSound != null && audioSource != null)
            audioSource.PlayOneShot(riseSound);

        float elapsed = 0f;

        while (elapsed < riseTime)
        {
            float t = elapsed / riseTime;

            // 位置插值（緩動效果）
            float easedT = EaseOutBack(t);
            transform.localPosition = Vector3.Lerp(hiddenPosition, activePosition, easedT);

            // 透明度插值
            if (spikeRenderer != null)
            {
                Color color = Color.Lerp(warningColor, activeColor, t);
                color.a = Mathf.Lerp(0.5f, 1f, t);
                spikeRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = activePosition;

        if (spikeRenderer != null)
        {
            spikeRenderer.color = activeColor;
        }
    }

    /// <summary>
    /// 活動階段（造成傷害）
    /// </summary>
    IEnumerator ActivePhase()
    {
        isActive = true;

        // 啟用碰撞體
        if (spikeCollider != null)
            spikeCollider.enabled = true;

        yield return new WaitForSeconds(stayTime);

        isActive = false;
    }

    /// <summary>
    /// 下降階段
    /// </summary>
    IEnumerator FallPhase()
    {
        // 禁用碰撞體
        if (spikeCollider != null)
            spikeCollider.enabled = false;

        float elapsed = 0f;

        while (elapsed < fallTime)
        {
            float t = elapsed / fallTime;

            // 位置插值
            transform.localPosition = Vector3.Lerp(activePosition, hiddenPosition, t);

            // 透明度插值
            if (spikeRenderer != null)
            {
                Color color = activeColor;
                color.a = Mathf.Lerp(1f, 0f, t);
                spikeRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Hide();
    }

    /// <summary>
    /// 緩動函數 - 回彈效果
    /// </summary>
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// 碰撞檢測 - 進入時造成傷害
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other.gameObject);
        }
    }

    /// <summary>
    /// 持續碰撞 - 如果啟用持續傷害
    /// </summary>
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive || !enableContinuousDamage) return;

        if (other.CompareTag("Player"))
        {
            // 檢查傷害間隔
            if (Time.time - lastDamageTime >= damageInterval)
            {
                DealDamageToPlayer(other.gameObject);
            }
        }
    }

    /// <summary>
    /// 對玩家造成傷害
    /// </summary>
    void DealDamageToPlayer(GameObject playerObject)
    {
        PlayerController2D player = playerObject.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.TakeDamage(damage);
            lastDamageTime = Time.time;

            // 播放擊中音效
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    /// <summary>
    /// 檢查尖刺是否正在觸發中
    /// </summary>
    public bool IsTriggering()
    {
        return isTriggering;
    }

    /// <summary>
    /// 檢查尖刺是否處於活動狀態（可造成傷害）
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
}