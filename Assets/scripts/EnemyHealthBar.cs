using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("血條設定")]
    public float barWidth = 1f;
    public float barHeight = 0.15f;
    public float borderWidth = 0.02f;

    [Header("顏色設定")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color borderColor = Color.black;

    private Canvas canvas;
    private Image borderImage;
    private Image fillImage;
    private float maxHealth;
    private float currentHealth;

    void Awake()
    {
        CreateHealthBar();
    }

    public void CreateHealthBar()
    {
        // 創建Canvas (World Space)
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 10;

        // 設定Canvas大小
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);

        // 創建邊框
        CreateBorder();

        // 創建填充
        CreateFill();
    }

    void CreateBorder()
    {
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(canvas.transform, false);

        borderImage = borderObj.AddComponent<Image>();
        borderImage.color = borderColor;

        RectTransform borderRect = borderImage.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;
    }

    void CreateFill()
    {
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvas.transform, false);

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullHealthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-borderWidth * 2, -borderWidth * 2);
        fillRect.anchoredPosition = Vector2.zero;
    }

    public void SetMaxHealth(float maxHP)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        UpdateHealthBar();
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Max(0, health);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (fillImage == null) return;

        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0;

        // 更新填充量
        fillImage.fillAmount = healthPercentage;

        // 更新顏色 (綠色到紅色漸變)
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);

        // 如果血量為0，隱藏血條
        if (currentHealth <= 0)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 讓血條始終面向攝影機
        if (Camera.main != null && canvas != null)
        {
            canvas.transform.LookAt(Camera.main.transform);
            canvas.transform.Rotate(0, 180, 0); // 翻轉以正確顯示
        }
    }

    void Update()
    {
        // 子彈向前飛行
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查是否擊中敵人
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // 創建擊中特效
            CreateHitEffect();

            // 銷毀子彈
            Destroy(gameObject);
        }

        // 擊中地面或牆壁也銷毀
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        // 這裡可以添加擊中特效
        // 例如粒子爆炸效果
    }
}