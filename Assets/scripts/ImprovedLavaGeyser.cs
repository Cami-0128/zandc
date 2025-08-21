// === 1. ��i�����߼Q�u (���X�A�� LavaGeyser ����) ===
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ImprovedLavaGeyser : MonoBehaviour
{
    [Header("�����Q�u�򥻳]�w")]
    [Tooltip("�Q�o���j�]��^")]
    public float eruptionInterval = 4f;

    [Tooltip("�Q�o����ɶ��]��^")]
    public float eruptionDuration = 2f;

    [Tooltip("�Q�o����")]
    public float eruptionHeight = 6f;

    [Tooltip("�Q�u�e��")]
    [Range(1f, 4f)]
    public float geyserWidth = 2f;

    [Tooltip("�缾�a�y�����ˮ`")]
    public int damageAmount = 25;

    [Tooltip("���O�j�ס]�N���a�V�W���^")]
    public float pushForce = 10f;

    [Header("�wĵ�t��")]
    [Tooltip("�Q�o�e�wĵ�ɶ�")]
    public float warningTime = 1f;

    [Tooltip("�wĵ�{�{�W�v")]
    public float warningBlinkRate = 5f;

    [Header("��ı�P����")]
    [Tooltip("�����C��")]
    public Color baseColor = new Color(0.6f, 0.1f, 0f, 1f);

    [Tooltip("�wĵ�C��")]
    public Color warningColor = new Color(1f, 0.8f, 0f, 0.8f);

    [Tooltip("��w�C��")]
    public Color bubbleColor = new Color(1f, 0.9f, 0.6f, 0.8f);

    [Tooltip("�ҥβɤl�ĪG")]
    public bool enableParticles = true;

    [Header("���x�]�w")]
    [Tooltip("�O�_���i���ߪ����x")]
    public bool hasPlatform = false;

    [Tooltip("���x�W�ɳt��")]
    public float platformRiseSpeed = 3f;

    [Header("����")]
    public AudioClip warningSound;
    public AudioClip eruptionSound;
    public AudioClip bubbleSound;

    // �p���ܼ�
    private bool isErupting = false;
    private bool isWarning = false;
    private GameObject geyserVisual;
    private GameObject warningIndicator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer warningRenderer;
    private BoxCollider2D triggerCollider;
    private ParticleSystem bubbleParticles;
    private ParticleSystem steamParticles;
    private AudioSource audioSource;
    private Coroutine eruptionCoroutine;
    private GameObject platform;

    void Start()
    {
        SetupGeyser();
        StartEruptionLoop();
    }

    void Update()
    {
        // �wĵ�{�{�ĪG
        if (isWarning && warningRenderer != null)
        {
            float alpha = 0.3f + 0.7f * Mathf.Abs(Mathf.Sin(Time.time * warningBlinkRate));
            Color color = warningColor;
            color.a = alpha;
            warningRenderer.color = color;
        }

        // ���x�W���޿�
        if (isErupting && platform != null && hasPlatform)
        {
            Vector3 targetPos = Vector3.up * (eruptionHeight * 0.8f);
            platform.transform.localPosition = Vector3.MoveTowards(
                platform.transform.localPosition,
                targetPos,
                platformRiseSpeed * Time.deltaTime
            );
        }
    }

    void SetupGeyser()
    {
        // �ЫإD�n�������Q�u��ı
        SetupVisuals();

        // �]�m�I����
        SetupCollider();

        // �Ыا���M�K��
        CreateGeyserTexture();

        // �]�m����
        SetupAudio();

        // �Ыزɤl�ĪG
        CreateParticleEffects();

        // �Ыإ��x�]�p�G�ݭn�^
        if (hasPlatform)
        {
            CreatePlatform();
        }
    }

    void SetupVisuals()
    {
        // �D�n�Q�u��ı
        geyserVisual = new GameObject("GeyserVisual");
        geyserVisual.transform.SetParent(transform);
        geyserVisual.transform.localPosition = Vector3.zero;
        spriteRenderer = geyserVisual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 1;
        geyserVisual.SetActive(false);

        // �wĵ���ܾ�
        warningIndicator = new GameObject("WarningIndicator");
        warningIndicator.transform.SetParent(transform);
        warningIndicator.transform.localPosition = Vector3.zero;
        warningRenderer = warningIndicator.AddComponent<SpriteRenderer>();
        warningRenderer.sortingOrder = 0;
        warningIndicator.SetActive(false);
    }

    void SetupCollider()
    {
        triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(geyserWidth, eruptionHeight);
        triggerCollider.offset = new Vector2(0, eruptionHeight * 0.5f);
        triggerCollider.enabled = false;
    }

    void CreateGeyserTexture()
    {
        int textureWidth = Mathf.RoundToInt(32 * (geyserWidth / 2f));
        int textureHeight = 128;

        // �D�n�Q�u�K��
        Texture2D mainTexture = CreateLavaTexture(textureWidth, textureHeight, false);
        spriteRenderer.sprite = Sprite.Create(mainTexture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0f));

        // �wĵ���ܾ��K��
        Texture2D warningTexture = CreateLavaTexture(textureWidth, textureHeight / 4, true);
        warningRenderer.sprite = Sprite.Create(warningTexture,
            new Rect(0, 0, textureWidth, textureHeight / 4),
            new Vector2(0.5f, 0.5f));
    }

    Texture2D CreateLavaTexture(int width, int height, bool isWarning)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                if (isWarning)
                {
                    // �wĵ���ܾ��G²�檺�������
                    float centerX = width * 0.5f;
                    float centerY = height * 0.5f;
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    float maxDistance = Mathf.Min(width, height) * 0.4f;

                    if (distance < maxDistance)
                    {
                        pixels[index] = warningColor;
                    }
                    else
                    {
                        pixels[index] = Color.clear;
                    }
                }
                else
                {
                    // �D�n�Q�u�G���ܦ�
                    float heightRatio = (float)y / height;
                    Color color;

                    if (heightRatio < 0.2f)
                        color = new Color(0.6f, 0.1f, 0f, 1f); // �`��
                    else if (heightRatio < 0.5f)
                        color = new Color(1f, 0.3f, 0f, 1f);   // ���
                    else if (heightRatio < 0.8f)
                        color = new Color(1f, 0.6f, 0f, 1f);   // ���
                    else
                        color = new Color(1f, 0.9f, 0.6f, 0.9f); // �L��

                    pixels[index] = color;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void CreateParticleEffects()
    {
        if (!enableParticles) return;

        // ��w�ɤl�t��
        CreateBubbleParticles();

        // �]�T�ɤl�t��
        CreateSteamParticles();
    }

    void CreateBubbleParticles()
    {
        GameObject bubbleObj = new GameObject("BubbleParticles");
        bubbleObj.transform.SetParent(transform);
        bubbleObj.transform.localPosition = Vector3.up * (eruptionHeight * 0.9f);
        bubbleParticles = bubbleObj.AddComponent<ParticleSystem>();

        var main = bubbleParticles.main;
        main.startColor = bubbleColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.maxParticles = 50;
        main.startLifetime = 1.5f;

        var emission = bubbleParticles.emission;
        emission.rateOverTime = 25;

        var shape = bubbleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = geyserWidth * 0.3f;

        bubbleParticles.Stop();
    }

    void CreateSteamParticles()
    {
        GameObject steamObj = new GameObject("SteamParticles");
        steamObj.transform.SetParent(transform);
        steamObj.transform.localPosition = Vector3.zero;
        steamParticles = steamObj.AddComponent<ParticleSystem>();

        var main = steamParticles.main;
        main.startColor = new Color(1f, 1f, 1f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.maxParticles = 20;
        main.startLifetime = 2f;

        var emission = steamParticles.emission;
        emission.rateOverTime = 10;

        steamParticles.Play(); // �]�T�@���s�b
    }

    void CreatePlatform()
    {
        platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.transform.SetParent(transform);
        platform.transform.localScale = new Vector3(geyserWidth * 0.8f, 0.2f, 1f);
        platform.transform.localPosition = Vector3.zero;

        // �]�m���x�I��
        BoxCollider2D platformCollider = platform.GetComponent<BoxCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = platform.AddComponent<BoxCollider2D>();
        }

        // �]�m���x����
        SpriteRenderer platformRenderer = platform.GetComponent<SpriteRenderer>();
        if (platformRenderer != null)
        {
            platformRenderer.color = new Color(0.4f, 0.2f, 0.1f, 1f); // �`�Ŧ⩥��
        }

        platform.tag = "Platform";
    }

    void StartEruptionLoop()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
        }
        eruptionCoroutine = StartCoroutine(EruptionLoop());
    }

    IEnumerator EruptionLoop()
    {
        while (true)
        {
            // ���ݶ��j
            yield return new WaitForSeconds(eruptionInterval - warningTime);

            // �}�l�wĵ
            yield return StartCoroutine(ShowWarning());

            // �}�l�Q�o
            yield return StartCoroutine(Erupt());
        }
    }

    IEnumerator ShowWarning()
    {
        isWarning = true;
        warningIndicator.SetActive(true);

        // ����wĵ����
        if (audioSource != null && warningSound != null)
        {
            audioSource.clip = warningSound;
            audioSource.Play();
        }

        yield return new WaitForSeconds(warningTime);

        isWarning = false;
        warningIndicator.SetActive(false);
    }

    IEnumerator Erupt()
    {
        isErupting = true;

        // �Ұʵ�ı�ĪG
        geyserVisual.SetActive(true);
        geyserVisual.transform.localScale = new Vector3(geyserWidth, eruptionHeight, 1f);

        // �ҰʸI����
        triggerCollider.enabled = true;

        // ����Q�o����
        if (audioSource != null && eruptionSound != null)
        {
            audioSource.clip = eruptionSound;
            audioSource.Play();
        }

        // �Ұʲɤl�ĪG
        if (enableParticles && bubbleParticles != null)
        {
            bubbleParticles.Play();
        }

        yield return new WaitForSeconds(eruptionDuration);

        // �����Q�o
        isErupting = false;
        geyserVisual.SetActive(false);
        triggerCollider.enabled = false;

        if (enableParticles && bubbleParticles != null)
        {
            bubbleParticles.Stop();
        }

        // ���m���x��m
        if (platform != null)
        {
            platform.transform.localPosition = Vector3.zero;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isErupting && other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                // �y���ˮ`
                player.TakeDamage(damageAmount);

                // �V�W���O
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, pushForce);
                }

                Debug.Log($"���aĲ�I�����Q�u�I�y�� {damageAmount} �I�ˮ`����o�V�W���O");
            }
        }
    }

    // ���}��k
    public void ForceErupt()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
        }
        StartCoroutine(Erupt());
        StartEruptionLoop();
    }

    public void StopEruption()
    {
        if (eruptionCoroutine != null)
        {
            StopCoroutine(eruptionCoroutine);
            eruptionCoroutine = null;
        }

        isErupting = false;
        isWarning = false;
        geyserVisual.SetActive(false);
        warningIndicator.SetActive(false);
        triggerCollider.enabled = false;

        if (bubbleParticles != null)
            bubbleParticles.Stop();
    }
}