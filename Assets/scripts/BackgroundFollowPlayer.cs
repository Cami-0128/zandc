using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFollowPlayer : MonoBehaviour  //廢
{
    public Transform player;                // 玩家
    public float backgroundWidth = 10f;     // 一張背景的寬度
    public Transform[] backgrounds;         // 所有背景物件

    void Update()
    {
        foreach (Transform bg in backgrounds)
        {
            // 如果背景離玩家太遠，就移動到最右邊
            if (player.position.x - bg.position.x > backgroundWidth)
            {
                bg.position += Vector3.right * backgroundWidth * backgrounds.Length;
            }
        }
    }
}