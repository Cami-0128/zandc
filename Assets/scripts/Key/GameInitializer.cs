//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class GameInitializer : MonoBehaviour
//{
//    [Header("����]�w")]
//    public KeyBindingUI keyBindingUI;
//    public bool showKeyBindingOnFirstLaunch = true;

//    [Header("�C���}�l�]�w")]
//    public float delayBeforeGameStart = 0.5f;

//    private bool hasShownKeyBinding = false;

//    void Start()
//    {
//        // �T�O KeyBindingManager �s�b
//        if (KeyBindingManager.Instance == null)
//        {
//            GameObject manager = new GameObject("KeyBindingManager");
//            manager.AddComponent<KeyBindingManager>();
//            Debug.Log("[GameInitializer] �w�۰ʳЫ� KeyBindingManager");
//        }

//        // �ˬd�O�_�O�Ĥ@���Ұ�
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
//            Debug.LogWarning("[GameInitializer] KeyBindingUI ���]�w�A���L����]�w�ɭ�");
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
//        Debug.Log("[GameInitializer] �C���}�l");
//    }

//    // �ѥ~���եΡG���m�Ĥ@���ҰʼаO�]�Ω���ա^
//    public void ResetFirstLaunchFlag()
//    {
//        PlayerPrefs.DeleteKey("HasLaunchedBefore");
//        PlayerPrefs.Save();
//        Debug.Log("[GameInitializer] �w���m�Ĥ@���ҰʼаO");
//    }
//}