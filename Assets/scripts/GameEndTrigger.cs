using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    public GameObject gameCompleteUI;
    private bool gameEnded = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !gameEnded)
        {
            gameEnded = true;

            PlayerController2D playerController = other.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = false;
                playerController.hasReachedEnd = true;  // �s�W�G�i�D���a�w��F���I
            }

            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }

            Time.timeScale = 0f;
            gameCompleteUI.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        gameEnded = false;
        Time.timeScale = 1f;
        gameCompleteUI.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController2D playerController = player.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = true;
                playerController.hasReachedEnd = false; // ���s�}�l�᭫�m���A
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
