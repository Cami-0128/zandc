using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour     //Square
{
    public GameObject gameCompleteUI; // ���w UI Canvas �̪����O

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // ���ʨ���n�] tag �� "Player"
        {
            Time.timeScale = 0f; // �Ȱ��C��
            gameCompleteUI.SetActive(true); // ��� UI
            Time.timeScale = 0f; // �Ȱ��C��
        }
    }
}

