using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxEnemies = 5;

    private float nextSpawnTime;
    private int currentEnemyCount;

    void Update()
    {
        if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // 更新敵人數量
        currentEnemyCount = FindObjectsOfType<Enemy>().Length;
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            Debug.Log($"生成敵人於: {spawnPoint.name}");
        }
    }
}