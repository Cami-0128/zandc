//using UnityEngine;

//public class Act1Manager : MonoBehaviour
//{
//    [SerializeField] private DialogueManager dialogueManager;

//    private void Start()
//    {
//        DialogueManager.DialogueLine[] act1 = new DialogueManager.DialogueLine[]
//        {
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "那天晚上，風很冷。",
//                characterName = null,
//                backgroundName = "全黑"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "主角原本只是被叫去隔壁村送藥。等她回來時，天空已經完全黑了，雲層壓得很低，連月亮都被吞沒。",
//                characterName = null,
//                backgroundName = "全黑"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她站在村口，聞到一股不屬於夜晚的氣味—— 鐵鏽味。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "火焰正在燃燒。不是零星，而是整片。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "木屋倒塌的聲音在遠處炸裂，牲畜的哀鳴混雜著人的尖叫，又很快歸於死寂。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她的腳像是被釘在地上，直到一具屍體從屋頂滾落在路中央。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "脖子被撕開。血還在流。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "主角",
//                text = "……不可能。",
//                characterName = "主角",
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她往前衝。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "街道上躺滿了人。有人雙眼睜大，有人臉上仍保留逃跑時的恐懼。牆壁、井口、門板，全被血染成深色。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她看見了那個影子。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "高處掠過的黑影，像翅膀，又不像。還有一瞬間閃過的——紅色眼睛。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "謠言在她腦中成形的速度，比火焰還快。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她的家在村子最裡面。",
//                characterName = null,
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "門半開著。",
//                characterName = null,
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "父親倒在門口，背靠著牆，胸口被貫穿。血已經乾了一半，顏色發暗。",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "主角",
//                text = "爸……？",
//                characterName = "主角",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她跪下來，手抖得不成樣子。",
//                characterName = null,
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "父親的眼睛動了一下。",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "父親",
//                text = "別哭……聽我說。",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她用力點頭，眼淚一滴一滴掉在血裡。",
//                characterName = null,
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "父親",
//                text = "等妳……十六歲……去那座城堡……",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "她愣住了。",
//                characterName = null,
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "父親",
//                text = "不要……急著恨吸血鬼……",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "這句話，像一根針，狠狠扎進她的心。",
//                characterName = null,
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "父親從懷中掏出一枚黑紅色的徽印，顫抖地塞進她手裡。",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "父親",
//                text = "真相……在那裡……",
//                characterName = "父親",
//                backgroundName = "家"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "他的手垂下。再也沒有動靜。",
//                characterName = null,
//                backgroundName = "全黑"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "那一夜，她失去了父親。也失去了回頭的路。",
//                characterName = null,
//                backgroundName = "全黑"
//            }
//        };

//        if (dialogueManager != null)
//        {
//            dialogueManager.PlayDialogue(act1);
//            Invoke("ShowChoiceMenu", 2f);
//        }
//    }

//    private void ShowChoiceMenu()
//    {
//        DialogueManager.DialogueLine[] enterForest = new DialogueManager.DialogueLine[]
//        {
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "五年後……",
//                characterName = null,
//                backgroundName = "全黑"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "妳，16歲了。",
//                characterName = "主角",
//                backgroundName = "夜晚"
//            }
//        };

//        DialogueManager.DialogueLine[] normalLife = new DialogueManager.DialogueLine[]
//        {
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "妳選擇了放下仇恨。",
//                characterName = "主角",
//                backgroundName = "村子"
//            },
//            new DialogueManager.DialogueLine
//            {
//                speakerName = "旁白",
//                text = "妳在村子裡度過了簡單而平凡的一生。",
//                characterName = null,
//                backgroundName = "村子"
//            }
//        };

//        DialogueManager.DialogueChoice[] choices = new DialogueManager.DialogueChoice[]
//        {
//            new DialogueManager.DialogueChoice
//            {
//                choiceText = "是 - 進入森林尋求真相",
//                nextDialogue = enterForest
//            },
//            new DialogueManager.DialogueChoice
//            {
//                choiceText = "否 - 度過簡單而平凡的一生",
//                nextDialogue = normalLife
//            }
//        };

//        dialogueManager.ShowChoices(choices);
//    }
//}