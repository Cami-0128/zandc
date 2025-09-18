using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    public int value = 1; // �������ȡA�i�H�ۥѽվ�

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CoinManager.Instance.AddMoney(value);
            Destroy(gameObject);
        }
    }
}
