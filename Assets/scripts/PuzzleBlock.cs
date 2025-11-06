using UnityEngine;

/// <summary>
/// 放在觸發問題的方塊上
/// 需要添加 Collider2D 並勾選 Is Trigger
/// 版本：修正版 - 答對後不再顯示，答錯可重答
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PuzzleBlock : MonoBehaviour
{
    [Header("問題設定")]
    [Tooltip("指定這個方塊觸發的問題")]
    public PuzzleQuestion puzzleQuestion;

    [Header("視覺效果")]
    public Color highlightColor = Color.yellow;
    public Color completedColor = Color.gray;
    public bool enableHighlight = true;

    [Header("路障物件 (如果獎勵是移除路障)")]
    [Tooltip("要移除的路障物件，留空則根據Tag尋找")]
    public GameObject barrierObject;

    [Header("音效 (可選)")]
    public AudioClip triggerSound;

    private bool hasAnsweredCorrectly = false; // 是否已答對
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;

    void Start()
    {
        // 確保有 Collider2D 且設為 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 取得顏色組件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 音效設定
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && triggerSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 驗證問題設定
        if (puzzleQuestion == null)
        {
            Debug.LogWarning($"[PuzzleBlock] {gameObject.name} 沒有設定問題！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 檢查是否為玩家
        if (!other.CompareTag("Player"))
            return;

        // 如果已經答對，不再顯示問題
        if (hasAnsweredCorrectly)
        {
            Debug.Log($"[PuzzleBlock] {gameObject.name} 已經答對過了，不再顯示問題");
            return;
        }

        // 檢查是否有設定問題
        if (puzzleQuestion == null)
        {
            Debug.LogError($"[PuzzleBlock] {gameObject.name} 沒有設定問題資料！");
            return;
        }

        // 播放音效
        if (triggerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }

        // 顯示問題UI
        PuzzleUIManager uiManager = FindObjectOfType<PuzzleUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowPuzzle(puzzleQuestion, this);
        }
        else
        {
            Debug.LogError("[PuzzleBlock] 找不到 PuzzleUIManager！請確保場景中有此組件。");
        }

        Debug.Log($"[PuzzleBlock] 觸發問題: {puzzleQuestion.questionText}");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // 當玩家停留在方塊上時，顯示高亮效果
        if (other.CompareTag("Player") && !hasAnsweredCorrectly && enableHighlight)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 玩家離開時恢復顏色
        if (other.CompareTag("Player") && enableHighlight)
        {
            if (spriteRenderer != null && !hasAnsweredCorrectly)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// 給予獎勵
    /// </summary>
    public void GiveReward()
    {
        switch (puzzleQuestion.rewardType)
        {
            case RewardType.AddCoins:
                CoinManager coinManager = CoinManager.Instance;
                if (coinManager != null)
                {
                    coinManager.AddMoney(puzzleQuestion.rewardAmount);
                    Debug.Log($"[獎勵] 獲得 {puzzleQuestion.rewardAmount} 金幣");
                }
                else
                {
                    Debug.LogWarning("[獎勵] 找不到 CoinManager");
                }
                break;

            case RewardType.AddHealth:
                PlayerController2D player = FindObjectOfType<PlayerController2D>();
                if (player != null)
                {
                    player.Heal(puzzleQuestion.rewardAmount);
                    Debug.Log($"[獎勵] 回復 {puzzleQuestion.rewardAmount} 血量");
                }
                else
                {
                    Debug.LogWarning("[獎勵] 找不到 PlayerController2D");
                }
                break;

            case RewardType.AddMana:
                PlayerController2D playerForMana = FindObjectOfType<PlayerController2D>();
                if (playerForMana != null)
                {
                    playerForMana.ManaHeal(puzzleQuestion.rewardAmount);
                    Debug.Log($"[獎勵] 回復 {puzzleQuestion.rewardAmount} 魔力");
                }
                else
                {
                    Debug.LogWarning("[獎勵] 找不到 PlayerController2D");
                }
                break;

            case RewardType.RemoveBarrier:
                RemoveBarrier();
                break;

            case RewardType.None:
                Debug.Log("[獎勵] 無獎勵");
                break;

            case RewardType.Custom:
                Debug.Log("[獎勵] 自定義獎勵（請在程式碼中實作）");
                break;
        }
    }

    /// <summary>
    /// 施加懲罰
    /// </summary>
    public void ApplyPenalty()
    {
        if (!puzzleQuestion.hasPenalty)
        {
            Debug.Log("[懲罰] 無懲罰");
            return;
        }

        switch (puzzleQuestion.penaltyType)
        {
            case PenaltyType.ReduceHealth:
                PlayerController2D player = FindObjectOfType<PlayerController2D>();
                if (player != null)
                {
                    player.TakeDamage(puzzleQuestion.penaltyAmount);
                    Debug.Log($"[懲罰] 扣除 {puzzleQuestion.penaltyAmount} 血量");
                }
                else
                {
                    Debug.LogWarning("[懲罰] 找不到 PlayerController2D");
                }
                break;

            case PenaltyType.ReduceMana:
                PlayerAttack attack = FindObjectOfType<PlayerAttack>();
                if (attack != null)
                {
                    int currentMana = attack.GetCurrentMana();
                    int newMana = Mathf.Max(0, currentMana - puzzleQuestion.penaltyAmount);
                    // 使用負數來扣除魔力
                    int actualReduction = currentMana - newMana;
                    if (actualReduction > 0)
                    {
                        attack.RestoreMana(-actualReduction);
                    }
                    Debug.Log($"[懲罰] 扣除 {actualReduction} 魔力");
                }
                else
                {
                    Debug.LogWarning("[懲罰] 找不到 PlayerAttack");
                }
                break;

            case PenaltyType.ReduceCoins:
                CoinManager coinManager = CoinManager.Instance;
                if (coinManager != null)
                {
                    coinManager.SpendMoney(puzzleQuestion.penaltyAmount);
                    Debug.Log($"[懲罰] 扣除 {puzzleQuestion.penaltyAmount} 金幣");
                }
                else
                {
                    Debug.LogWarning("[懲罰] 找不到 CoinManager");
                }
                break;

            case PenaltyType.None:
                Debug.Log("[懲罰] 無懲罰");
                break;

            case PenaltyType.Custom:
                Debug.Log("[懲罰] 自定義懲罰（請在程式碼中實作）");
                break;
        }
    }

    /// <summary>
    /// 移除路障
    /// </summary>
    private void RemoveBarrier()
    {
        // 優先使用指定的路障物件
        if (barrierObject != null)
        {
            Debug.Log($"[獎勵] 移除路障: {barrierObject.name}");
            Destroy(barrierObject);
            return;
        }

        // 根據 Tag 尋找路障
        GameObject barrier = GameObject.FindGameObjectWithTag(puzzleQuestion.barrierTag);
        if (barrier != null)
        {
            Debug.Log($"[獎勵] 根據Tag移除路障: {barrier.name}");
            Destroy(barrier);
        }
        else
        {
            Debug.LogWarning($"[獎勵] 找不到Tag為 '{puzzleQuestion.barrierTag}' 的路障物件");
        }
    }

    /// <summary>
    /// 答題後處理（由 UIManager 調用）
    /// </summary>
    /// <param name="isCorrect">是否答對</param>
    public void OnAnswered(bool isCorrect)
    {
        if (isCorrect)
        {
            // 答對了，標記為已完成
            hasAnsweredCorrectly = true;
            Debug.Log($"[PuzzleBlock] ✓ {gameObject.name} 答對了！此方塊不再顯示問題");

            // 如果設定為答對後銷毀方塊
            if (puzzleQuestion.destroyBlockAfterAnswer)
            {
                Debug.Log($"[PuzzleBlock] 銷毀方塊: {gameObject.name}");
                Destroy(gameObject);
            }
            else
            {
                // 改變顏色表示已完成
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = completedColor;
                }
            }
        }
        else
        {
            // 答錯了，可以重新作答
            Debug.Log($"[PuzzleBlock] ✗ {gameObject.name} 答錯了，可以重新觸碰作答");

            // 恢復原本顏色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// 重置方塊狀態（供外部調用，例如重新開始關卡）
    /// </summary>
    public void ResetBlock()
    {
        hasAnsweredCorrectly = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        Debug.Log($"[PuzzleBlock] {gameObject.name} 狀態已重置");
    }
}