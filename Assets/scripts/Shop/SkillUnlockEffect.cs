using UnityEngine;

public class SkillUnlockEffect : IShopItemEffect
{
    private int addCount;

    public SkillUnlockEffect(int count = 1)
    {
        addCount = count;
    }

    public void ApplyEffect(PlayerController2D player)
    {
        CaptureSkillManager skillManager = player.GetComponent<CaptureSkillManager>();
        if (skillManager == null)
        {
            skillManager = player.gameObject.AddComponent<CaptureSkillManager>();
            Debug.Log("[�ޯ�t��] �w�۰ʲK�[ CaptureSkillManager �ե�");
        }

        skillManager.AddSkillCount(addCount);
    }
}