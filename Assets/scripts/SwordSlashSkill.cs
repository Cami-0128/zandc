using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlashSkill : MonoBehaviour
{
    [Header("技能啟用設定")]
    public bool isUnlocked = true;

    [Header("劍物件設定")]
    public GameObject swordObject;

    [Header("揮劍動作設定")]
    public float slashDownDuration = 0.15f;
    public float holdDuration = 0.2f;
    public float returnDuration = 0.1f;

    [Header("揮劍冷卻設定")]
    public float cooldown = 0.5f;
    public KeyCode slashKey = KeyCode.W;

    [Header("【新增】傷害設置（可在Inspector調整）")]
    [Tooltip("對普通敵人的傷害")]
    public float normalEnemyDamage = 30f;
    [Tooltip("對Boss的傷害")]
    public float bossDamage = 50f;

    [Header("【新增】攻擊檢測（使用物理OverlapCircle）")]
    [Tooltip("劍的攻擊範圍半徑")]
    public float slashDetectionRadius = 1f;
    [Tooltip("用於檢測的LayerMask（建議選擇Enemy和Boss層）")]
    public LayerMask detectionLayerMask = -1;

    [Header("殘影設定")]
    public bool enableTrail = true;
    public int trailCount = 5;
    public Color trailColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
    public float trailFadeDuration = 0.3f;

    [Header("音效設定")]
    public AudioClip slashSound;
    public AudioClip hitSound;

    [Header("軸心調整")]
    [Range(0f, 1f)]
    public float pivotOffsetRatio = 0.95f;

    private bool isSlashing = false;
    private float slashTimer = 0f;
    private float lastSlashTime = -999f;
    private SlashPhase currentPhase = SlashPhase.None;
    private Vector3 originalSwordLocalPosition;
    private SpriteRenderer swordSpriteRenderer;
    private PlayerController2D playerController;
    private AudioSource audioSource;

    // 【新增】本次揮劍已擊中的敵人集合
    private HashSet<GameObject> hitThisSlash = new HashSet<GameObject>();

    private enum SlashPhase { None, SlashingDown, Holding, Returning }

    public void UnlockSkill()
    {
        isUnlocked = true;
        Debug.Log("[SwordSlashSkill] 揮劍技能已解鎖！");
    }

    public void LockSkill()
    {
        isUnlocked = false;
        Debug.Log("[SwordSlashSkill] 揮劍技能已鎖定！");
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    void Start()
    {
        playerController = GetComponent<PlayerController2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeSword();
    }

    void InitializeSword()
    {
        if (swordObject == null)
        {
            Debug.LogError("[SwordSlashSkill] 劍物件未設定！");
            return;
        }

        SpriteRenderer sr = swordObject.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Bounds bounds = sr.bounds;
            Vector3 pivotOffset = new Vector3(0, -bounds.extents.y * pivotOffsetRatio, 0);

            GameObject pivotObject = new GameObject("SwordPivot");
            pivotObject.transform.SetParent(transform);
            pivotObject.transform.localPosition = swordObject.transform.localPosition;

            swordObject.transform.SetParent(pivotObject.transform);
            swordObject.transform.localPosition = pivotOffset;

            originalSwordLocalPosition = pivotObject.transform.localPosition;
            swordObject = pivotObject;
        }
        else
        {
            originalSwordLocalPosition = swordObject.transform.localPosition;
        }

        swordSpriteRenderer = swordObject.GetComponentInChildren<SpriteRenderer>();

        Debug.Log("[SwordSlashSkill] 劍已初始化（使用OverlapCircle檢測）");
    }

    void Update()
    {
        if (!isUnlocked) return;
        if (playerController != null && !playerController.canControl) return;

        if (!isSlashing)
        {
            UpdateSwordPosition();
        }

        HandleSlashInput();

        if (isSlashing)
        {
            UpdateSlashAnimation();
        }
    }

    void UpdateSwordPosition()
    {
        if (swordObject == null || playerController == null) return;

        Vector3 newLocalPosition = originalSwordLocalPosition;

        if (playerController.LastHorizontalDirection == -1)
        {
            newLocalPosition.x = -Mathf.Abs(originalSwordLocalPosition.x);
            swordObject.transform.localEulerAngles = new Vector3(0, 0, 120f);
        }
        else
        {
            newLocalPosition.x = Mathf.Abs(originalSwordLocalPosition.x);
            swordObject.transform.localEulerAngles = new Vector3(0, 0, 60f);
        }

        swordObject.transform.localPosition = newLocalPosition;
    }

    void HandleSlashInput()
    {
        if (Input.GetKeyDown(slashKey) && CanSlash())
        {
            StartSlash();
        }
    }

    bool CanSlash()
    {
        if (!isUnlocked) return false;
        if (isSlashing) return false;
        if (swordObject == null) return false;
        if (Time.time - lastSlashTime < cooldown) return false;
        return true;
    }

    void StartSlash()
    {
        isSlashing = true;
        slashTimer = 0f;
        currentPhase = SlashPhase.SlashingDown;
        lastSlashTime = Time.time;
        hitThisSlash.Clear();

        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound);
        }

        Debug.Log("[SwordSlashSkill] 揮劍開始！");
    }

    void UpdateSlashAnimation()
    {
        slashTimer += Time.deltaTime;

        switch (currentPhase)
        {
            case SlashPhase.SlashingDown:
                UpdateSlashDown();
                break;
            case SlashPhase.Holding:
                UpdateHold();
                break;
            case SlashPhase.Returning:
                UpdateReturn();
                break;
        }
    }

    void UpdateSlashDown()
    {
        float progress = slashTimer / slashDownDuration;

        if (progress >= 1f)
        {
            currentPhase = SlashPhase.Holding;
            slashTimer = 0f;
            swordObject.transform.localEulerAngles = GetSlashEndRotation();
        }
        else
        {
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            swordObject.transform.localEulerAngles = GetSlashRotation(easedProgress);

            // 【新增】在揮劍下降時進行碰撞檢測
            DetectSlashHits();

            if (enableTrail && progress % (1f / trailCount) < Time.deltaTime / slashDownDuration)
            {
                CreateTrail();
            }
        }
    }

    void UpdateHold()
    {
        if (slashTimer >= holdDuration)
        {
            currentPhase = SlashPhase.Returning;
            slashTimer = 0f;
        }
    }

    void UpdateReturn()
    {
        float progress = slashTimer / returnDuration;

        if (progress >= 1f)
        {
            isSlashing = false;
            currentPhase = SlashPhase.None;
            slashTimer = 0f;
            UpdateSwordPosition();

            Debug.Log("[SwordSlashSkill] 揮劍結束！");
        }
        else
        {
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            swordObject.transform.localEulerAngles = GetReturnRotation(easedProgress);
        }
    }

    // 【核心方法】使用OverlapCircle檢測攻擊範圍內的敵人
    void DetectSlashHits()
    {
        if (swordObject == null) return;

        Vector3 swordPos = swordObject.transform.position;

        // 使用OverlapCircle檢測範圍內的所有碰撞體
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(swordPos, slashDetectionRadius, detectionLayerMask);

        foreach (Collider2D col in hitColliders)
        {
            // 跳過已經擊中過的敵人
            if (hitThisSlash.Contains(col.gameObject))
                continue;

            // 跳過玩家自己
            if (col.CompareTag("Player"))
                continue;

            // 檢測Boss
            BossController2D boss = col.GetComponent<BossController2D>();
            if (boss != null)
            {
                hitThisSlash.Add(col.gameObject);
                boss.TakeDamage(bossDamage, "PlayerSword");
                Debug.Log($"[SwordSlashSkill] ✅ 劍擊中Boss！造成 {bossDamage} 點傷害");

                if (audioSource != null && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                continue;
            }

            // 檢測普通敵人
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                hitThisSlash.Add(col.gameObject);
                enemy.TakeDamage(normalEnemyDamage, "PlayerSword");
                Debug.Log($"[SwordSlashSkill] ✅ 劍擊中敵人！造成 {normalEnemyDamage} 點傷害");

                if (audioSource != null && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }

    Vector3 GetSlashRotation(float progress)
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            float angle = Mathf.Lerp(120f, 240f, progress);
            return new Vector3(0, 0, angle);
        }
        else
        {
            float angle = Mathf.Lerp(60f, -60f, progress);
            return new Vector3(0, 0, angle);
        }
    }

    Vector3 GetSlashEndRotation()
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            return new Vector3(0, 0, 240f);
        }
        else
        {
            return new Vector3(0, 0, 300f);
        }
    }

    Vector3 GetReturnRotation(float progress)
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            float angle = Mathf.Lerp(240f, 120f, progress);
            return new Vector3(0, 0, angle);
        }
        else
        {
            float angle = Mathf.Lerp(-60f, 60f, progress);
            return new Vector3(0, 0, angle);
        }
    }

    void CreateTrail()
    {
        if (swordObject == null) return;

        Transform swordTransform = swordObject.transform.childCount > 0 ?
            swordObject.transform.GetChild(0) : swordObject.transform;

        SpriteRenderer sr = swordTransform.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        GameObject trail = new GameObject("SwordTrail");
        trail.transform.position = swordTransform.position;
        trail.transform.rotation = swordTransform.rotation;
        trail.transform.localScale = swordTransform.lossyScale;

        SpriteRenderer trailRenderer = trail.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = sr.sprite;
        trailRenderer.color = trailColor;
        trailRenderer.sortingLayerName = sr.sortingLayerName;
        trailRenderer.sortingOrder = sr.sortingOrder - 1;

        SwordTrailFade fade = trail.AddComponent<SwordTrailFade>();
        fade.fadeDuration = trailFadeDuration;
    }
}

public class SwordTrailFade : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            startColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (spriteRenderer == null)
            return;

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);

        Color newColor = startColor;
        newColor.a = alpha;
        spriteRenderer.color = newColor;

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}