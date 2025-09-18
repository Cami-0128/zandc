using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int value = 1;  // ��������

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CoinManager.Instance.AddMoney(value);
            Destroy(gameObject);
        }
    }
}
