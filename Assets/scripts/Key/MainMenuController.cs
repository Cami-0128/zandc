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

//    [Header("�����]�w")]
//    public string gameSceneName = "GameScene";

//    void Start()
//    {
//        // �T�O�D��歱�O��ܡA����]�w���O����
//        if (mainMenuPanel != null)
//            mainMenuPanel.SetActive(true);

//        if (keyBindingUI != null)
//            keyBindingUI.HidePanel();

//        // �]�w���s�ƥ�
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
//        Debug.Log("[MainMenu] �}�l�C��");
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
//        Debug.Log("[MainMenu] �h�X�C��");
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//    }

//    // �q����]�w��^�D���
//    public void ReturnToMainMenu()
//    {
//        if (keyBindingUI != null)
//            keyBindingUI.HidePanel();

//        if (mainMenuPanel != null)
//            mainMenuPanel.SetActive(true);
//    }
//}