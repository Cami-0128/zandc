using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [Header("基本設定")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;
    public int meteorsPerWave = 5;

    [Header("發射位置")]
    public Vector2 spawnPosition = new Vector2(10f, 5f);

    [Header("角度設定")]
    public float spreadAngle = 60f;
    public float baseAngle = 225f;

    [Header("隕石屬性")]
    public float meteorSpeed = 8f;
    public int meteorDamage = 30;

    void Start()
    {
        Debug.Log("隕石發射器啟動");

        // 立即發射第一波，然後每3秒發射一波
        InvokeRepeating("SpawnMeteorWave", 0f, spawnInterval);
    }

    void SpawnMeteorWave()
    {
        if (meteorPrefab == null)
        {
            Debug.LogError("沒有設定隕石預製物！");
            return;
        }

        Debug.Log($"發射一波隕石，共 {meteorsPerWave} 顆");

        for (int i = 0; i < meteorsPerWave; i++)
        {
            SpawnSingleMeteor(i);
        }
    }

    void SpawnSingleMeteor(int index)
    {
        // 計算角度
        float angleStep = spreadAngle / (meteorsPerWave - 1);
        float currentAngle = baseAngle - (spreadAngle / 2) + (angleStep * index);

        // 轉換成方向向量
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // 創建隕石
        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

        // 設定隕石屬性
        MeteorController meteorController = meteor.GetComponent<MeteorController>();
        if (meteorController != null)
        {
            meteorController.SetDirection(direction);
            meteorController.moveSpeed = meteorSpeed;
            meteorController.damage = meteorDamage;
        }

        Debug.Log($"隕石 {index + 1} 發射，角度: {currentAngle} 度");
    }

    // 手動發射測試
    [ContextMenu("測試發射一波")]
    void TestSpawn()
    {
        SpawnMeteorWave();
    }
}