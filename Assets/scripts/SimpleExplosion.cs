using System.Collections;
using System.Collections.Generic;
// �Ыؤ@��²�檺�z���ĪG�}��
using UnityEngine;

public class SimpleExplosion : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        // ²�檺�Y��ʵe
        LeanTween.scale(gameObject, Vector3.one * 2f, 0.3f)
                 .setEase(LeanTweenType.easeOutBack);

        // �H�X�ĪG
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            LeanTween.alpha(gameObject, 0f, lifetime);
        }

        // �۰ʾP��
        Destroy(gameObject, lifetime);
    }
}