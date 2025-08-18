using UnityEngine;
public class MeteorSpawner2 : MonoBehaviour
{
    [Header("基本設定")]
    public GameObject meteorPrefab;
    public float spawnInterval = 3f;

    [Header("發射位置")]
    [Tooltip("是否使用自定義發射位置，不勾選則從此物件位置發射")]
    public bool useCustomSpawnPosition = false;
    public Vector2 customSpawnPosition = new Vector2(10f, 5f);

    [Header("角度設定")]
    public float startAngle = 10f;      // 起始角度
    public float endAngle = 360f;       // 結束角度
    public float angleStep = 10f;       // 角度間隔

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

        // 計算隕石總數
        int meteorCount = Mathf.RoundToInt((endAngle - startAngle) / angleStep) + 1;
        Debug.Log($"發射一波隕石，共 {meteorCount} 顆");

        int index = 0;
        for (float angle = startAngle; angle <= endAngle; angle += angleStep)
        {
            SpawnSingleMeteor(angle, index);
            index++;
        }
    }

    void SpawnSingleMeteor(float angle, int index)
    {
        // 決定發射位置
        Vector2 spawnPos = useCustomSpawnPosition ? customSpawnPosition : (Vector2)transform.position;

        // 轉換成方向向量
        float angleInRadians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        // 創建隕石
        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);

        // 設定隕石屬性
        MeteorController meteorController = meteor.GetComponent<MeteorController>();
        if (meteorController != null)
        {
            meteorController.SetDirection(direction);
            meteorController.moveSpeed = meteorSpeed;
            meteorController.damage = meteorDamage;
        }

        Debug.Log($"隕石 {index + 1} 發射，角度: {angle} 度，位置: {spawnPos}");
    }

    // 手動發射測試
    [ContextMenu("測試發射一波")]
    void TestSpawn()
    {
        SpawnMeteorWave();
    }
}