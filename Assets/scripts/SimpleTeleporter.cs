using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTeleporter : MonoBehaviour
{
    [Header("玩家設定")]
    public Transform player;

    [Header("傳送點設定")]
    public List<Transform> teleportPoints = new List<Transform>();

    [Header("選擇設定")]
    public float selectionTime = 3f; // 選擇時間

    private bool isSelecting = false;
    private Coroutine selectionCoroutine;

    void Awake()
    {
        // 讓TeleportManager在切換關卡時不被銷毀
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        RefreshTeleportSystem();
    }

    void OnLevelWasLoaded(int level)
    {
        // 每次載入新關卡時重新設置
        RefreshTeleportSystem();
    }

    void RefreshTeleportSystem()
    {
        // 重新尋找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // 重新收集當前關卡的傳送點
        teleportPoints.Clear();
        GameObject[] points = GameObject.FindGameObjectsWithTag("TeleportPoint");
        foreach (GameObject point in points)
        {
            teleportPoints.Add(point.transform);
        }

        Debug.Log($"找到 {teleportPoints.Count} 個傳送點");
    }

    void Update()
    {
        // 檢查是否同時按住L、O、P
        if (Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.P))
        {
            // 防止重複觸發，只在其中一個鍵剛按下時觸發
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P))
            {
                StartSelection();
            }
        }

        // 在選擇模式中檢查數字鍵
        if (isSelecting)
        {
            CheckNumberKeys();
        }
    }

    void StartSelection()
    {
        if (teleportPoints.Count == 0 || player == null)
        {
            Debug.Log("沒有設置傳送點或玩家！");
            return;
        }

        // 先傳送到第一個點
        TeleportToPoint(0);
        Debug.Log("傳送到第1個點！請在3秒內按數字鍵選擇其他傳送點");

        // 開始選擇模式
        isSelecting = true;
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
        }
        selectionCoroutine = StartCoroutine(SelectionTimer());
    }

    void CheckNumberKeys()
    {
        // 檢查數字鍵1-9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectTeleportPoint(i);
                return;
            }
        }

        // 檢查數字鍵0 (對應第10個傳送點)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectTeleportPoint(10);
        }
    }

    void SelectTeleportPoint(int number)
    {
        int index = number - 1; // 轉換為陣列索引

        if (index < 0 || index >= teleportPoints.Count)
        {
            Debug.Log($"傳送點 {number} 不存在！(共有 {teleportPoints.Count} 個傳送點)");
            return;
        }

        TeleportToPoint(index);
        Debug.Log($"傳送到第 {number} 個點！");

        // 結束選擇模式
        EndSelection();
    }

    void TeleportToPoint(int index)
    {
        if (index < 0 || index >= teleportPoints.Count) return;

        // 獲取目標位置
        Vector3 targetPosition = teleportPoints[index].position;

        // 如果玩家有CharacterController組件
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = targetPosition;
            cc.enabled = true;
        }
        else
        {
            // 直接移動
            player.position = targetPosition;
        }
    }

    IEnumerator SelectionTimer()
    {
        yield return new WaitForSeconds(selectionTime);

        if (isSelecting)
        {
            Debug.Log("選擇時間結束！");
            EndSelection();
        }
    }

    void EndSelection()
    {
        isSelecting = false;
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }
    }

    // 手動重新整理系統（可選）
    [ContextMenu("重新整理傳送系統")]
    public void ManualRefresh()
    {
        RefreshTeleportSystem();
    }
}