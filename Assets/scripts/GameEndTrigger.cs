using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour     //Square
{
    public GameObject gameCompleteUI; // 指定 UI Canvas 裡的面板
    private bool gameEnded = false; // 防止重複觸發

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !gameEnded) // 移動角色要設 tag 為 "Player"
        {
            gameEnded = true;

            // 使用你的 PlayerController2D 中的 canControl 變數
            PlayerController2D playerController = other.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = false; // 禁止玩家控制
            }

            // 停止玩家移動
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero; // 停止當前移動
            }

            // 暫停遊戲時間
            Time.timeScale = 0f;

            // 顯示完成 UI
            gameCompleteUI.SetActive(true);

            // 可選：播放音效或其他效果
            // AudioSource.PlayClipAtPoint(completionSound, transform.position);
        }
    }

    // 如果需要恢復遊戲的方法（比如重新開始按鈕調用）
    public void ResumeGame()
    {
        gameEnded = false;
        Time.timeScale = 1f;
        gameCompleteUI.SetActive(false);

        // 重新啟用玩家控制
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController2D playerController = player.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = true; // 恢復玩家控制
            }
        }
    }

    // 重新開始遊戲的方法
    public void RestartGame()
    {
        Time.timeScale = 1f; // 確保時間恢復正常
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}