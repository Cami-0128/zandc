using UnityEngine;
using UnityEngine.UI;

public class HourglassTimer : MonoBehaviour
{
    public Transform playerTransform;          // ���aTransform�A�ΨӨ��oX�y�ЧP�_
    public Vector3 offset = new Vector3(0, 2f, 0);  // �F�|�۹缾�a����
    public float startXThreshold = -16f;        // ���aX�j�󦹭ȶ}�l�˼ƭp��

    public Sprite fullHourglass;                // �F�|��
    public Sprite midHourglass;                 // �F�|����
    public Sprite emptyHourglass;               // �F�|�Ū�

    public float totalTime = 10f;

    public Image hourglassImage;               // UI Image��ܥΡA�i�d�ť�SpriteRenderer

    private float currentTime;
    private bool isCounting = false;
    private bool hasStarted = false;

    public PlayerController2D player;          // ���a����Ѧ�

    void Start()
    {
        currentTime = totalTime;
        SetHourglassSprite(fullHourglass);
    }

    void Update()
    {
        if (!hasStarted && playerTransform != null && playerTransform.position.x > startXThreshold)
        {
            Debug.Log($"[HourglassTimer] �p�ɶ}�l�I���aX={playerTransform.position.x} �W�L�H��{startXThreshold}");
            StartTimer();
            hasStarted = true;
        }

        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isCounting = false;
            Debug.Log("[HourglassTimer] �p�ɵ����I");
            OnTimerFinished();
        }

        float ratio = currentTime / totalTime;
        if (ratio > 0.66f)
            SetHourglassSprite(fullHourglass);
        else if (ratio > 0.33f)
            SetHourglassSprite(midHourglass);
        else
            SetHourglassSprite(emptyHourglass);

        if (playerTransform != null)
            transform.position = playerTransform.position + offset;
    }

    public void StartTimer()
    {
        currentTime = totalTime;
        isCounting = true;
        SetHourglassSprite(fullHourglass);
    }

    private void SetHourglassSprite(Sprite sprite)
    {
        if (hourglassImage != null)
        {
            if (hourglassImage.sprite != sprite)
            {
                hourglassImage.sprite = sprite;
            }
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != sprite)
            {
                sr.sprite = sprite;
            }
        }
    }

    private void OnTimerFinished()
    {
        if (player != null && !player.hasReachedEnd)
        {
            Debug.Log("[HourglassTimer] �˼Ƶ����A���a���F���I�A���榺�`");
            player.Die();
        }
        else
        {
            Debug.Log("[HourglassTimer] �˼Ƶ����A���a�w�F���I�Τw���`�A�����榺�`");
        }
    }
}
