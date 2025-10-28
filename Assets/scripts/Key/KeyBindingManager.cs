using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����j�w�޲z�� - �ϥγ�ҼҦ�,�޲z�Ҧ�����]�w
/// </summary>
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    // �w�q�Ҧ��i�Ϊ��ʧ@����
    public enum ActionType
    {
        MoveLeft,
        MoveRight,
        Jump,
        OpenShop,
        OpenInfo,
        Attack1,
        Attack2,
        Skill1
    }

    // �x�s�C�Ӱʧ@����������
    private Dictionary<ActionType, KeyCode> keyBindings = new Dictionary<ActionType, KeyCode>();

    // �w�]����]�w
    private Dictionary<ActionType, KeyCode> defaultBindings = new Dictionary<ActionType, KeyCode>()
    {
        { ActionType.MoveLeft, KeyCode.LeftArrow },
        { ActionType.MoveRight, KeyCode.RightArrow },
        { ActionType.Jump, KeyCode.W },
        { ActionType.OpenShop, KeyCode.S },
        { ActionType.OpenInfo, KeyCode.I },
        { ActionType.Attack1, KeyCode.N },
        { ActionType.Attack2, KeyCode.M },
        { ActionType.Skill1, KeyCode.B }
    };

    // �ʧ@��������ܦW��
    private Dictionary<ActionType, string> actionNames = new Dictionary<ActionType, string>()
    {
        { ActionType.MoveLeft, "�V������" },
        { ActionType.MoveRight, "�V�k����" },
        { ActionType.Jump, "���D" },
        { ActionType.OpenShop, "�}���ө�" },
        { ActionType.OpenInfo, "�}����T" },
        { ActionType.Attack1, "����1" },
        { ActionType.Attack2, "����2" },
        { ActionType.Skill1, "�ޯ�1" }
    };

    // �����j�w���ܮ�Ĳ�o���ƥ�
    public event Action OnBindingsChanged;

    void Awake()
    {
        // ��ҼҦ��]�m
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBindings();
            Debug.Log("[KeyBindingManager] ��l�Ƨ���");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �q PlayerPrefs ���J����]�w,�p�G�S���h�ϥιw�]��
    /// </summary>
    void LoadBindings()
    {
        foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
        {
            string key = "KeyBinding_" + action.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt(key);
                keyBindings[action] = savedKey;
            }
            else
            {
                keyBindings[action] = defaultBindings[action];
            }
        }
    }

    /// <summary>
    /// �x�s�Ҧ�����]�w�� PlayerPrefs
    /// </summary>
    public void SaveBindings()
    {
        foreach (var binding in keyBindings)
        {
            string key = "KeyBinding_" + binding.Key.ToString();
            PlayerPrefs.SetInt(key, (int)binding.Value);
        }
        PlayerPrefs.Save();
        Debug.Log("[KeyBindingManager] ����]�w�w�O�s");
    }

    /// <summary>
    /// ������w�ʧ@������N�X
    /// </summary>
    public KeyCode GetKeyCode(ActionType action)
    {
        return keyBindings.ContainsKey(action) ? keyBindings[action] : KeyCode.None;
    }

    /// <summary>
    /// �]�w���w�ʧ@������
    /// </summary>
    public void SetKey(ActionType action, KeyCode newKey)
    {
        keyBindings[action] = newKey;
        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log($"[KeyBindingManager] {GetActionName(action)} �]�w��: {newKey}");
    }

    /// <summary>
    /// ����ʧ@����ܦW��
    /// </summary>
    public string GetActionName(ActionType action)
    {
        return actionNames.ContainsKey(action) ? actionNames[action] : action.ToString();
    }

    /// <summary>
    /// �ˬd�Y�ӫ���O�_�w�Q��L�ʧ@�ϥ�
    /// </summary>
    public bool IsKeyUsed(KeyCode key, ActionType excludeAction)
    {
        foreach (var binding in keyBindings)
        {
            if (binding.Key != excludeAction && binding.Value == key)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ���m�Ҧ����䬰�w�]��
    /// </summary>
    public void ResetToDefault()
    {
        keyBindings = new Dictionary<ActionType, KeyCode>(defaultBindings);
        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log("[KeyBindingManager] �w���m���w�]����");
    }

    /// <summary>
    /// ����Ҧ��ʧ@����(�Ω� UI ���)
    /// </summary>
    public ActionType[] GetAllActions()
    {
        return (ActionType[])Enum.GetValues(typeof(ActionType));
    }

    /// <summary>
    /// �ˬd���w�ʧ@������O�_�Q���U(�榸)
    /// </summary>
    public bool GetKeyDown(ActionType action)
    {
        return Input.GetKeyDown(GetKeyCode(action));
    }

    /// <summary>
    /// �ˬd���w�ʧ@������O�_�������
    /// </summary>
    public bool GetKeyPressed(ActionType action)
    {
        return Input.GetKey(GetKeyCode(action));
    }

    /// <summary>
    /// �ˬd���w�ʧ@������O�_�Q��}
    /// </summary>
    public bool GetKeyUp(ActionType action)
    {
        return Input.GetKeyUp(GetKeyCode(action));
    }
}