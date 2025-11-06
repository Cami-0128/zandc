using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 問題UI管理器 - 完整診斷版本
/// </summary>
public class PuzzleUIManager : MonoBehaviour
{
    [Header("UI 組件 - 必須拖入")]
    public GameObject puzzlePanel;
    public TextMeshProUGUI questionText;
    public Text questionTextLegacy;

    [Header("答案按鈕 - 請拖入 4 個按鈕")]
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;

    [Header("反饋組件 - 不再使用，可以刪除")]
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackText;
    public Text feedbackTextLegacy;

    [Header("UI 跟隨設定")]
    public Vector3 uiOffset = new Vector3(0, 2, 0);
    public float uiScale = 0.01f;

    [Header("遊戲控制設定")]
    public bool pauseGameWhenShow = false;
    public bool stopPlayerControl = false;

    [Header("顯示設定")]
    public float feedbackDisplayTime = 2f;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public bool hideQuestionDuringFeedback = true;

    [Header("音效")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    private AudioSource audioSource;

    [Header("除錯模式")]
    public bool debugMode = true;
    public bool enableTestKey = false;

    private PuzzleQuestion currentQuestion;
    private PuzzleBlock currentBlock;
    private PlayerController2D playerController;
    private bool isAnswering = false;
    private Button[] answerButtons;
    private Transform followTarget;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        answerButtons = new Button[4];
        answerButtons[0] = answerButton1;
        answerButtons[1] = answerButton2;
        answerButtons[2] = answerButton3;
        answerButtons[3] = answerButton4;
    }

    void Start()
    {
        Debug.Log("========== [PuzzleUI] 開始初始化 ==========");

        ValidateComponents();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[PuzzleUI] ❌ FeedbackPanel 未設定！反饋訊息將無法顯示！");
        }

        CheckEventSystem();
        SetupButtons();

        playerController = FindObjectOfType<PlayerController2D>();

