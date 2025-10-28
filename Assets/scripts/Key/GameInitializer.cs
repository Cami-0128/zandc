//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class GameInitializer : MonoBehaviour
//{
//    [Header("按鍵設定")]
//    public KeyBindingUI keyBindingUI;
//    public bool showKeyBindingOnFirstLaunch = true;

//    [Header("遊戲開始設定")]
//    public float delayBeforeGameStart = 0.5f;

//    private bool hasShownKeyBinding = false;

//    void Start()
//    {
//        // 確保 KeyBindingManager 存在
//        if (KeyBindingManager.Instance == null)
//        {
//            GameObject manager = new GameObject("KeyBindingManager");
//            manager.AddComponent<KeyBindingManager>();
//            Debug.Log("[GameInitializer] 已自動創建 KeyBindingManager");
//        }

//        // 檢查是否是第一次啟動
//        bool isFirstLaunch = PlayerPrefs.GetInt("HasLaunchedBefore", 0) == 0;

//        if (showKeyBindingOnFirstLaunch && isFirstLaunch)
//        {
//            ShowKeyBindingUI();
//            PlayerPrefs.SetInt("HasLaunchedBefore", 1);
//            PlayerPrefs.Save();
//        }
//        else
//        {
//            StartGame();
//        }
//    }

//    void ShowKeyBindingUI()
//    {
//        if (keyBindingUI != null)
//        {
//            keyBindingUI.ShowPanel();
//            hasShownKeyBinding = true;
//        }
//        else
//        {
//            Debug.LogWarning("[GameInitializer] KeyBindingUI 未設定，跳過按鍵設定界面");
//            StartGame();
//        }
//    }

//    public void StartGame()
//    {
//        Invoke(nameof(StartGameDelayed), delayBeforeGameStart);
//    }

//    void StartGameDelayed()
//    {
//        Time.timeScale = 1f;
//        Debug.Log("[GameInitializer] 遊戲開始");
//    }

//    // 供外部調用：重置第一次啟動標記（用於測試）
//    public void ResetFirstLaunchFlag()
//    {
//        PlayerPrefs.DeleteKey("HasLaunchedBefore");
//        PlayerPrefs.Save();
//        Debug.Log("[GameInitializer] 已重置第一次啟動標記");
//    }
//}