using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ޯ��ʶR�ĪG - �W�[�ޯ�ϥΦ���
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
        Debug.Log($"[�ޯ�t��] �ʶR�����ޯ� x{addCount}�A��e�Ѿl���ơG{skillManager.GetSkillCount()}");
    }
}

/// <summary>
/// �����ޯ�޲z�� - �����b���a���W�]���ƨ���^
/// </summary>
public class CaptureSkillManager : MonoBehaviour
{
    [Header("=== �ޯস�ƨt�� ===")]
    [Tooltip("��e�i�ϥΦ���")]
    public int skillCount = 0;

    [Header("=== �g�u�Ѽ� ===")]
    [Tooltip("�g�u����t��")]
    public float projectileSpeed = 12f;

    [Tooltip("�g�u�̤j����Z��")]
    public float maxDistance = 20f;

    [Header("=== ������y�Ѽ� ===")]
    [Tooltip("��y�W�ɳt��")]
    public float bubbleRiseSpeed = 3f;

    [Tooltip("��y�W�ɰ��ס]�W�L�����׫�����^")]
    public float bubbleRiseHeight = 8f;

    [Tooltip("�ĤH�Q������A����h�[�}�l�W��")]
    public float captureDelay = 0.3f;

    [Header("=== �w�s��]�m ===")]
    [Tooltip("�g�u�w�s��]�i���]�m�A�t�η|�۰ʥͦ��^")]
    public GameObject projectilePrefab;

    [Tooltip("��y�w�s��]�i���]�m�A�t�η|�۰ʥͦ��^")]
    public GameObject bubblePrefab;

    [Header("=== UI �]�m ===")]
    [Tooltip("�ޯ໡�����O�]�����]�m�^")]
    public GameObject skillTutorialPanel;

    [Tooltip("�������s�]�����]�m�^")]
    public UnityEngine.UI.Button closeButton;

    [Tooltip("��ܳѾl���ƪ���rUI�]�����]�m�^")]
    public TMPro.TextMeshProUGUI skillCountText;

    [Tooltip("���O�۰ʮ����ɶ��]��^")]
    public float panelAutoHideTime = 5f;

    [Header("=== ��ı�]�m ===")]
    [Tooltip("�g�u�C��")]
    public Color projectileColor = Color.cyan;

    [Tooltip("�g�u�e��")]
    public float projectileWidth = 0.1f;

    [Tooltip("��y�C��")]
    public Color bubbleColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Tooltip("��y�j�p")]
    public float bubbleSize = 2f;

    private PlayerController2D player;
    private Coroutine hideCoroutine;

    void Start()
    {
        player = GetComponent<PlayerController2D>();

        // �]�m�������s
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("[�ޯ�t��] ���]�m�������s�I");
        }

        // ��l���í��O
        if (skillTutorialPanel != null)
        {
            skillTutorialPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[�ޯ�t��] ���]�m�ޯ໡�����O�I");
        }

        // ��s�������
        UpdateSkillCountUI();
    }

    void Update()
    {
        // �u�������ƥB���a�s���ɤ~��ϥ�
        if (skillCount <= 0 || player == null || player.isDead || !player.canControl)
            return;

        // ���UB��ϥΧޯ�
        if (Input.GetKeyDown(KeyCode.B))
        {
            UseSkill();
        }
    }

    /// <summary>
    /// �W�[�ޯস�ơ]�ʶR�ɽեΡ^
    /// </summary>
    public void AddSkillCount(int count)
    {
        skillCount += count;
        UpdateSkillCountUI();
        ShowPanel();
        Debug.Log($"[�ޯ�t��] �ޯস�� +{count}�A��e�Ѿl�G{skillCount}");
    }

    /// <summary>
    /// �ϥΧޯ�
    /// </summary>
    void UseSkill()
    {
        if (skillCount <= 0)
        {
            Debug.Log("[�ޯ�t��] �ޯস�Ƥ����I");
            return;
        }

        // ��������
        skillCount--;
        UpdateSkillCountUI();

        // �o�g�g�u
        FireProjectile();

        Debug.Log($"[�ޯ�t��] �ޯ�w�ϥΡI�Ѿl���ơG{skillCount}");
    }

    /// <summary>
    /// �o�g�����g�u
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
            // �Ыعw�]�g�u
            projectile = CreateDefaultProjectile();
        }

        // �]�m�g�u�ե�
        CaptureProjectile projScript = projectile.GetComponent<CaptureProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<CaptureProjectile>();
        }

        // �ǻ��Ѽ�
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
    /// �Ыعw�]�g�u����
    /// </summary>
    GameObject CreateDefaultProjectile()
    {
        GameObject proj = new GameObject("CaptureProjectile");
        proj.transform.position = transform.position;

        // �K�[��ı�ĪG�]�u���^
        LineRenderer lr = proj.AddComponent<LineRenderer>();
        lr.startWidth = projectileWidth;
        lr.endWidth = projectileWidth;
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = projectileColor;
        lr.endColor = projectileColor;
        lr.sortingOrder = 5;

        // �K�[�I����
        BoxCollider2D col = proj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.2f);

        // �K�[ Rigidbody2D
        Rigidbody2D rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        return proj;
    }

    /// <summary>
    /// ��ܧޯୱ�O
    /// </summary>
    void ShowPanel()
    {
        if (skillTutorialPanel == null) return;

        skillTutorialPanel.SetActive(true);

        // �������e���۰����è�{
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // �Ұʷs���۰����è�{
        hideCoroutine = StartCoroutine(AutoHidePanel());
    }

    /// <summary>
    /// ���çޯୱ�O
    /// </summary>
    public void HidePanel()
    {
        if (skillTutorialPanel == null) return;

        skillTutorialPanel.SetActive(false);

        // �����۰����è�{
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    /// <summary>
    /// �۰����í��O��{
    /// </summary>
    IEnumerator AutoHidePanel()
    {
        yield return new WaitForSeconds(panelAutoHideTime);
        HidePanel();
    }

    /// <summary>
    /// ��s�������UI
    /// </summary>
    void UpdateSkillCountUI()
    {
        if (skillCountText != null)
        {
            skillCountText.text = $"�Ѿl���ơG{skillCount}";
        }
    }

    /// <summary>
    /// �����e�ޯস��
    /// </summary>
    public int GetSkillCount()
    {
        return skillCount;
    }

    /// <summary>
    /// ��ʳ]�m�ޯস�ơ]���եΡ^
    /// </summary>
    public void SetSkillCount(int count)
    {
        skillCount = Mathf.Max(0, count);
        UpdateSkillCountUI();
    }
}