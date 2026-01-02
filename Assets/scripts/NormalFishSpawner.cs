using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通魚自動生成系統
/// </summary>
public class NormalFishSpawner : MonoBehaviour
{
    [Header("═══ 生成設定 ═══")]
    [Tooltip("普通魚預製物")]
    public GameObject normalFishPrefab;

    [Tooltip("固定生成的魚隻數量")]
    [Range(1, 20)]
    public int fishCount = 8;

    [Tooltip("生成區域")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(20f, 10f);

    [Header("═══ 生成間隔（如果一次生成失敗）═══")]
    public float retrySpawnInterval = 1f;

    private List<GameObject> spawnedFish = new List<GameObject>();
    private Coroutine respawnCoroutine;

    void Start()
    {
        if (normalFishPrefab == null)
        {
            Debug.LogError("[NormalFishSpawner] 普通魚預製物未設定！");
            return;
        }

        SpawnAllFish();
        Debug.Log($"[NormalFishSpawner] 初始化完成，生成 {fishCount} 條普通魚");
    }

    void Update()
    {
        // 檢查是否有魚被銷毀，如果是則重新生成
        spawnedFish.RemoveAll(fish => fish == null);

        if (spawnedFish.Count < fishCount)
        {
            if (respawnCoroutine == null)
            {
                respawnCoroutine = StartCoroutine(RespawnDeadFish());
            }
        }
    }

    void SpawnAllFish()
    {
        for (int i = 0; i < fishCount; i++)
        {
            SpawnSingleFish();
        }
    }

    void SpawnSingleFish()
    {
        Vector3 randomPos = GetRandomSpawnPosition();
        GameObject fish = Instantiate(normalFishPrefab, randomPos, Quaternion.identity);
        fish.name = $"NormalFish_{spawnedFish.Count}";
        spawnedFish.Add(fish);

        Debug.Log($"[NormalFishSpawner] 生成普通魚 at {randomPos}");
    }

    Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(
            spawnAreaCenter.x - spawnAreaSize.x * 0.5f,
            spawnAreaCenter.x + spawnAreaSize.x * 0.5f
        );

        float randomY = Random.Range(
            spawnAreaCenter.y - spawnAreaSize.y * 0.5f,
            spawnAreaCenter.y + spawnAreaSize.y * 0.5f
        );

        return new Vector3(randomX, randomY, 0);
    }

    IEnumerator RespawnDeadFish()
    {
        yield return new WaitForSeconds(retrySpawnInterval);

        while (spawnedFish.Count < fishCount)
        {
            SpawnSingleFish();
            yield return new WaitForSeconds(0.5f);
        }

        respawnCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
    }
}