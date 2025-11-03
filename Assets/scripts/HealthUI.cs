using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI元素")]
    public Slider healthSlider;
    public Text healthText;

    private PlayerController2D playerController;

    void Start()
    {
        // 改為從 PlayerController2D 取得血量
        playerController = FindObjectOfType<PlayerController2D>();

        if (playerController == null)
        {
            Debug.LogError("[HealthUI] 找不到 PlayerController2D!");
            return;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerController.maxHealth;
            healthSlider.value = playerController.currentHealth;
        }
    }

    void Update()
    {
        if (playerController != null)
        {
            // 更新血量條
            if (healthSlider != null)
            {
                healthSlider.value = playerController.currentHealth;
            }

            // 更新血量文字
            if (healthText != null)
            {
                healthText.text = $"{playerController.currentHealth}/{playerController.maxHealth}";
            }
        }
    }
}