using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRoller : MonoBehaviour
{
    public float moveSpeed = 4f;   //球速
    public float torque = 10f;   //扭力

    private Rigidbody2D rb;

    public GameObject startPanel;   // 開始UI 面板
    public bool canRoll = false; // 控制是否允許移動

    void Start()
    {
        //Debug.Log("球來了~");
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 自動檢查 UI Panel，有關就不能動，沒關就能動
        if (startPanel != null && startPanel.activeSelf)
        {
            canRoll = false;
        }
        else
        {
            canRoll = true;
        }

        if (!canRoll) return; // 一開始不讓它滾
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        rb.AddTorque(-torque);
    }

    //// 偵測碰撞（若其他角色未設為 trigger）       //移到PlayerController2D.cs
    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        PauseGame();
    //    }
    //}

    //// 若使用 trigger（碰撞物體勾選 isTrigger）
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
