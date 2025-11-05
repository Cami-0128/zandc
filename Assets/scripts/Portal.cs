using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("傳送設定")]
    [Tooltip("目標傳送點位置")]
    public Transform destinationPortal;

    [Tooltip("傳送冷卻時間（避免重複傳送）")]
    public float teleportCooldown = 1f;

    [Tooltip("傳送後的無敵時間")]
    public float invincibilityTime = 0.5f;

    private bool canTeleport = true;

    [Header("視覺效果")]
    [Tooltip("傳送門顏色")]
    public Color portalColor = new Color(0.3f, 0.6f, 1f, 0.8f);

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 50f;

    [Tooltip("脈衝速度")]
    public float pulseSpeed = 2f;

    [Tooltip("脈衝幅度")]
    public float pulseAmount = 0.2f;

    [Tooltip("粒子效果預製體（可選）")]
    public GameObject particleEffectPrefab;

    [Header("音效")]
    [Tooltip("傳送音效")]
    public AudioClip teleportSound;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private ParticleSystem portalParticles;

    void Start()
    {
        // 初始化組件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = portalColor;
            originalScale = transform.localScale;
        }

        // 音效設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 創建粒子效果
        if (particleEffectPrefab != null)
        {
            GameObject particles = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity, transform);
            portalParticles = particles.GetComponent<ParticleSystem>();
        }
        else
        {
            // 如果沒有預製體，創建簡單的粒子系統
            CreateDefaultParticles();
        }

        // 檢查目標傳送點
        if (destinationPortal == null)
        {
            Debug.LogWarning($"[Portal] {gameObject.name} 沒有設定目標傳送點！");
        }
    }

    void Update()
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 檢查是否為玩家
        if (collision.CompareTag("Player") && canTeleport)
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null && destinationPortal != null)
            {
                StartCoroutine(TeleportPlayer(player));
            }
        }
    }

    IEnumerator TeleportPlayer(PlayerController2D player)
    {
        canTeleport = false;

        // 播放音效
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }

        // 傳送前效果
        yield return StartCoroutine(TeleportEffect(player, true));

        // 執行傳送
        player.transform.position = destinationPortal.position;
        Debug.Log($"[Portal] 玩家已傳送到 {destinationPortal.position}");

        // 傳送後效果
        yield return StartCoroutine(TeleportEffect(player, false));

        // 設定無敵時間（避免傳送後立即受傷）
        yield return new WaitForSeconds(invincibilityTime);

        // 冷卻時間
        yield return new WaitForSeconds(teleportCooldown);

        canTeleport = true;
    }

    IEnumerator TeleportEffect(PlayerController2D player, bool isBefore)
    {
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            // 淡出/淡入效果
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

            // 確保最終透明度正確
            playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, isBefore ? 0f : 1f);
        }

        yield return new WaitForSeconds(0.1f);
    }

    void CreateDefaultParticles()
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

        var colorOverLifetime = portalParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(portalColor, 0.0f), new GradientColorKey(portalColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
    }

    // 在編輯器中顯示連接線
    void OnDrawGizmos()
    {
        if (destinationPortal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destinationPortal.position);
            Gizmos.DrawWireSphere(destinationPortal.position, 0.5f);
        }
    }

    // 公開方法：設定目標傳送點
    public void SetDestination(Transform destination)
    {
        destinationPortal = destination;
    }

    // 公開方法：啟用/停用傳送門
    public void SetActive(bool active)
    {
        canTeleport = active;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
        if (portalParticles != null)
        {
            if (active)
                portalParticles.Play();
            else
                portalParticles.Stop();
        }
    }
}