using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 單個按鍵綁定按鈕組件
/// </summary>
public class KeyBindingButton : MonoBehaviour
{
    [Header("UI 元件")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private TextMeshProUGUI keyNameText;
    [SerializeField] private Button changeKeyButton;

    private KeyBindingManager.ActionType action;
    private KeyBindingUI parentUI;

    /// <summary>
    /// 初始化按鈕
    /// </summary>
    public void Initialize(KeyBindingManager.ActionType actionType, KeyBindingUI ui)
    {
        action = actionType;
        parentUI = ui;

        Debug.Log($"[KeyBindingButton] 初始化: {KeyBindingManager.Instance.GetActionName(action)}");

        // 設置按鈕點擊事件
        if (changeKeyButton != null)
        {
            changeKeyButton.onClick.RemoveAllListeners();
            changeKeyButton.onClick.AddListener(OnChangeKeyClicked);
        }
        else
        {
            Debug.LogError($"[KeyBindingButton] ChangeKeyButton 是 null!");
        }

        UpdateDisplay();
    }

    /// <summary>
    /// 更新顯示內容
    /// </summary>
    public void UpdateDisplay()
    {
        if (KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingButton] KeyBindingManager.Instance 是 null!");
            return;
        }

        if (actionNameText != null)
        {
            actionNameText.text = KeyBindingManager.Instance.GetActionName(action);
        }
        else
        {
            Debug.LogWarning("[KeyBindingButton] ActionNameText 是 null!");
        }

        if (keyNameText != null)
        {
            KeyCode key = KeyBindingManager.Instance.GetKeyCode(action);
            keyNameText.text = GetKeyDisplayName(key);
        }
        else
        {
            Debug.LogWarning("[KeyBindingButton] KeyNameText 是 null!");
        }
    }

    /// <summary>
    /// 當點擊更改按鍵按鈕時
    /// </summary>
    void OnChangeKeyClicked()
    {
        Debug.Log($"[KeyBindingButton] 點擊更改: {KeyBindingManager.Instance.GetActionName(action)}");

        if (parentUI != null)
        {
            parentUI.StartWaitingForKey(action);
        }
        else
        {
            Debug.LogError("[KeyBindingButton] ParentUI 是 null!");
        }
    }

    /// <summary>
    /// 獲取按鍵的顯示名稱(將英文轉換為更友善的顯示)
    /// </summary>
    string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.Space: return "空白鍵";
            case KeyCode.LeftShift: return "左Shift";
            case KeyCode.RightShift: return "右Shift";
            case KeyCode.LeftControl: return "左Ctrl";
            case KeyCode.RightControl: return "右Ctrl";
            case KeyCode.LeftAlt: return "左Alt";
            case KeyCode.RightAlt: return "右Alt";
            case KeyCode.Return: return "Enter";
            case KeyCode.Backspace: return "Backspace";
            case KeyCode.Tab: return "Tab";
            default: return key.ToString();
        }
    }
}