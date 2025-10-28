using UnityEngine;

public class Fireball2D : MonoBehaviour
{
    public int damage = 20;
    public float speed = 15f;
    public float lifetime = 4f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController2D>();
            if (player != null) player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }
    }
}
