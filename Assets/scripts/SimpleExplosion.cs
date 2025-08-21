using System.Collections;
using System.Collections.Generic;
// 創建一個簡單的爆炸效果腳本
using UnityEngine;

public class SimpleExplosion : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        // 簡單的縮放動畫
        LeanTween.scale(gameObject, Vector3.one * 2f, 0.3f)
                 .setEase(LeanTweenType.easeOutBack);

        // 淡出效果
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            LeanTween.alpha(gameObject, 0f, lifetime);
        }

        // 自動銷毀
        Destroy(gameObject, lifetime);
    }
}