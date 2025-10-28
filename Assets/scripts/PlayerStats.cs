using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public float invincibleCooldown = 0.15f; // 基本連擊防護
    float lastDamageTime = -999f;
    public event Action<int, int> onHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < invincibleCooldown)
            return;
        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"玩家受傷 {damage} -> {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    void Die()
    {
        Debug.Log("玩家死亡");
        // 死亡流程
    }
}
