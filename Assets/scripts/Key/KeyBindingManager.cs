using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按鍵綁定管理器 - 使用單例模式,管理所有按鍵設定
/// </summary>
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    // 定義所有可用的動作類型
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

    // 儲存每個動作對應的按鍵
    private Dictionary<ActionType, KeyCode> keyBindings = new Dictionary<ActionType, KeyCode>();

    // 預設按鍵設定
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

    // 動作的中文顯示名稱
    private Dictionary<ActionType, string> actionNames = new Dictionary<ActionType, string>()
    {
        { ActionType.MoveLeft, "向左移動" },
        { ActionType.MoveRight, "向右移動" },
        { ActionType.Jump, "跳躍" },
        { ActionType.OpenShop, "開關商店" },
        { ActionType.OpenInfo, "開關資訊" },
        { ActionType.Attack1, "攻擊1" },
        { ActionType.Attack2, "攻擊2" },
        { ActionType.Skill1, "技能1" }
    };

    // 當按鍵綁定改變時觸發的事件
    public event Action OnBindingsChanged;

    void Awake()
    {
        // 單例模式設置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBindings();
            Debug.Log("[KeyBindingManager] 初始化完成");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 從 PlayerPrefs 載入按鍵設定,如果沒有則使用預設值
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
    /// 儲存所有按鍵設定到 PlayerPrefs
    /// </summary>
    public void SaveBindings()
    {
        foreach (var binding in keyBindings)
        {
            string key = "KeyBinding_" + binding.Key.ToString();
            PlayerPrefs.SetInt(key, (int)binding.Value);
        }
        PlayerPrefs.Save();
        Debug.Log("[KeyBindingManager] 按鍵設定已保存");
    }

    /// <summary>
    /// 獲取指定動作的按鍵代碼
    /// </summary>
    public KeyCode GetKeyCode(ActionType action)
    {
        return keyBindings.ContainsKey(action) ? keyBindings[action] : KeyCode.None;
    }

    /// <summary>
    /// 設定指定動作的按鍵
    /// </summary>
    public void SetKey(ActionType action, KeyCode newKey)
    {
        keyBindings[action] = newKey;
        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log($"[KeyBindingManager] {GetActionName(action)} 設定為: {newKey}");
    }

    /// <summary>
    /// 獲取動作的顯示名稱
    /// </summary>
    public string GetActionName(ActionType action)
    {
        return actionNames.ContainsKey(action) ? actionNames[action] : action.ToString();
    }

    /// <summary>
    /// 檢查某個按鍵是否已被其他動作使用
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
    /// 重置所有按鍵為預設值
    /// </summary>
    public void ResetToDefault()
    {
        keyBindings = new Dictionary<ActionType, KeyCode>(defaultBindings);
        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log("[KeyBindingManager] 已重置為預設按鍵");
    }

    /// <summary>
    /// 獲取所有動作類型(用於 UI 顯示)
    /// </summary>
    public ActionType[] GetAllActions()
    {
        return (ActionType[])Enum.GetValues(typeof(ActionType));
    }

    /// <summary>
    /// 檢查指定動作的按鍵是否被按下(單次)
    /// </summary>
    public bool GetKeyDown(ActionType action)
    {
        return Input.GetKeyDown(GetKeyCode(action));
    }

    /// <summary>
    /// 檢查指定動作的按鍵是否持續按住
    /// </summary>
    public bool GetKeyPressed(ActionType action)
    {
        return Input.GetKey(GetKeyCode(action));
    }

    /// <summary>
    /// 檢查指定動作的按鍵是否被放開
    /// </summary>
    public bool GetKeyUp(ActionType action)
    {
        return Input.GetKeyUp(GetKeyCode(action));
    }
}