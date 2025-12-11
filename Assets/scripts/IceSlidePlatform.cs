using UnityEngine;

public class IceSlidePlatform : MonoBehaviour
{
    [Header("滑動設定")]
    public Vector2 slideDirection = Vector2.right;  // 滑動方向
    public float slideForce = 10f;                   // 滑動力度
    public bool isActivated = false;

    private Rigidbody2D playerRb;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            isActivated = true;
            Debug.Log("[IceSlidePlatform] 玩家踏上滑冰平台，開始滑動！");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isActivated = false;
            playerRb = null;
        }
    }

    void FixedUpdate()
    {
        if (!isActivated || playerRb == null) return;

        // 施加滑動力量
        playerRb.velocity = new Vector2(
            slideDirection.x * slideForce,
            playerRb.velocity.y
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)slideDirection * 2);
    }
}
