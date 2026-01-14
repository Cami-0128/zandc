using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ============================================
// BossLoseEndingManager.cs
// 玩家輸的結局場景
// ============================================
public class BossLoseEndingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    // 圖片引用
    [SerializeField] private Sprite bgCastle;
    [SerializeField] private Sprite charMain;
    [SerializeField] private Sprite charBoss;

    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;
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
    private bool hasShownChoice = false;

    private void Start()
    {
        typewriter = dialogueText.GetComponent<TypewriterEffect>();
        if (typewriter == null)
        {
            typewriter = dialogueText.gameObject.AddComponent<TypewriterEffect>();
        }

        choiceContainer.gameObject.SetActive(false);

        // 綁定 Skip 按鈕
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToChoice);
        }

        InitializeLoseEnding();
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
                    // 對話結束，顯示選擇
                    isPlaying = false;
                    if (!hasShownChoice)
                    {
                        ShowTrustChoice();
                    }
                }
            }
        }
    }

    private void InitializeLoseEnding()
    {
        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "主角跪在地上,無法再站起來", background = bgCastle, character = null },
            new StoryLine { speaker = "Boss", dialogue = "你看這就是妳的力量,遠遠還不夠", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "吸血鬼走近她,眼神中沒有殺意,只有遺憾", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "但妳的血,告訴了我——妳是特別的", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "妳想聽我說真相嗎?", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "妳抬起頭,看著對方", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "妳需要做出選擇", background = bgCastle, character = null }
        };
    }

    private void ShowTrustChoice()
    {
        hasShownChoice = true;
        choiceContainer.gameObject.SetActive(true);

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // 選項 1：相信 Boss
        GameObject btn1 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button1 = btn1.GetComponent<Button>();
        TextMeshProUGUI text1 = btn1.GetComponentInChildren<TextMeshProUGUI>();
        text1.text = "相信 - 聽 Boss 說真相";
        button1.onClick.AddListener(() => PlayTrustEnding());

        // 選項 2：不相信，自爆
        GameObject btn2 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button2 = btn2.GetComponent<Button>();
        TextMeshProUGUI text2 = btn2.GetComponentInChildren<TextMeshProUGUI>();
        text2.text = "不相信 - 同歸於盡";
        button2.onClick.AddListener(() => PlayDeathEnding());
    }

    private void PlayTrustEnding()
    {
        choiceContainer.gameObject.SetActive(false);
        isPlaying = true;
        currentLine = 0;

        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "妳停止了掙扎", background = bgCastle, character = null },
            new StoryLine { speaker = "Boss", dialogue = "乖孩子", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "吸血鬼將真相娓娓道來", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "妳的父親,死於人類獵人之手,而我,曾試圖拯救他", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "但我來得太晚了", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "妳的眼淚流下.但這一次不是因為憤怒,而是滿滿的悲傷......", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "{和平結局}未完待續......", background = bgCastle, character = null }
        };

        ShowLine(0);
    }

    private void PlayDeathEnding()
    {
        choiceContainer.gameObject.SetActive(false);
        isPlaying = true;
        currentLine = 0;

        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "妳的身體開始發光,血液正在燃燒", background = bgCastle, character = null },
            new StoryLine { speaker = "主角", dialogue = "我絕對不相信任何謊言", background = bgCastle, character = charMain },
            new StoryLine { speaker = "旁白", dialogue = "妳引爆了所有的力量", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "城堡在爆炸中崩塌,一切全消失在黑暗中", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "沒有人活著離開", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "{死亡結局}故事到這暫時畫下了句號", background = bgCastle, character = null }
        };

        ShowLine(0);
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

    private void SkipToChoice()
    {
        isPlaying = false;
        typewriter.Stop();

        if (!hasShownChoice)
        {
            int lastLine = storyLines.Length - 1;
            StoryLine line = storyLines[lastLine];

            speakerText.text = line.speaker;
            dialogueText.text = line.dialogue;
            backgroundImage.sprite = line.background;

            if (line.character != null)
            {
                characterImage.sprite = line.character;
                characterImage.enabled = true;
            }
            else
            {
                characterImage.enabled = false;
            }

            ShowTrustChoice();
        }
    }
}