using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("�ޯ�ե�ޥ�")]
    [Tooltip("���C�ޯ�")]
    public SwordSlashSkill swordSlashSkill;

    [Header("�ޯ���ꪬ�A�]�i�b���d�}�l�ɳ]�w�^")]
    [Tooltip("�C���}�l�ɬO�_���괧�C�ޯ�")]
    public bool startWithSwordUnlocked = true;

    [Header("Debug �]�w")]
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
            Debug.LogWarning("[PlayerSkillManager] �䤣�� SwordSlashSkill �ե�I");
            return;
        }

        if (startWithSwordUnlocked)
            swordSlashSkill.UnlockSkill();
        else
            swordSlashSkill.LockSkill();

        if (debugMode)
            Debug.Log($"[PlayerSkillManager] ���C�ޯ��l���A: {(startWithSwordUnlocked ? "����" : "��w")}");
    }

    public void UnlockSwordSkill()
    {
        if (swordSlashSkill != null)
        {
            swordSlashSkill.UnlockSkill();
            if (debugMode)
                Debug.Log("[PlayerSkillManager] ���C�ޯ�w����I");
        }
        else
        {
            Debug.LogWarning("[PlayerSkillManager] �䤣�� SwordSlashSkill �ե�I");
        }
    }

    public void LockSwordSkill()
    {
        if (swordSlashSkill != null)
        {
            swordSlashSkill.LockSkill();
            if (debugMode)
                Debug.Log("[PlayerSkillManager] ���C�ޯ�w��w�I");
        }
        else
        {
            Debug.LogWarning("[PlayerSkillManager] �䤣�� SwordSlashSkill �ե�I");
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
            Debug.Log("[PlayerSkillManager] �Ҧ��ޯ�w����I");
    }

    public void LockAllSkills()
    {
        LockSwordSkill();
        if (debugMode)
            Debug.Log("[PlayerSkillManager] �Ҧ��ޯ�w��w�I");
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
