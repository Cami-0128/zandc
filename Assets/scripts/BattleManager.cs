// ========================================
// BattleManager.cs
// 管理戰鬥結果並切換場景
// 放在 BossBattle 場景中的 GameManager 或類似物件上
// ========================================
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("戰鬥管理")]
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private BossController bossController;

    [Header("玩家血量檢測")]
    [SerializeField] private EnemyHealthBar playerHealthBar;

    private bool battleEnded = false;
    private float checkInterval = 0.5f;
    private float lastCheckTime = 0f;

    private void Start()
    {
        // 自動尋找玩家和 Boss
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController2D>();
                Debug.Log("[BattleManager] ✅ 找到玩家");
            }
            else
            {
                Debug.LogError("[BattleManager] ❌ 找不到玩家（標籤為'Player'）");
            }
        }

        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
            if (bossController != null)
            {
                Debug.Log("[BattleManager] ✅ 找到 Boss");
            }
            else
            {
                Debug.LogError("[BattleManager] ❌ 找不到 Boss");
            }
        }

        if (playerHealthBar == null)
        {
            playerHealthBar = FindObjectOfType<EnemyHealthBar>();
            Debug.Log(playerHealthBar != null ?
                "[BattleManager] ✅ 找到玩家血條" :
                "[BattleManager] ⚠️ 找不到玩家血條（非必要）");
        }

        Debug.Log("[BattleManager] 戰鬥管理器初始化完成");
    }

    private void Update()
    {
        if (battleEnded) return;

        // 每 0.5 秒檢查一次戰鬥結果
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;
            CheckBattleResult();
        }
    }

    private void CheckBattleResult()
    {
        // 檢查 Boss 是否死亡
        if (bossController != null && bossController.isDead)
        {
            Debug.Log("[BattleManager] ✅ 玩家贏！Boss 已被擊敗");
            EndBattle(true);
            return;
        }

        // 檢查玩家是否死亡
        if (playerController != null && playerController.isDead)
        {
            Debug.Log("[BattleManager] ❌ 玩家輸！玩家已被擊敗");
            EndBattle(false);
            return;
        }

        // 使用公開方法檢查 Boss 血量
        if (bossController != null && bossController.GetCurrentHealth() <= 0)
        {
            Debug.Log("[BattleManager] ✅ 玩家贏！（血量檢測）");
            EndBattle(true);
            return;
        }
    }

    private void EndBattle(bool playerWon)
    {
        if (battleEnded) return;

        battleEnded = true;

        // 停止玩家和 Boss 的行動
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (bossController != null)
        {
            bossController.canMove = false;
        }

        // 延遲後切換場景
        StartCoroutine(TransitionToEndingScene(playerWon, 1f));
    }

    private System.Collections.IEnumerator TransitionToEndingScene(bool playerWon, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerWon)
        {
            Debug.Log("[BattleManager] 加載 BossWinEnding 場景...");
            SceneManager.LoadScene("BossWinEnding");
        }
        else
        {
            Debug.Log("[BattleManager] 加載 BossLoseEnding 場景...");
            SceneManager.LoadScene("BossLoseEnding");
        }
    }

    // 公開方法，方便外部呼叫
    public void PlayerWon()
    {
        Debug.Log("[BattleManager] 外部呼叫：玩家勝利");
        EndBattle(true);
    }

    public void PlayerLost()
    {
        Debug.Log("[BattleManager] 外部呼叫：玩家失敗");
        EndBattle(false);
    }
}