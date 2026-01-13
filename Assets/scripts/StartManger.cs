using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遊戲開始管理器 - 控制玩家和所有Boss在遊戲開始前不能移動
/// 支援 BossController
/// </summary>
public class StartManger : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("包含開始按鈕的 UI Panel")]
    public GameObject startPanel;

    [Header("遊戲物件引用")]
    [Tooltip("玩家控制器")]
    public PlayerController2D player;

    [Tooltip("球滾動控制器（可選）")]
    public BallRoller ball;

    [Tooltip("Boss控制器（可選）")]
    public BossController boss;

    [Tooltip("冰元素Boss控制器")]
    public IceBossController iceBoss;

    [Header("批量控制（可選）")]
    [Tooltip("需要控制的所有冰元素Boss（支援多個）")]
    public IceBossController[] iceBosses;

    [Tooltip("需要控制的所有Boss（支援多個）")]
    public BossController[] normalBosses;

    [Header("Debug設定")]
    [Tooltip("是否顯示詳細Log")]
    public bool debugMode = true;

    void Start()
    {
        if (debugMode)
        {
            Debug.Log("========== [StartManager] 初始化開始 ==========");
        }

        // 顯示UI並暫停遊戲
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            if (debugMode) Debug.Log("[StartManager] 開始面板已顯示");
        }
        else
        {
            Debug.LogWarning("[StartManager] ⚠️ 未設定開始面板！");
        }

        // 暫停時間
        Time.timeScale = 0f;
        if (debugMode) Debug.Log($"[StartManager] Time.timeScale 已設為: {Time.timeScale}");

        // === 禁用玩家控制 ===
        if (player != null)
        {
            player.canControl = false;
            if (debugMode) Debug.Log("[StartManager] ✓ 玩家控制已禁用");
        }
        else
        {
            Debug.LogWarning("[StartManager] ⚠️ 未設定玩家！");
        }

        // === 禁用球滾動（可選）===
        if (ball != null)
        {
            ball.canRoll = false;
            if (debugMode) Debug.Log("[StartManager] ✓ 球滾動已禁用");
        }

        // === 禁用普通Boss（單一）===
        if (boss != null)
        {
            boss.canMove = false;
            if (debugMode) Debug.Log($"[StartManager] ✓ Boss已禁用");
        }

        // === 禁用冰元素Boss（單一）===
        if (iceBoss != null)
        {
            iceBoss.canMove = false;
            if (debugMode) Debug.Log($"[StartManager] ✓ 冰元素Boss已禁用 - canMove: {iceBoss.canMove}");
        }

        // === 批量禁用所有冰元素Boss ===
        if (iceBosses != null && iceBosses.Length > 0)
        {
            int count = 0;
            foreach (var iceB in iceBosses)
            {
                if (iceB != null)
                {
                    iceB.canMove = false;
                    count++;
                    if (debugMode) Debug.Log($"[StartManager] ✓ 冰Boss #{count} ({iceB.gameObject.name}) 已禁用");
                }
            }
            if (debugMode) Debug.Log($"[StartManager] 總共禁用了 {count} 個冰元素Boss");
        }

        // === 批量禁用所有普通Boss ===
        if (normalBosses != null && normalBosses.Length > 0)
        {
            int count = 0;
            foreach (var normalB in normalBosses)
            {
                if (normalB != null)
                {
                    normalB.canMove = false;
                    count++;
                    if (debugMode) Debug.Log($"[StartManager] ✓ Boss #{count} ({normalB.gameObject.name}) 已禁用");
                }
            }
            if (debugMode) Debug.Log($"[StartManager] 總共禁用了 {count} 個Boss");
        }

        if (debugMode)
        {
            Debug.Log("========== [StartManager] 初始化完成 ==========");
            Debug.Log("等待玩家點擊開始按鈕...");
        }
    }

    /// <summary>
    /// 開始按鈕點擊事件
    /// </summary>
    public void OnStartButtonClicked()
    {
        if (debugMode)
        {
            Debug.Log("========== [StartManager] 🎮 遊戲開始 ==========");
        }

        // 隱藏開始面板
        if (startPanel != null)
        {
            startPanel.SetActive(false);
            if (debugMode) Debug.Log("[StartManager] 開始面板已隱藏");
        }

        // 恢復時間
        Time.timeScale = 1f;
        if (debugMode) Debug.Log($"[StartManager] Time.timeScale 已恢復為: {Time.timeScale}");

        // === 啟用玩家控制 ===
        if (player != null)
        {
            player.canControl = true;
            if (debugMode) Debug.Log("[StartManager] ✅ 玩家控制已啟用");
        }

        // === 啟用球滾動 ===
        if (ball != null)
        {
            ball.canRoll = true;
            if (debugMode) Debug.Log("[StartManager] ✅ 球滾動已啟用");
        }

        // === 啟用普通Boss（單一）===
        if (boss != null)
        {
            boss.canMove = true;
            if (debugMode) Debug.Log($"[StartManager] ✅ Boss已啟用");
        }

        // === 啟用冰元素Boss（單一）===
        if (iceBoss != null)
        {
            iceBoss.canMove = true;
            if (debugMode) Debug.Log($"[StartManager] ✅ 冰元素Boss已啟用 - canMove: {iceBoss.canMove}");
        }

        // === 批量啟用所有冰元素Boss ===
        if (iceBosses != null && iceBosses.Length > 0)
        {
            int count = 0;
            foreach (var iceB in iceBosses)
            {
                if (iceB != null)
                {
                    iceB.canMove = true;
                    count++;
                    if (debugMode) Debug.Log($"[StartManager] ✅ 冰Boss #{count} ({iceB.gameObject.name}) 已啟用");
                }
            }
            if (debugMode) Debug.Log($"[StartManager] 總共啟用了 {count} 個冰元素Boss");
        }

        // === 批量啟用所有普通Boss ===
        if (normalBosses != null && normalBosses.Length > 0)
        {
            int count = 0;
            foreach (var normalB in normalBosses)
            {
                if (normalB != null)
                {
                    normalB.canMove = true;
                    count++;
                    if (debugMode) Debug.Log($"[StartManager] ✅ Boss #{count} ({normalB.gameObject.name}) 已啟用");
                }
            }
            if (debugMode) Debug.Log($"[StartManager] 總共啟用了 {count} 個Boss");
        }

        if (debugMode)
        {
            Debug.Log("========== [StartManager] 遊戲已開始！ ==========");
        }
    }

    void Update()
    {
        // 按T鍵快速測試
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[StartManager] ⚡ 測試鍵T - 強制開始遊戲");
            OnStartButtonClicked();
        }

        // 按I鍵顯示當前狀態
        if (Input.GetKeyDown(KeyCode.I) && debugMode)
        {
            ShowCurrentStatus();
        }
    }

    /// <summary>
    /// 顯示當前狀態（Debug用）
    /// </summary>
    void ShowCurrentStatus()
    {
        Debug.Log("========== [StartManager] 當前狀態 ==========");
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"玩家可控制: {(player != null ? player.canControl.ToString() : "無")}");

        if (boss != null)
        {
            Debug.Log($"Boss可移動: {boss.canMove}");
        }

        if (iceBoss != null)
        {
            Debug.Log($"冰Boss可移動: {iceBoss.canMove}");
        }

        if (normalBosses != null && normalBosses.Length > 0)
        {
            for (int i = 0; i < normalBosses.Length; i++)
            {
                if (normalBosses[i] != null)
                {
                    Debug.Log($"Boss[{i}] ({normalBosses[i].gameObject.name}) 可移動: {normalBosses[i].canMove}");
                }
            }
        }

        if (iceBosses != null && iceBosses.Length > 0)
        {
            for (int i = 0; i < iceBosses.Length; i++)
            {
                if (iceBosses[i] != null)
                {
                    Debug.Log($"冰Boss[{i}] ({iceBosses[i].gameObject.name}) 可移動: {iceBosses[i].canMove}");
                }
            }
        }

        Debug.Log("============================================");
    }

    /// <summary>
    /// 自動尋找場景中的所有Boss（可選功能）
    /// 在Inspector中右鍵點擊腳本 > 自動尋找所有Boss
    /// </summary>
    [ContextMenu("自動尋找所有Boss")]
    public void AutoFindBosses()
    {
        // 自動尋找冰Boss
        iceBosses = FindObjectsOfType<IceBossController>();
        Debug.Log($"[StartManager] 自動找到 {iceBosses.Length} 個冰元素Boss");

        // 自動尋找普通Boss
        normalBosses = FindObjectsOfType<BossController>();
        Debug.Log($"[StartManager] 自動找到 {normalBosses.Length} 個Boss");

        // 如果只有一個，也設定到單一欄位
        if (normalBosses.Length == 1)
        {
            boss = normalBosses[0];
            Debug.Log($"[StartManager] 已設定單一Boss: {boss.gameObject.name}");
        }

        if (iceBosses.Length == 1)
        {
            iceBoss = iceBosses[0];
            Debug.Log($"[StartManager] 已設定單一冰Boss: {iceBoss.gameObject.name}");
        }

        Debug.Log("[StartManager] ✅ 自動尋找完成！");
    }
}