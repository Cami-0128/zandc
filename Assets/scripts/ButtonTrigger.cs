using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("要控制的地板(可以多個)")]
    public FloorController[] targetFloors;

    [Header("按鈕設定")]
    [Tooltip("按鈕類型")]
    public ButtonType buttonType = ButtonType.FloorButton;

    [Tooltip("觸發標籤(例如: Player)")]
    public string triggerTag = "Player";

    [Tooltip("是否可以重複觸發")]
    public bool canRetrigger = false;

    [Tooltip("按下時是否需要持續壓著")]
    public bool requireHold = false;

    [Header("視覺反饋")]
    [Tooltip("按鈕按下的位置偏移")]
    public Vector3 pressedOffset = new Vector3(0, -0.1f, 0);

    [Tooltip("按下動畫時間(秒)")]
    public float pressAnimationTime = 0.15f;

    [Tooltip("彈起動畫時間(秒)")]
    public float releaseAnimationTime = 0.2f;

    [Tooltip("使用平滑動畫")]
    public bool useSmoothAnimation = true;

    [Tooltip("按鈕顏色變化")]
    public Color pressedColor = Color.green;

    [Tooltip("按下時是否發光")]
    public bool glowWhenPressed = true;

    [Tooltip("發光顏色")]
    public Color glowColor = Color.yellow;

    [Tooltip("是否顯示粒子特效")]
    public bool showParticles = false;

    [Tooltip("粒子特效預製體")]
    public GameObject particlePrefab;

    [Header("音效設定(可選)")]
    public AudioClip pressSound;
    public AudioClip releaseSound;

    private Vector3 originalPosition;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isPressed = false;
    private bool hasTriggered = false;
    private int objectsOnButton = 0;
    private GameObject glowEffect;
    private GameObject currentParticles;
    private Coroutine pressCoroutine;
    private Coroutine releaseCoroutine;

    public enum ButtonType
    {
        FloorButton,    // 地板按鈕(踩踏)
        WallButton      // 牆壁按鈕(碰觸)
    }

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 確保有 Collider2D 且設為 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("ButtonTrigger: 按鈕需要 Collider2D 組件！");
        }

        // 創建發光效果物件
        if (glowWhenPressed && spriteRenderer != null)
        {
            CreateGlowEffect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag))
        {
            objectsOnButton++;

            if (!isPressed)
            {
                PressButton();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag))
        {
            objectsOnButton--;

            if (objectsOnButton <= 0 && isPressed && requireHold)
            {
                ReleaseButton();
            }
        }
    }

    private void PressButton()
    {
        if (!canRetrigger && hasTriggered)
            return;

        isPressed = true;
        hasTriggered = true;

        // 停止正在進行的釋放動畫
        if (releaseCoroutine != null)
        {
            StopCoroutine(releaseCoroutine);
            releaseCoroutine = null;
        }

        // 開始按下動畫
        if (useSmoothAnimation)
        {
            pressCoroutine = StartCoroutine(PressAnimationCoroutine());
        }
        else
        {
            // 立即按下
            transform.position = originalPosition + pressedOffset;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = pressedColor;
            }
        }

        // 發光效果
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
        }

        // 粒子特效
        if (showParticles && particlePrefab != null)
        {
            currentParticles = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            Destroy(currentParticles, 2f);
        }

        // 播放音效
        if (pressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        // 觸發地板
        TriggerFloors();
    }

    private void ReleaseButton()
    {
        isPressed = false;

        // 停止正在進行的按下動畫
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        // 開始彈起動畫
        if (useSmoothAnimation)
        {
            releaseCoroutine = StartCoroutine(ReleaseAnimationCoroutine());
        }
        else
        {
            // 立即彈起
            transform.position = originalPosition;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        // 關閉發光
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }

        // 播放音效
        if (releaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }
    }

    private System.Collections.IEnumerator PressAnimationCoroutine()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = originalPosition + pressedOffset;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < pressAnimationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pressAnimationTime;

            // 使用 EaseOut 曲線讓按下更有彈性
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(startColor, pressedColor, smoothT);
            }

            yield return null;
        }

        transform.position = targetPos;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = pressedColor;
        }
    }

    private System.Collections.IEnumerator ReleaseAnimationCoroutine()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < releaseAnimationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / releaseAnimationTime;

            // 使用 EaseOutBack 曲線製造彈跳效果
            float smoothT = 1f + (--t) * t * ((1.70158f + 1f) * t + 1.70158f);

            transform.position = Vector3.Lerp(startPos, originalPosition, smoothT);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(startColor, originalColor, t);
            }

            yield return null;
        }

        transform.position = originalPosition;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void CreateGlowEffect()
    {
        glowEffect = new GameObject("GlowEffect");
        glowEffect.transform.SetParent(transform);
        glowEffect.transform.localPosition = Vector3.zero;

        SpriteRenderer glowRenderer = glowEffect.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = spriteRenderer.sprite;
        glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
        glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

        // 稍微放大發光效果
        glowEffect.transform.localScale = Vector3.one * 1.2f;
        glowEffect.SetActive(false);
    }

    private void TriggerFloors()
    {
        if (targetFloors == null || targetFloors.Length == 0)
        {
            Debug.LogWarning("ButtonTrigger: 未設定目標地板！");
            return;
        }

        foreach (FloorController floor in targetFloors)
        {
            if (floor != null)
            {
                floor.TriggerDisappear();
            }
        }
    }

    // 重置按鈕(可供外部調用)
    public void ResetButton()
    {
        hasTriggered = false;
        ReleaseButton();
    }

    // 在編輯器中繪製按鈕範圍
    void OnDrawGizmos()
    {
        Gizmos.color = isPressed ? Color.green : Color.red;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}