using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 10f;
    public float damage = 50f;
    public float lifetime = 5f;

    [Header("視覺效果")]
    public ParticleSystem trailEffect;
    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f); // 淡藍色

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 設定子彈顏色
        GetComponent<SpriteRenderer>().color = bulletColor;

        // 自動銷毀
        Destroy(gameObject, lifetime);
    }
