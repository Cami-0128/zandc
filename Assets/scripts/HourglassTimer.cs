using UnityEngine;
using UnityEngine.UI;

public class HourglassTimer : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 2f, 0);
    public float startXThreshold = -16f;

    public Sprite fullHourglass;
    public Sprite midHourglass;
    public Sprite emptyHourglass;

    public float totalTime = 10f;

    public Image hourglassImage;

    private float currentTime;
    private bool isCounting = false;
    private bool hasStarted = false;

    public PlayerController2D player;

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
        if (player == null)
        {
            Debug.LogWarning("[HourglassTimer] ���a�ޥά���");
            return;
        }
        Debug.Log($"[HourglassTimer] ���a���A - hasReachedEnd: {player.hasReachedEnd}, isDead: {player.isDead}");
        if (!player.hasReachedEnd && !player.isDead)
        {
            Debug.Log("[HourglassTimer] �˼ƨ�A���a���F���I�A���榺�`");
            player.Die();
        }
        else
        {
            Debug.Log("[HourglassTimer] �˼ƨ�A���a�w�F���I�Τw���`�A�����榺�`");
        }
    }
}
