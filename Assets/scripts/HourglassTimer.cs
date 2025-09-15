using UnityEngine;
using UnityEngine.UI;

public class HourglassTimer : MonoBehaviour
{
    public Transform playerTransform;          // 玩家Transform，用來取得X座標判斷
    public Vector3 offset = new Vector3(0, 2f, 0);  // 沙漏相對玩家偏移
    public float startXThreshold = -16f;        // 當玩家X大於此值開始倒數計時

    public Sprite fullHourglass;                // 沙漏滿
    public Sprite midHourglass;                 // 沙漏中度
    public Sprite emptyHourglass;               // 沙漏空的

    public float totalTime = 10f;

    public Image hourglassImage;               // UI Image顯示用，可留空用SpriteRenderer

    private float currentTime;
    private bool isCounting = false;
    private bool hasStarted = false;

    public PlayerController2D player;          // 玩家控制器參考

    void Start()
    {
        currentTime = totalTime;
        SetHourglassSprite(fullHourglass);
    }

    void Update()
    {
        if (!hasStarted && playerTransform != null && playerTransform.position.x > startXThreshold)
        {
            Debug.Log($"[HourglassTimer] 計時開始！玩家X={playerTransform.position.x} 超過閾值{startXThreshold}");
            StartTimer();
            hasStarted = true;
        }

        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isCounting = false;
            Debug.Log("[HourglassTimer] 計時結束！");
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
            Debug.Log("[HourglassTimer] 倒數結束，玩家未達終點，執行死亡");
            player.Die();
        }
        else
        {
            Debug.Log("[HourglassTimer] 倒數結束，玩家已達終點或已死亡，不執行死亡");
        }
    }
}
