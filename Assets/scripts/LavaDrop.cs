// === 3. 岩漿滴腳本（保持原有設計） ===
using System.Collections;
using UnityEngine;

public class LavaDrop : MonoBehaviour
{
    [Header("岩漿滴設定")]
    public int damage = 20;
    public float lifetime = 5f;
    public float groundBurnTime = 3f;
    public GameObject explosionEffect;
    public AudioClip hitSound;

    [Header("視覺效果")]
    public Color hotColor = Color.red;
    public Color coolColor = Color.yellow;
    public TrailRenderer trail;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool hasHitGround = false;
    private bool hasHitPlayer = false;
    private float startTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        startTime = Time.time;

        if (spriteRenderer != null)
            spriteRenderer.color = hotColor;

        if (trail != null)
        {
            trail.startColor = hotColor;
            trail.endColor = Color.clear;
        }

        StartCoroutine(LifetimeCountdown());
    }

    void Update()
    {
        if (spriteRenderer != null && !hasHitGround)
        {
            float t = (Time.time - startTime) / lifetime;
            spriteRenderer.color = Color.Lerp(hotColor, coolColor, t);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasHitPlayer)
        {
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasHitPlayer = true;
                Debug.Log($"岩漿滴對玩家造成 {damage} 點傷害！");
            }

            CreateExplosion();
            DestroySelf();
        }
        else if (collision.gameObject.CompareTag("Ground") && !hasHitGround)
        {
            HitGround();
        }
    }

    void HitGround()
    {
        hasHitGround = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        CreateExplosion();
        StartCoroutine(GroundBurn());
    }

    IEnumerator GroundBurn()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0f, 1f); // 橘色 (R=1, G=0.5, B=0, A=1)
        }

        if (trail != null)
        {
            trail.enabled = false;
        }

        float burnTimer = 0f;
        while (burnTimer < groundBurnTime)
        {
            burnTimer += Time.deltaTime;

            if (spriteRenderer != null)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * 10f);
                Color currentColor = spriteRenderer.color;
                currentColor.a = alpha;
                spriteRenderer.color = currentColor;
            }

            yield return null;
        }

        DestroySelf();
    }

    void CreateExplosion()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
    }

    IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        if (!hasHitGround)
        {
            DestroySelf();
        }
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasHitPlayer = true;
            }

            CreateExplosion();
            DestroySelf();
        }
    }
}