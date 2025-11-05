using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StartManger : MonoBehaviour  //未和自定義按鍵功能結合
{
    public GameObject startPanel; // 包含按鈕的 UI Panel
    public PlayerController2D player;
    public BallRoller ball;
    void Start()
    {
        // 開始時顯示 UI 並暫停遊戲
        startPanel.SetActive(true);
        Time.timeScale = 0f; // 暫停時間
        player.canControl = false; //
        ball.canRoll = false;          // 
    }
    public void OnStartButtonClicked()
    {
        // 點擊按鈕時隱藏 UI，恢復遊戲
        startPanel.SetActive(false);
        Time.timeScale = 1f; // 恢復時間
        player.canControl = true; // 可以動了
        ball.canRoll = true;          // 球開始滾動
    }
}