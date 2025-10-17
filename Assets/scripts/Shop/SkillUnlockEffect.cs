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
            Debug.Log("[技能系統] 已自動添加 CaptureSkillManager 組件");
        }

        skillManager.AddSkillCount(addCount);
    }
}