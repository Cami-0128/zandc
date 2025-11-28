using UnityEngine;

public class InvincibilityStar : MonoBehaviour   //無敵星星
{
    [Header("星星旋轉效果")]
    [Tooltip("星星每秒旋轉的角度")]
    public float rotationSpeed = 90f;

    [Header("星星收集音效")]
    public AudioClip collectSound;

    private void Update()
    {
        // 讓星星持續旋轉
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查是否碰到玩家
        InvincibilityController invincibility = other.GetComponent<InvincibilityController>();
        if (invincibility != null)
        {
            // 啟動玩家的無敵狀態
            invincibility.ActivateInvincibility();

            // 播放音效（這裡播放可選，因為 InvincibilityController 也會播放）
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            Debug.Log("[無敵星星] 玩家收集了無敵星星！");

            // 銷毀星星物件
            Destroy(gameObject);
        }
    }
}