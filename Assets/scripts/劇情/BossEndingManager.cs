using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossEndingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    // 圖片引用
    [SerializeField] private Sprite bgCastle;
    [SerializeField] private Sprite charMain;
    [SerializeField] private Sprite charBoss;

    // 選擇按鈕
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private Button skipButton;

    // Canvas Group 引用
    [SerializeField] private CanvasGroup dialogueUICanvasGroup;
    [SerializeField] private CanvasGroup battleUICanvasGroup;

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

    private enum DialogueStage
    {
        BossIntro,
        BeforeBattle,
        PeacefulEnding
    }

    private DialogueStage currentStage = DialogueStage.BossIntro;

    private void Start()
    {
        typewriter = dialogueText.GetComponent<TypewriterEffect>();
        if (typewriter == null)
        {
            typewriter = dialogueText.gameObject.AddComponent<TypewriterEffect>();
        }

        choiceContainer.gameObject.SetActive(false);

        // 初始化 Canvas Group
        if (dialogueUICanvasGroup != null)
        {
            dialogueUICanvasGroup.alpha = 1;
            dialogueUICanvasGroup.blocksRaycasts = true;
        }

        if (battleUICanvasGroup != null)
        {
            battleUICanvasGroup.alpha = 0;
            battleUICanvasGroup.blocksRaycasts = false;
        }

        // 綁定 Skip 按鈕
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToChoice);
        }

        InitializeBossIntroDialogue();
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
                    isPlaying = false;

                    if (currentStage == DialogueStage.BossIntro)
                    {
                        ShowBattleChoice();
                    }
                }
            }
        }
    }

    private void InitializeBossIntroDialogue()
    {
        currentStage = DialogueStage.BossIntro;
        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "紅色地毯延伸至王座。蠟燭在她踏入大廳的瞬間點亮。", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "翅膀拍動的聲音自高處傳來。", background = bgCastle, character = null },
            new StoryLine { speaker = "Boss", dialogue = "妳終於來了。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "她的心臟劇烈跳動。不是恐懼，而是熟悉。", background = bgCastle, character = charMain },
            new StoryLine { speaker = "主角", dialogue = "村子……父親……是妳做的吧？", background = bgCastle, character = charMain },
            new StoryLine { speaker = "旁白", dialogue = "短暫的沉默。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "不是。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "主角", dialogue = "騙人！", background = bgCastle, character = charMain },
            new StoryLine { speaker = "Boss", dialogue = "人類殺人，卻總需要怪物來背負罪名。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "這座城堡，封存的是被掩埋的真相。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "光影浮現——燃燒的村莊、倒下的吸血鬼、身披人類鎧甲的身影。", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "她頭痛欲裂。", background = bgCastle, character = charMain },
            new StoryLine { speaker = "主角", dialogue = "為什麼……讓我看這些？", background = bgCastle, character = charMain },
            new StoryLine { speaker = "Boss", dialogue = "因為妳的血，正在甦醒。妳有選擇的權利。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "主角", dialogue = "如果我拒絕呢？", background = bgCastle, character = charMain },
            new StoryLine { speaker = "Boss", dialogue = "那這條路，只剩下復仇。", background = bgCastle, character = charBoss }
        };
    }

    private void ShowBattleChoice()
    {
        currentStage = DialogueStage.BeforeBattle;
        choiceContainer.gameObject.SetActive(true);

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // 選項 1：不戰鬥
        GameObject btn1 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button1 = btn1.GetComponent<Button>();
        TextMeshProUGUI text1 = btn1.GetComponentInChildren<TextMeshProUGUI>();
        text1.text = "不戰鬥 - 聽 Boss 說出真相";
        button1.onClick.AddListener(() => PlayPeacefulEnding());

        // 選項 2：戰鬥
        GameObject btn2 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button2 = btn2.GetComponent<Button>();
        TextMeshProUGUI text2 = btn2.GetComponentInChildren<TextMeshProUGUI>();
        text2.text = "戰鬥 - 用力量證明";
        button2.onClick.AddListener(() => EnterBattlePhase());
    }

    private void PlayPeacefulEnding()
    {
        choiceContainer.gameObject.SetActive(false);
        isPlaying = true;
        currentLine = 0;
        currentStage = DialogueStage.PeacefulEnding;

        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "主角放下了武器。", background = bgCastle, character = charMain },
            new StoryLine { speaker = "Boss", dialogue = "你做出了選擇。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "吸血鬼走下台階，緩緩靠近。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "Boss", dialogue = "你的父親，是被人類獵人殺害的。真兇就藏在村民之中。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "主角", dialogue = "……", background = bgCastle, character = charMain },
            new StoryLine { speaker = "Boss", dialogue = "而你，擁有我們的血。你可以選擇原諒，也可以選擇遺忘。", background = bgCastle, character = charBoss },
            new StoryLine { speaker = "旁白", dialogue = "和平，並非源於勝利，而源於理解。", background = bgCastle, character = null },
            new StoryLine { speaker = "旁白", dialogue = "【和平結局】故事未完，待續……", background = bgCastle, character = null }
        };

        ShowLine(0);
    }

    private void EnterBattlePhase()
    {
        choiceContainer.gameObject.SetActive(false);
        isPlaying = false;

        Debug.Log("進入 Boss 戰鬥場景");
        // 加載 BossBattle 場景
        SceneManager.LoadScene("BossBattle");
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

        if (currentStage == DialogueStage.BossIntro)
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

            ShowBattleChoice();
        }
        else if (currentStage == DialogueStage.PeacefulEnding)
        {
            int lastLine = storyLines.Length - 1;
            StoryLine line = storyLines[lastLine];

            speakerText.text = line.speaker;
            dialogueText.text = line.dialogue;
            backgroundImage.sprite = line.background;
            characterImage.enabled = false;
        }
    }
}