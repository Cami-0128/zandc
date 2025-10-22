using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptureSkillManager : MonoBehaviour
{
    [Header("=== 技能次數系統 ===")]
    public int skillCount = 0;

    [Header("=== 射線參數 ===")]
    public float projectileSpeed = 10f;  // 降低速度，更容易觀察
    public float maxDistance = 20f;

    [Header("=== 捕捉圓球參數 ===")]
    public float bubbleRiseSpeed = 3f;
    public float bubbleRiseHeight = 8f;
    public float captureDelay = 0.3f;

    [Header("=== 預製體設置 ===")]
    public GameObject projectilePrefab;
    public GameObject bubblePrefab;

    [Header("=== UI 設置 ===")]
    public GameObject skillTutorialPanel;
    public Button closeButton;
    public TextMeshProUGUI skillCountText;
    public float panelAutoHideTime = 5f;

    [Header("=== 視覺設置 ===")]
    public Color projectileColor = Color.cyan;
    public float projectileWidth = 0.15f;
    public Color bubbleColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    public float bubbleSize = 2.5f;

    [Header("=== 碰撞設置（重要！）===")]
    [Tooltip("射線碰撞體寬度")]
    public float colliderWidth = 1f;
    [Tooltip("射線碰撞體高度")]
    public float colliderHeight = 1f;

    [Header("=== 面板控制設定 ===")]
    [Tooltip("當SkillTutorialPanel顯示時，自動隱藏Shop與Selected面板")]
    public bool autoHideOtherPanels = true;

    public ShopManager shopManager;  // 請在Inspector中指派此物件

    private PlayerController2D player;
    private Coroutine hideCoroutine;

    void Start()
    {
        player = GetComponent<PlayerController2D>();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }

        if (skillTutorialPanel != null)
        {
            skillTutorialPanel.SetActive(false);
        }

        UpdateSkillCountUI();
        Debug.Log("[技能系統] CaptureSkillManager 初始化完成");
    }

    void Update()
    {
        if (skillCount <= 0 || player == null || player.isDead || !player.canControl)
            return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            UseSkill();
        }
    }

    public void AddSkillCount(int count)
    {
        skillCount += count;
        UpdateSkillCountUI();
        ShowPanel();
        Debug.Log($"[技能系統] 技能次數 +{count}，當前剩餘：{skillCount}");
    }

    void UseSkill()
    {
        if (skillCount <= 0) return;

        skillCount--;
        UpdateSkillCountUI();
        FireProjectile();

        Debug.Log($"[技能系統] 技能已使用！剩餘次數：{skillCount}");
    }

    void FireProjectile()
    {
        GameObject projectile;

        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            projectile = CreateDefaultProjectile();
        }

        CaptureProjectile projScript = projectile.GetComponent<CaptureProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<CaptureProjectile>();
        }

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

    GameObject CreateDefaultProjectile()
    {
        GameObject proj = new GameObject("CaptureProjectile");
        proj.transform.position = transform.position;
        proj.layer = gameObject.layer; // 使用與 Player 相同的 Layer

        BoxCollider2D col = proj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(colliderWidth, colliderHeight);

        Debug.Log($"[技能系統] 射線碰撞體大小：{col.size}");

        Rigidbody2D rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        LineRenderer lr = proj.AddComponent<LineRenderer>();
        lr.startWidth = projectileWidth;
        lr.endWidth = projectileWidth;
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = projectileColor;
        lr.endColor = projectileColor;
        lr.sortingOrder = 5;

        Debug.Log($"[技能系統] 已創建預設射線，Layer：{LayerMask.LayerToName(proj.layer)}");
        return proj;
    }

    void ShowPanel()
    {
        if (skillTutorialPanel == null) return;
        skillTutorialPanel.SetActive(true);

        if (autoHideOtherPanels && shopManager != null)
        {
            if (shopManager.shopPanel != null) shopManager.shopPanel.SetActive(false);
            if (shopManager.detailPanel != null) shopManager.detailPanel.SetActive(false);
        }

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(AutoHidePanel());
    }

    public void HidePanel()
    {
        if (skillTutorialPanel == null) return;
        skillTutorialPanel.SetActive(false);
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    IEnumerator AutoHidePanel()
    {
        yield return new WaitForSeconds(panelAutoHideTime);
        HidePanel();
    }

    void UpdateSkillCountUI()
    {
        if (skillCountText != null)
        {
            skillCountText.text = $"剩餘次數：{skillCount}";
        }
    }

    public int GetSkillCount() => skillCount;

    public void SetSkillCount(int count)
    {
        skillCount = Mathf.Max(0, count);
        UpdateSkillCountUI();
    }
}
