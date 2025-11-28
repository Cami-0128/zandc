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

        // 視覺反饋
        transform.position = originalPosition + pressedOffset;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = pressedColor;
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

        // 恢復視覺
        transform.position = originalPosition;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
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