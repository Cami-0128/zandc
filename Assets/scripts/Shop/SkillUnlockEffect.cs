using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能購買效果 - 增加技能使用次數
/// </summary>
public class SkillUnlockEffect : IShopItemEffect
{
    private int addCount;

    public SkillUnlockEffect(int count = 1)
    {
        addCount = count;
    }

    public void ApplyEffect(PlayerController2D player)
    {
        CaptureSkillManager skillManager = player.GetComponent<CaptureSkillManager>();
        if (skillManager == null)
        {
            skillManager = player.gameObject.AddComponent<CaptureSkillManager>();
        }

        skillManager.AddSkillCount(addCount);
        Debug.Log($"[技能系統] 購買捕捉技能 x{addCount}，當前剩餘次數：{skillManager.GetSkillCount()}");
    }
}

/// <summary>
/// 捕捉技能管理器 - 掛載在玩家身上（次數制版本）
/// </summary>
public class CaptureSkillManager : MonoBehaviour
{
    [Header("=== 技能次數系統 ===")]
    [Tooltip("當前可使用次數")]
    public int skillCount = 0;

    [Header("=== 射線參數 ===")]
    [Tooltip("射線飛行速度")]
    public float projectileSpeed = 12f;

    [Tooltip("射線最大飛行距離")]
    public float maxDistance = 20f;

    [Header("=== 捕捉圓球參數 ===")]
    [Tooltip("圓球上升速度")]
    public float bubbleRiseSpeed = 3f;

    [Tooltip("圓球上升高度（超過此高度後消失）")]
    public float bubbleRiseHeight = 8f;

    [Tooltip("敵人被捕捉後，延遲多久開始上升")]
    public float captureDelay = 0.3f;

    [Header("=== 預製體設置 ===")]
    [Tooltip("射線預製體（可不設置，系統會自動生成）")]
    public GameObject projectilePrefab;

    [Tooltip("圓球預製體（可不設置，系統會自動生成）")]
    public GameObject bubblePrefab;

    [Header("=== UI 設置 ===")]
    [Tooltip("技能說明面板（必須設置）")]
    public GameObject skillTutorialPanel;

    [Tooltip("關閉按鈕（必須設置）")]
    public UnityEngine.UI.Button closeButton;

    [Tooltip("顯示剩餘次數的文字UI（必須設置）")]
    public TMPro.TextMeshProUGUI skillCountText;

    [Tooltip("面板自動消失時間（秒）")]
    public float panelAutoHideTime = 5f;

    [Header("=== 視覺設置 ===")]
    [Tooltip("射線顏色")]
    public Color projectileColor = Color.cyan;

    [Tooltip("射線寬度")]
    public float projectileWidth = 0.1f;

    [Tooltip("圓球顏色")]
    public Color bubbleColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Tooltip("圓球大小")]
    public float bubbleSize = 2f;

    private PlayerController2D player;
    private Coroutine hideCoroutine;

    void Start()
    {
        player = GetComponent<PlayerController2D>();

        // 設置關閉按鈕
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("[技能系統] 未設置關閉按鈕！");
        }

        // 初始隱藏面板
        if (skillTutorialPanel != null)
        {
            skillTutorialPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[技能系統] 未設置技能說明面板！");
        }

        // 更新次數顯示
        UpdateSkillCountUI();
    }

    void Update()
    {
        // 只有有次數且玩家存活時才能使用
        if (skillCount <= 0 || player == null || player.isDead || !player.canControl)
            return;

        // 按下B鍵使用技能
        if (Input.GetKeyDown(KeyCode.B))
        {
            UseSkill();
        }
    }

    /// <summary>
    /// 增加技能次數（購買時調用）
    /// </summary>
    public void AddSkillCount(int count)
    {
        skillCount += count;
        UpdateSkillCountUI();
        ShowPanel();
        Debug.Log($"[技能系統] 技能次數 +{count}，當前剩餘：{skillCount}");
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    void UseSkill()
    {
        if (skillCount <= 0)
        {
            Debug.Log("[技能系統] 技能次數不足！");
            return;
        }

        // 扣除次數
        skillCount--;
        UpdateSkillCountUI();

        // 發射射線
        FireProjectile();

        Debug.Log($"[技能系統] 技能已使用！剩餘次數：{skillCount}");
    }

    /// <summary>
    /// 發射捕捉射線
    /// </summary>
    void FireProjectile()
    {
        GameObject projectile;

        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // 創建預設射線
            projectile = CreateDefaultProjectile();
        }

        // 設置射線組件
        CaptureProjectile projScript = projectile.GetComponent<CaptureProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<CaptureProjectile>();
        }

        // 傳遞參數
        projScript.Initialize(
            player.LastHorizontalDirection,
            projectileSpeed,
            maxDistance,
            bubblePrefab,
            bubbleRiseSpeed,
            bubbleRiseHeight,
            captureDelay,
            bubbleColor,
            bubbleSize
        );
    }

    /// <summary>
    /// 創建預設射線物件
    /// </summary>
    GameObject CreateDefaultProjectile()
    {
        GameObject proj = new GameObject("CaptureProjectile");
        proj.transform.position = transform.position;

        // 添加視覺效果（線條）
        LineRenderer lr = proj.AddComponent<LineRenderer>();
        lr.startWidth = projectileWidth;
        lr.endWidth = projectileWidth;
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = projectileColor;
        lr.endColor = projectileColor;
        lr.sortingOrder = 5;

        // 添加碰撞體
        BoxCollider2D col = proj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.2f);

        // 添加 Rigidbody2D
        Rigidbody2D rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        return proj;
    }

    /// <summary>
    /// 顯示技能面板
    /// </summary>
    void ShowPanel()
    {
        if (skillTutorialPanel == null) return;

        skillTutorialPanel.SetActive(true);

        // 取消之前的自動隱藏協程
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 啟動新的自動隱藏協程
        hideCoroutine = StartCoroutine(AutoHidePanel());
    }

    /// <summary>
    /// 隱藏技能面板
    /// </summary>
    public void HidePanel()
    {
        if (skillTutorialPanel == null) return;

        skillTutorialPanel.SetActive(false);

        // 取消自動隱藏協程
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    /// <summary>
    /// 自動隱藏面板協程
    /// </summary>
    IEnumerator AutoHidePanel()
    {
        yield return new WaitForSeconds(panelAutoHideTime);
        HidePanel();
    }

    /// <summary>
    /// 更新次數顯示UI
    /// </summary>
    void UpdateSkillCountUI()
    {
        if (skillCountText != null)
        {
            skillCountText.text = $"剩餘次數：{skillCount}";
        }
    }

    /// <summary>
    /// 獲取當前技能次數
    /// </summary>
    public int GetSkillCount()
    {
        return skillCount;
    }

    /// <summary>
    /// 手動設置技能次數（測試用）
    /// </summary>
    public void SetSkillCount(int count)
    {
        skillCount = Mathf.Max(0, count);
        UpdateSkillCountUI();
    }
}