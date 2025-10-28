using UnityEngine;

public class HazardZone2D : MonoBehaviour
{
    public int damage = 20;
    public float damageInterval = 0.5f;
    private float timer = 0f;

    void Start()
    {
        Debug.Log("HazardZone�w�ͦ�");
        Destroy(gameObject, 3f);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                var player = other.GetComponent<PlayerController2D>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Debug.Log("HazardZone�缾�a�y���ˮ`");
                }
                timer = 0f;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            timer = 0f;
    }
}