        Debug.Log("========== [PuzzleUI] 初始化完成 ==========");
    }

    void Update()
    {
        if (followTarget != null && puzzlePanel != null && puzzlePanel.activeSelf)
        {
            UpdateUIPosition();
        }

        if (enableTestKey && Input.GetKeyDown(KeyCode.T))
        {
            if (puzzlePanel != null && puzzlePanel.activeSelf)
            {
                Debug.Log("[PuzzleUI] 🧪 測試：手動觸發按鈕 0");
                OnAnswerSelected(0);
            }
        }
    }

    void UpdateUIPosition()
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            puzzlePanel.transform.position = followTarget.position + uiOffset;
        }
        else
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(followTarget.position + uiOffset);
            puzzlePanel.transform.position = screenPos;
        }
    }

    void ValidateComponents()
    {
        bool hasError = false;

        if (puzzlePanel == null)
        {
            Debug.LogError("[PuzzleUI] ❌ PuzzlePanel 未設定！");
            hasError = true;
        }

        if (questionText == null && questionTextLegacy == null)
        {
            Debug.LogError("[PuzzleUI] ❌ QuestionText 未設定！");
            hasError = true;
        }

        int buttonCount = 0;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
                buttonCount++;
        }

        if (buttonCount == 0)
        {
            Debug.LogError("[PuzzleUI] ❌ AnswerButtons 未設定！");
            hasError = true;
        }
        else
        {
            Debug.Log($"[PuzzleUI] ✓ 已設定 {buttonCount} 個按鈕");
        }

        // 檢查反饋組件
        if (feedbackPanel == null)
        {
            Debug.LogError("[PuzzleUI] ❌ FeedbackPanel 未設定！答案結果將無法顯示！");
            hasError = true;
        }
        else
        {
            Debug.Log("[PuzzleUI] ✓ FeedbackPanel 已設定");
        }

        if (feedbackText == null && feedbackTextLegacy == null)
        {
            Debug.LogError("[PuzzleUI] ❌ FeedbackText 未設定！答案結果將無法顯示！");
            hasError = true;
        }
        else
        {
            Debug.Log("[PuzzleUI] ✓ FeedbackText 已設定");
        }

        if (!hasError)
        {
            Debug.Log("[PuzzleUI] ✓ 所有必要組件已設定");
        }
    }

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
    }

    void SetupButtons()
    {
        if (answerButtons == null || answerButtons.Length == 0)
            return;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => {
                Debug.Log($"[PuzzleUI] 🖱️ 按鈕 {index} 被點擊！");
                OnAnswerSelected(index);
            });

            Debug.Log($"[PuzzleUI] ✓ 按鈕 {i} 事件已綁定");
        }
    }

    public void ShowPuzzle(PuzzleQuestion question, PuzzleBlock block)
    {
        Debug.Log("========== [PuzzleUI] 顯示問題 ==========");

        if (question == null)
        {
            Debug.LogError("[PuzzleUI] ❌ 問題資料為空！");
            return;
        }

        if (puzzlePanel == null)
        {
            Debug.LogError("[PuzzleUI] ❌ PuzzlePanel 為空！");
            return;
        }

        currentQuestion = question;
        currentBlock = block;
        isAnswering = false;
        followTarget = block.transform;

        if (pauseGameWhenShow)
        {
            Time.timeScale = 0f;
            Debug.Log("[PuzzleUI] ⏸️ 遊戲已暫停");
        }

        if (stopPlayerControl && playerController != null)
        {
            playerController.canControl = false;
        }

        puzzlePanel.SetActive(true);
        puzzlePanel.transform.localScale = Vector3.one * uiScale;
        UpdateUIPosition();

        // 確保問題文字顯示
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
            questionText.text = question.questionText;
        }
        else if (questionTextLegacy != null)
        {
            questionTextLegacy.gameObject.SetActive(true);
            questionTextLegacy.text = question.questionText;
        }

        Debug.Log($"[PuzzleUI] 問題: {question.questionText}");

        SetupAnswerButtons(question);

        // 確保反饋面板初始隱藏
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
            Debug.Log("[PuzzleUI] FeedbackPanel 初始隱藏");
        }

        Debug.Log("========== [PuzzleUI] 問題顯示完成 ==========");
    }

    void SetupAnswerButtons(PuzzleQuestion question)
    {
        int activeCount = 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
                continue;

            bool hasAnswer = i < question.answerOptions.Length &&
                           !string.IsNullOrEmpty(question.answerOptions[i]);

            if (hasAnswer)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;
                SetButtonText(answerButtons[i], question.answerOptions[i]);
                activeCount++;
                Debug.Log($"[PuzzleUI] 按鈕 {i}: {question.answerOptions[i]}");
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"[PuzzleUI] 啟用 {activeCount} 個按鈕");
    }

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
        }
    }

    public void OnAnswerSelected(int answerIndex)
    {
        Debug.Log($"========== [PuzzleUI] OnAnswerSelected 被調用: 索引 {answerIndex} ==========");

        if (isAnswering)
        {
            Debug.Log("[PuzzleUI] ⚠️ 已在處理答案中");
            return;
        }

        if (currentQuestion == null)
        {
            Debug.LogError("[PuzzleUI] ❌ currentQuestion 為空！");
            return;
        }

        if (currentBlock == null)
        {
            Debug.LogError("[PuzzleUI] ❌ currentBlock 為空！");
            return;
        }

        isAnswering = true;

        // 禁用所有按鈕
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }

        // 判斷答案
        bool isCorrect = (answerIndex == currentQuestion.correctAnswerIndex);

        Debug.Log($"[PuzzleUI] 選擇答案: {answerIndex}");
        Debug.Log($"[PuzzleUI] 正確答案: {currentQuestion.correctAnswerIndex}");
        Debug.Log($"[PuzzleUI] 結果: {(isCorrect ? "✓ 正確" : "✗ 錯誤")}");

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

    void HandleCorrectAnswer()
    {
        Debug.Log("[PuzzleUI] 🎉 處理正確答案");

        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        if (currentBlock != null)
        {
            currentBlock.GiveReward();
        }

        ShowFeedback(currentQuestion.correctMessage, correctColor);
    }

    void HandleWrongAnswer()
    {
        Debug.Log("[PuzzleUI] 💔 處理錯誤答案");

        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        if (currentBlock != null)
        {
            currentBlock.ApplyPenalty();
        }

        ShowFeedback(currentQuestion.wrongMessage, wrongColor);
    }

    void ShowFeedback(string message, Color color)
    {
        Debug.Log($"========== [PuzzleUI] 開始顯示反饋（使用問題面板）==========");
        Debug.Log($"[PuzzleUI] 反饋訊息: {message}");
        Debug.Log($"[PuzzleUI] 反饋顏色: {color}");

        // 隱藏所有按鈕
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }
        Debug.Log("[PuzzleUI] 隱藏所有按鈕");

        // 直接使用問題文字顯示結果
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
            questionText.text = message;
            questionText.color = color;
            questionText.fontSize = 48; // 放大字體
            Debug.Log($"[PuzzleUI] ✓ 使用 QuestionText 顯示反饋");
            Debug.Log($"[PuzzleUI]   - Text: {questionText.text}");
            Debug.Log($"[PuzzleUI]   - Color: {questionText.color}");
            Debug.Log($"[PuzzleUI]   - Font Size: {questionText.fontSize}");
        }
        else if (questionTextLegacy != null)
        {
            questionTextLegacy.gameObject.SetActive(true);
            questionTextLegacy.text = message;
            questionTextLegacy.color = color;
            questionTextLegacy.fontSize = 36;
            Debug.Log($"[PuzzleUI] ✓ 使用 QuestionTextLegacy 顯示反饋");
        }

        // 可選：改變整個面板的背景顏色
        if (puzzlePanel != null)
        {
            Image panelImage = puzzlePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color bgColor = (color == correctColor) ?
                    new Color(0.5f, 1f, 0.5f, 0.8f) : // 淡綠色背景
                    new Color(1f, 0.5f, 0.5f, 0.8f);   // 淡紅色背景
                panelImage.color = bgColor;
                Debug.Log($"[PuzzleUI] 設定 PuzzlePanel 背景色: {bgColor}");
            }
        }

        Debug.Log($"========== [PuzzleUI] 反饋顯示完成 ==========");
    }

    IEnumerator ShowFeedbackAndClose(bool isCorrect)
    {
        Debug.Log($"[PuzzleUI] ⏰ 等待 {feedbackDisplayTime} 秒後關閉");
        Debug.Log($"[PuzzleUI] currentBlock 是否為 null: {currentBlock == null}");

        if (pauseGameWhenShow)
        {
            yield return new WaitForSecondsRealtime(feedbackDisplayTime);
        }
        else
        {
            yield return new WaitForSeconds(feedbackDisplayTime);
        }

        Debug.Log("[PuzzleUI] ⏰ 等待完成，開始關閉UI");

        // 通知方塊答題結果 - 必須在關閉UI之前
        if (currentBlock != null)
        {
            Debug.Log($"[PuzzleUI] 📢 即將通知方塊答題結果: {(isCorrect ? "正確" : "錯誤")}");
            Debug.Log($"[PuzzleUI] 方塊名稱: {currentBlock.gameObject.name}");

            currentBlock.OnAnswered(isCorrect);

            Debug.Log($"[PuzzleUI] ✓ 已調用 currentBlock.OnAnswered({isCorrect})");
        }
        else
        {
            Debug.LogError("[PuzzleUI] ❌ currentBlock 為 null，無法通知答題結果！");
        }

        ClosePuzzle();

        isAnswering = false;

        Debug.Log($"========== [PuzzleUI] 反饋流程結束 - 答案{(isCorrect ? "正確" : "錯誤")} ==========");
    }

    public void ClosePuzzle()
    {
        Debug.Log("[PuzzleUI] 🚪 關閉UI");

        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);

        // 恢復問題面板的背景色
        if (puzzlePanel != null)
        {
            Image panelImage = puzzlePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = Color.white; // 恢復原色
            }
        }

        // 恢復問題文字的字體大小
        if (questionText != null)
        {
            questionText.fontSize = 36; // 恢復原始大小
        }

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        // 恢復顯示
        if (questionText != null)
            questionText.gameObject.SetActive(true);
        if (questionTextLegacy != null)
            questionTextLegacy.gameObject.SetActive(true);

        foreach (Button btn in answerButtons)
        {
            if (btn != null)
                btn.gameObject.SetActive(true);
        }

        if (pauseGameWhenShow)
        {
            Time.timeScale = 1f;
        }

        if (stopPlayerControl && playerController != null)
        {
            playerController.canControl = true;
        }

        currentQuestion = null;
        currentBlock = null;
        followTarget = null;
    }
}