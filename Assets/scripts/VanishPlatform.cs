using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishPlatform : MonoBehaviour
{   //消失的地板
    public float requiredStayTime = 3f; // 站立幾秒後消失
    public float respawnTime = 5f;      // 幾秒後復活

    public GameObject[] triangleObjects;

    private float stayTimer = 0f;
    private bool playerOnPlatform = false;
    private bool isVanishing = false;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (playerOnPlatform && !isVanishing)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= requiredStayTime)
            {
                StartCoroutine(VanishAndRespawn());
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnPlatform = true;
            stayTimer = 0f; // 開始計時
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnPlatform = false;
            stayTimer = 0f; // 離開就重置
        }
    }

    System.Collections.IEnumerator VanishAndRespawn()
    {
        isVanishing = true;

        // 消失平台（關掉外觀和碰撞）
        sr.enabled = false;
        col.enabled = false;
        foreach (GameObject go in triangleObjects)
        {
            if (go != null)
            {
                VanishTriangle vt = go.GetComponent<VanishTriangle>();
                if (vt != null) vt.Vanish();
            }
        }
        //GameObject.Find("Triangle").GetComponent<VanishTriangle>().Vanish(); // ← 只對 Triangle 有作用
        //GameObject.Find("Triangle (1)").GetComponent<VanishTriangle>().Vanish();
        //GameObject.Find("Triangle (2)").GetComponent<VanishTriangle>().Vanish();
        //FindObjectOfType<VanishTriangle>().Vanish();      //這四行都多少有問題 不要用哈

        yield return new WaitForSeconds(respawnTime);

        // 回來囉～
        sr.enabled = true;
        col.enabled = true;
        isVanishing = false;
        foreach (GameObject go in triangleObjects)
        {
            if (go != null)
            {
                VanishTriangle vt = go.GetComponent<VanishTriangle>();
                if (vt != null) vt.Appear();
            }
        }
        //GameObject.Find("Triangle").GetComponent<VanishTriangle>().Appear(); // ← 只對 Triangle 有作用
        //GameObject.Find("Triangle (1)").GetComponent<VanishTriangle>().Appear();
        //GameObject.Find("Triangle (2)").GetComponent<VanishTriangle>().Appear();
        //FindObjectOfType<VanishTriangle>().Appear();      //這四行都多少有問題 不要用哈
    }
}
