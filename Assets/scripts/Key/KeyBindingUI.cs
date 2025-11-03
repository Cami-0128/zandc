using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 按鍵綁定 UI 管理器
/// </summary>
public class KeyBindingUI : MonoBehaviour
{
    [Header("UI 參考")]
    public GameObject keyBindingPanel;
    public Transform buttonContainer;
    public GameObject keyBindingButtonPrefab;
    public Button resetButton;
    public Button closeButton;
    public GameObject waitingForKeyPanel;
    public TextMeshProUGUI waitingText;

    [Header("流程控制")]
    public StartManger startManager;

    private KeyBindingManager.ActionType currentWaitingAction;
    private bool isWaitingForKey = false;
    private bool isInitialized = false;

    void Start()
    {
        InitializeUI();
    }

    /// <summary>
    /// 初始化 UI (只執行一次)
    /// </summary>
    void InitializeUI()
    {
        if (isInitialized) return;

        Debug.Log("[KeyBindingUI] ========== 初始化開始 ==========");

        // 檢查必要組件
        if (keyBindingButtonPrefab == null)
        {
            Debug.LogError("[KeyBindingUI] ❌ Prefab 是 null!");
            return;
        }

        // 設置按鈕事件
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetAllBindings);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        // 創建按鍵按鈕
        CreateKeyBindingButtons();

        isInitialized = true;

        Debug.Log("[KeyBindingUI] ========== 初始化完成 ==========");
    }

    void CreateKeyBindingButtons()
    {
        if (buttonContainer == null || KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingUI] ButtonContainer 或 KeyBindingManager 是 null!");
            return;
        }

        // 清除現有按鈕
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        var actions = KeyBindingManager.Instance.GetAllActions();
        Debug.Log($"[KeyBindingUI] 創建 {actions.Length} 個按鈕");

        foreach (var action in actions)
        {
            GameObject buttonObj = Instantiate(keyBindingButtonPrefab, buttonContainer);
            buttonObj.SetActive(true);

            KeyBindingButton buttonScript = buttonObj.GetComponent<KeyBindingButton>();
            if (buttonScript != null)
            {
                buttonScript.Initialize(action, this);
                Debug.Log($"[KeyBindingUI] ✓ 創建按鈕: {KeyBindingManager.Instance.GetActionName(action)}");
            }
            else
            {
                Debug.LogError("[KeyBindingUI] ❌ Prefab 沒有 KeyBindingButton 腳本!");
            }
        }

        // 強制更新佈局
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonContainer as RectTransform);
    }

    public void StartWaitingForKey(KeyBindingManager.ActionType action)
    {
        currentWaitingAction = action;
        isWaitingForKey = true;

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(true);

        if (waitingText != null)
        {
            string actionName = KeyBindingManager.Instance.GetActionName(action);
            waitingText.text = $"請按下 [{actionName}] 的新按鍵\n\n按 ESC 取消";
        }

        StartCoroutine(WaitForKeyInput());
    }

    IEnumerator WaitForKeyInput()
    {
        while (isWaitingForKey)
        {
            // ESC 取消
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelWaiting();
                yield break;
            }

            // 偵測所有按鍵
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (IsValidKey(keyCode))
                    {
                        KeyBindingManager.Instance.SetKey(currentWaitingAction, keyCode);
                        UpdateAllButtons();
                        CancelWaiting();
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }

    bool IsValidKey(KeyCode keyCode)
    {
        // 過濾掉滑鼠按鍵和 ESC
        if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6) return false;
        if (keyCode == KeyCode.Escape) return false;
        return true;
    }

    void CancelWaiting()
    {
        isWaitingForKey = false;
        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);
    }

    void UpdateAllButtons()
    {
        KeyBindingButton[] buttons = buttonContainer.GetComponentsInChildren<KeyBindingButton>();
        foreach (var button in buttons)
        {
            button.UpdateDisplay();
        }
    }

    void ResetAllBindings()
    {
        KeyBindingManager.Instance.ResetToDefault();
        UpdateAllButtons();
        Debug.Log("[KeyBindingUI] 已重置所有按鍵");
    }

    /// <summary>
    /// 顯示按鍵設定面板 (從 Info 或其他地方呼叫)
    /// </summary>
    public void ShowPanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(true);
            Debug.Log("[KeyBindingUI] 顯示按鍵設定面板");
        }

        // 暫停遊戲
        Time.timeScale = 0f;

        // 禁用玩家控制
        if (startManager != null)
        {
            startManager.DisableAllControls();
        }
    }

    /// <summary>
    /// 關閉按鍵設定面板 (完成設定時)
    /// </summary>
    void ClosePanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(false);
            Debug.Log("[KeyBindingUI] 關閉按鍵設定面板");
        }

        // 通知 StartManager 按鍵設定已完成
        if (startManager != null)
        {
            startManager.OnKeyBindingComplete();
        }
        else
        {
            Debug.LogWarning("[KeyBindingUI] StartManager 未設定,直接恢復遊戲");
            Time.timeScale = 1f;
        }
    }
}