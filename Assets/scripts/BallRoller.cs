using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRoller : MonoBehaviour
{
    public float moveSpeed = 4f;   //�y�t
    public float torque = 10f;   //��O

    private Rigidbody2D rb;

    public GameObject startPanel;   // �}�lUI ���O
    public bool canRoll = false; // ����O�_���\����

    void Start()
    {
        //Debug.Log("�y�ӤF~");
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // �۰��ˬd UI Panel�A�����N����ʡA�S���N���
        if (startPanel != null && startPanel.activeSelf)
        {
            canRoll = false;
        }
        else
        {
            canRoll = true;
        }

        if (!canRoll) return; // �@�}�l�������u
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        rb.AddTorque(-torque);
    }

    //// �����I���]�Y��L���⥼�]�� trigger�^       //����PlayerController2D.cs
    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        PauseGame();
    //    }
    //}

    //// �Y�ϥ� trigger�]�I������Ŀ� isTrigger�^
    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Enemy"))
    //    {
    //        PauseGame();
    //    }
    //}

    void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Paused - Ball hit enemy.");
    }
}
