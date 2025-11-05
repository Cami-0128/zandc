using UnityEngine;

[CreateAssetMenu(fileName = "NewPuzzleQuestion", menuName = "Puzzle/Question")]
public class PuzzleQuestion : ScriptableObject
{
    [Header("問題內容")]
    [TextArea(3, 6)]
    public string questionText = "這是問題內容";

    [Header("選項設定 (最多4個)")]
    public string[] answerOptions = new string[4] { "選項A", "選項B", "選項C", "選項D" };

    [Tooltip("正確答案的索引 (0=第一個選項, 1=第二個, 以此類推)")]
    public int correctAnswerIndex = 0;

    [Header("獎勵設定")]
    public RewardType rewardType;
    public int rewardAmount = 10;

    [Tooltip("如果獎勵是移除路障，請指定路障物件的Tag")]
    public string barrierTag = "Barrier";

    [Header("懲罰設定")]
    public bool hasPenalty = true;
    public PenaltyType penaltyType;
    public int penaltyAmount = 5;

    [Header("其他設定")]
    [Tooltip("答題後是否銷毀觸發方塊")]
    public bool destroyBlockAfterAnswer = true;

    [Tooltip("答對後的提示訊息")]
    public string correctMessage = "答對了！獲得獎勵！";

    [Tooltip("答錯後的提示訊息")]
    public string wrongMessage = "答錯了！";
}

public enum RewardType
{
    None,           // 無獎勵
    AddCoins,       // 增加金幣
    AddHealth,      // 增加血量
    AddMana,        // 增加魔力
    RemoveBarrier,  // 移除路障
    Custom          // 自定義（可擴展）
}

public enum PenaltyType
{
    None,           // 無懲罰
    ReduceHealth,   // 減少血量
    ReduceMana,     // 減少魔力
    ReduceCoins,    // 減少金幣
    Custom          // 自定義（可擴展）
}