using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Portal : MonoBehaviour
{
    [Header("=== 基本設定 ===")]
    [Tooltip("傳送門名稱")]
    public string portalName = "Portal";

    [Header("=== 傳送模式 ===")]
    [Tooltip("是否啟用雙向傳送")]
    public bool isBidirectional = false;

    [Tooltip("單點傳送：目標傳送點")]
    public Transform destinationPortal;

    [Tooltip("是否啟用多點傳送")]
    public bool enableMultipleDestinations = false;

    [Tooltip("多點傳送：多個目標傳送點")]
    public List<Transform> multipleDestinations = new List<Transform>();

    [Tooltip("多點傳送模式")]
    public MultiPointMode multiPointMode = MultiPointMode.Cyclic;

    private int currentDestinationIndex = 0;

    [Header("=== 移動設定 ===")]
    [Tooltip("傳送門是否會移動")]
    public bool isMoving = false;

    [Tooltip("移動模式")]
    public MovementMode movementMode = MovementMode.Horizontal;

    [Tooltip("移動速度")]
    public float moveSpeed = 2f;

    [Tooltip("移動範圍（距離起點的最大距離）")]
    public float moveRange = 3f;

    [Tooltip("移動方向（1 = 正向, -1 = 反向）")]
    public float moveDirection = 1f;

    [Tooltip("跳躍移動：跳躍力度")]
    public float jumpForce = 5f;

    [Tooltip("跳躍移動：跳躍間隔")]
    public float jumpInterval = 2f;

    private Vector3 startPosition;
    private float moveTimer = 0f;
    private Rigidbody2D rb;

    [Header("=== 鑰匙系統 ===")]
    [Tooltip("是否需要鑰匙")]
    public bool requiresKey = false;

    [Tooltip("所需鑰匙 ID")]
    public string requiredKeyID = "Key_1";

    [Tooltip("鑰匙顯示名稱")]
    public string keyDisplayName = "金鑰匙";

    [Tooltip("是否消耗鑰匙（false = 可重複使用）")]
    public bool consumeKey = true;

    [Tooltip("鎖定狀態顏色")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Tooltip("提示UI預製體")]
    public GameObject keyHintUIPrefab;

    private GameObject keyHintUI;
    private bool isUnlocked = false;

    [Header("=== 傳送設定 ===")]
    [Tooltip("傳送冷卻時間")]
    public float teleportCooldown = 1f;

    [Tooltip("傳送後無敵時間")]
    public float invincibilityTime = 0.5f;

    private bool canTeleport = true;

    [Header("=== 視覺效果 ===")]
    [Tooltip("傳送門顏色")]
    public Color portalColor = new Color(0.3f, 0.6f, 1f, 0.8f);

    [Tooltip("傳送門整體大小倍率")]
    [Range(0.1f, 3f)]
    public float portalSizeMultiplier = 1f;

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 50f;

    [Tooltip("脈衝速度")]
    public float pulseSpeed = 2f;

    [Tooltip("脈衝幅度")]
    public float pulseAmount = 0.2f;

    [Tooltip("粒子效果預製體")]
    public GameObject particleEffectPrefab;

    [Header("=== 粒子效果細節設定 ===")]
    [Tooltip("粒子大小")]
    [Range(0.1f, 2f)]
    public float particleSize = 0.3f;

    [Tooltip("粒子發射範圍半徑")]
    [Range(0.1f, 3f)]
    public float particleRadius = 1f;

    [Tooltip("粒子數量")]
    [Range(10, 100)]
    public int particleMaxCount = 50;

    [Tooltip("粒子發射速率")]
    [Range(5, 50)]
    public int particleEmissionRate = 20;

    [Header("=== 音效 ===")]
    [Tooltip("傳送音效")]
    public AudioClip teleportSound;

    [Tooltip("解鎖音效")]
    public AudioClip unlockSound;

    [Tooltip("鎖定音效")]
    public AudioClip lockedSound;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private ParticleSystem portalParticles;

    // 列舉類型
    public enum MultiPointMode { Cyclic, Random }
    public enum MovementMode { Horizontal, Vertical, Jump }

    void Start()
    {
        InitializeComponents();
        startPosition = transform.position;

        if (!requiresKey)
        {
            isUnlocked = true;
        }

        UpdateVisualState();
    }

    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalScale = transform.localScale;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (isMoving && movementMode == MovementMode.Jump)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
            }
        }

        CreateParticleEffect();
        CreateKeyHintUI();
    }

    void Update()
    {
        // 視覺效果
        AnimatePortal();

        // 移動邏輯
        if (isMoving)
        {
            HandleMovement();
        }

        // 更新提示UI位置
        UpdateKeyHintPosition();
    }

    void AnimatePortal()
    {
        // 旋轉效果
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // 脈衝效果
        if (spriteRenderer != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    void HandleMovement()
    {
        switch (movementMode)
        {
            case MovementMode.Horizontal:
                MoveHorizontal();
                break;
            case MovementMode.Vertical:
                MoveVertical();
                break;
            case MovementMode.Jump:
                MoveJump();
                break;
        }
    }

    void MoveHorizontal()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2) - moveRange;
        transform.position = startPosition + Vector3.right * offset * moveDirection;
    }

    void MoveVertical()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2) - moveRange;
        transform.position = startPosition + Vector3.up * offset * moveDirection;
    }

    void MoveJump()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer >= jumpInterval && rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * moveDirection);
            moveTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                if (requiresKey && !isUnlocked)
                {
                    TryUnlock(player);
                }
                else if (canTeleport)
                {
                    Transform destination = GetDestination();
                    if (destination != null)
                    {
                        StartCoroutine(TeleportPlayer(player, destination));
                    }
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && requiresKey && !isUnlocked)
        {
            ShowKeyHintUI(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowKeyHintUI(false);
        }
    }

    void TryUnlock(PlayerController2D player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null && inventory.HasKey(requiredKeyID))
        {
            if (consumeKey)
            {
                inventory.UseKey(requiredKeyID);
            }
            Unlock();
        }
        else
        {
            if (lockedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            ShowKeyHintUI(true);
            Debug.Log($"[Portal] 需要 {keyDisplayName} 才能使用傳送門！");
        }
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log($"[Portal] {portalName} 已解鎖！");

        if (unlockSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        UpdateVisualState();
        ShowKeyHintUI(false);
        StartCoroutine(UnlockEffect());
    }

    Transform GetDestination()
    {
        if (enableMultipleDestinations && multipleDestinations.Count > 0)
        {
            switch (multiPointMode)
            {
                case MultiPointMode.Cyclic:
                    Transform dest = multipleDestinations[currentDestinationIndex];
                    currentDestinationIndex = (currentDestinationIndex + 1) % multipleDestinations.Count;
                    return dest;

                case MultiPointMode.Random:
                    return multipleDestinations[Random.Range(0, multipleDestinations.Count)];
            }
        }

        return destinationPortal;
    }

    IEnumerator TeleportPlayer(PlayerController2D player, Transform destination)
    {
        canTeleport = false;

        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        yield return StartCoroutine(TeleportEffect(player, true));

        player.transform.position = destination.position;
        Debug.Log($"[Portal] 玩家已傳送到 {destination.name}");

        yield return StartCoroutine(TeleportEffect(player, false));
        yield return new WaitForSeconds(invincibilityTime);
        yield return new WaitForSeconds(teleportCooldown);

        canTeleport = true;
    }

    IEnumerator TeleportEffect(PlayerController2D player, bool isBefore)
    {
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Color originalColor = playerSprite.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = isBefore ? Mathf.Lerp(1f, 0f, elapsed / duration) : Mathf.Lerp(0f, 1f, elapsed / duration);
                playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, isBefore ? 0f : 1f);
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator UnlockEffect()
    {
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = portalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    void UpdateVisualState()
    {
        Color targetColor = isUnlocked ? portalColor : lockedColor;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }

        if (portalParticles != null)
        {
            var main = portalParticles.main;
            main.startColor = targetColor;
        }
    }

    void CreateParticleEffect()
    {
        if (particleEffectPrefab != null)
        {
            GameObject particles = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity, transform);
            portalParticles = particles.GetComponent<ParticleSystem>();
        }
        else
        {
            GameObject particleObj = new GameObject("PortalParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            portalParticles = particleObj.AddComponent<ParticleSystem>();
            var main = portalParticles.main;
            main.startColor = portalColor;
            main.startSize = 0.3f;
            main.startSpeed = 2f;
            main.startLifetime = 1f;
            main.maxParticles = 50;

            var emission = portalParticles.emission;
            emission.rateOverTime = 20;

            var shape = portalParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
        }
    }

    void CreateKeyHintUI()
    {
        if (requiresKey && keyHintUIPrefab != null)
        {
            keyHintUI = Instantiate(keyHintUIPrefab, transform);
            keyHintUI.transform.localPosition = new Vector3(0, 1.5f, 0);
            keyHintUI.SetActive(false);

            TextMeshProUGUI text = keyHintUI.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"需要 {keyDisplayName}";
            }
        }
        else if (requiresKey && keyHintUIPrefab == null)
        {
            // 創建簡單的文字提示
            GameObject hintObj = new GameObject("KeyHint");
            hintObj.transform.SetParent(transform);
            hintObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            TextMesh textMesh = hintObj.AddComponent<TextMesh>();
            textMesh.text = $"🔒 需要 {keyDisplayName}";
            textMesh.fontSize = 30;
            textMesh.color = Color.yellow;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            keyHintUI = hintObj;
            keyHintUI.SetActive(false);
        }
    }

    void ShowKeyHintUI(bool show)
    {
        if (keyHintUI != null)
        {
            keyHintUI.SetActive(show);
        }
    }

    void UpdateKeyHintPosition()
    {
        if (keyHintUI != null && keyHintUI.activeSelf)
        {
            // 讓提示始終保持在傳送門上方
            keyHintUI.transform.position = transform.position + Vector3.up * 1.5f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isUnlocked ? Color.cyan : Color.gray;

        if (enableMultipleDestinations)
        {
            foreach (Transform dest in multipleDestinations)
            {
                if (dest != null)
                {
                    Gizmos.DrawLine(transform.position, dest.position);
                    Gizmos.DrawWireSphere(dest.position, 0.3f);
                }
            }
        }
        else if (destinationPortal != null)
        {
            Gizmos.DrawLine(transform.position, destinationPortal.position);
            Gizmos.DrawWireSphere(destinationPortal.position, 0.5f);
        }

        if (isMoving)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = Application.isPlaying ? startPosition : transform.position;

            switch (movementMode)
            {
                case MovementMode.Horizontal:
                    Gizmos.DrawLine(start + Vector3.left * moveRange, start + Vector3.right * moveRange);
                    break;
                case MovementMode.Vertical:
                    Gizmos.DrawLine(start + Vector3.down * moveRange, start + Vector3.up * moveRange);
                    break;
            }
        }
    }

    public void ForceUnlock()
    {
        requiresKey = false;
        Unlock();
    }

    public void SetActive(bool active)
    {
        canTeleport = active;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
    }
}