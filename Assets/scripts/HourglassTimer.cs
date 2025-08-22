using UnityEngine;
using UnityEngine.UI;

public class HourglassTimer : MonoBehaviour
{
    [Header("玩家設定")]
    public Transform playerTransform;          // 玩家Transform
    public Vector3 offset = new Vector3(0, 2f, 0);  // 沙漏偏移玩家頭頂位置

    [Header("沙漏圖像")]
    public Sprite fullHourglass;                // 沙漏滿狀態
    public Sprite midHourglass;                 // 沙漏中間狀態
    public Sprite emptyHourglass;               // 沙漏空狀態

    [Header("計時設定")]
    public float totalTime = 10f;               // 總倒數時間（秒）

    [Header("參考元件")]
    public Image hourglassImage;                // UI Image 顯示沙漏 (若是用SpriteRenderer改為public SpriteRenderer)

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

        // 更新計時
        currentTime -= Time.deltaTime;
        if (currentTime < 0)
        {
            currentTime = 0;
            isCounting = false;
            // 計時結束可加額外事件
        }

        // 更新沙漏圖像
        float ratio = currentTime / totalTime;
        if (ratio > 0.66f)
            SetHourglassSprite(fullHourglass);
        else if (ratio > 0.33f)
            SetHourglassSprite(midHourglass);
        else
            SetHourglassSprite(emptyHourglass);

        // 維持沙漏位置跟隨玩家頭頂
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + offset;
        }
    }

    // 啟動倒數計時
    public void StartTimer()
    {
        currentTime = totalTime;
        isCounting = true;
        SetHourglassSprite(fullHourglass);
    }

    // 切換沙漏圖片
    private void SetHourglassSprite(Sprite sprite)
    {
        if (hourglassImage != null && hourglassImage.sprite != sprite)
        {
            hourglassImage.sprite = sprite;
        }
        else if (hourglassImage == null)
        {
            // 若未使用UI Image，改成控制SpriteRenderer
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != sprite)
                sr.sprite = sprite;
        }
    }
}
