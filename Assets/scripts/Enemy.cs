using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI元素")]
    public Slider healthSlider;
    public Text healthText;

    private PlayerAttack playerAttack;

    void Start()
    {
        playerAttack = FindObjectOfType<PlayerAttack>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerAttack.maxHealth;
        }
    }

    void Update()
    {
        if (playerAttack != null)
        {
            // 更新血量條
            if (healthSlider != null)
            {
                healthSlider.value = playerAttack.currentHealth;
            }

            // 更新血量文字
            if (healthText != null)
            {
                healthText.text = $"{playerAttack.currentHealth:F0}/{playerAttack.maxHealth}";
            }
        }
    }
}