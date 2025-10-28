using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ����]�w UI �ɭ����
/// </summary>
public class KeyBindingUI : MonoBehaviour
{
    [Header("UI �Ѧ�")]
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
        Debug.Log("[KeyBindingUI] ��l�ƶ}�l");

        // �]�m���s�ƥ�
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetAllBindings);
        else
            Debug.LogError("[KeyBindingUI] ResetButton �O null!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("[KeyBindingUI] CloseButton �O null!");

        // ��l�Ƭɭ�
        CreateKeyBindingButtons();

        // �C���}�l����ܳ]�w�ɭ�
        ShowPanel();
    }

    /// <summary>
    /// �ЫةҦ�����]�w���s
    /// </summary>
    void CreateKeyBindingButtons()
    {
        if (buttonContainer == null)
        {
            Debug.LogError("[KeyBindingUI] Button Container �O null!");
            return;
        }

        if (keyBindingButtonPrefab == null)
        {
            Debug.LogError("[KeyBindingUI] KeyBindingButton Prefab �O null!");
            return;
        }

        if (KeyBindingManager.Instance == null)
        {
            Debug.LogError("[KeyBindingUI] KeyBindingManager.Instance �O null!");
            return;
        }

        // �M���{�����s
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // ���C�Ӱʧ@�Ыث��s
        var actions = KeyBindingManager.Instance.GetAllActions();
        Debug.Log($"[KeyBindingUI] �}�l�Ы� {actions.Length} �ӫ��s");

        foreach (var action in actions)
        {
            GameObject buttonObj = Instantiate(keyBindingButtonPrefab, buttonContainer);
            KeyBindingButton buttonScript = buttonObj.GetComponent<KeyBindingButton>();

            if (buttonScript != null)
            {
                buttonScript.Initialize(action, this);
                Debug.Log($"[KeyBindingUI] �w�Ыث��s: {KeyBindingManager.Instance.GetActionName(action)}");
            }
            else
            {
                Debug.LogError($"[KeyBindingUI] KeyBindingButton Prefab �W�S�� KeyBindingButton �}��!");
            }
        }
    }

    /// <summary>
    /// �}�l���ݪ��a���U�s����
    /// </summary>
    public void StartWaitingForKey(KeyBindingManager.ActionType action)
    {
        currentWaitingAction = action;
        isWaitingForKey = true;

        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(true);
        else
            Debug.LogError("[KeyBindingUI] WaitingForKeyPanel �O null!");

        if (waitingText != null)
            waitingText.text = $"�Ы��U [{KeyBindingManager.Instance.GetActionName(action)}] ���s����\n\n�� ESC ����";

        StartCoroutine(WaitForKeyInput());
    }

    /// <summary>
    /// ���ݪ��a��J�s����
    /// </summary>
    IEnumerator WaitForKeyInput()
    {
        Debug.Log("[KeyBindingUI] ���ݫ����J...");

        while (isWaitingForKey)
        {
            // �ˬd ESC ����
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[KeyBindingUI] ��������]�w");
                CancelWaiting();
                yield break;
            }

            // �ˬd��������J
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    // �ư��@�Ǥ��A�X������
                    if (IsValidKey(keyCode))
                    {
                        // �ˬd����O�_�w�Q�ϥ�
                        if (KeyBindingManager.Instance.IsKeyUsed(keyCode, currentWaitingAction))
                        {
                            Debug.LogWarning($"[KeyBindingUI] ���� {keyCode} �w�Q��L�\��ϥ�!");
                            // �p�G�n���\���ƨϥ�,�i�H�����o�ӧP�_
                        }

                        // �]�w�s����
                        KeyBindingManager.Instance.SetKey(currentWaitingAction, keyCode);

                        // ��s UI
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
    /// �ˬd����O�_����(�ư� Escape, Mouse ���S�����)
    /// </summary>
    bool IsValidKey(KeyCode keyCode)
    {
        // �ư��ƹ�����
        if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
            return false;

        // �ư� Escape
        if (keyCode == KeyCode.Escape)
            return false;

        return true;
    }

    /// <summary>
    /// �������ݿ�J
    /// </summary>
    void CancelWaiting()
    {
        isWaitingForKey = false;
        if (waitingForKeyPanel != null)
            waitingForKeyPanel.SetActive(false);
    }

    /// <summary>
    /// ��s�Ҧ����s���
    /// </summary>
    void UpdateAllButtons()
    {
        if (buttonContainer == null) return;

        KeyBindingButton[] buttons = buttonContainer.GetComponentsInChildren<KeyBindingButton>();
        foreach (var button in buttons)
        {
            button.UpdateDisplay();
        }

        Debug.Log("[KeyBindingUI] �w��s�Ҧ����s���");
    }

    /// <summary>
    /// ���m�Ҧ�����j�w
    /// </summary>
    void ResetAllBindings()
    {
        KeyBindingManager.Instance.ResetToDefault();
        UpdateAllButtons();
        Debug.Log("[KeyBindingUI] �w���m�Ҧ�����");
    }

    /// <summary>
    /// ��ܳ]�w���O
    /// </summary>
    public void ShowPanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(true);
            Debug.Log("[KeyBindingUI] ��ܳ]�w���O");
        }
        else
        {
            Debug.LogError("[KeyBindingUI] KeyBindingPanel �O null!");
        }

        Time.timeScale = 0f; // �Ȱ��C��
    }

    /// <summary>
    /// �����]�w���O
    /// </summary>
    public void ClosePanel()
    {
        if (keyBindingPanel != null)
        {
            keyBindingPanel.SetActive(false);
            Debug.Log("[KeyBindingUI] �����]�w���O");
        }

        Time.timeScale = 1f; // ��_�C��
    }
}