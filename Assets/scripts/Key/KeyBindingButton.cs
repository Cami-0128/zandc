using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ��ӫ���j�w���s�ե�
/// </summary>
public class KeyBindingButton : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private TextMeshProUGUI keyNameText;
    [SerializeField] private Button changeKeyButton;

    private KeyBindingManager.ActionType action;
    private KeyBindingUI parentUI;

    /// <summary>
    /// ��l�ƫ��s
    /// </summary>
    public void Initialize(KeyBindingManager.ActionType actionType, KeyBindingUI ui)
    {
        action = actionType;
        parentUI = ui;

        Debug.Log($"[KeyBindingButton] ��l��: {KeyBindingManager.Instance.GetActionName(action)}");

        // �]�m���s�I���ƥ�
        if (changeKeyButton != null)
        {
            changeKeyButton.onClick.RemoveAllListeners();
            changeKeyButton.onClick.AddListener(OnChangeKeyClicked);
        }
        else
        {
            Debug.LogError($"[KeyBindingButton] ChangeKeyButton �O null!");
        }

        UpdateDisplay();
    }

    /// <summary>
    /// ��s��ܤ��e
    /// </summary>
    public void UpdateDisplay()
    {
        if (KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingButton] KeyBindingManager.Instance �O null!");
            return;
        }

        if (actionNameText != null)
        {
            actionNameText.text = KeyBindingManager.Instance.GetActionName(action);
        }
        else
        {
            Debug.LogWarning("[KeyBindingButton] ActionNameText �O null!");
        }

        if (keyNameText != null)
        {
            KeyCode key = KeyBindingManager.Instance.GetKeyCode(action);
            keyNameText.text = GetKeyDisplayName(key);
        }
        else
        {
            Debug.LogWarning("[KeyBindingButton] KeyNameText �O null!");
        }
    }

    /// <summary>
    /// ���I����������s��
    /// </summary>
    void OnChangeKeyClicked()
    {
        Debug.Log($"[KeyBindingButton] �I�����: {KeyBindingManager.Instance.GetActionName(action)}");

        if (parentUI != null)
        {
            parentUI.StartWaitingForKey(action);
        }
        else
        {
            Debug.LogError("[KeyBindingButton] ParentUI �O null!");
        }
    }

    /// <summary>
    /// ������䪺��ܦW��(�N�^���ഫ����͵������)
    /// </summary>
    string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return "��";
            case KeyCode.RightArrow: return "��";
            case KeyCode.UpArrow: return "��";
            case KeyCode.DownArrow: return "��";
            case KeyCode.Space: return "�ť���";
            case KeyCode.LeftShift: return "��Shift";
            case KeyCode.RightShift: return "�kShift";
            case KeyCode.LeftControl: return "��Ctrl";
            case KeyCode.RightControl: return "�kCtrl";
            case KeyCode.LeftAlt: return "��Alt";
            case KeyCode.RightAlt: return "�kAlt";
            case KeyCode.Return: return "Enter";
            case KeyCode.Backspace: return "Backspace";
            case KeyCode.Tab: return "Tab";
            default: return key.ToString();
        }
    }
}