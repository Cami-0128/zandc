using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 放在Canvas上，管理問題UI的顯示和互動
/// 改進版：增強除錯、修正按鈕問題、支援UI縮放
/// </summary>
public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI 組件 - 必須拖入")]
    public GameObject puzzlePanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;  // 最多4個按鈕
    public TextMeshProUGUI feedbackText;
    public GameObject feedbackPanel;

    [Header("UI 尺寸設定")]
    [Tooltip("問題面板的縮放比例 (0.5 = 50%大小, 1 = 100%大小)")]
    [Range(0.3f, 1.5f)]
    public float panelScale = 0.6f;

    [Header("音效 (可選)")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    private AudioSource audioSource;

    [Header("顯示設定")]
    public float feedbackDisplayTime = 2f;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("除錯模式")]
    public bool debugMode = true;

    private PuzzleQuestion currentQuestion;
    private PuzzleBlock currentBlock;
    private PlayerController2D playerController;
    private bool isAnswering = false;

    void Awake()
    {
        // 確保 Canvas 設定正確
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning("[PuzzleUI] Canvas 建議設為 Screen Space - Overlay");
            }

            // 確保有 GraphicRaycaster
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("[PuzzleUI] 自動添加 GraphicRaycaster");
            }
        }
    }

    void Start()
    {
        if (debugMode)
            Debug.Log("========== [PuzzleUI] 開始初始化 ==========");

        // 驗證必要組件
        ValidateComponents();

        // 初始化音效
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // 設定面板縮放和隱藏
        if (puzzlePanel != null)
        {
            puzzlePanel.transform.localScale = Vector3.one * panelScale;
            puzzlePanel.SetActive(false);
            if (debugMode)
                Debug.Log($"[PuzzleUI] 面板縮放設為 {panelScale}");
        }

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // 檢查並創建 EventSystem
        CheckEventSystem();

        // 設定按鈕事件
        SetupButtons();

        // 取得玩家控制器
        playerController = FindObjectOfType<PlayerController2D>();
        if (playerController == null)
        {
            Debug.LogWarning("[PuzzleUI] 找不到 PlayerController2D");
        }

        if (debugMode)
            Debug.Log("========== [PuzzleUI] 初始化完成 ==========");
    }

    /// <summary>
    /// 驗證組件是否正確設定
    /// </summary>
    void ValidateComponents()
    {
        bool hasError = false;

        if (puzzlePanel == null)
        {
            Debug.LogError("[PuzzleUI] ❌ PuzzlePanel 未設定！");
            hasError = true;
        }

        if (questionText == null)
        {
            Debug.LogError("[PuzzleUI] ❌ QuestionText 未設定！");
            hasError = true;
        }

        if (answerButtons == null || answerButtons.Length == 0)
        {
            Debug.LogError("[PuzzleUI] ❌ AnswerButtons 未設定！");
            hasError = true;
        }
        else
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null)
                {
                    Debug.LogWarning($"[PuzzleUI] ⚠️ AnswerButton[{i}] 為空");
                }
            }
        }

        if (feedbackText == null)
        {
            Debug.LogWarning("[PuzzleUI] ⚠️ FeedbackText 未設定");
        }

        if (feedbackPanel == null)
        {
            Debug.LogWarning("[PuzzleUI] ⚠️ FeedbackPanel 未設定");
        }

        if (!hasError)
        {
            Debug.Log("[PuzzleUI] ✓ 所有必要組件已設定");
        }
    }

    /// <summary>
    /// 檢查 EventSystem
    /// </summary>
    void CheckEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[PuzzleUI] ✓ 自動創建 EventSystem");
        }
        else
        {
            if (debugMode)
                Debug.Log("[PuzzleUI] ✓ EventSystem 存在");
        }
    }

    /// <summary>
    /// 設定按鈕事件
    /// </summary>
    void SetupButtons()
    {
        if (answerButtons == null || answerButtons.Length == 0)
            return;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            int index = i;  // 防止閉包問題

            // 清除舊事件
            answerButtons[i].onClick.RemoveAllListeners();

            // 添加新事件
            answerButtons[i].onClick.AddListener(() => {
                if (debugMode)
                    Debug.Log($"[PuzzleUI] 🖱️ 按鈕 {index} 被點擊！");
                OnAnswerSelected(index);
            });

            // 驗證按鈕設定
            if (!answerButtons[i].interactable)
            {
                Debug.LogWarning($"[PuzzleUI] 按鈕 {i} 的 Interactable 未勾選");
            }

            // 檢查按鈕是否有 Image
            Image btnImage = answerButtons[i].GetComponent<Image>();
            if (btnImage == null)
            {
                Debug.LogWarning($"[PuzzleUI] 按鈕 {i} 沒有 Image 組件，可能無法點擊");
            }

            if (debugMode)
                Debug.Log($"[PuzzleUI] ✓ 按鈕 {i} 事件已綁定");
        }
    }

    /// <summary>
    /// 顯示問題UI
    /// </summary>
    public void ShowPuzzle(PuzzleQuestion question, PuzzleBlock block)
    {
        if (debugMode)
            Debug.Log("========== [PuzzleUI] 顯示問題 ==========");

        if (question == null)
        {
            Debug.LogError("[PuzzleUI] ❌ 問題資料為空！");
            return;
        }

        if (puzzlePanel == null)
        {
            Debug.LogError("[PuzzleUI] ❌ PuzzlePanel 為空，無法顯示UI");
            return;
        }

        currentQuestion = question;
        currentBlock = block;
        isAnswering = false;

        // 暫停遊戲（使用 unscaled time）
        Time.timeScale = 0f;
        if (debugMode)
            Debug.Log("[PuzzleUI] ⏸️ 遊戲已暫停");

        // 停止玩家控制
        if (playerController != null)
        {
            playerController.canControl = false;
        }

        // 顯示問題面板
        puzzlePanel.SetActive(true);

        // 設定問題文字
        if (questionText != null)
        {
            questionText.text = question.questionText;
            if (debugMode)
                Debug.Log($"[PuzzleUI] 問題: {question.questionText}");
        }

        // 設定答案按鈕
        SetupAnswerButtons(question);

        // 隱藏反饋面板
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        if (debugMode)
            Debug.Log("========== [PuzzleUI] 問題顯示完成 ==========");
    }

    /// <summary>
    /// 設定答案按鈕的文字和顯示
    /// </summary>
    void SetupAnswerButtons(PuzzleQuestion question)
    {
        int activeCount = 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            // 檢查是否有有效的答案選項
            bool hasAnswer = i < question.answerOptions.Length &&
                           !string.IsNullOrEmpty(question.answerOptions[i]);

            if (hasAnswer)
            {
                // 顯示並啟用按鈕
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;

                // 設定按鈕文字
                SetButtonText(answerButtons[i], question.answerOptions[i]);

                activeCount++;

                if (debugMode)
                    Debug.Log($"[PuzzleUI] 按鈕 {i}: {question.answerOptions[i]}");
            }
            else
            {
                // 隱藏按鈕
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        if (debugMode)
            Debug.Log($"[PuzzleUI] 啟用 {activeCount} 個按鈕");
    }

    /// <summary>
    /// 設定按鈕文字（支援 TextMeshPro 和傳統 Text）
    /// </summary>
    void SetButtonText(Button button, string text)
    {
        TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = text;
            return;
        }

        Text legacyText = button.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = text;
            return;
        }

        Debug.LogWarning($"[PuzzleUI] 按鈕 {button.name} 找不到文字組件");
    }

    /// <summary>
    /// 當選擇答案時 - 核心函數
    /// </summary>
    public void OnAnswerSelected(int answerIndex)
    {
        if (debugMode)
            Debug.Log($"========== [PuzzleUI] OnAnswerSelected 被調用: 索引 {answerIndex} ==========");

        // 防止重複點擊
        if (isAnswering)
        {
            Debug.Log("[PuzzleUI] ⚠️ 已在處理答案中，忽略重複點擊");
            return;
        }

        if (currentQuestion == null)
        {
            Debug.LogError("[PuzzleUI] ❌ currentQuestion 為空！");
            return;
        }

        isAnswering = true;

        // 禁用所有按鈕
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }

        // 判斷答案正確性
        bool isCorrect = (answerIndex == currentQuestion.correctAnswerIndex);

        if (debugMode)
        {
            Debug.Log($"[PuzzleUI] 選擇答案: {answerIndex}");
            Debug.Log($"[PuzzleUI] 正確答案: {currentQuestion.correctAnswerIndex}");
            Debug.Log($"[PuzzleUI] 結果: {(isCorrect ? "✓ 正確" : "✗ 錯誤")}");
        }

        // 處理答案
        if (isCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }

        // 開始反饋流程
        StartCoroutine(ShowFeedbackAndClose(isCorrect));
    }

    /// <summary>
    /// 處理正確答案
    /// </summary>
    void HandleCorrectAnswer()
    {
        if (debugMode)
            Debug.Log("[PuzzleUI] 🎉 處理正確答案");

        // 播放音效
        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        // 給予獎勵
        if (currentBlock != null)
        {
            currentBlock.GiveReward();
        }
        else
        {
            Debug.LogWarning("[PuzzleUI] currentBlock 為空");
        }

        // 顯示反饋
        ShowFeedback(currentQuestion.correctMessage, correctColor);
    }

    /// <summary>
    /// 處理錯誤答案
    /// </summary>
    void HandleWrongAnswer()
    {
        if (debugMode)
            Debug.Log("[PuzzleUI] 💔 處理錯誤答案");

        // 播放音效
        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        // 施加懲罰
        if (currentBlock != null)
        {
            currentBlock.ApplyPenalty();
        }
        else
        {
            Debug.LogWarning("[PuzzleUI] currentBlock 為空");
        }

        // 顯示反饋
        ShowFeedback(currentQuestion.wrongMessage, wrongColor);
    }

    /// <summary>
    /// 顯示反饋訊息
    /// </summary>
    void ShowFeedback(string message, Color color)
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }

        if (debugMode)
            Debug.Log($"[PuzzleUI] 顯示反饋: {message}");
    }

    /// <summary>
    /// 顯示反饋並關閉UI
    /// </summary>
    IEnumerator ShowFeedbackAndClose(bool isCorrect)
    {
        // 使用 unscaled time 因為遊戲已暫停
        yield return new WaitForSecondsRealtime(feedbackDisplayTime);

        // 關閉UI
        ClosePuzzle();

        // 通知方塊答題完成
        if (currentBlock != null)
        {
            currentBlock.OnAnswered();
        }

        isAnswering = false;

        if (debugMode)
            Debug.Log("[PuzzleUI] 反饋流程結束");
    }

    /// <summary>
    /// 關閉問題UI
    /// </summary>
    public void ClosePuzzle()
    {
        if (debugMode)
            Debug.Log("[PuzzleUI] 🚪 關閉UI");

        // 隱藏面板
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // 恢復遊戲
        Time.timeScale = 1f;

        // 恢復玩家控制
        if (playerController != null)
        {
            playerController.canControl = true;
        }

        // 清除當前問題
        currentQuestion = null;
        currentBlock = null;
    }

    /// <summary>
    /// 測試用：按 T 鍵手動觸發第一個按鈕
    /// </summary>
    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.T))
        {
            if (puzzlePanel != null && puzzlePanel.activeSelf)
            {
                Debug.Log("[PuzzleUI] 🧪 測試：手動觸發按鈕 0");
                OnAnswerSelected(0);
            }
        }
    }
}