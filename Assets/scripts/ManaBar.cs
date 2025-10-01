using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("�]�O��UI�ե�")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private Image manaBarBackground;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("��ı�ĪG - �]�O���C�⺥��")]
    [Tooltip("���]�O�C�� (100%-76%)")]
    [SerializeField] private Color fullManaColor = new Color(0.2f, 0.5f, 1f); // �Ŧ�

    [Tooltip("���]�O�C�� (75%-51%)")]
    [SerializeField] private Color highManaColor = new Color(0.5f, 0.3f, 0.8f); // ����

    [Tooltip("���]�O�C�� (50%-26%)")]
    [SerializeField] private Color mediumManaColor = new Color(0.4f, 0.7f, 1f); // �L��

    [Tooltip("�C�]�O�C�� (25%-0%)")]
    [SerializeField] private Color lowManaColor = new Color(0.1f, 0.2f, 0.5f); // �`��

    [Header("���ƹL��]�w")]
    [Tooltip("�ҥ��C�⥭�ƹL��")]
    [SerializeField] private bool enableColorLerp = true;

    [Tooltip("�C��L��t��")]
    [SerializeField] private float colorTransitionSpeed = 5f;

    [Header("��ı�^�X")]
    [Tooltip("�ϥ��]�O�ɰ{�{�ĪG")]
    [SerializeField] private bool enableFlashOnUse = true;

    [Tooltip("�{�{����ɶ�")]
    [SerializeField] private float flashDuration = 0.2f;

    private PlayerAttack playerAttack;
    private Color currentColor;
    private Color targetColor;
    private int lastMana;
    private bool isFlashing = false;

    void Awake()
    {
        // �M�� PlayerAttack �ե�
        playerAttack = FindObjectOfType<PlayerAttack>();

        if (playerAttack == null)
        {
            Debug.LogError("�䤣�� PlayerAttack �ե�I�нT�O�����������a����ê��[ PlayerAttack �}���C");
            enabled = false;
            return;
        }

        // ���� UI �ե�
        if (manaBarFill == null || manaBarBackground == null || manaText == null)
        {
            Debug.LogError("ManaBarUI �ե󥼧���]�w�I�Цb Inspector �����w�Ҧ� UI ����C");
            enabled = false;
            return;
        }

        // ��l��
        lastMana = playerAttack.GetCurrentMana();
        UpdateManaBarImmediate();
    }

    void Update()
    {
        int currentMana = playerAttack.GetCurrentMana();

        // �˴��]�O����
        if (enableFlashOnUse && currentMana < lastMana && !isFlashing)
        {
            StartCoroutine(FlashEffect());
        }

        lastMana = currentMana;
        UpdateManaBar();
    }

    /// <summary>
    /// ��s�]�O���]���ƪ����^
    /// </summary>
    private void UpdateManaBar()
    {
        int currentMana = playerAttack.GetCurrentMana();
        int maxMana = playerAttack.maxMana;
        float currentManaPercentage = (float)currentMana / maxMana;

        // ��s��R�q
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = currentManaPercentage;
        }

        // ��s��r
        if (manaText != null)
        {
            manaText.text = currentMana + "/" + maxMana;
        }

        // ��s�C��
        targetColor = GetColorForManaPercentage(currentManaPercentage);

        if (enableColorLerp)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
        }
        else
        {
            currentColor = targetColor;
        }

        if (manaBarFill != null)
        {
            manaBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// �ߧY��s�]�O���]�L�ʵe�A�Ω��l�ơ^
    /// </summary>
    private void UpdateManaBarImmediate()
    {
        int currentMana = playerAttack.GetCurrentMana();
        int maxMana = playerAttack.maxMana;
        float currentManaPercentage = (float)currentMana / maxMana;

        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = currentManaPercentage;
        }

        if (manaText != null)
        {
            manaText.text = currentMana + "/" + maxMana;
        }

        currentColor = GetColorForManaPercentage(currentManaPercentage);
        targetColor = currentColor;

        if (manaBarFill != null)
        {
            manaBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// �ھ��]�O�ʤ�����������C��
    /// </summary>
    private Color GetColorForManaPercentage(float percentage)
    {
        if (percentage > 0.75f)
            return fullManaColor;
        else if (percentage > 0.50f)
            return highManaColor;
        else if (percentage > 0.25f)
            return mediumManaColor;
        else
            return lowManaColor;
    }

    /// <summary>
    /// �{�{�ĪG��{
    /// </summary>
    private System.Collections.IEnumerator FlashEffect()
    {
        isFlashing = true;
        Color originalColor = manaBarFill.color;

        // �ܥ�
        manaBarFill.color = Color.white;
        yield return new UnityEngine.WaitForSeconds(flashDuration / 2);

        // ��_���
        manaBarFill.color = originalColor;
        yield return new UnityEngine.WaitForSeconds(flashDuration / 2);

        isFlashing = false;
    }
}