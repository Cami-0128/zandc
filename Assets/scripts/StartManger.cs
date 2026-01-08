using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManger : MonoBehaviour
{
    [Header("UI設定")]
    public GameObject startPanel; // 包含按鈕的 UI Panel

    [Header("遊戲物件引用")]
    public PlayerController2D player;
    public BallRoller ball;
    public BossController boss; // Boss引用

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
        else
        {
            Debug.LogError("[StartManager] startPanel 未設定！");
        }

        Time.timeScale = 0f; // 暫停時間

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
        bool allValid = true;

        if (startPanel == null)
        {
            Debug.LogError("[StartManager] startPanel 未設定！");
            allValid = false;
        }

        if (player == null)
        {
            Debug.LogError("[StartManager] player 未設定！");
            allValid = false;
        }

        if (ball == null)
        {
            Debug.LogWarning("[StartManager] ball 未設定（如果場景沒有球可以忽略）");
        }

        if (boss == null)
        {
            Debug.LogError("[StartManager] boss 未設定！Boss將無法啟動");
            allValid = false;
        }

        if (allValid && debugMode)
        {
            Debug.Log("[StartManager] 所有引用驗證通過");
        }
    }

    /// <summary>
    /// 開始按鈕點擊事件
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("=== [StartManager] 開始按鈕被點擊 ===");

        // 隱藏開始面板
        if (startPanel != null)
        {
            startPanel.SetActive(false);
            Debug.Log("[StartManager] 開始面板已隱藏");
        }

        // 恢復時間
        Time.timeScale = 1f;
        Debug.Log("[StartManager] 時間已恢復");

        // 啟用玩家控制
        if (player != null)
        {
            player.canControl = true;
            Debug.Log("[StartManager] 玩家控制已啟用");
        }
        else
        {
            Debug.LogError("[StartManager] 無法啟用玩家：player為null");
        }

        // 啟用球滾動
        if (ball != null)
        {
            ball.canRoll = true;
            Debug.Log("[StartManager] 球滾動已啟用");
        }

        // ===== 【關鍵】啟用Boss移動 =====
        if (boss != null)
        {
            boss.canMove = true;
            Debug.Log($"[StartManager] ✅ Boss移動已啟用 - canMove: {boss.canMove}");

            // 額外驗證
            StartCoroutine(VerifyBossMovement());
        }
        else
        {
            Debug.LogError("[StartManager] ❌ 無法啟用Boss：boss為null！請檢查Inspector設定");
        }

        Debug.Log("=== [StartManager] 遊戲已開始 ===");
    }

    /// <summary>
    /// 驗證Boss是否真的開始移動
    /// </summary>
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

    /// <summary>
    /// 手動測試用：立即開始遊戲（按T鍵測試）
    /// </summary>
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