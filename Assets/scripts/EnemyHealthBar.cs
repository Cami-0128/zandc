using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("����]�w")]
    public float barWidth = 1f;
    public float barHeight = 0.15f;
    public float borderWidth = 0.02f;

    [Header("�C��]�w")]
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
        // �Ы�Canvas (World Space)
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 10;

        // �]�wCanvas�j�p
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);

        // �Ы����
        CreateBorder();

        // �Ыض�R
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

        // ��s��R�q
        fillImage.fillAmount = healthPercentage;

        // ��s�C�� (������⺥��)
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);

        // �p�G��q��0�A���æ��
        if (currentHealth <= 0)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // ������l�׭��V��v��
        if (Camera.main != null && canvas != null)
        {
            canvas.transform.LookAt(Camera.main.transform);
            canvas.transform.Rotate(0, 180, 0); // ½��H���T���
        }
    }

    void Update()
    {
        // �l�u�V�e����
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �ˬd�O�_�����ĤH
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // �Ы������S��
            CreateHitEffect();

            // �P���l�u
            Destroy(gameObject);
        }

        // �����a��������]�P��
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        // �o�̥i�H�K�[�����S��
        // �Ҧp�ɤl�z���ĪG
    }
}