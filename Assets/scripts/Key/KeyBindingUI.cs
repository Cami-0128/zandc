using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 按鍵設定 UI 界面控制器
/// </summary>
public class KeyBindingUI : MonoBehaviour
{
    [Header("UI 參考")]
    [SerializeField] private GameObject keyBindingPanel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject keyBindingButtonPrefab;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject waitingForKeyPanel;
    [SerializeField] private TextMeshProUGUI waitingText;

    private KeyBindingManager.ActionType currentWaitingAction;
    private bool isWaitingForKey = false;

    void Start()
    {
        Debug.Log("[KeyBindingUI] 初始化開始");

        // 設置按鈕事件
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetAllBindings);
        else
            Debug.LogError("[KeyBindingUI] ResetButton 是 null!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("[KeyBindingUI] CloseButton 是 null!");

        // 初始化界面
        CreateKeyBindingButtons();

        // 遊戲開始時顯示設定界面
        ShowPanel();
    }

    /// <summary>
    /// 創建所有按鍵設定按鈕
    /// </summary>
    void CreateKeyBindingButtons()
    {
        if (buttonContainer == null)
        {
            Debug.LogError("[KeyBindingUI] Button Container 是 null!");
            return;
        }

        if (keyBindingButtonPrefab == null)
        {
            Debug.LogError("[KeyBindingUI] KeyBindingButton Prefab 是 null!");
            return;
        }

        if (KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingUI] KeyBindingManager.Instance 是 null!");
            return;
        }

        // 清除現有按鈕
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 為每個動作創建按鈕
        var actions = KeyBindingManager.Instance.GetAllActions();
        Debug.Log($"[KeyBindingUI] 開始創建 {actions.Length} 個按鈕");

        foreach (var action in actions)
        {
            GameObject buttonObj = Instantiate(keyBindingButtonPrefab, buttonContainer);
            KeyBindingButton buttonScript = buttonObj.GetComponent<KeyBindingButton>();

            if (buttonScript != null)
            {
                buttonScript.Initialize(action, this);
                Debug.Log($"[KeyBindingUI] 已創建按鈕: {KeyBindingManager.Instance.GetActionName(action)}");
            }
            else
            {
                Debug.LogError($"[KeyBindingUI] KeyBindingButton Prefab 上沒有 KeyBindingButton 腳本!");
            }
        }
    }

    /// <summary>
    /// 開始等待玩家按下新按鍵
    /// </summary>
    public void StartWaitingForKey(KeyBindingManager.ActionType action)
    {
        currentWaitingAction = action;
        isWaitingForKey = true;

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(true);
        else
            Debug.LogError("[KeyBindingUI] WaitingForKeyPanel 是 null!");

        if (waitingText != null)
            waitingText.text = $"請按下 [{KeyBindingManager.Instance.GetActionName(action)}] 的新按鍵\n\n按 ESC 取消";

        StartCoroutine(WaitForKeyInput());
    }

    /// <summary>
    /// 等待玩家輸入新按鍵
    /// </summary>
    IEnumerator WaitForKeyInput()
    {
        Debug.Log("[KeyBindingUI] 等待按鍵輸入...");

        while (isWaitingForKey)
        {
            // 檢查 ESC 取消
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[KeyBindingUI] 取消按鍵設定");
                CancelWaiting();
                yield break;
            }

            // 檢查任何按鍵輸入
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // 排除一些不適合的按鍵
                    if (IsValidKey(keyCode))
                    {
                        // 檢查按鍵是否已被使用
                        if (KeyBindingManager.Instance.IsKeyUsed(keyCode, currentWaitingAction))
                        {
                            Debug.LogWarning($"[KeyBindingUI] 按鍵 {keyCode} 已被其他功能使用!");
                            // 如果要允許重複使用,可以移除這個判斷
                        }

                        // 設定新按鍵
                        KeyBindingManager.Instance.SetKey(currentWaitingAction, keyCode);

                        // 更新 UI
                        UpdateAllButtons();

                        CancelWaiting();
                        yield break;
                    }
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// 檢查按鍵是否有效(排除 Escape, Mouse 等特殊按鍵)
    /// </summary>
    bool IsValidKey(KeyCode keyCode)
    {
        // 排除滑鼠按鍵
        if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
            return false;

        // 排除 Escape
        if (keyCode == KeyCode.Escape)
            return false;

        return true;
    }

    /// <summary>
    /// 取消等待輸入
    /// </summary>
    void CancelWaiting()
    {
        isWaitingForKey = false;
        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);
    }

    /// <summary>
    /// 更新所有按鈕顯示
    /// </summary>
    void UpdateAllButtons()
    {
        if (buttonContainer == null) return;

        KeyBindingButton[] buttons = buttonContainer.GetComponentsInChildren<KeyBindingButton>();
        foreach (var button in buttons)
        {
            button.UpdateDisplay();
        }

        Debug.Log("[KeyBindingUI] 已更新所有按鈕顯示");
    }

    /// <summary>
    /// 重置所有按鍵綁定
    /// </summary>
    void ResetAllBindings()
    {
        KeyBindingManager.Instance.ResetToDefault();
        UpdateAllButtons();
        Debug.Log("[KeyBindingUI] 已重置所有按鍵");
    }

    /// <summary>
    /// 顯示設定面板
    /// </summary>
    public void ShowPanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(true);
            Debug.Log("[KeyBindingUI] 顯示設定面板");
        }
        else
        {
            Debug.LogError("[KeyBindingUI] KeyBindingPanel 是 null!");
        }

        Time.timeScale = 0f; // 暫停遊戲
    }

    /// <summary>
    /// 關閉設定面板
    /// </summary>
    public void ClosePanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(false);
            Debug.Log("[KeyBindingUI] 關閉設定面板");
        }

        Time.timeScale = 1f; // 恢復遊戲
    }
}