using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ============================================
// BossWinEndingManager.cs
// 玩家贏的結局場景
// ============================================
public class BossWinEndingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    // 圖片引用
    [SerializeField] private Sprite bgCastle;
    [SerializeField] private Sprite charMain;
    [SerializeField] private Sprite charBoss;

    [SerializeField] private Button skipButton;

    // 劇情數據結構
    [System.Serializable]
    private class StoryLine
    {
        public string speaker;
        public string dialogue;
        public Sprite background;
        public Sprite character;
    }

    private TypewriterEffect typewriter;
    private StoryLine[] storyLines;
    private int currentLine = 0;
    private bool isPlaying = true;

    private void Start()
    {
        typewriter = dialogueText.GetComponent<TypewriterEffect>();
        if (typewriter == null)
        {
            typewriter = dialogueText.gameObject.AddComponent<TypewriterEffect>();
        }

        // 綁定 Skip 按鈕
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToEnd);
        }

        InitializeWinEnding();
        ShowLine(0);
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (typewriter.IsTyping())
            {
                typewriter.ShowAll();
            }
            else
            {
                currentLine++;
                if (currentLine < storyLines.Length)
                {
                    ShowLine(currentLine);
                }
                else
                {
                    // 所有對話播放完畢，顯示結束選項或進入下一個場景
                    isPlaying = false;
                    EndScene();
                }
            }
        }
    }

    private void InitializeWinEnding()
    {
        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "吸血鬼的身體開始崩散。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "……妳做到了。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "一股力量從對方體內湧出，流入主角的血液。", background = bgCastle, character = charMain },
            new StoryLine { speaker = "旁白", dialogue = "她感到一切都改變了。她的血液沸騰，力量充滿全身。", background = bgCastle, character = charMain },
            new StoryLine { speaker = "主角", dialogue = "我……變成了什麼？", background = bgCastle, character = charMain },
            new StoryLine { speaker = "旁白", dialogue = "窗外，天色已明。黎明來臨。", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "而她，永遠改變了。", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "【未知結局】下一章，敬請期待……", background = bgCastle, character = null }
        };
    }

    private void ShowLine(int index)
    {
        StoryLine line = storyLines[index];

        speakerText.text = line.speaker;
        typewriter.StartTyping(line.dialogue, 0.05f);

        if (line.background != null)
        {
            backgroundImage.sprite = line.background;
        }

        if (line.character != null)
        {
            characterImage.sprite = line.character;
            characterImage.enabled = true;
        }
        else
        {
            characterImage.enabled = false;
        }
    }

    private void EndScene()
    {
        Debug.Log("玩家贏的結局播放完畢");
        // 這裡可以加載下一個場景或顯示選單
        // SceneManager.LoadScene("MainMenu");
    }

    private void SkipToEnd()
    {
        isPlaying = false;
        typewriter.Stop();

        int lastLine = storyLines.Length - 1;
        StoryLine line = storyLines[lastLine];

        speakerText.text = line.speaker;
        dialogueText.text = line.dialogue;
        backgroundImage.sprite = line.background;
        characterImage.enabled = false;
    }
}