using System.Collections;
using UnityEngine;

/// <summary>
/// 地面尖刺陷阱 - 平時隱藏，觸發時升起
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

    [Header("Debug")]
    public bool debugMode = false;

    private Vector3 hiddenPosition;
    private Vector3 activePosition;
    private Collider2D spikeCollider;
    private AudioSource audioSource;
    private bool isActive = false;
    private bool isTriggering = false;

    void Awake()
    {
        // 獲取組件
        if (spikeRenderer == null)
            spikeRenderer = GetComponent<SpriteRenderer>();

        spikeCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

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

        if (debugMode)
            Debug.Log("[GroundSpike] 尖刺已隱藏");
    }

    /// <summary>
    /// 觸發尖刺升起
    /// </summary>
    public void Trigger()
    {
        if (isTriggering)
        {
            if (debugMode)
                Debug.Log("[GroundSpike] 尖刺正在觸發中，忽略重複觸發");
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

        if (debugMode)
            Debug.Log("[GroundSpike] === 開始尖刺序列 ===");

        // 1. 警告階段（閃爍）
        yield return StartCoroutine(WarningPhase());

        // 2. 升起階段
        yield return StartCoroutine(RisePhase());

        // 3. 停留階段（造成傷害）
        yield return StartCoroutine(ActivePhase());

        // 4. 下降階段
        yield return StartCoroutine(FallPhase());

        if (debugMode)
            Debug.Log("[GroundSpike] === 尖刺序列結束 ===");

        isTriggering = false;
    }

    /// <summary>
    /// 警告階段
    /// </summary>
    IEnumerator WarningPhase()
    {
        if (debugMode)
            Debug.Log("[GroundSpike] 警告階段");

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
        if (debugMode)
            Debug.Log("[GroundSpike] 升起階段");

        // 播放音效
        if (riseSound != null && audioSource != null)
            audioSource.PlayOneShot(riseSound);

        float elapsed = 0f;

        while (elapsed < riseTime)
        {
            float t = elapsed / riseTime;

            // 位置插值
            transform.localPosition = Vector3.Lerp(hiddenPosition, activePosition, t);

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
        if (debugMode)
            Debug.Log("[GroundSpike] 活動階段（造成傷害）");

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
        if (debugMode)
            Debug.Log("[GroundSpike] 下降階段");

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
    /// 碰撞檢測
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            if (debugMode)
                Debug.Log($"[GroundSpike] 擊中玩家，造成 {damage} 點傷害");

            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            // 播放擊中音效
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // 持續傷害（每0.5秒）
        if (isActive && other.CompareTag("Player"))
        {
            // 這裡可以加入持續傷害邏輯
        }
    }
}