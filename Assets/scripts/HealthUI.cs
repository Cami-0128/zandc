using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI����")]
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
            // ��s��q��
            if (healthSlider != null)
            {
                healthSlider.value = playerAttack.currentHealth;
            }

            // ��s��q��r
            if (healthText != null)
            {
                healthText.text = $"{playerAttack.currentHealth:F0}/{playerAttack.maxHealth}";
            }
        }
    }
}