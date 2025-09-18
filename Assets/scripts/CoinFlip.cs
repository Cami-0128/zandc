using UnityEngine;

public class CoinFlip : MonoBehaviour
{
    public float flipSpeed = 5f;
    private float timer = 0f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime * flipSpeed;
        float scaleX = Mathf.Cos(timer);
        transform.localScale = new Vector3(scaleX * originalScale.x, originalScale.y, originalScale.z);
    }
}
