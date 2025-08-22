using UnityEngine;
using UnityEngine.UI;

public class HourglassTimer : MonoBehaviour
{
    [Header("���a�]�w")]
    public Transform playerTransform;          // ���aTransform
    public Vector3 offset = new Vector3(0, 2f, 0);  // �F�|�������a�Y����m

    [Header("�F�|�Ϲ�")]
    public Sprite fullHourglass;                // �F�|�����A
    public Sprite midHourglass;                 // �F�|�������A
    public Sprite emptyHourglass;               // �F�|�Ū��A

    [Header("�p�ɳ]�w")]
    public float totalTime = 10f;               // �`�˼Ʈɶ��]��^

    [Header("�ѦҤ���")]
    public Image hourglassImage;                // UI Image ��ܨF�| (�Y�O��SpriteRenderer�אּpublic SpriteRenderer)

    private float currentTime;
    private bool isCounting = false;

    void Start()
    {
        currentTime = totalTime;
        SetHourglassSprite(fullHourglass);
    }

    void Update()
    {
        if (!isCounting) return;

        // ��s�p��
        currentTime -= Time.deltaTime;
        if (currentTime < 0)
        {
            currentTime = 0;
            isCounting = false;
            // �p�ɵ����i�[�B�~�ƥ�
        }

        // ��s�F�|�Ϲ�
        float ratio = currentTime / totalTime;
        if (ratio > 0.66f)
            SetHourglassSprite(fullHourglass);
        else if (ratio > 0.33f)
            SetHourglassSprite(midHourglass);
        else
            SetHourglassSprite(emptyHourglass);

        // �����F�|��m���H���a�Y��
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + offset;
        }
    }

    // �Ұʭ˼ƭp��
    public void StartTimer()
    {
        currentTime = totalTime;
        isCounting = true;
        SetHourglassSprite(fullHourglass);
    }

    // �����F�|�Ϥ�
    private void SetHourglassSprite(Sprite sprite)
    {
        if (hourglassImage != null && hourglassImage.sprite != sprite)
        {
            hourglassImage.sprite = sprite;
        }
        else if (hourglassImage == null)
        {
            // �Y���ϥ�UI Image�A�令����SpriteRenderer
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != sprite)
                sr.sprite = sprite;
        }
    }
}
