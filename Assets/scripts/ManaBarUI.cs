using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("�]�O��UI�ե�")]
    public Image manaBarFill;
    public Image manaBarBackground;
    public TextMeshProUGUI manaText;

    [Header("��ı�ĪG - �]�O���C�⺥��")]
    [Tooltip("100-76% �]�O�ɪ��C��")]
    public Color fullManaColor = new Color(0.2f, 0.5f, 1f);      // �Ŧ�

    [Tooltip("75-51% �]�O�ɪ��C��")]
    public Color highManaColor = new Color(0.5f, 0.3f, 0.8f);    // ����

    [Tooltip("50-26% �]�O�ɪ��C��")]
    public Color mediumManaColor = new Color(0.4f, 0.7f, 1f);    // �L��

    [Tooltip("25-0% �]�O�ɪ��C��")]
    public Color lowManaColor = new Color(0.1f, 0.2f, 0.5f);     // �`��

    [Header("�ʵe�]�w")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;
    public bool enableFlashOnUse = true;
    public Color useFlashColor = Color.cyan;
    public float flashDuration = 0.15f;

    [Header("�C�⺥�ܳ]�w")]
    public bool enableColorLerp = true;
    public float colorTransitionSpeed = 3f;

    private PlayerAttack playerAttack;
    private float targetFillAmount;
    private float currentFillAmount;
    private int lastKnownMana = -1;
    private Color currentColor;
    private Color targetColor;
    private bool isFlashing = false;
    private bool isInitialized = false;
    private Coroutine flashCoroutine;

    // === ����G�b Editor ���N��ܺ������A ===
    void OnValidate()
    {
        // �o�Ӥ�k�|�b Inspector ���ק����Ȯɰ���
        if (manaBarFill != null)
        {
            manaBarFill.type = Image.Type.Filled;
            manaBarFill.fillMethod = Image.FillMethod.Horizontal;
            manaBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            manaBarFill.fillAmount = 1f;  // �w�]��ܺ���
            manaBarFill.color = fullManaColor;
        }

        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Awake()
    {
        // === �b Start ���e�N�]�w�n��l�ȡA�קK�{�{ ===
        currentFillAmount = 1f;
        targetFillAmount = 1f;
        currentColor = fullManaColor;
        targetColor = fullManaColor;

        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = 1f;
            manaBarFill.color = fullManaColor;
        }

        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Start()
    {
        StartCoroutine(InitializeManaBar());
    }

    // === ��l�ƨ�{�]�ѦҧA�� HP ���g�k�^===
    IEnumerator InitializeManaBar()
    {
        // ���ݤ@�V�A�T�O���a�w�g��l��
        yield return new WaitForEndOfFrame();

        // �M�䪱�a�����ե�
        playerAttack = FindObjectOfType<PlayerAttack>();

        // ���ݪ��a���J
        int attempts = 0;
        while (playerAttack == null && attempts < 100)
        {
            yield return new WaitForEndOfFrame();
            playerAttack = FindObjectOfType<PlayerAttack>();
            attempts++;
        }

        if (playerAttack != null)
        {
            // �ھڪ��a������]�O��l���]�O��
            int currentMana = playerAttack.GetCurrentMana();
            int maxMana = playerAttack.maxMana;

            Debug.Log($"[�]�O����l��] ���J���a�]�O: {currentMana}/{maxMana}");

            // �p���]�O�ʤ���
            float manaPercentage = (float)currentMana / maxMana;

            // �]�m�]�O��
            currentFillAmount = manaPercentage;
            targetFillAmount = manaPercentage;

            if (manaBarFill != null)
            {
                manaBarFill.fillAmount = manaPercentage;
                UpdateManaBarColor(manaPercentage);
                Debug.Log($"�]�O����l�Ƨ��� - �]�O: {currentMana}/{maxMana} ({manaPercentage:P0})");
            }

            // ��s�]�O��r
            UpdateManaText(currentMana, maxMana);

            // �O����l�]�O
            lastKnownMana = currentMana;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("�䤣�� PlayerAttack �ե�I�]�O���L�k���T��l�ơC");

            // �ƥΤ�סG�]�����]�O
            if (manaBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                manaBarFill.fillAmount = 1f;
                manaBarFill.color = fullManaColor;
                Debug.Log("�ϥγƥΪ�l�Ƥ�סA�]�m�]�O�������]�O");
            }
        }
    }

    void Update()
    {
        // �۰ʦP�B�]�O
        if (isInitialized)
        {
            AutoSyncWithPlayer();
        }

        // ���ƹL��ʵe
        if (enableSmoothTransition && manaBarFill != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
                manaBarFill.fillAmount = currentFillAmount;

                // �T�O�]�O��0�ɧ�������
                if (targetFillAmount <= 0f && currentFillAmount < 0.01f)
                {
                    currentFillAmount = 0f;
                    manaBarFill.fillAmount = 0f;
                }
            }
        }

        // ���Ƨ�s�C��
        if (enableColorLerp && manaBarFill != null && !isFlashing)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            manaBarFill.color = currentColor;
        }
    }

    // === �۰ʦP�B���a�]�O ===
    void AutoSyncWithPlayer()
    {
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();

            // �p�G�]�O���ܤơA�۰ʧ�s�]�O��
            if (currentMana != lastKnownMana)
            {
                int maxMana = playerAttack.maxMana;
                Debug.Log($"[�۰ʦP�B] �˴����]�O�ܤ�: {lastKnownMana} �� {currentMana}");
                UpdateManaBar(currentMana, maxMana);
                lastKnownMana = currentMana;
            }
        }
    }

    // === �D�n�\��G��s�]�O�� ===
    public void UpdateManaBar(int currentMana, int maxMana)
    {
        Debug.Log($"ManaBarUI.UpdateManaBar �Q�I�s - �]�O: {currentMana}/{maxMana}");

        if (manaBarFill == null)
        {
            Debug.LogError("�]�O����R�Ϥ����]�m�I");
            return;
        }

        // �p���]�O�ʤ���
        float manaPercentage = (float)currentMana / maxMana;
        Debug.Log($"�]�O�ʤ���: {manaPercentage:F2} ({manaPercentage:P0})");

        // �˴��O�_�ϥ��]�O�]�Ω�{�{�ĪG�^
        bool usedMana = (lastKnownMana > 0 && currentMana < lastKnownMana);

        // �]�m�ؼж�R�q
        targetFillAmount = manaPercentage;

        // �p�G���ϥΥ��ƹL��A�����]�m
        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            manaBarFill.fillAmount = currentFillAmount;
        }

        // �j��]�m�]�O��0�ɪ���R�q
        if (currentMana <= 0)
        {
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            manaBarFill.fillAmount = 0f;
        }

        // ��s�]�O���C��
        UpdateManaBarColor(manaPercentage);

        // ��s�]�O��r
        UpdateManaText(currentMana, maxMana);

        // �u���ϥ��]�O�ɤ~����{�{�ĪG
        if (enableFlashOnUse && usedMana)
        {
            PlayManaFlash();
            Debug.Log($"�˴���ϥ��]�O�A����{�{�ĪG ({lastKnownMana} �� {currentMana})");
        }

        // ��s�O�����]�O
        lastKnownMana = currentMana;

        // �аO���w��l��
        if (!isInitialized)
        {
            isInitialized = true;
        }
    }

    // === ��s�]�O���C�� ===
    void UpdateManaBarColor(float manaPercentage)
    {
        if (manaBarFill == null) return;

        Color newTargetColor;
        string colorName;

        if (manaPercentage > 0.75f)
        {
            newTargetColor = fullManaColor;
            colorName = "�Ŧ�";
        }
        else if (manaPercentage > 0.5f)
        {
            newTargetColor = highManaColor;
            colorName = "����";
        }
        else if (manaPercentage > 0.25f)
        {
            newTargetColor = mediumManaColor;
            colorName = "�L��";
        }
        else
        {
            newTargetColor = lowManaColor;
            colorName = "�`��";
        }

        targetColor = newTargetColor;

        if (!enableColorLerp)
        {
            currentColor = targetColor;
            manaBarFill.color = targetColor;
        }

        Debug.Log($"�]�O���C���ܧ󬰡G{colorName} (�]�O�ʤ���: {manaPercentage:P0})");
    }

    // === ��s�]�O��r ===
    void UpdateManaText(int currentMana, int maxMana)
    {
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }

    // === ����ϥ��]�O�{�{�ĪG ===
    void PlayManaFlash()
    {
        // �p�G�w�g�b�{�{�A������
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (manaBarFill != null)
        {
            flashCoroutine = StartCoroutine(ManaFlashCoroutine());
            Debug.Log("�}�l�]�O�ϥΰ{�{�ĪG");
        }
    }

    // === �{�{��{ ===
    IEnumerator ManaFlashCoroutine()
    {
        if (manaBarFill == null)
        {
            Debug.LogWarning("�{�{�ĪG�G�䤣���]�O����R�Ϥ�");
            yield break;
        }

        isFlashing = true;

        // �O���Ӫ��C��
        Color originalColor = currentColor;

        // �ܦ��{�{�C��
        manaBarFill.color = useFlashColor;

        // ���ݰ{�{�ɶ�
        yield return new WaitForSeconds(flashDuration);

        // ��_��Ӫ��C��
        manaBarFill.color = originalColor;

        isFlashing = false;
        flashCoroutine = null;
        Debug.Log("�{�{�ĪG�����A��_��l�C��");
    }

    // === �]�O�����m ===
    public void ResetManaBar()
    {
        PlayerAttack player = FindObjectOfType<PlayerAttack>();

        if (player != null)
        {
            int currentMana = player.GetCurrentMana();
            int maxMana = player.maxMana;

            Debug.Log($"[�]�O�����m] ���m�����a��e�]�O: {currentMana}/{maxMana}");
            UpdateManaBar(currentMana, maxMana);
        }
        else
        {
            // �ƥΤ�סG�]�����]�O
            if (manaBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                manaBarFill.fillAmount = 1f;
                UpdateManaBarColor(1f);
            }

            if (manaText != null)
            {
                manaText.text = "100/100";
            }

            Debug.Log("�]�O�����m���w�]���]�O���A");
        }

        // ���m���A
        lastKnownMana = -1;
        isInitialized = false;

        // ���s��l��
        StartCoroutine(InitializeManaBar());
    }

    // === �j��P�B�]�O ===
    public void ForceSync()
    {
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();
            int maxMana = playerAttack.maxMana;

            Debug.Log($"[�j��P�B] �P�B�]�O: {currentMana}/{maxMana}");
            UpdateManaBar(currentMana, maxMana);
        }
    }
}