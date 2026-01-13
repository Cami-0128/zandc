using UnityEngine;

/// <summary>
/// 尖刺與Boss測試工具
/// </summary>
public class SpikeQuickTest : MonoBehaviour
{
    public BossController boss;

    void Update()
    {
        // 按K鍵：直接觸發所有尖刺
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("========== 按K鍵：測試所有尖刺 ==========");

            GroundSpikeTrap[] spikes = FindObjectsOfType<GroundSpikeTrap>();

            Debug.Log($"場景中找到 {spikes.Length} 個尖刺");

            foreach (GroundSpikeTrap spike in spikes)
            {
                Debug.Log($"手動觸發尖刺: {spike.name}");
                spike.Trigger();
            }
        }

        // 按L鍵：讓Boss觸發尖刺（測試Boss的TriggerNearbySpikes函數）
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("========== 按L鍵：測試Boss觸發尖刺 ==========");

            if (boss == null)
            {
                boss = FindObjectOfType<BossController>();
            }

            if (boss == null)
            {
                Debug.LogError("找不到Boss！");
                return;
            }

            Debug.Log("呼叫 Boss.TriggerNearbySpikes()");

            // 使用反射調用私有方法
            var method = boss.GetType().GetMethod("TriggerNearbySpikes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(boss, null);
            }
            else
            {
                Debug.LogError("找不到 TriggerNearbySpikes 方法！");
            }
        }

        // 按M鍵：顯示Boss和玩家的距離
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("========== 按M鍵：檢查距離 ==========");

            if (boss == null)
            {
                boss = FindObjectOfType<BossController>();
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (boss != null && player != null)
            {
                float distance = Vector2.Distance(boss.transform.position, player.transform.position);
                Debug.Log($"Boss位置: {boss.transform.position}");
                Debug.Log($"玩家位置: {player.transform.position}");
                Debug.Log($"距離: {distance:F2}m");
                Debug.Log($"Boss的 playerTooCloseDistance: {boss.playerTooCloseDistance}");
                Debug.Log($"是否太近: {distance <= boss.playerTooCloseDistance}");
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 500, 30), "K鍵：直接觸發所有尖刺", style);
        GUI.Label(new Rect(10, 40, 500, 30), "L鍵：讓Boss觸發尖刺", style);
        GUI.Label(new Rect(10, 70, 500, 30), "M鍵：顯示Boss和玩家距離", style);
        GUI.Label(new Rect(10, 100, 500, 30), "T鍵：開始遊戲", style);
    }
}