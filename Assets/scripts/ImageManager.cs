//using UnityEngine;
//using UnityEngine.UI;

//public class ImageManager : MonoBehaviour
//{
//    [SerializeField] private Image characterImage;
//    [SerializeField] private Image backgroundImage;

//    // 角色圖片字典
//    public Sprite charNone;      // 無角色
//    public Sprite charMain;      // 主角
//    public Sprite charFather;    // 父親

//    // 背景圖片字典
//    public Sprite bgBlack;       // 全黑
//    public Sprite bgNight;       // 夜晚
//    public Sprite bgVillage;     // 村子
//    public Sprite bgHome;        // 家

//    public void SetCharacter(Sprite character)
//    {
//        if (character == null)
//        {
//            characterImage.enabled = false;
//        }
//        else
//        {
//            characterImage.sprite = character;
//            characterImage.enabled = true;
//        }
//    }

//    public void SetBackground(Sprite background)
//    {
//        if (background != null)
//        {
//            backgroundImage.sprite = background;
//        }
//    }

//    public Sprite GetCharacter(string characterName)
//    {
//        return characterName switch
//        {
//            "主角" => charMain,
//            "父親" => charFather,
//            _ => null
//        };
//    }

//    public Sprite GetBackground(string backgroundName)
//    {
//        return backgroundName switch
//        {
//            "全黑" => bgBlack,
//            "夜晚" => bgNight,
//            "村子" => bgVillage,
//            "家" => bgHome,
//            _ => bgBlack
//        };
//    }
//}
