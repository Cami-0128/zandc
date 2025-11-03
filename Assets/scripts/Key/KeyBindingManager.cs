using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按鍵綁定管理器 - 單例模式
/// </summary>
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    /// <summary>
    /// 可綁定的動作類型
    /// </summary>
    public enum ActionType
    {
        MoveLeft,      // 向左移動
        MoveRight,     // 向右移動
        Jump,          // 跳躍
        OpenShop,      // 開關商店
        OpenInfo,      // 開關資訊
        Attack1,       // 攻擊1
        Attack2,       // 攻擊2
        Skill1         // 技能1
    }

    // 按鍵綁定字典
    private Dictionary<ActionType, KeyCode> keyBindings = new Dictionary<ActionType, KeyCode>();

    // 預設按鍵設定
    private Dictionary<ActionType, KeyCode> defaultBindings = new Dictionary<ActionType, KeyCode>()
    {
        { ActionType.MoveLeft, KeyCode.A },
        { ActionType.MoveRight, KeyCode.D },
        { ActionType.Jump, KeyCode.W },
        { ActionType.OpenShop, KeyCode.S },
        { ActionType.OpenInfo, KeyCode.I },
        { ActionType.Attack1, KeyCode.N },
        { ActionType.Attack2, KeyCode.M },
        { ActionType.Skill1, KeyCode.B }
    };

    // 動作名稱對應 (中文顯示)
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

    // 按鍵改變事件
    public event Action OnBindingsChanged;

    void Awake()
    {
        // 單例模式
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
    /// 載入按鍵設定 (從 PlayerPrefs 或使用預設值)
    /// </summary>
    void LoadBindings()
    {
        keyBindings.Clear();

        foreach (var action in defaultBindings.Keys)
        {
            string key = "KeyBinding_" + action.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                int keyCodeValue = PlayerPrefs.GetInt(key);
                keyBindings[action] = (KeyCode)keyCodeValue;
            }
            else
            {
                keyBindings[action] = defaultBindings[action];
            }
        }

        Debug.Log("[KeyBindingManager] 按鍵設定已載入");
    }

    /// <summary>
    /// 儲存按鍵設定到 PlayerPrefs
    /// </summary>
    void SaveBindings()
    {
        foreach (var binding in keyBindings)
        {
            string key = "KeyBinding_" + binding.Key.ToString();
            PlayerPrefs.SetInt(key, (int)binding.Value);
        }

        PlayerPrefs.Save();
        Debug.Log("[KeyBindingManager] 按鍵設定已儲存");
    }

    /// <summary>
    /// 設定按鍵
    /// </summary>
    public void SetKey(ActionType action, KeyCode key)
    {
        keyBindings[action] = key;
        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log($"[KeyBindingManager] {action} 設定為 {key}");
    }

    /// <summary>
    /// 獲取按鍵
    /// </summary>
    public KeyCode GetKeyCode(ActionType action)
    {
        if (keyBindings.ContainsKey(action))
        {
            return keyBindings[action];
        }
        return KeyCode.None;
    }

    /// <summary>
    /// 獲取動作名稱
    /// </summary>
    public string GetActionName(ActionType action)
    {
        if (actionNames.ContainsKey(action))
        {
            return actionNames[action];
        }
        return action.ToString();
    }

    /// <summary>
    /// 重置為預設按鍵
    /// </summary>
    public void ResetToDefault()
    {
        keyBindings.Clear();

        foreach (var binding in defaultBindings)
        {
            keyBindings[binding.Key] = binding.Value;
        }

        SaveBindings();
        OnBindingsChanged?.Invoke();
        Debug.Log("[KeyBindingManager] 已重置為預設按鍵");
    }

    /// <summary>
    /// 獲取所有動作類型
    /// </summary>
    public ActionType[] GetAllActions()
    {
        return (ActionType[])System.Enum.GetValues(typeof(ActionType));
    }

    /// <summary>
    /// 檢查按鍵是否按下 (替代 Input.GetKeyDown)
    /// </summary>
    public bool GetKeyDown(ActionType action)
    {
        if (keyBindings.ContainsKey(action))
        {
            return Input.GetKeyDown(keyBindings[action]);
        }
        return false;
    }

    /// <summary>
    /// 檢查按鍵是否持續按住 (替代 Input.GetKey)
    /// </summary>
    public bool GetKeyPressed(ActionType action)
    {
        if (keyBindings.ContainsKey(action))
        {
            return Input.GetKey(keyBindings[action]);
        }
        return false;
    }

    /// <summary>
    /// 檢查按鍵是否放開 (替代 Input.GetKeyUp)
    /// </summary>
    public bool GetKeyUp(ActionType action)
    {
        if (keyBindings.ContainsKey(action))
        {
            return Input.GetKeyUp(keyBindings[action]);
        }
        return false;
    }
}