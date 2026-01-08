using UnityEngine;

/// <summary>
/// 尖刺測試管理器 - 用於快速測試尖刺功能
/// 測試完成後可以刪除此腳本
/// </summary>
public class SpikeTestManager : MonoBehaviour
{
    [Header("測試設定")]
    public GroundSpikeTrap[] testSpikes;
    public KeyCode triggerKey = KeyCode.Space;
    public KeyCode triggerAllKey = KeyCode.T;

    [Header("測試選項")]
    public bool testSingleSpike = true;
    public int testSpikeIndex = 0;

    void Update()
    {
        // 按空白鍵測試單個尖刺
        if (Input.GetKeyDown(triggerKey))
        {
            TestSingleSpike();
        }

        // 按T鍵測試所有尖刺
        if (Input.GetKeyDown(triggerAllKey))
        {
            TestAllSpikes();
        }
    }

    /// <summary>
    /// 測試單個尖刺
    /// </summary>
    void TestSingleSpike()
    {
        if (testSpikes == null || testSpikes.Length == 0)
        {
            Debug.LogError("[SpikeTest] 沒有設定測試尖刺！");
            return;
        }

        if (testSpikeIndex < 0 || testSpikeIndex >= testSpikes.Length)
        {
            Debug.LogError($"[SpikeTest] 索引超出範圍：{testSpikeIndex}");
            return;
        }

        GroundSpikeTrap spike = testSpikes[testSpikeIndex];
        if (spike != null)
        {
            Debug.Log($"[SpikeTest] 觸發尖刺 #{testSpikeIndex}");
            spike.Trigger();
        }
        else
        {
            Debug.LogError($"[SpikeTest] 尖刺 #{testSpikeIndex} 為 null");
        }
    }

    /// <summary>
    /// 測試所有尖刺（依序觸發）
    /// </summary>
    void TestAllSpikes()
    {
        if (testSpikes == null || testSpikes.Length == 0)
        {
            Debug.LogError("[SpikeTest] 沒有設定測試尖刺！");
            return;
        }

        Debug.Log($"[SpikeTest] 觸發所有尖刺（共 {testSpikes.Length} 個）");

        for (int i = 0; i < testSpikes.Length; i++)
        {
            if (testSpikes[i] != null)
            {
                testSpikes[i].Trigger();
            }
        }
    }

    void OnGUI()
    {
        // 顯示測試說明
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 500, 30),
            $"按 [{triggerKey}] 測試單個尖刺 (索引: {testSpikeIndex})", style);
        GUI.Label(new Rect(10, 40, 500, 30),
            $"按 [{triggerAllKey}] 測試所有尖刺", style);
    }
}