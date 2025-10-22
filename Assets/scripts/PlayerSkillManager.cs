using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("技能組件引用")]
    [Tooltip("揮劍技能")]
    public SwordSlashSkill swordSlashSkill;

    [Header("技能解鎖狀態（可在關卡開始時設定）")]
    [Tooltip("遊戲開始時是否解鎖揮劍技能")]
    public bool startWithSwordUnlocked = true;

    [Header("Debug 設定")]
    public bool debugMode = true;

    void Start()
    {
        if (swordSlashSkill == null)
            swordSlashSkill = GetComponent<SwordSlashSkill>();

        InitializeSkills();
    }

    void InitializeSkills()
    {
        if (swordSlashSkill == null)
        {
            Debug.LogWarning("[PlayerSkillManager] 找不到 SwordSlashSkill 組件！");
            return;
        }

        if (startWithSwordUnlocked)
            swordSlashSkill.UnlockSkill();
        else
            swordSlashSkill.LockSkill();

        if (debugMode)
            Debug.Log($"[PlayerSkillManager] 揮劍技能初始狀態: {(startWithSwordUnlocked ? "解鎖" : "鎖定")}");
    }

    public void UnlockSwordSkill()
    {
        if (swordSlashSkill != null)
        {
            swordSlashSkill.UnlockSkill();
            if (debugMode)
                Debug.Log("[PlayerSkillManager] 揮劍技能已解鎖！");
        }
        else
        {
            Debug.LogWarning("[PlayerSkillManager] 找不到 SwordSlashSkill 組件！");
        }
    }

    public void LockSwordSkill()
    {
        if (swordSlashSkill != null)
        {
            swordSlashSkill.LockSkill();
            if (debugMode)
                Debug.Log("[PlayerSkillManager] 揮劍技能已鎖定！");
        }
        else
        {
            Debug.LogWarning("[PlayerSkillManager] 找不到 SwordSlashSkill 組件！");
        }
    }

    public bool IsSwordSkillUnlocked()
    {
        return swordSlashSkill != null && swordSlashSkill.IsUnlocked();
    }

    public void UnlockAllSkills()
    {
        UnlockSwordSkill();
        if (debugMode)
            Debug.Log("[PlayerSkillManager] 所有技能已解鎖！");
    }

    public void LockAllSkills()
    {
        LockSwordSkill();
        if (debugMode)
            Debug.Log("[PlayerSkillManager] 所有技能已鎖定！");
    }

    void Update()
    {
        if (!debugMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) UnlockSwordSkill();
        if (Input.GetKeyDown(KeyCode.Alpha2)) LockSwordSkill();
        if (Input.GetKeyDown(KeyCode.Alpha9)) UnlockAllSkills();
        if (Input.GetKeyDown(KeyCode.Alpha0)) LockAllSkills();
    }

    public SwordSlashSkill GetSwordSkill()
    {
        return swordSlashSkill;
    }
}
