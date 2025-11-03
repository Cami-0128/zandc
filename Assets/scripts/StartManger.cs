using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遊戲開始管理器 - 控制開始流程和自定義按鍵
/// </summary>
public class StartManger : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject startPanel;           // 開始遊戲 UI
    public GameObject keyBindingPanel;      // 自定義按鍵 UI (KeyBindingUIManager 的面板)

    [Header("遊戲控制")]
    public PlayerController2D player;
    public BallRoller ball;

    [Header("其他管理器")]
    public ShopManager shopManager;         // 商店管理器

    private bool isKeyBindingComplete = false;  // 按鍵設定是否完成

    void Start()
    {
        Debug.Log("[StartManager] 遊戲啟動");

        // 初始化:全部暫停,顯示自定義按鍵面板
        InitializeGame();
    }

    /// <summary>
    /// 初始化遊戲 - 顯示按鍵設定面板
    /// </summary>
    void InitializeGame()
    {
        // 暫停遊戲
        Time.timeScale = 0f;

        // 禁用所有控制
        DisableAllControls();

        // 顯示自定義按鍵面板
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(true);
            Debug.Log("[StartManager] 顯示自定義按鍵面板");
        }

        // 隱藏開始面板
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 禁用所有遊戲控制
    /// </summary>
    public void DisableAllControls()
    {
        if (player != null)
        {
            player.canControl = false;
        }

        if (ball != null)
        {
            ball.canRoll = false;
        }

        Debug.Log("[StartManager] 已禁用所有控制");
    }

    /// <summary>
    /// 啟用所有遊戲控制
    /// </summary>
    void EnableAllControls()
    {
        if (player != null)
        {
            player.canControl = true;
        }

        if (ball != null)
        {
            ball.canRoll = true;
        }

        Debug.Log("[StartManager] 已啟用所有控制");
    }

    /// <summary>
    /// 當自定義按鍵完成時呼叫 (由 KeyBindingUI 呼叫)
    /// </summary>
    public void OnKeyBindingComplete()
    {
        isKeyBindingComplete = true;

        // 隱藏自定義按鍵面板
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(false);
            Debug.Log("[StartManager] 隱藏自定義按鍵面板");
        }

        // 顯示開始遊戲面板
        ShowStartPanel();
    }

    /// <summary>
    /// 顯示開始遊戲面板
    /// </summary>
    public void ShowStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log("[StartManager] 顯示開始遊戲面板");
        }

        // 遊戲仍然暫停,直到點擊開始遊戲
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 點擊開始遊戲按鈕時呼叫
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("[StartManager] 開始遊戲");

        // 隱藏開始面板
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // 恢復遊戲時間
        Time.timeScale = 1f;

        // 啟用所有控制
        EnableAllControls();

        // 關閉商店和詳情面板(如果開著的話)
        if (shopManager != null)
        {
            shopManager.ForceCloseShop();
        }
    }

    /// <summary>
    /// 暫停遊戲並顯示 StartPanel (從其他地方呼叫時使用)
    /// </summary>
    public void PauseGameAndShowStart()
    {
        Time.timeScale = 0f;
        DisableAllControls();
        ShowStartPanel();
    }
}