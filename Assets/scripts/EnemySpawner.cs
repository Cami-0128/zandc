using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 3;
    public float spawnInterval = 2f;
    public Transform[] spawnPoints;

    private int currentEnemyCount = 0;

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;
        int index = Random.Range(0, spawnPoints.Length);

        Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);
        currentEnemyCount++;
    }
}
