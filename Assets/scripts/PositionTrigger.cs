using UnityEngine;

public class PositionTrigger : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("要監測的玩家物件")]
    public Transform player;

    [Tooltip("要控制的地板")]
    public FloorController targetFloor;

    [Header("觸發條件")]
    [Tooltip("條件類型")]
    public ConditionType conditionType = ConditionType.HigherThanY;

    [Tooltip("觸發值")]
    public float triggerValue = 5f;

    [Tooltip("是否只觸發一次")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    public enum ConditionType
    {
        HigherThanY,    // Y 座標高於某值
        LowerThanY,     // Y 座標低於某值
        HigherThanX,    // X 座標高於某值(右側)
        LowerThanX      // X 座標低於某值(左側)
    }

    void Start()
    {
        // 自動尋找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("PositionTrigger: 找不到玩家物件！請確保玩家有 'Player' 標籤或手動指定。");
            }
        }

        // 檢查地板
        if (targetFloor == null)
        {
            Debug.LogError("PositionTrigger: 未設定目標地板！");
        }
    }

    void Update()
    {
        if (player == null || targetFloor == null)
            return;

        if (triggerOnce && hasTriggered)
            return;

        // 檢查條件
        bool conditionMet = false;

        switch (conditionType)
        {
            case ConditionType.HigherThanY:
                conditionMet = player.position.y > triggerValue;
                break;

            case ConditionType.LowerThanY:
                conditionMet = player.position.y < triggerValue;
                break;

            case ConditionType.HigherThanX:
                conditionMet = player.position.x > triggerValue;
                break;

            case ConditionType.LowerThanX:
                conditionMet = player.position.x < triggerValue;
                break;
        }

        if (conditionMet)
        {
            targetFloor.TriggerDisappear();
            hasTriggered = true;
        }
    }

    // 在編輯器中顯示觸發線
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        switch (conditionType)
        {
            case ConditionType.HigherThanY:
            case ConditionType.LowerThanY:
                // 繪製水平線
                Gizmos.DrawLine(
                    new Vector3(-100, triggerValue, 0),
                    new Vector3(100, triggerValue, 0)
                );
                break;

            case ConditionType.HigherThanX:
            case ConditionType.LowerThanX:
                // 繪製垂直線
                Gizmos.DrawLine(
                    new Vector3(triggerValue, -100, 0),
                    new Vector3(triggerValue, 100, 0)
                );
                break;
        }
    }
}