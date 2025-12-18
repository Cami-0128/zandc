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
    [SerializeField] private Sprite charMain;
    [SerializeField] private Sprite charFather;

    private TypewriterEffect typewriter;
    private int currentLine = 0;
    private bool isPlaying = true;

    private void Start()
    {
        // 確保 DialogueText 有打字機效果
        typewriter = dialogueText.GetComponent<TypewriterEffect>();
        if (typewriter == null)
        {
            typewriter = dialogueText.gameObject.AddComponent<TypewriterEffect>();
        }

        // 開始第一句
        ShowLine(0);
    }

    private void Update()
    {
        if (!isPlaying) return;

        // 左鍵點擊
        if (Input.GetMouseButtonDown(0))
        {
            if (typewriter.IsTyping())
            {
                // 如果還在打字，立即顯示完成
                typewriter.ShowAll();
            }
            else
            {
                // 如果已經打完，進行下一句
                currentLine++;
                if (currentLine < 3)
                {
                    ShowLine(currentLine);
                }
                else
                {
                    Debug.Log("故事結束");
                    isPlaying = false;
                }
            }
        }
    }

    private void ShowLine(int lineIndex)
    {
        if (lineIndex == 0)
        {
            // 旁白，背景 1，無角色
            speakerText.text = "旁白";
            typewriter.StartTyping("那天晚上，風很冷。", 0.05f);
            backgroundImage.sprite = bgBlack;
            characterImage.enabled = false;
        }
        else if (lineIndex == 1)
        {
            // 主角，背景 2，顯示主角
            speakerText.text = "主角";
            typewriter.StartTyping("……不可能。", 0.05f);
            backgroundImage.sprite = bgVillage;
            characterImage.sprite = charMain;
            characterImage.enabled = true;
        }
        else if (lineIndex == 2)
        {
            // 父親，背景 2，顯示父親
            speakerText.text = "父親";
            typewriter.StartTyping("別哭，聽我說。", 0.05f);
            backgroundImage.sprite = bgVillage;
            characterImage.sprite = charFather;
            characterImage.enabled = true;
        }
    }
}