using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public string keyDisplayName = "KEY";   //"金鑰匙"

    [Tooltip("是否消耗鑰匙（false = 可重複使用）")]
    public bool consumeKey = true;

    [Tooltip("鎖定狀態顏色")]
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Tooltip("解鎖狀態顏色")]
    public Color unlockedColor = new Color(0.3f, 0.6f, 1f, 0.8f);

    [Tooltip("提示 UI Prefab (World Space Canvas)")]
    public GameObject keyHintUIPrefab;

    [Tooltip("提示 UI 在傳送門上方的高度")]
    public float hintUIHeight = 2f;

    [Tooltip("提示 UI 顯示時間（秒）")]
    public float hintDisplayDuration = 5f;

    [Tooltip("鎖定狀態的粒子效果減弱倍率")]
    [Range(0.1f, 1f)]
    public float lockedParticleMultiplier = 0.3f;

    private GameObject keyHintUI;
    private Canvas keyHintCanvas;
    private TextMeshProUGUI hintText;
    private Coroutine hideHintCoroutine;
    private bool isUnlocked = false;
    private bool playerInRange = false;

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

    [Tooltip("粒子生命週期（秒）")]
    [Range(0.3f, 2f)]
    public float particleLifetime = 1f;

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
    private float originalParticleSize;
    private float originalParticleRadius;
    private int originalParticleMaxCount;
    private int originalParticleEmissionRate;

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

        // 保存原始粒子參數
        originalParticleSize = particleSize;
        originalParticleRadius = particleRadius;
        originalParticleMaxCount = particleMaxCount;
        originalParticleEmissionRate = particleEmissionRate;

        UpdateVisualState();
    }

    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalScale = transform.localScale * portalSizeMultiplier;
            transform.localScale = originalScale;
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
        AnimatePortal();

        if (isMoving)
        {
            HandleMovement();
        }

        UpdateKeyHintPosition();
    }

    void AnimatePortal()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

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
            playerInRange = true;
            PlayerController2D player = collision.GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (requiresKey && !isUnlocked)
                {
                    TryUnlock();
                }
                else if (canTeleport && isUnlocked)
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

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void TryUnlock()
    {
        bool hasKey = KeyItem.PlayerHasKey(requiredKeyID);

        if (!hasKey)
        {
            PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null)
            {
                hasKey = inventory.HasKey(requiredKeyID);
            }
        }

        if (hasKey)
        {
            if (consumeKey)
            {
                KeyItem.UseKey(requiredKeyID);

                PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.UseKey(requiredKeyID);
                }
            }

            Unlock();
        }
        else
        {
            if (lockedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            ShowKeyHintUI();
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
        HideKeyHintUI();
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
                spriteRenderer.color = isUnlocked ? (portalColor != Color.clear ? portalColor : unlockedColor) : lockedColor;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (portalParticles != null)
        {
            var emission = portalParticles.emission;
            emission.rateOverTime = particleEmissionRate * 5;
            yield return new WaitForSeconds(0.5f);
            emission.rateOverTime = particleEmissionRate;
        }
    }

    void UpdateVisualState()
    {
        Color targetColor = isUnlocked ?
            (portalColor != Color.clear ? portalColor : unlockedColor) :
            lockedColor;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }

        if (portalParticles != null)
        {
            var main = portalParticles.main;
            main.startColor = targetColor;

            // 根據鎖定狀態調整粒子效果
            if (requiresKey && !isUnlocked)
            {
                // 鎖定狀態：減弱粒子效果
                main.startSize = originalParticleSize * portalSizeMultiplier * lockedParticleMultiplier;
                main.startLifetime = particleLifetime * 0.5f; // 生命週期減半

                var shape = portalParticles.shape;
                shape.radius = originalParticleRadius * portalSizeMultiplier * lockedParticleMultiplier;

                var emission = portalParticles.emission;
                emission.rateOverTime = (int)(originalParticleEmissionRate * lockedParticleMultiplier);

                main.maxParticles = (int)(originalParticleMaxCount * lockedParticleMultiplier);
            }
            else
            {
                // 解鎖狀態：正常粒子效果
                main.startSize = originalParticleSize * portalSizeMultiplier;
                main.startLifetime = particleLifetime;

                var shape = portalParticles.shape;
                shape.radius = originalParticleRadius * portalSizeMultiplier;

                var emission = portalParticles.emission;
                emission.rateOverTime = originalParticleEmissionRate;

                main.maxParticles = originalParticleMaxCount;
            }
        }
    }

    void CreateParticleEffect()
    {
        if (particleEffectPrefab != null)
        {
            GameObject particles = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity, transform);
            portalParticles = particles.GetComponent<ParticleSystem>();

            if (portalParticles != null)
            {
                var main = portalParticles.main;
                main.startSize = particleSize * portalSizeMultiplier;
                main.startLifetime = particleLifetime;

                var shape = portalParticles.shape;
                shape.radius = particleRadius * portalSizeMultiplier;
            }
        }
        else
        {
            GameObject particleObj = new GameObject("PortalParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            portalParticles = particleObj.AddComponent<ParticleSystem>();
            var main = portalParticles.main;
            main.startColor = portalColor;
            main.startSize = particleSize * portalSizeMultiplier;
            main.startSpeed = 2f;
            main.startLifetime = particleLifetime;
            main.maxParticles = particleMaxCount;

            var emission = portalParticles.emission;
            emission.rateOverTime = particleEmissionRate;

            var shape = portalParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = particleRadius * portalSizeMultiplier;
        }
    }

    void CreateKeyHintUI()
    {
        if (requiresKey && keyHintUIPrefab != null)
        {
            keyHintUI = Instantiate(keyHintUIPrefab, transform.position + Vector3.up * hintUIHeight, Quaternion.identity);
            keyHintCanvas = keyHintUI.GetComponent<Canvas>();

            if (keyHintCanvas != null)
            {
                keyHintCanvas.worldCamera = Camera.main;
            }

            // 尋找 HintText
            hintText = keyHintUI.GetComponentInChildren<TextMeshProUGUI>();
            if (hintText != null)
            {
                hintText.text = $"need {keyDisplayName}";   //$"需要 {keyDisplayName}"
            }

            keyHintUI.SetActive(false);
        }
    }

    void ShowKeyHintUI()
    {
        if (keyHintUI != null)
        {
            keyHintUI.SetActive(true);

            // 取消之前的隱藏協程
            if (hideHintCoroutine != null)
            {
                StopCoroutine(hideHintCoroutine);
            }

            // 啟動新的隱藏協程
            hideHintCoroutine = StartCoroutine(HideHintAfterDelay());
        }
    }

    void HideKeyHintUI()
    {
        if (keyHintUI != null)
        {
            keyHintUI.SetActive(false);
        }

        if (hideHintCoroutine != null)
        {
            StopCoroutine(hideHintCoroutine);
            hideHintCoroutine = null;
        }
    }

    IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(hintDisplayDuration);
        HideKeyHintUI();
    }

    void UpdateKeyHintPosition()
    {
        if (keyHintUI != null && keyHintUI.activeSelf)
        {
            keyHintUI.transform.position = transform.position + Vector3.up * hintUIHeight;
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

        if (requiresKey)
        {
            Gizmos.color = isUnlocked ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            // 顯示提示UI位置
            Gizmos.color = Color.yellow;
            Vector3 hintPos = transform.position + Vector3.up * hintUIHeight;
            Gizmos.DrawWireCube(hintPos, new Vector3(3, 1, 0));
        }
    }

    void OnDestroy()
    {
        if (keyHintUI != null)
        {
            Destroy(keyHintUI);
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

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}