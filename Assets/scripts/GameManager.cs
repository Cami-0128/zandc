using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject deathUI;
    public GameObject completeUI;
    public ShopManager shopManager;

    [Header("Scene Settings")]
    public string restartSceneName = "Level0.5";

    public void PlayerDied()
    {
        Time.timeScale = 0f;

        if (!completeUI.activeSelf)
        {
            deathUI.SetActive(true);

            if (shopManager != null)
                shopManager.ForceCloseShop();
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(restartSceneName);
    }
}
