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

    [Header("傷害設置")]
    public float swordDamage = 30f;
    public LayerMask enemyLayer;

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
    public float pivotOffsetRatio = 0.95f; // 0.95 表示更靠近劍尾

    private bool isSlashing = false;
    private float slashTimer = 0f;
    private float lastSlashTime = -999f;
    private SlashPhase currentPhase = SlashPhase.None;
    private Vector3 originalSwordLocalPosition;
    private SpriteRenderer swordSpriteRenderer;
    private Collider2D swordCollider;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
    private PlayerController2D playerController;
    private AudioSource audioSource;

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
        if (swordObject == null) return;

        // 設定旋轉軸心在劍柄(尾端) - 使用可調整的比例
        SpriteRenderer sr = swordObject.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // 計算劍柄位置 (使用 pivotOffsetRatio 來更精確地定位劍尾)
            Bounds bounds = sr.bounds;
            // pivotOffsetRatio = 0.95 表示軸心點在距離底部 95% 的位置（更靠近劍尾）
            Vector3 pivotOffset = new Vector3(0, -bounds.extents.y * pivotOffsetRatio, 0);

            // 創建一個父物件作為旋轉軸心
            GameObject pivotObject = new GameObject("SwordPivot");
            pivotObject.transform.SetParent(transform);
            pivotObject.transform.localPosition = swordObject.transform.localPosition;

            // 將劍設為 pivot 的子物件
            swordObject.transform.SetParent(pivotObject.transform);
            swordObject.transform.localPosition = pivotOffset;

            // 更新引用
            originalSwordLocalPosition = pivotObject.transform.localPosition;
            swordObject = pivotObject;
        }
        else
        {
            originalSwordLocalPosition = swordObject.transform.localPosition;
        }

        swordSpriteRenderer = swordObject.GetComponentInChildren<SpriteRenderer>();
        swordCollider = swordObject.GetComponentInChildren<Collider2D>();

        if (swordCollider == null)
        {
            GameObject swordChild = swordObject.transform.childCount > 0 ?
                swordObject.transform.GetChild(0).gameObject : swordObject;
            BoxCollider2D boxCollider = swordChild.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            swordCollider = boxCollider;
        }
        else
        {
            swordCollider.isTrigger = true;
        }
        swordCollider.enabled = false;

        GameObject colliderObject = swordCollider.gameObject;
        SwordCollisionHandler collisionHandler = colliderObject.GetComponent<SwordCollisionHandler>();
        if (collisionHandler == null)
        {
            collisionHandler = colliderObject.AddComponent<SwordCollisionHandler>();
        }
        collisionHandler.skill = this;
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
            // 角色向左,劍握成 \ (120度)
            newLocalPosition.x = -Mathf.Abs(originalSwordLocalPosition.x);
            swordObject.transform.localEulerAngles = new Vector3(0, 0, 120f);
        }
        else
        {
            // 角色向右,劍握成 / (60度)
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
        hitEnemies.Clear();

        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }

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

            if (swordCollider != null)
            {
                swordCollider.enabled = false;
            }

            Debug.Log("[SwordSlashSkill] 揮劍結束！");
        }
        else
        {
            // 使用相同的 easing 函數，但是逆向進行
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            swordObject.transform.localEulerAngles = GetReturnRotation(easedProgress);
        }
    }

    Vector3 GetSlashRotation(float progress)
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            // 角色向左: 從 120° 到 240° (順時針)
            float angle = Mathf.Lerp(120f, 240f, progress);
            return new Vector3(0, 0, angle);
        }
        else
        {
            // 角色向右: 從 60° (/) 順時針到 -60° (\)
            float angle = Mathf.Lerp(60f, -60f, progress);
            return new Vector3(0, 0, angle);
        }
    }

    Vector3 GetSlashEndRotation()
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            // 角色向左: 240°
            return new Vector3(0, 0, 240f);
        }
        else
        {
            // 角色向右: 300° (也就是 -60°)
            return new Vector3(0, 0, 300f);
        }
    }

    Vector3 GetReturnRotation(float progress)
    {
        if (playerController != null && playerController.LastHorizontalDirection == -1)
        {
            // 角色向左: 從 240° 逆向回到 120° (沿著原本軌跡返回)
            float angle = Mathf.Lerp(240f, 120f, progress);
            return new Vector3(0, 0, angle);
        }
        else
        {
            // 角色向右: 從 -60° (300°) 逆向回到 60° (沿著原本軌跡返回)
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

    public void OnSwordHitEnemy(GameObject enemy)
    {
        if (hitEnemies.Contains(enemy))
        {
            return;
        }

        hitEnemies.Add(enemy);

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(swordDamage, "PlayerSword");

            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }
}

public class SwordCollisionHandler : MonoBehaviour
{
    public SwordSlashSkill skill;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Enemy1"))
        {
            if (skill != null)
            {
                skill.OnSwordHitEnemy(other.gameObject);
            }
        }
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