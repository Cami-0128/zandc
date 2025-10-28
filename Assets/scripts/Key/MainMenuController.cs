//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;

//public class MainMenuController : MonoBehaviour
//{
//    [Header("UI References")]
//    public Button startGameButton;
//    public Button keyBindingButton;
//    public Button quitButton;
//    public GameObject mainMenuPanel;
//    public KeyBindingUI keyBindingUI;

//    [Header("場景設定")]
//    public string gameSceneName = "GameScene";

//    void Start()
//    {
//        // 確保主選單面板顯示，按鍵設定面板隱藏
//        if (mainMenuPanel != null)
//            mainMenuPanel.SetActive(true);

//        if (keyBindingUI != null)
//            keyBindingUI.HidePanel();

//        // 設定按鈕事件
//        if (startGameButton != null)
//            startGameButton.onClick.AddListener(OnStartGameClicked);

//        if (keyBindingButton != null)
//            keyBindingButton.onClick.AddListener(OnKeyBindingClicked);

//        if (quitButton != null)
//            quitButton.onClick.AddListener(OnQuitClicked);

//        Time.timeScale = 1f;
//    }

//    void OnStartGameClicked()
//    {
//        Debug.Log("[MainMenu] 開始遊戲");
//        SceneManager.LoadScene(gameSceneName);
//    }

//    void OnKeyBindingClicked()
//    {
//        if (keyBindingUI != null)
//        {
//            mainMenuPanel.SetActive(false);
//            keyBindingUI.ShowPanel();
//        }
//    }

//    void OnQuitClicked()
//    {
//        Debug.Log("[MainMenu] 退出遊戲");
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//    }

//    // 從按鍵設定返回主選單
//    public void ReturnToMainMenu()
//    {
//        if (keyBindingUI != null)
//            keyBindingUI.HidePanel();

//        if (mainMenuPanel != null)
//            mainMenuPanel.SetActive(true);
//    }
//}