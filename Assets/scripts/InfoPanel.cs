using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 資訊面板 - 顯示當前按鍵設定和遊戲說明
/// </summary>
public class InfoPanel : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;
    public Button closeButton;
    public Button reconfigureButton; // 重新設定按鍵按鈕

    [Header("按鍵綁定")]
    public KeyBindingUI keyBindingUI;

    [Header("說明文字模板")]
    [TextArea(15, 30)]
    public string infoTemplate =
@"===== 遊戲操作說明 =====

【移動控制】
{MoveLeft} - 向左移動
{MoveRight} - 向右移動
{Jump} - 跳躍

【介面操作】
{OpenShop} - 開關商店
{OpenInfo} - 開關資訊面板

【戰鬥操作】
{Attack1} - 攻擊1
{Attack2} - 攻擊2
{Skill1} - 技能1 (需從商店購買)

提示: 可以點擊下方按鈕重新設定按鍵!";

    void Start()
    {
        // 初始化時隱藏面板
        if (infoPanel != null)
            infoPanel.SetActive(false);

        // 設置按鈕事件
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }

        if (reconfigureButton != null)
        {
            reconfigureButton.onClick.RemoveAllListeners();
            reconfigureButton.onClick.AddListener(OnReconfigureClicked);
        }

        // 監聽按鍵改變事件
        if (KeyBindingManager.Instance != null)
        {
            KeyBindingManager.Instance.OnBindingsChanged += UpdateInfoText;
        }

        // 初始化文字
        UpdateInfoText();
    }

    void OnDestroy()
    {
        // 取消監聽
        if (KeyBindingManager.Instance != null)
        {
            KeyBindingManager.Instance.OnBindingsChanged -= UpdateInfoText;
        }
    }

    /// <summary>
    /// 更新資訊文字 (根據當前按鍵設定)
    /// </summary>
    public void UpdateInfoText()
    {
        if (infoText == null || KeyBindingManager.Instance == null)
            return;

        string finalText = infoTemplate;

        // 替換所有按鍵佔位符
        var actions = KeyBindingManager.Instance.GetAllActions();
        foreach (var action in actions)
        {
            string placeholder = "{" + action.ToString() + "}";
            KeyCode key = KeyBindingManager.Instance.GetKeyCode(action);
            string keyName = GetKeyDisplayName(key);

            finalText = finalText.Replace(placeholder, keyName);
        }

        infoText.text = finalText;
    }

    /// <summary>
    /// 獲取按鍵的友善顯示名稱
    /// </summary>
    string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return "← (左方向鍵)";
            case KeyCode.RightArrow: return "→ (右方向鍵)";
            case KeyCode.UpArrow: return "↑ (上方向鍵)";
            case KeyCode.DownArrow: return "↓ (下方向鍵)";
            case KeyCode.Space: return "空白鍵";
            case KeyCode.LeftShift: return "左 Shift";
            case KeyCode.RightShift: return "右 Shift";
            case KeyCode.LeftControl: return "左 Ctrl";
            case KeyCode.RightControl: return "右 Ctrl";
            case KeyCode.LeftAlt: return "左 Alt";
            case KeyCode.RightAlt: return "右 Alt";
            case KeyCode.Return: return "Enter";
            case KeyCode.Backspace: return "Backspace";
            case KeyCode.Tab: return "Tab";
            default: return key.ToString() + " 鍵";
        }
    }

    /// <summary>
    /// 顯示資訊面板
    /// </summary>
    public void ShowPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
            UpdateInfoText();

            // 暫停遊戲
            Time.timeScale = 0f;

            Debug.Log("[InfoPanel] 顯示資訊面板");
        }
    }

    /// <summary>
    /// 隱藏資訊面板
    /// </summary>
    public void HidePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);

            // 恢復遊戲
            Time.timeScale = 1f;

            Debug.Log("[InfoPanel] 隱藏資訊面板");
        }
    }

    /// <summary>
    /// 點擊重新設定按鍵按鈕
    /// </summary>
    void OnReconfigureClicked()
    {
        Debug.Log("[InfoPanel] 重新設定按鍵");

        // 隱藏 Info 面板
        HidePanel();

        // 顯示按鍵設定面板
        if (keyBindingUI != null)
        {
            keyBindingUI.ShowPanel();
        }
        else
        {
            Debug.LogError("[InfoPanel] KeyBindingUI 未設定!");
        }
    }
}