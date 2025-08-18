using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour     //Square
{
    public GameObject gameCompleteUI; // ���w UI Canvas �̪����O
    private bool gameEnded = false; // �����Ĳ�o

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !gameEnded) // ���ʨ���n�] tag �� "Player"
        {
            gameEnded = true;

            // �ϥΧA�� PlayerController2D ���� canControl �ܼ�
            PlayerController2D playerController = other.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = false; // �T��a����
            }

            // ����a����
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero; // �����e����
            }

            // �Ȱ��C���ɶ�
            Time.timeScale = 0f;

            // ��ܧ��� UI
            gameCompleteUI.SetActive(true);

            // �i��G���񭵮ĩΨ�L�ĪG
            // AudioSource.PlayClipAtPoint(completionSound, transform.position);
        }
    }

    // �p�G�ݭn��_�C������k�]��p���s�}�l���s�եΡ^
    public void ResumeGame()
    {
        gameEnded = false;
        Time.timeScale = 1f;
        gameCompleteUI.SetActive(false);

        // ���s�ҥΪ��a����
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController2D playerController = player.GetComponent<PlayerController2D>();
            if (playerController != null)
            {
                playerController.canControl = true; // ��_���a����
            }
        }
    }

    // ���s�}�l�C������k
    public void RestartGame()
    {
        Time.timeScale = 1f; // �T�O�ɶ���_���`
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}