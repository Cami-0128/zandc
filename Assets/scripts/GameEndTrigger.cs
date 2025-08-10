using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour     //Square
{
    public GameObject gameCompleteUI; // 指定 UI Canvas 裡的面板

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 移動角色要設 tag 為 "Player"
        {
            Time.timeScale = 0f; // 暫停遊戲
            gameCompleteUI.SetActive(true); // 顯示 UI
            Time.timeScale = 0f; // 暫停遊戲
        }
    }
}

