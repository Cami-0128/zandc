using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishPlatform : MonoBehaviour
{   //�������a�O
    public float requiredStayTime = 3f; // ���ߴX������
    public float respawnTime = 5f;      // �X���_��

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
            stayTimer = 0f; // �}�l�p��
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnPlatform = false;
            stayTimer = 0f; // ���}�N���m
        }
    }

    System.Collections.IEnumerator VanishAndRespawn()
    {
        isVanishing = true;

        // �������x�]�����~�[�M�I���^
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
        //GameObject.Find("Triangle").GetComponent<VanishTriangle>().Vanish(); // �� �u�� Triangle ���@��
        //GameObject.Find("Triangle (1)").GetComponent<VanishTriangle>().Vanish();
        //GameObject.Find("Triangle (2)").GetComponent<VanishTriangle>().Vanish();
        //FindObjectOfType<VanishTriangle>().Vanish();      //�o�|�泣�h�֦����D ���n�Ϋ�

        yield return new WaitForSeconds(respawnTime);

        // �^���o��
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
        //GameObject.Find("Triangle").GetComponent<VanishTriangle>().Appear(); // �� �u�� Triangle ���@��
        //GameObject.Find("Triangle (1)").GetComponent<VanishTriangle>().Appear();
        //GameObject.Find("Triangle (2)").GetComponent<VanishTriangle>().Appear();
        //FindObjectOfType<VanishTriangle>().Appear();      //�o�|�泣�h�֦����D ���n�Ϋ�
    }
}
