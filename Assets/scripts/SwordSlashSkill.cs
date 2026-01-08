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
    public KeyCode slashKey = KeyCode.E;

    [Header("✨ 技能組合鍵")]
    [Tooltip("上挑組合鍵1 (預設W)")]
    public KeyCode upSlashKey = KeyCode.W;
    [Tooltip("衝刺斬組合鍵 (預設Shift)")]
    public KeyCode dashSlashKey = KeyCode.LeftShift;

    [Header("收劍系統")]
    [Tooltip("收劍組合鍵1")]
    public KeyCode sheatheKey1 = KeyCode.E;
    [Tooltip("收劍組合鍵2")]
    public KeyCode sheatheKey2 = KeyCode.F;
    [Tooltip("劍是否已收起")]
    public bool isSwordSheathed = false;
    [Tooltip("收劍狀態移動速度加成(0.1 = 10%加速)")]
    [Range(0f, 0.3f)]
    public float sheathedSpeedBonus = 0.1f;
    [Tooltip("拔劍音效")]
    public AudioClip unsheatheSound;
    [Tooltip("收劍音效")]
    public AudioClip sheatheSound;

    [Header("✨ 蓄力系統")]
    [Tooltip("是否啟用蓄力系統")]
    public bool enableChargeSystem = true;
    [Tooltip("完全蓄力所需時間(秒)")]
    public float fullChargeTime = 1.5f;
    [Tooltip("蓄力傷害倍率(1.0 = 無蓄力時傷害)")]
    public AnimationCurve chargeDamageMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 3f);
    [Tooltip("蓄力攻擊範圍倍率")]
    public AnimationCurve chargeRangeMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
    [Tooltip("蓄力特效顏色")]
    public Color chargeEffectColor = Color.yellow;
    [Tooltip("蓄力完成音效")]
    public AudioClip chargeReadySound;
    [Tooltip("蓄力釋放音效")]
    public AudioClip chargeReleaseSound;

    // 蓄力相關變數
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private float currentChargeProgress = 0f;
    private GameObject chargeEffect;

    [Header("✨ 上挑技能")]
    [Tooltip("上挑傷害倍率")]
    public float upSlashDamageMultiplier = 1.2f;
    [Tooltip("上挑攻擊範圍倍率")]
    public float upSlashRangeMultiplier = 1.3f;
    [Tooltip("上挑音效")]
    public AudioClip upSlashSound;

    [Header("✨ 衝刺斬技能")]
    [Tooltip("衝刺距離")]
    public float dashDistance = 3f;
    [Tooltip("衝刺速度")]
    public float dashSpeed = 20f;
    [Tooltip("衝刺斬傷害倍率")]
    public float dashSlashDamageMultiplier = 1.5f;
    [Tooltip("衝刺斬攻擊範圍倍率")]
    public float dashSlashRangeMultiplier = 1.0f;
    [Tooltip("衝刺斬冷卻時間")]
    public float dashSlashCooldown = 2f;
    [Tooltip("衝刺斬音效")]
    public AudioClip dashSlashSound;
    private float lastDashSlashTime = -999f;
    private bool isDashing = false;
    private Vector2 dashStartPos;
    private Vector2 dashTargetPos;

    [Header("傷害設置")]
    [Tooltip("對普通敵人的傷害")]
    public float normalEnemyDamage = 30f;
    [Tooltip("對Boss的傷害")]
    public float bossDamage = 50f;

    [Header("攻擊檢測")]
    [Tooltip("劍的攻擊範圍半徑")]
    public float slashDetectionRadius = 1f;
    [Tooltip("用於檢測的LayerMask")]
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

    [Header("Debug設定")]
    [Tooltip("顯示攻擊範圍")]
    public bool showAttackRadius = true;
    [Tooltip("顯示詳細調試信息")]
    public bool showDetailedDebug = true;

    private bool isSlashing = false;
    private float slashTimer = 0f;
    private float lastSlashTime = -999f;
    private SlashPhase currentPhase = SlashPhase.None;
    private SlashType currentSlashType = SlashType.Normal; // ✅ 新增:當前揮劍類型
    private Vector3 originalSwordLocalPosition;
    private SpriteRenderer swordSpriteRenderer;
    private PlayerController2D playerController;
    private AudioSource audioSource;
    private HashSet<GameObject> hitThisSlash = new HashSet<GameObject>();

    private enum SlashPhase { None, SlashingDown, Holding, Returning }
    private enum SlashType { Normal, UpSlash, DashSlash } // ✅ 新增:揮劍類型

    public void UnlockSkill()
    {
        isUnlocked = true;
        Debug.Log("[SwordSlashSkill] 揮劍技能已解鎖!");
    }

    public void LockSkill()
    {
        isUnlocked = false;
        Debug.Log("[SwordSlashSkill] 揮劍技能已鎖定!");
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    // ✅ 新增:供PlayerController調用的速度加成
    public float GetSpeedMultiplier()
    {
        if (isSwordSheathed)
        {
            return 1f + sheathedSpeedBonus; // 例如 1.1 = 110%速度
        }
        return 1f;
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
            Debug.LogError("[SwordSlashSkill] 劍物件未設定!");
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

        Debug.Log("[SwordSlashSkill] 劍已初始化(E蓄力/揮劍, E+F收劍)");
    }

    void Update()
    {
        if (!isUnlocked) return;
        if (playerController != null && !playerController.canControl) return;

        if (!isSlashing && !isSwordSheathed && !isCharging && !isDashing)
        {
            UpdateSwordPosition();
        }

        HandleSlashInput();

        if (isSlashing)
        {
            UpdateSlashAnimation();
        }

        // 蓄力系統更新
        if (isCharging)
        {
            UpdateCharging();
        }

        // ✅ 衝刺斬更新
        if (isDashing)
        {
            UpdateDashing();
        }
    }

    void UpdateSwordPosition()
    {
        if (swordObject == null || playerController == null) return;

        Vector3 newLocalPosition = originalSwordLocalPosition;

        if (playerController.LastHorizontalDirection == -1)
        {
            // ✅ 修正:左轉時X軸取負,並鏡像Y軸
            newLocalPosition.x = -Mathf.Abs(originalSwordLocalPosition.x);
            newLocalPosition.y = originalSwordLocalPosition.y; // Y軸保持一致
            swordObject.transform.localEulerAngles = new Vector3(0, 0, 120f);
        }
        else
        {
            // 右轉
            newLocalPosition.x = Mathf.Abs(originalSwordLocalPosition.x);
            newLocalPosition.y = originalSwordLocalPosition.y;
            swordObject.transform.localEulerAngles = new Vector3(0, 0, 60f);
        }

        swordObject.transform.localPosition = newLocalPosition;
    }

    // ✅ 修改後的輸入邏輯(支援上挑、衝刺斬)
    void HandleSlashInput()
    {
        // ✅ 檢測 E+F 組合鍵收劍
        if (Input.GetKey(sheatheKey1) && Input.GetKeyDown(sheatheKey2))
        {
            if (!isSwordSheathed)
            {
                SheatheSword();
            }
            return;
        }

        // ✅ 檢測 Shift+E 衝刺斬
        if (Input.GetKey(dashSlashKey) && Input.GetKeyDown(slashKey))
        {
            if (!isSwordSheathed && CanDashSlash())
            {
                StartDashSlash();
            }
            return;
        }

        // ✅ 檢測 W+E 上挑
        if (Input.GetKey(upSlashKey) && Input.GetKeyDown(slashKey))
        {
            if (!isSwordSheathed && CanSlash())
            {
                StartUpSlash();
            }
            return;
        }

        // 按下E鍵 (普通揮劍/蓄力)
        if (Input.GetKeyDown(slashKey))
        {
            if (isSwordSheathed)
            {
                UnsheatheSword();
            }
            else if (enableChargeSystem && CanSlash())
            {
                StartCharging();
            }
            else if (!enableChargeSystem && CanSlash())
            {
                StartSlash(1f, SlashType.Normal);
            }
        }

        // 放開E鍵 → 釋放蓄力攻擊
        if (Input.GetKeyUp(slashKey) && isCharging)
        {
            ReleaseChargedSlash();
        }
    }

    // ✅ 新增:開始蓄力
    void StartCharging()
    {
        if (isSlashing || isCharging) return;

        isCharging = true;
        chargeStartTime = Time.time;
        currentChargeProgress = 0f;

        // 創建蓄力特效
        CreateChargeEffect();

        Debug.Log("[SwordSlashSkill] 🔥 開始蓄力!");
    }

    // ✅ 新增:更新蓄力進度
    void UpdateCharging()
    {
        float chargeTime = Time.time - chargeStartTime;
        currentChargeProgress = Mathf.Clamp01(chargeTime / fullChargeTime);

        // 更新蓄力特效
        UpdateChargeEffect();

        // 蓄力完成音效(只播放一次)
        if (currentChargeProgress >= 1f && chargeReadySound != null && audioSource != null)
        {
            if (!audioSource.isPlaying || audioSource.clip != chargeReadySound)
            {
                audioSource.PlayOneShot(chargeReadySound);
            }
        }

        if (showDetailedDebug)
        {
            Debug.Log($"[蓄力] 進度: {currentChargeProgress:P0} ({chargeTime:F2}s)");
        }
    }

    // ✅ 新增:釋放蓄力攻擊
    void ReleaseChargedSlash()
    {
        if (!isCharging) return;

        float finalChargeProgress = currentChargeProgress;
        isCharging = false;

        DestroyChargeEffect();

        if (chargeReleaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(chargeReleaseSound);
        }

        StartSlash(finalChargeProgress, SlashType.Normal);

        Debug.Log($"[SwordSlashSkill] ⚡ 釋放蓄力攻擊! 蓄力程度: {finalChargeProgress:P0}");
    }

    // ✅ 新增:開始上挑
    void StartUpSlash()
    {
        StartSlash(0f, SlashType.UpSlash);
        Debug.Log("[SwordSlashSkill] ⬆️ 上挑!");
    }

    // ✅ 新增:開始衝刺斬
    void StartDashSlash()
    {
        isDashing = true;
        lastDashSlashTime = Time.time;

        // 計算衝刺目標位置
        int direction = playerController ? playerController.LastHorizontalDirection : 1;
        dashStartPos = transform.position;
        dashTargetPos = dashStartPos + new Vector2(direction * dashDistance, 0);

        // 開始揮劍動作
        StartSlash(0f, SlashType.DashSlash);

        Debug.Log($"[SwordSlashSkill] 💨 衝刺斬! 目標距離: {dashDistance}");
    }

    // ✅ 新增:更新衝刺斬移動
    void UpdateDashing()
    {
        if (playerController == null) return;

        // 衝刺移動
        Vector2 currentPos = transform.position;
        float dashProgress = (currentPos - dashStartPos).magnitude / dashDistance;

        if (dashProgress < 1f)
        {
            // 繼續衝刺
            Vector2 direction = (dashTargetPos - currentPos).normalized;
            playerController.GetComponent<Rigidbody2D>().velocity = direction * dashSpeed;
        }
        else
        {
            // 衝刺結束
            isDashing = false;
            playerController.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            Debug.Log("[SwordSlashSkill] 衝刺斬結束");
        }
    }

    // ✅ 新增:檢查是否可以衝刺斬
    bool CanDashSlash()
    {
        if (!isUnlocked) return false;
        if (isSlashing) return false;
        if (isCharging) return false;
        if (isDashing) return false;
        if (isSwordSheathed) return false;
        if (swordObject == null) return false;
        if (Time.time - lastDashSlashTime < dashSlashCooldown) return false;
        return true;
    }

    // ✅ 修改:揮劍方法加入類型參數
    void StartSlash(float chargeLevel = 0f, SlashType slashType = SlashType.Normal)
    {
        isSlashing = true;
        slashTimer = 0f;
        currentPhase = SlashPhase.SlashingDown;
        currentSlashType = slashType;
        lastSlashTime = Time.time;
        hitThisSlash.Clear();

        currentChargeProgress = chargeLevel;

        // 根據不同類型播放音效
        AudioClip soundToPlay = slashSound;
        if (slashType == SlashType.UpSlash && upSlashSound != null)
        {
            soundToPlay = upSlashSound;
        }
        else if (slashType == SlashType.DashSlash && dashSlashSound != null)
        {
            soundToPlay = dashSlashSound;
        }

        if (audioSource != null && soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }

        string typeName = slashType == SlashType.UpSlash ? "上挑" :
                         slashType == SlashType.DashSlash ? "衝刺斬" : "普通";
        Debug.Log($"[SwordSlashSkill] {typeName}揮劍開始! (蓄力: {chargeLevel:P0})");
    }

    void ToggleSwordSheathe()
    {
        if (isSlashing || isCharging) return;

        if (isSwordSheathed)
        {
            UnsheatheSword();
        }
        else
        {
            SheatheSword();
        }
    }

    void UnsheatheSword()
    {
        isSwordSheathed = false;
        swordObject.SetActive(true);

        if (audioSource != null && unsheatheSound != null)
        {
            audioSource.PlayOneShot(unsheatheSound);
        }

        Debug.Log("[SwordSlashSkill] ⚔️ 拔劍!");
    }

    void SheatheSword()
    {
        isSwordSheathed = true;
        swordObject.SetActive(false);

        if (audioSource != null && sheatheSound != null)
        {
            audioSource.PlayOneShot(sheatheSound);
        }

        Debug.Log($"[SwordSlashSkill] 🛡️ 收劍! (速度加成: {sheathedSpeedBonus:P0})");
    }

    bool CanSlash()
    {
        if (!isUnlocked) return false;
        if (isSlashing) return false;
        if (isCharging) return false;
        if (isDashing) return false;
        if (isSwordSheathed) return false;
        if (swordObject == null) return false;
        if (Time.time - lastSlashTime < cooldown) return false;
        return true;
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
            currentChargeProgress = 0f; // 重置蓄力
            UpdateSwordPosition();

            Debug.Log("[SwordSlashSkill] 揮劍結束!");
        }
        else
        {
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            swordObject.transform.localEulerAngles = GetReturnRotation(easedProgress);
        }
    }

    // ✅ 修改:根據技能類型調整傷害和範圍
    void DetectSlashHits()
    {
        if (swordObject == null) return;

        Vector3 swordPos = swordObject.transform.position;

        // ✅ 根據技能類型計算範圍和傷害倍率
        float rangeMult = 1f;
        float damageMult = 1f;

        switch (currentSlashType)
        {
            case SlashType.Normal:
                rangeMult = chargeRangeMultiplier.Evaluate(currentChargeProgress);
                damageMult = chargeDamageMultiplier.Evaluate(currentChargeProgress);
                break;
            case SlashType.UpSlash:
                rangeMult = upSlashRangeMultiplier;
                damageMult = upSlashDamageMultiplier;
                break;
            case SlashType.DashSlash:
                rangeMult = dashSlashRangeMultiplier;
                damageMult = dashSlashDamageMultiplier;
                break;
        }

        float currentRange = slashDetectionRadius * rangeMult;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(swordPos, currentRange, detectionLayerMask);

        if (showDetailedDebug && hitColliders.Length > 0)
            Debug.Log($"[SwordSlashSkill] 檢測到 {hitColliders.Length} 個碰撞體 (範圍: {currentRange:F2}, 類型: {currentSlashType})");

        foreach (Collider2D col in hitColliders)
        {
            if (hitThisSlash.Contains(col.gameObject))
                continue;

            if (col.CompareTag("Player"))
                continue;

            SlimePredatorEnemy slime = col.GetComponent<SlimePredatorEnemy>();
            if (slime != null)
            {
                float finalDamage = normalEnemyDamage * damageMult;
                hitThisSlash.Add(col.gameObject);
                slime.TakeDamage(finalDamage, "PlayerSword");
                Debug.Log($"[SwordSlashSkill] ✅ 劍擊中史萊姆! 傷害: {finalDamage:F0} (倍率x{damageMult:F1})");

                if (audioSource != null && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                continue;
            }

            BossController2D boss = col.GetComponent<BossController2D>();
            if (boss != null)
            {
                float finalDamage = bossDamage * damageMult;
                hitThisSlash.Add(col.gameObject);
                boss.TakeDamage(finalDamage, "PlayerSword");
                Debug.Log($"[SwordSlashSkill] ✅ 劍擊中Boss! 傷害: {finalDamage:F0} (倍率x{damageMult:F1})");

                if (audioSource != null && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                continue;
            }

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                float finalDamage = normalEnemyDamage * damageMult;
                hitThisSlash.Add(col.gameObject);
                enemy.TakeDamage(finalDamage, "PlayerSword");
                Debug.Log($"[SwordSlashSkill] ✅ 劍擊中敵人! 傷害: {finalDamage:F0} (倍率x{damageMult:F1})");

                if (audioSource != null && hitSound != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
                continue;
            }

            if (showDetailedDebug)
                Debug.Log($"[SwordSlashSkill] ⚠️ {col.gameObject.name} 沒有找到可識別的敵人組件");
        }
    }

    // ✅ 新增:創建蓄力特效(簡易版,使用圓形光暈)
    void CreateChargeEffect()
    {
        if (chargeEffect != null)
            Destroy(chargeEffect);

        chargeEffect = new GameObject("ChargeEffect");
        chargeEffect.transform.SetParent(swordObject.transform);
        chargeEffect.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = chargeEffect.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(32);
        sr.color = new Color(chargeEffectColor.r, chargeEffectColor.g, chargeEffectColor.b, 0.3f);
        sr.sortingOrder = 100;

        chargeEffect.transform.localScale = Vector3.zero;
    }

    // ✅ 新增:更新蓄力特效
    void UpdateChargeEffect()
    {
        if (chargeEffect == null) return;

        // 特效大小隨蓄力增長
        float scale = Mathf.Lerp(0.5f, 2f, currentChargeProgress);
        chargeEffect.transform.localScale = Vector3.one * scale;

        // 特效顏色變化
        SpriteRenderer sr = chargeEffect.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float alpha = Mathf.Lerp(0.3f, 0.8f, currentChargeProgress);
            Color color = Color.Lerp(Color.yellow, Color.red, currentChargeProgress);
            sr.color = new Color(color.r, color.g, color.b, alpha);
        }

        // 旋轉特效
        chargeEffect.transform.Rotate(0, 0, 360f * Time.deltaTime);
    }

    // ✅ 新增:移除蓄力特效
    void DestroyChargeEffect()
    {
        if (chargeEffect != null)
        {
            Destroy(chargeEffect);
            chargeEffect = null;
        }
    }

    // ✅ 新增:創建圓形Sprite(用於蓄力特效)
    Sprite CreateCircleSprite(int segments)
    {
        Texture2D texture = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64));
                float alpha = Mathf.Clamp01(1f - (distance / 64f));
                colors[y * 128 + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
    }

    Vector3 GetSlashRotation(float progress)
    {
        // ✅ 根據技能類型調整揮劍角度
        if (currentSlashType == SlashType.UpSlash)
        {
            // 上挑:從下往上砍
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                float angle = Mathf.Lerp(200f, 80f, progress); // 左側上挑
                return new Vector3(0, 0, angle);
            }
            else
            {
                float angle = Mathf.Lerp(-20f, 100f, progress); // 右側上挑
                return new Vector3(0, 0, angle);
            }
        }
        else if (currentSlashType == SlashType.DashSlash)
        {
            // 衝刺斬:水平突刺角度
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                return new Vector3(0, 0, 180f); // 左側水平
            }
            else
            {
                return new Vector3(0, 0, 0f); // 右側水平
            }
        }
        else
        {
            // 普通下砍
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
    }

    Vector3 GetSlashEndRotation()
    {
        // ✅ 根據技能類型調整結束角度
        if (currentSlashType == SlashType.UpSlash)
        {
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                return new Vector3(0, 0, 80f);
            }
            else
            {
                return new Vector3(0, 0, 100f);
            }
        }
        else if (currentSlashType == SlashType.DashSlash)
        {
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                return new Vector3(0, 0, 180f);
            }
            else
            {
                return new Vector3(0, 0, 0f);
            }
        }
        else
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
    }

    Vector3 GetReturnRotation(float progress)
    {
        // ✅ 根據技能類型調整返回角度
        if (currentSlashType == SlashType.UpSlash)
        {
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                float angle = Mathf.Lerp(80f, 120f, progress);
                return new Vector3(0, 0, angle);
            }
            else
            {
                float angle = Mathf.Lerp(100f, 60f, progress);
                return new Vector3(0, 0, angle);
            }
        }
        else if (currentSlashType == SlashType.DashSlash)
        {
            if (playerController != null && playerController.LastHorizontalDirection == -1)
            {
                float angle = Mathf.Lerp(180f, 120f, progress);
                return new Vector3(0, 0, angle);
            }
            else
            {
                float angle = Mathf.Lerp(0f, 60f, progress);
                return new Vector3(0, 0, angle);
            }
        }
        else
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

    void OnDrawGizmosSelected()
    {
        if (!showAttackRadius || swordObject == null) return;

        // 顯示基礎攻擊範圍
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(swordObject.transform.position, slashDetectionRadius);

        // ✅ 顯示蓄力最大攻擊範圍
        if (enableChargeSystem)
        {
            float maxRange = slashDetectionRadius * chargeRangeMultiplier.Evaluate(1f);
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(swordObject.transform.position, maxRange);
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