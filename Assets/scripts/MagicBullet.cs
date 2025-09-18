using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("�l�u�]�w")]
    public float speed = 10f;
    public float damage = 50f;
    public float lifetime = 5f;

    [Header("��ı�ĪG")]
    public ParticleSystem trailEffect;
    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f); // �H�Ŧ�

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // �]�w�l�u�C��
        GetComponent<SpriteRenderer>().color = bulletColor;

        // �۰ʾP��
        Destroy(gameObject, lifetime);
    }
