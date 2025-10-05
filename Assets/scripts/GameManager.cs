// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject deathUI;
    public GameObject completeUI;

    public ShopManager shopManager;

    public void PlayerDied()
    {
        Time.timeScale = 0f;
        if (!completeUI.activeSelf)
        {
            deathUI.SetActive(true);
            //if (shopManager != null)
            //    shopManager.ForceCloseShop();  //ForceCloseShop()已刪
            Debug.Log("Game Paused 遊戲暫停 - 玩家死亡");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
