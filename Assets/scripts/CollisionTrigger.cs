using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("要控制的地板(可以多個)")]
    public FloorController[] targetFloors;

    [Header("觸發設定")]
    [Tooltip("觸發標籤(例如: Player)")]
    public string triggerTag = "Player";

    [Tooltip("是否只觸發一次")]
    public bool triggerOnce = true;

    [Tooltip("觸發類型")]
    public TriggerMode triggerMode = TriggerMode.OnEnter;

    private bool hasTriggered = false;

    public enum TriggerMode
    {
        OnEnter,    // 進入時觸發
        OnStay,     // 停留時觸發
        OnExit      // 離開時觸發
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerMode == TriggerMode.OnEnter)
        {
            CheckAndTrigger(other);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (triggerMode == TriggerMode.OnStay)
        {
            CheckAndTrigger(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggerMode == TriggerMode.OnExit)
        {
            CheckAndTrigger(other);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggerMode == TriggerMode.OnEnter)
        {
            CheckAndTrigger(collision.collider);
        }
    }

    private void CheckAndTrigger(Collider2D other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (other.CompareTag(triggerTag))
        {
            TriggerFloors();
            hasTriggered = true;
        }
    }

    private void TriggerFloors()
    {
        if (targetFloors == null || targetFloors.Length == 0)
        {
            Debug.LogWarning("CollisionTrigger: 未設定目標地板！");
            return;
        }

        foreach (FloorController floor in targetFloors)
        {
            if (floor != null)
            {
                floor.TriggerDisappear();
            }
        }
    }

    // 重置觸發狀態(可供外部調用)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}