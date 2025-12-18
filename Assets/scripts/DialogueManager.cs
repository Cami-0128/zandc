//using UnityEngine;
//using TMPro;
//using System.Collections;
//using UnityEngine.UI;

//public class DialogueManager : MonoBehaviour
//{
//    [System.Serializable]
//    public class DialogueLine
//    {
//        public string speakerName;
//        public string text;
//        public string characterName;   // 改用字串而不是 Sprite
//        public string backgroundName;  // 改用字串而不是 Sprite
//    }

//    [System.Serializable]
//    public class DialogueChoice
//    {
//        public string choiceText;
//        public DialogueLine[] nextDialogue;
//    }

//    [SerializeField] private TextMeshProUGUI dialogueText;
//    [SerializeField] private TextMeshProUGUI speakerNameText;
//    [SerializeField] private CanvasGroup dialoguePanel;
//    [SerializeField] private TextMeshProUGUI continueIndicator;
//    [SerializeField] private ImageManager imageManager;
//    [SerializeField] private GameObject choiceButtonPrefab;
//    [SerializeField] private Transform choiceContainer;

//    private TypewriterEffect typewriter;
//    private DialogueLine[] currentDialogue;
//    private int currentLineIndex = 0;
//    private bool isDialogueActive = false;

//    private void Start()
//    {
//        typewriter = dialogueText.GetComponent<TypewriterEffect>();
//        if (typewriter == null)
//        {
//            typewriter = dialogueText.gameObject.AddComponent<TypewriterEffect>();
//        }

//        if (dialoguePanel != null)
//            dialoguePanel.alpha = 0;

//        choiceContainer.gameObject.SetActive(false);
//    }

//    public void PlayDialogue(DialogueLine[] dialogue)
//    {
//        choiceContainer.gameObject.SetActive(false);
//        currentDialogue = dialogue;
//        currentLineIndex = 0;
//        isDialogueActive = true;

//        if (dialoguePanel != null)
//            dialoguePanel.alpha = 1;

//        ShowDialogueLine(0);
//    }

//    private void ShowDialogueLine(int index)
//    {
//        if (index >= currentDialogue.Length)
//        {
//            EndDialogue();
//            return;
//        }

//        DialogueLine line = currentDialogue[index];
//        speakerNameText.text = line.speakerName;
//        typewriter.StartTyping(line.text, 0.05f);

//        // 使用 ImageManager 設定角色
//        if (imageManager != null)
//        {
//            Sprite character = imageManager.GetCharacter(line.characterName);
//            imageManager.SetCharacter(character);

//            Sprite background = imageManager.GetBackground(line.backgroundName);
//            imageManager.SetBackground(background);
//        }

//        Debug.Log($"說話人: {line.speakerName}, 角色: {line.characterName}, 背景: {line.backgroundName}");
//    }

//    private void Update()
//    {
//        if (!isDialogueActive) return;

//        if (Input.GetMouseButtonDown(0))
//        {
//            if (typewriter.IsTyping())
//            {
//                typewriter.ShowAll();
//            }
//            else
//            {
//                currentLineIndex++;
//                ShowDialogueLine(currentLineIndex);
//            }
//        }

//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            SkipDialogue();
//        }
//    }

//    private void SkipDialogue()
//    {
//        typewriter.Stop();
//        EndDialogue();
//    }

//    private void EndDialogue()
//    {
//        isDialogueActive = false;

//        if (continueIndicator != null)
//            continueIndicator.enabled = false;

//        StartCoroutine(FadeOutDialogue());
//    }

//    private IEnumerator FadeOutDialogue()
//    {
//        float elapsed = 0f;
//        float duration = 0.5f;

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            if (dialoguePanel != null)
//                dialoguePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
//            yield return null;
//        }

//        if (dialoguePanel != null)
//            dialoguePanel.alpha = 0f;
//    }

//    public void ShowChoices(DialogueChoice[] choices)
//    {
//        isDialogueActive = false;
//        choiceContainer.gameObject.SetActive(true);

//        foreach (Transform child in choiceContainer)
//        {
//            Destroy(child.gameObject);
//        }

//        for (int i = 0; i < choices.Length; i++)
//        {
//            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
//            Button button = buttonObj.GetComponent<Button>();
//            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

//            buttonText.text = choices[i].choiceText;

//            DialogueLine[] nextDialogue = choices[i].nextDialogue;
//            button.onClick.AddListener(() =>
//            {
//                PlayDialogue(nextDialogue);
//            });
//        }
//    }
//}


