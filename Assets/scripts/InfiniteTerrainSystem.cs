using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 無限地形生成系統 - 修正版
/// Boss每降低20%血量切換一次地形
/// </summary>
public class InfiniteTerrainSystem : MonoBehaviour
{
    [Header("追蹤目標")]
    [Tooltip("玩家Transform（會自動尋找）")]
    public Transform player;

    [Tooltip("Boss Transform（會自動尋找）")]
    public Transform boss;

    [Header("地形塊設定")]
    [Tooltip("所有可用的地形塊預製體")]
    public GameObject[] terrainChunkPrefabs;

    [Tooltip("地形塊寬度")]
    public float chunkWidth = 15f;

    [Tooltip("地形塊高度")]
    public float chunkHeight = 1.5f;

    [Header("生成範圍")]
    [Tooltip("在目標前方生成幾塊地形")]
    public int chunksAhead = 5;

    [Tooltip("在目標後方保留幾塊地形")]
    public int chunksBehind = 3;

    [Header("高低起伏設定")]
    [Tooltip("是否啟用地形高低起伏")]
    public bool enableHeightVariation = true;

    [Tooltip("最大高度變化（±單位）")]
    [Range(0f, 2f)]
    public float maxHeightVariation = 0.3f;

    [Tooltip("高度變化平滑度（數值越大越平滑）")]
    [Range(0f, 1f)]
    public float heightSmoothness = 0.7f;

    [Header("初始設定")]
    [Tooltip("遊戲開始時的地形塊數量")]
    public int initialChunkCount = 10;

    [Tooltip("起始位置")]
    public Vector3 startPosition = Vector3.zero;

    [Header("地形切換設定")]
    [Tooltip("是否啟用地形切換功能")]
    public bool enableTerrainSwitching = true;

    [Tooltip("閃光特效預製體")]
    public GameObject switchFlashPrefab;

    [Tooltip("閃光持續時間")]
    public float flashDuration = 0.5f;

    [Tooltip("閃光顏色")]
    public Color flashColor = Color.white;

    [Header("效能優化")]
    [Tooltip("檢查頻率（秒）")]
    public float updateInterval = 0.5f;

    [Header("Debug")]
    public bool showDebugLog = true;
    public bool showGizmos = true;

    // 私有變數
    private List<TerrainChunkData> activeChunks = new List<TerrainChunkData>();
    private float lastUpdateTime = 0f;
    private float nextChunkX;
    private float previousChunkX;
    private int currentTerrainTypeIndex = 0;
    private float lastChunkHeight = 0f;
    private bool isSwitching = false;

    // Boss血量追蹤
    private HashSet<int> triggeredHealthThresholds = new HashSet<int>();
    private BossController bossController;

    // 地形塊資料結構
    private class TerrainChunkData
    {
        public GameObject gameObject;
        public float xPosition;
        public int terrainTypeIndex;

        public TerrainChunkData(GameObject obj, float x, int typeIndex)
        {
            gameObject = obj;
            xPosition = x;
            terrainTypeIndex = typeIndex;
        }
    }

