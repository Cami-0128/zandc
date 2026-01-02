//using UnityEngine;

//public class BloodlineManager : MonoBehaviour
//{
//    public static BloodlineManager Instance { get; private set; }

//    [Header("覺醒狀態")]
//    public bool isAwakened = false;

//    [Header("覺醒效果設定")]
//    [Tooltip("血量回復間隔（秒）")]
//    public float healthRegenInterval = 5f;
//    [Tooltip("每次回復的血量")]
//    public int healthRegenAmount = 1;

//    [Tooltip("魔力回復間隔（秒）")]
//    public float manaRegenInterval = 10f;
//    [Tooltip("每次回復的魔力")]
//    public int manaRegenAmount = 1;

//    [Tooltip("速度倍率（1.5 = 150%）")]
//    public float speedMultiplier = 1.5f;

//    [Tooltip("沙漏時間倍率（0.5 = 砍半）")]
//    public float timeReductionMultiplier = 0.5f;

//    [Header("覺醒條件（測試用）")]
//    [Tooltip("需要通關的關卡數")]
//    public int requiredLevel = 3;
//    [Tooltip("是否需要收集道具")]
//    public bool requireSpecialItem = true;
//    [Tooltip("玩家是否已收集特殊道具")]
//    public bool hasSpecialItem = false;

//    [Header("測試模式")]
//    [Tooltip("按 K 鍵強制觸發覺醒選擇")]
//    public bool enableTestMode = true;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//            LoadBloodlineState();
//            Debug.Log("[BloodlineManager] 血統管理器已初始化");
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void Update()
//    {
//        // 測試模式：按 K 鍵強制觸發覺醒選擇
//        if (enableTestMode && Input.GetKeyDown(KeyCode.K))
//        {
//            Debug.Log("[測試模式] 強制觸發覺醒選擇視窗");
//            ShowAwakeningUI();
//        }
//    }

//    // 檢查是否可以覺醒
//    public bool CanAwaken(int currentLevel)
//    {
//        if (isAwakened)
//        {
//            Debug.Log("[BloodlineManager] 已經覺醒過了");
//            return false;
//        }

//        bool levelCondition = currentLevel >= requiredLevel;
//        bool itemCondition = !requireSpecialItem || hasSpecialItem;

//        Debug.Log($"[BloodlineManager] 覺醒條件檢查 - 關卡: {levelCondition} ({currentLevel}/{requiredLevel}), 道具: {itemCondition}");

//        return levelCondition && itemCondition;
//    }

//    // 覺醒血統
//    public void AwakenBloodline()
//    {
//        isAwakened = true;
//        SaveBloodlineState();

//        Debug.Log("=== 血統覺醒成功！===");
//        Debug.Log($"效果：血量每{healthRegenInterval}秒回復{healthRegenAmount}點");
//        Debug.Log($"效果：魔力每{manaRegenInterval}秒回復{manaRegenAmount}點");
//        Debug.Log($"效果：移動速度提升至 {speedMultiplier}倍");
//        Debug.Log($"效果：沙漏時間減少至 {timeReductionMultiplier}倍");

//        // 立即應用效果到當前場景
//        ApplyAwakeningEffects();
//    }

//    // 應用覺醒效果到所有系統
//    public void ApplyAwakeningEffects()
//    {
//        // 應用到玩家
//        PlayerController2D player = FindObjectOfType<PlayerController2D>();
//        if (player != null)
//        {
//            player.ApplyBloodlineBonus();
//            Debug.Log("[BloodlineManager] 已應用覺醒效果到 PlayerController2D");
//        }

//        // 應用到攻擊系統
//        PlayerAttack attack = FindObjectOfType<PlayerAttack>();
//        if (attack != null)
//        {
//            attack.ApplyBloodlineBonus();
//            Debug.Log("[BloodlineManager] 已應用覺醒效果到 PlayerAttack");
//        }

//        // 應用到沙漏計時器
//        HourglassTimer timer = FindObjectOfType<HourglassTimer>();
//        if (timer != null)
//        {
//            timer.ApplyBloodlineBonus();
//            Debug.Log("[BloodlineManager] 已應用覺醒效果到 HourglassTimer");
//        }
//    }

//    // 顯示覺醒UI
//    private void ShowAwakeningUI()
//    {
//        BloodlineAwakeningUI ui = FindObjectOfType<BloodlineAwakeningUI>();
//        if (ui != null)
//        {
//            ui.ShowAwakeningPanel();
//        }
//        else
//        {
//            Debug.LogWarning("[BloodlineManager] 找不到 BloodlineAwakeningUI！");
//        }
//    }

//    // 收集特殊道具（之後完善）
//    public void CollectSpecialItem()
//    {
//        hasSpecialItem = true;
//        Debug.Log("[BloodlineManager] 已收集特殊道具！");
//    }

//    // 儲存覺醒狀態
//    private void SaveBloodlineState()
//    {
//        PlayerPrefs.SetInt("BloodlineAwakened", isAwakened ? 1 : 0);
//        PlayerPrefs.Save();
//        Debug.Log("[BloodlineManager] 覺醒狀態已儲存");
//    }

//    // 讀取覺醒狀態
//    private void LoadBloodlineState()
//    {
//        isAwakened = PlayerPrefs.GetInt("BloodlineAwakened", 0) == 1;
//        if (isAwakened)
//        {
//            Debug.Log("[BloodlineManager] 讀取到已覺醒狀態");
//        }
//    }

//    // 重置覺醒（測試用）
//    public void ResetBloodline()
//    {
//        isAwakened = false;
//        hasSpecialItem = false;
//        PlayerPrefs.DeleteKey("BloodlineAwakened");
//        PlayerPrefs.Save();
//        Debug.Log("[BloodlineManager] 血統已重置");
//    }
//}