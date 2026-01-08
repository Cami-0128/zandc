using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManger : MonoBehaviour
{
    [Header("UI設定")]
    public GameObject startPanel;

    [Header("遊戲物件引用")]
    public PlayerController2D player;
    public BallRoller ball;

    [Header("Boss設定")]
    [Tooltip("拖入Boss物件（需要有BossController腳本）")]
    public BossController boss;

    [Header("Debug設定")]
    public bool debugMode = true;

    void Start()
    {
        if (debugMode)
        {
            Debug.Log("=== [StartManager] 初始化開始 ===");
        }

        // 驗證所有引用
        ValidateReferences();

        // 開始時顯示 UI 並暫停遊戲
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            if (debugMode) Debug.Log("[StartManager] 開始面板已顯示");
        }

        Time.timeScale = 0f;

        // 禁用玩家控制
        if (player != null)
        {
            player.canControl = false;
            if (debugMode) Debug.Log("[StartManager] 玩家控制已禁用");
        }

        // 禁用球滾動
        if (ball != null)
        {
            ball.canRoll = false;
            if (debugMode) Debug.Log("[StartManager] 球滾動已禁用");
        }

        // 禁用Boss移動
        if (boss != null)
        {
            boss.canMove = false;
            if (debugMode) Debug.Log($"[StartManager] Boss移動已禁用 - canMove: {boss.canMove}");
        }
        else
        {
            Debug.LogWarning("[StartManager] Boss引用為null！請在Inspector中設定Boss");
        }

        if (debugMode)
        {
            Debug.Log("=== [StartManager] 初始化完成 ===");
        }
    }

    /// <summary>
    /// 驗證所有必要的引用
    /// </summary>
    void ValidateReferences()
    {
        if (startPanel == null)
        {
            Debug.LogError("[StartManager] startPanel 未設定！");
        }

        if (player == null)
        {
            Debug.LogError("[StartManager] player 未設定！");
        }

        if (ball == null)
        {
            Debug.LogWarning("[StartManager] ball 未設定（如果場景沒有球可以忽略）");
        }

        if (boss == null)
        {
            Debug.LogError("[StartManager] boss 未設定！Boss將無法啟動");
        }
    }

    /// <summary>
    /// 開始按鈕點擊事件
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("=== [StartManager] 開始按鈕被點擊 ===");

        if (startPanel != null)
        {
            startPanel.SetActive(false);
            Debug.Log("[StartManager] 開始面板已隱藏");
        }

        Time.timeScale = 1f;
        Debug.Log("[StartManager] 時間已恢復");

        if (player != null)
        {
            player.canControl = true;
            Debug.Log("[StartManager] 玩家控制已啟用");
        }

        if (ball != null)
        {
            ball.canRoll = true;
            Debug.Log("[StartManager] 球滾動已啟用");
        }

        // 啟用Boss移動
        if (boss != null)
        {
            boss.canMove = true;
            Debug.Log($"[StartManager] ✅ Boss移動已啟用 - canMove: {boss.canMove}");
            StartCoroutine(VerifyBossMovement());
        }
        else
        {
            Debug.LogError("[StartManager] ❌ 無法啟用Boss：boss為null");
        }

        Debug.Log("=== [StartManager] 遊戲已開始 ===");
    }

    IEnumerator VerifyBossMovement()
    {
        yield return new WaitForSeconds(1f);

        if (boss != null)
        {
            Debug.Log($"[StartManager] 驗證Boss狀態 - canMove: {boss.canMove}, isDead: {boss.isDead}");

            if (!boss.canMove)
            {
                Debug.LogError("[StartManager] Boss的canMove仍然是false！可能有其他腳本在干擾");
            }
        }
    }

    void Update()
    {
        // 按T鍵快速開始（測試用）
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[StartManager] 測試鍵T被按下，強制開始遊戲");
            OnStartButtonClicked();
        }
    }
}