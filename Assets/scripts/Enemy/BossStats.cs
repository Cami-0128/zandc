using UnityEngine;
using UnityEngine.UI;

public class BossStats : MonoBehaviour
{
    public int maxHealth = 10000;
    public int currentHealth;
    public Slider healthBar;
    public Transform canvasTransform;
    public Camera mainCamera;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        if (mainCamera == null) mainCamera = Camera.main;
    }
    void Update()
    {
        healthBar.value = currentHealth;
        canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - mainCamera.transform.position);
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        // 死亡處理可加動畫與特效
    }
}
