using UnityEngine;

public class FrostAccumulationTrap : MonoBehaviour
{
    [Header("堆積設定")]
    public float maxScale = 2f;             // 最大堆積高度
    public float growthSpeed = 0.5f;        // 生長速度
    public float detectionRange = 3f;
    public LayerMask playerLayer;

    private Vector3 originalScale;
    private bool isGrowing = false;
    private float currentHeight = 0.1f;

    void Start()
    {
        originalScale = transform.localScale;
        Vector3 startScale = originalScale;
        startScale.y = 0.1f;
        transform.localScale = startScale;
    }

    void Update()
    {
        CheckPlayerNearby();

        if (isGrowing)
        {
            currentHeight = Mathf.Min(currentHeight + Time.deltaTime * growthSpeed, maxScale);
            Vector3 newScale = originalScale;
            newScale.y = currentHeight;
            transform.localScale = newScale;
        }
    }

    void CheckPlayerNearby()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, playerLayer);
        isGrowing = colliders.Length > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}