using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject deathUI;
    public GameObject completeUI;

    public void PlayerDied()
    {
        Time.timeScale = 0f;
        if (!completeUI.activeSelf)
        {
            deathUI.SetActive(true);
            Debug.Log("Game Paused �C���Ȱ� - ���a���`");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}


