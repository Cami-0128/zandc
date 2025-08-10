using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManger : MonoBehaviour
{
    public GameObject startPanel; // �]�t���s�� UI Panel
    public PlayerController2D player;
    public BallRoller ball;

    void Start()
    {
        // �}�l����� UI �üȰ��C��
        startPanel.SetActive(true);
        Time.timeScale = 0f; // �Ȱ��ɶ�
        player.canControl = false; //
        ball.canRoll = false;          // 
    }

    public void OnStartButtonClicked()
    {
        // �I�����s������ UI�A��_�C��
        startPanel.SetActive(false);
        Time.timeScale = 1f; // ��_�ɶ�

        player.canControl = true; // �i�H�ʤF
        ball.canRoll = true;          // �y�}�l�u��
    }
}
