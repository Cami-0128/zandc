using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 單個按鍵綁定按鈕組件
/// 這個腳本會掛載在 KeyBindingButtonItem Prefab 上
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
    /// 初始化按鈕 (由 KeyBindingUI 呼叫)
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
    /// 更新顯示內容 (當按鍵改變時呼叫)
    /// </summary>
    public void UpdateDisplay()
    {
        if (KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingButton] KeyBindingManager.Instance 是 null!");
            return;
        }

        // 更新動作名稱 (例如: "向左移動")
        if (actionNameText != null)
        {
            actionNameText.text = KeyBindingManager.Instance.GetActionName(action);
        }
        else
        {
            Debug.LogWarning("[KeyBindingButton] ActionNameText 是 null!");
        }

        // 更新按鍵顯示 (例如: "A 鍵")
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
    /// 當點擊"更改"按鈕時
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
    /// 獲取按鍵的友善顯示名稱 (將 KeyCode 轉換為中文/符號)
    /// </summary>
    string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            // 方向鍵
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";

            // 特殊鍵
            case KeyCode.Space: return "空白鍵";
            case KeyCode.Return: return "Enter";
            case KeyCode.Backspace: return "Backspace";
            case KeyCode.Tab: return "Tab";
            case KeyCode.Escape: return "ESC";

            // Shift
            case KeyCode.LeftShift: return "左 Shift";
            case KeyCode.RightShift: return "右 Shift";

            // Ctrl
            case KeyCode.LeftControl: return "左 Ctrl";
            case KeyCode.RightControl: return "右 Ctrl";

            // Alt
            case KeyCode.LeftAlt: return "左 Alt";
            case KeyCode.RightAlt: return "右 Alt";

            // 字母鍵 (顯示為 "X 鍵")
            case KeyCode.A: return "A 鍵";
            case KeyCode.B: return "B 鍵";
            case KeyCode.C: return "C 鍵";
            case KeyCode.D: return "D 鍵";
            case KeyCode.E: return "E 鍵";
            case KeyCode.F: return "F 鍵";
            case KeyCode.G: return "G 鍵";
            case KeyCode.H: return "H 鍵";
            case KeyCode.I: return "I 鍵";
            case KeyCode.J: return "J 鍵";
            case KeyCode.K: return "K 鍵";
            case KeyCode.L: return "L 鍵";
            case KeyCode.M: return "M 鍵";
            case KeyCode.N: return "N 鍵";
            case KeyCode.O: return "O 鍵";
            case KeyCode.P: return "P 鍵";
            case KeyCode.Q: return "Q 鍵";
            case KeyCode.R: return "R 鍵";
            case KeyCode.S: return "S 鍵";
            case KeyCode.T: return "T 鍵";
            case KeyCode.U: return "U 鍵";
            case KeyCode.V: return "V 鍵";
            case KeyCode.W: return "W 鍵";
            case KeyCode.X: return "X 鍵";
            case KeyCode.Y: return "Y 鍵";
            case KeyCode.Z: return "Z 鍵";

            // 數字鍵
            case KeyCode.Alpha0: return "0 鍵";
            case KeyCode.Alpha1: return "1 鍵";
            case KeyCode.Alpha2: return "2 鍵";
            case KeyCode.Alpha3: return "3 鍵";
            case KeyCode.Alpha4: return "4 鍵";
            case KeyCode.Alpha5: return "5 鍵";
            case KeyCode.Alpha6: return "6 鍵";
            case KeyCode.Alpha7: return "7 鍵";
            case KeyCode.Alpha8: return "8 鍵";
            case KeyCode.Alpha9: return "9 鍵";

            // F 功能鍵
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // 其他常用鍵
            case KeyCode.CapsLock: return "Caps Lock";
            case KeyCode.Insert: return "Insert";
            case KeyCode.Delete: return "Delete";
            case KeyCode.Home: return "Home";
            case KeyCode.End: return "End";
            case KeyCode.PageUp: return "Page Up";
            case KeyCode.PageDown: return "Page Down";

            // 預設: 直接顯示 KeyCode 名稱
            default: return key.ToString();
        }
    }
}