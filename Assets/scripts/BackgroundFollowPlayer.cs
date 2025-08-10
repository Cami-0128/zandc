using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollowPlayer : MonoBehaviour  //�o
{
    public Transform player;                // ���a
    public float backgroundWidth = 10f;     // �@�i�I�����e��
    public Transform[] backgrounds;         // �Ҧ��I������

    void Update()
    {
        foreach (Transform bg in backgrounds)
        {
            // �p�G�I�������a�ӻ��A�N���ʨ�̥k��
            if (player.position.x - bg.position.x > backgroundWidth)
            {
                bg.position += Vector3.right * backgroundWidth * backgrounds.Length;
            }
        }
    }
}