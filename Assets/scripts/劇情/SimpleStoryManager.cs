using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleStoryManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    // 圖片引用
    [SerializeField] private Sprite bgBlack;
    [SerializeField] private Sprite bgVillage;
    [SerializeField] private Sprite bgHome;
    [SerializeField] private Sprite charMain;
    [SerializeField] private Sprite charFather;

    // 選擇按鈕
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

        InitializeStory();
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
                    ShowChoice();
                }
            }
        }
    }

    private void InitializeStory()
    {
        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "那天晚上，風很冷。", background = bgBlack, character = null },
            new StoryLine { speaker = "旁白", dialogue = "她只是被叫去隔壁村送藥，回來時，夜色已經吞沒了一切。", background = bgBlack, character = null },
            new StoryLine { speaker = "旁白", dialogue = "空氣裡瀰漫著不對勁的氣味。不是夜晚該有的味道。", background = bgVillage, character = null },
            new StoryLine { speaker = "旁白", dialogue = "火正在燃燒。不是一間，而是整個村子。", background = bgVillage, character = null },
            new StoryLine { speaker = "旁白", dialogue = "她站在原地，直到看見倒在路中央的身影。", background = bgVillage, character = null },
            new StoryLine { speaker = "主角", dialogue = "「……不可能。」", background = bgVillage, character = charMain },
            new StoryLine { speaker = "旁白", dialogue = "她開始奔跑。", background = bgVillage, character = null },
            new StoryLine { speaker = "旁白", dialogue = "街道上滿是倒下的人，世界安靜得不正常。高處掠過一道黑影，還有一瞬間對上她的——紅色目光。", background = bgVillage, character = null },
            new StoryLine { speaker = "旁白", dialogue = "她的家，在村子最裡面。", background = bgVillage, character = null },
            new StoryLine { speaker = "旁白", dialogue = "門半開著。父親倒在門口，勉強睜著眼。", background = bgHome, character = null },
            new StoryLine { speaker = "主角", dialogue = "「爸……？」", background = bgHome, character = charMain },
            new StoryLine { speaker = "父親(虛弱)", dialogue = "「別哭……聽我說。」", background = bgHome, character = charFather },
            new StoryLine { speaker = "旁白", dialogue = "他顫抖地將一枚黑紅色的徽印塞進她手中。", background = bgHome, character = charFather },
            new StoryLine { speaker = "父親", dialogue = "「等妳十六歲……去那座城堡……」", background = bgHome, character = charFather },
            new StoryLine { speaker = "父親", dialogue = "「不要……急著恨吸血鬼……」", background = bgHome, character = charFather },
            new StoryLine { speaker = "父親", dialogue = "「真相……在那裡……」", background = bgHome, character = charFather },
            new StoryLine { speaker = "旁白", dialogue = "他的手垂下。", background = bgHome, character = null },
            new StoryLine { speaker = "旁白", dialogue = "那一夜，她失去了父親。也失去了回頭的路。", background = bgBlack, character = null },
            new StoryLine { speaker = "", dialogue = ".", background = bgBlack, character = null },
            new StoryLine { speaker = "", dialogue = ".", background = bgBlack, character = null },
            new StoryLine { speaker = "旁白", dialogue = "五年後……", background = bgBlack, character = null },
            new StoryLine { speaker = "旁白", dialogue = "你，十六歲了。", background = bgBlack, character = null }
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

    private void ShowChoice()
    {
        choiceContainer.gameObject.SetActive(true);

        // 清空舊按鈕
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        // 選項 1：進入森林
        GameObject btn1 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button1 = btn1.GetComponent<Button>();
        TextMeshProUGUI text1 = btn1.GetComponentInChildren<TextMeshProUGUI>();
        text1.text = "是 - 進入森林";
        button1.onClick.AddListener(() =>
        {
            Debug.Log("進入遊戲主線");
            // 可以在這裡加載下一個場景
            // SceneManager.LoadScene("GameScene");
        });

        // 選項 2：普通人生
        GameObject btn2 = Instantiate(choiceButtonPrefab, choiceContainer);
        Button button2 = btn2.GetComponent<Button>();
        TextMeshProUGUI text2 = btn2.GetComponentInChildren<TextMeshProUGUI>();
        text2.text = "否 - 遠離那一夜";
        button2.onClick.AddListener(() =>
        {
            PlayNormalEnding();
        });
    }

    private void PlayNormalEnding()
    {
        choiceContainer.gameObject.SetActive(false);
        isPlaying = true;
        currentLine = 0;

        storyLines = new StoryLine[]
        {
            new StoryLine { speaker = "旁白", dialogue = "你選擇遠離那一夜，度過平凡的一生。", background = bgBlack, character = null },
            new StoryLine { speaker = "旁白", dialogue = "遊戲結束。", background = bgBlack, character = null }
        };

        ShowLine(0);
    }

    private void SkipToChoice()
    {
        isPlaying = false;
        typewriter.Stop();

        // 顯示最後一幕的樣子
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

        // 顯示選項
        ShowChoice();
    }
}