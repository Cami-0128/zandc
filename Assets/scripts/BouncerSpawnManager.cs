using UnityEngine;

/// <summary>
/// 彈簧生成管理器 - 用於集中管理彈簧預製體
/// 這個腳本要掛載在場景中的空物件上
/// </summary>
public class BouncerSpawnManager : MonoBehaviour
{
    [Header("=== 彈簧預製體 ===")]
    [Tooltip("拖入 Bouncer 預製體")]
    public GameObject bouncerPrefab;

    [Header("=== 生成設定 ===")]
    [Tooltip("生成距離（玩家右方多遠）")]
    public float spawnDistance = 2f;

    // Singleton 模式
    public static BouncerSpawnManager Instance { get; private set; }

    void Awake()
    {
        // Singleton 設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切換場景時不銷毀
            Debug.Log("[BouncerSpawnManager] 初始化成功");
        }
        else
        {
            Debug.LogWarning("[BouncerSpawnManager] 場景中已存在實例，銷毀多餘的物件");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 檢查預製體是否設定
        if (bouncerPrefab == null)
        {
            Debug.LogError("[BouncerSpawnManager] 未設定 Bouncer Prefab！請在 Inspector 中拖入預製體。");
        }
        else
        {
            Debug.Log($"[BouncerSpawnManager] Bouncer Prefab 已設定: {bouncerPrefab.name}");
        }
    }

    /// <summary>
    /// 創建彈簧效果（給 ShopManager 調用）
    /// </summary>
    public SimpleBouncerEffect CreateBouncerEffect()
    {
        if (bouncerPrefab == null)
        {
            Debug.LogError("[BouncerSpawnManager] Bouncer Prefab 未設定！無法創建效果。");
            return null;
        }

        Debug.Log($"[BouncerSpawnManager] 創建 SimpleBouncerEffect，距離: {spawnDistance}");
        return new SimpleBouncerEffect(bouncerPrefab, spawnDistance);
    }

    /// <summary>
    /// 獲取彈簧預製體
    /// </summary>
    public GameObject GetBouncerPrefab()
    {
        return bouncerPrefab;
    }

    /// <summary>
    /// 設定彈簧預製體（可在運行時修改）
    /// </summary>
    public void SetBouncerPrefab(GameObject prefab)
    {
        bouncerPrefab = prefab;
        Debug.Log($"[BouncerSpawnManager] 彈簧預製體已更新: {prefab.name}");
    }
}