    void Start()
    {
        if (showDebugLog)
            Debug.Log("=== [InfiniteTerrain] 開始初始化 ===");

        // 自動尋找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (showDebugLog)
                    Debug.Log("[InfiniteTerrain] ✅ 找到玩家");
            }
            else
            {
                Debug.LogError("[InfiniteTerrain] ❌ 找不到玩家！請確保玩家有 'Player' Tag");
                return;
            }
        }

        // 自動尋找 Boss
        if (boss == null)
        {
            bossController = FindObjectOfType<BossController>();
            if (bossController != null)
            {
                boss = bossController.transform;
                if (showDebugLog)
                    Debug.Log("[InfiniteTerrain] ✅ 找到Boss");
            }
            else
            {
                Debug.LogWarning("[InfiniteTerrain] ⚠️ 找不到Boss，只追蹤玩家");
            }
        }

        // 驗證地形預製體
        if (terrainChunkPrefabs == null || terrainChunkPrefabs.Length == 0)
        {
            Debug.LogError("[InfiniteTerrain] ❌ 地形預製體陣列為空！請在 Inspector 中設定");
            return;
        }

        // 移除空引用
        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < terrainChunkPrefabs.Length; i++)
        {
            if (terrainChunkPrefabs[i] != null)
            {
                validPrefabs.Add(terrainChunkPrefabs[i]);
            }
            else
            {
                Debug.LogWarning($"[InfiniteTerrain] 地形預製體 #{i} 為空，已忽略");
            }
        }
        terrainChunkPrefabs = validPrefabs.ToArray();

        if (terrainChunkPrefabs.Length == 0)
        {
            Debug.LogError("[InfiniteTerrain] ❌ 沒有有效的地形預製體！");
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"[InfiniteTerrain] 已載入 {terrainChunkPrefabs.Length} 個地形類型");
            for (int i = 0; i < terrainChunkPrefabs.Length; i++)
            {
                Debug.Log($"  - 類型 {i}: {terrainChunkPrefabs[i].name}");
            }
        }

        // 初始化地形生成位置
        nextChunkX = startPosition.x;
        previousChunkX = startPosition.x - chunkWidth;
        lastChunkHeight = startPosition.y;

        // 生成初始地形
        GenerateInitialTerrain();

        if (showDebugLog)
            Debug.Log("=== [InfiniteTerrain] 初始化完成 ===");
    }

    void Update()
    {
        // 定期檢查並更新地形
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateTerrain();
        }

        // 檢查Boss血量觸發地形切換
        if (enableTerrainSwitching && bossController != null && !bossController.isDead)
        {
            CheckBossHealthTrigger();
        }
    }

    /// <summary>
    /// 檢查Boss血量是否觸發地形切換
    /// </summary>
    void CheckBossHealthTrigger()
    {
        if (isSwitching) return;

        float healthPercentage = bossController.GetHealthPercentage();

        // 定義觸發閾值：80%, 60%, 40%, 20%
        int[] thresholds = { 80, 60, 40, 20 };

        foreach (int threshold in thresholds)
        {
            // 如果血量低於閾值且尚未觸發過
            if (healthPercentage * 100 <= threshold && !triggeredHealthThresholds.Contains(threshold))
            {
                triggeredHealthThresholds.Add(threshold);

                if (showDebugLog)
                {
                    Debug.Log($"[InfiniteTerrain] 🩸 Boss血量降至 {threshold}%，觸發地形切換！");
                }

                StartCoroutine(SwitchAllTerrainType());
                break; // 一次只觸發一個
            }
        }
    }

    /// <summary>
    /// 生成初始地形
    /// </summary>
    void GenerateInitialTerrain()
    {
        if (showDebugLog)
            Debug.Log("[InfiniteTerrain] 開始生成初始地形...");

        // 以玩家位置為中心生成
        float centerX = player != null ? player.position.x : startPosition.x;

        // 生成玩家左右兩側的地形
        int halfCount = initialChunkCount / 2;

        for (int i = -halfCount; i <= halfCount; i++)
        {
            float xPos = centerX + (i * chunkWidth);
            SpawnChunkAtPosition(xPos);
        }

        // 更新邊界
        nextChunkX = centerX + ((halfCount + 1) * chunkWidth);
        previousChunkX = centerX - ((halfCount + 1) * chunkWidth);

        if (showDebugLog)
            Debug.Log($"[InfiniteTerrain] ✅ 初始地形生成完成，共 {activeChunks.Count} 塊");
    }

    /// <summary>
    /// 更新地形（生成新塊，刪除舊塊）
    /// </summary>
    void UpdateTerrain()
    {
        if (player == null) return;

        float leftMost = player.position.x;
        float rightMost = player.position.x;

        // 如果Boss存在，擴大範圍
        if (boss != null)
        {
            leftMost = Mathf.Min(leftMost, boss.position.x);
            rightMost = Mathf.Max(rightMost, boss.position.x);
        }

        // 在右側生成新地形
        while (nextChunkX < rightMost + (chunksAhead * chunkWidth))
        {
            SpawnChunkAtPosition(nextChunkX);
            nextChunkX += chunkWidth;
        }

        // 在左側生成新地形
        while (previousChunkX > leftMost - (chunksBehind * chunkWidth))
        {
            SpawnChunkAtPosition(previousChunkX);
            previousChunkX -= chunkWidth;
        }

        // 刪除遠離的地形塊
        float focusX = (leftMost + rightMost) / 2f;
        RemoveDistantChunks(focusX);
    }

    /// <summary>
    /// 在指定位置生成地形塊
    /// </summary>
    void SpawnChunkAtPosition(float xPosition)
    {
        // 檢查是否已存在
        foreach (TerrainChunkData chunkData in activeChunks)
        {
            if (Mathf.Abs(chunkData.xPosition - xPosition) < 0.1f)
                return; // 已存在
        }

        // 計算高度（帶起伏）
        float yPosition = startPosition.y;

        if (enableHeightVariation && maxHeightVariation > 0f)
        {
            float targetHeight = Random.Range(-maxHeightVariation, maxHeightVariation);
            yPosition = Mathf.Lerp(lastChunkHeight, startPosition.y + targetHeight, 1f - heightSmoothness);
            lastChunkHeight = yPosition;
        }

        Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0f);

        // 選擇地形類型
        GameObject prefab = terrainChunkPrefabs[currentTerrainTypeIndex];
        GameObject chunkObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
        chunkObj.transform.SetParent(this.transform);
        chunkObj.name = $"Chunk_X{xPosition:F0}_{prefab.name}";

        // 添加到列表
        TerrainChunkData newChunk = new TerrainChunkData(chunkObj, xPosition, currentTerrainTypeIndex);
        activeChunks.Add(newChunk);

        if (showDebugLog && activeChunks.Count % 5 == 0) // 每5塊顯示一次
        {
            Debug.Log($"[InfiniteTerrain] 地形塊總數: {activeChunks.Count}");
        }
    }

    /// <summary>
    /// 刪除遠離焦點的地形塊
    /// </summary>
    void RemoveDistantChunks(float focusX)
    {
        float removeDistance = (chunksAhead + chunksBehind + 2) * chunkWidth;

        List<TerrainChunkData> chunksToRemove = new List<TerrainChunkData>();

        foreach (TerrainChunkData chunkData in activeChunks)
        {
            float distance = Mathf.Abs(chunkData.xPosition - focusX);

            if (distance > removeDistance)
            {
                chunksToRemove.Add(chunkData);
            }
        }

        // 刪除
        foreach (TerrainChunkData chunkData in chunksToRemove)
        {
            activeChunks.Remove(chunkData);
            if (chunkData.gameObject != null)
            {
                Destroy(chunkData.gameObject);
            }
        }
    }

    /// <summary>
    /// 切換所有地形塊的類型
    /// </summary>
    IEnumerator SwitchAllTerrainType()
    {
        if (isSwitching)
        {
            if (showDebugLog)
                Debug.Log("[InfiniteTerrain] 正在切換中，忽略");
            yield break;
        }

        isSwitching = true;

        // 選擇新的地形類型（不重複）
        int oldIndex = currentTerrainTypeIndex;

        if (terrainChunkPrefabs.Length > 1)
        {
            int attempts = 0;
            do
            {
                currentTerrainTypeIndex = Random.Range(0, terrainChunkPrefabs.Length);
                attempts++;

                if (attempts > 100)
                {
                    Debug.LogWarning("[InfiniteTerrain] 無法選擇不同地形，強制切換");
                    break;
                }
            }
            while (currentTerrainTypeIndex == oldIndex);
        }

        if (showDebugLog)
        {
            Debug.Log($"[InfiniteTerrain] 🔄 地形切換: {terrainChunkPrefabs[oldIndex].name} → {terrainChunkPrefabs[currentTerrainTypeIndex].name}");
        }

        // 顯示閃光特效
        yield return StartCoroutine(ShowFlashEffect());

        // 替換所有地形塊
        List<TerrainChunkData> chunksToReplace = new List<TerrainChunkData>(activeChunks);
        int replacedCount = 0;

        foreach (TerrainChunkData oldChunkData in chunksToReplace)
        {
            if (oldChunkData.gameObject == null) continue;

            // 生成新地形塊
            GameObject newPrefab = terrainChunkPrefabs[currentTerrainTypeIndex];
            Vector3 position = oldChunkData.gameObject.transform.position;
            Quaternion rotation = oldChunkData.gameObject.transform.rotation;

            GameObject newChunkObj = Instantiate(newPrefab, position, rotation);
            newChunkObj.transform.SetParent(this.transform);
            newChunkObj.name = $"Chunk_X{oldChunkData.xPosition:F0}_{newPrefab.name}";

            // 更新列表
            activeChunks.Remove(oldChunkData);
            Destroy(oldChunkData.gameObject);

            TerrainChunkData newChunkData = new TerrainChunkData(newChunkObj, oldChunkData.xPosition, currentTerrainTypeIndex);
            activeChunks.Add(newChunkData);

            replacedCount++;
        }

        if (showDebugLog)
        {
            Debug.Log($"[InfiniteTerrain] ✅ 地形切換完成！替換了 {replacedCount} 塊地形");
        }

        yield return new WaitForSeconds(0.5f);
        isSwitching = false;
    }

    /// <summary>
    /// 顯示閃光特效
    /// </summary>
    IEnumerator ShowFlashEffect()
    {
        // 如果有自訂特效
        if (switchFlashPrefab != null && player != null)
        {
            GameObject flash = Instantiate(switchFlashPrefab, player.position, Quaternion.identity);
            Destroy(flash, flashDuration);
            yield return new WaitForSeconds(flashDuration * 0.5f);
        }
        else
        {
            // 內建閃光效果：讓所有地形塊閃白
            List<SpriteRenderer> renderers = new List<SpriteRenderer>();
            List<Color> originalColors = new List<Color>();

            foreach (TerrainChunkData chunkData in activeChunks)
            {
                if (chunkData.gameObject != null)
                {
                    SpriteRenderer sr = chunkData.gameObject.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        renderers.Add(sr);
                        originalColors.Add(sr.color);
                    }
                }
            }

            // 閃白
            float elapsed = 0f;
            float halfDuration = flashDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;

                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].color = Color.Lerp(originalColors[i], flashColor, t);
                    }
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            // 恢復顏色（新地形會自動有正確顏色）
        }
    }

    /// <summary>
    /// 手動觸發地形切換（測試用）
    /// </summary>
    [ContextMenu("測試切換地形")]
    public void TestSwitch()
    {
        if (!isSwitching)
        {
            StartCoroutine(SwitchAllTerrainType());
        }
    }

    /// <summary>
    /// 重置血量觸發記錄
    /// </summary>
    public void ResetHealthTriggers()
    {
        triggeredHealthThresholds.Clear();
        if (showDebugLog)
            Debug.Log("[InfiniteTerrain] 已重置血量觸發記錄");
    }

    public int GetCurrentTerrainType() => currentTerrainTypeIndex;
    public int GetActiveChunkCount() => activeChunks.Count;
    public bool IsSwitching() => isSwitching;

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 顯示生成範圍
        if (player != null)
        {
            Gizmos.color = Color.green;
            Vector3 playerPos = player.position;

            // 前方範圍
            Vector3 aheadEnd = new Vector3(playerPos.x + chunksAhead * chunkWidth, playerPos.y, 0f);
            Gizmos.DrawLine(playerPos, aheadEnd);

            // 後方範圍
            Gizmos.color = Color.yellow;
            Vector3 behindEnd = new Vector3(playerPos.x - chunksBehind * chunkWidth, playerPos.y, 0f);
            Gizmos.DrawLine(playerPos, behindEnd);
        }

        // 顯示地形塊
        if (Application.isPlaying && activeChunks != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            foreach (TerrainChunkData chunkData in activeChunks)
            {
                if (chunkData.gameObject != null)
                {
                    Vector3 pos = chunkData.gameObject.transform.position;
                    Gizmos.DrawWireCube(pos, new Vector3(chunkWidth, chunkHeight, 0.1f));
                }
            }
        }
    }
}