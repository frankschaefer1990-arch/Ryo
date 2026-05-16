using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public int skillPoints = 0;
    
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    public List<string> learnedOrder = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (skillLevels == null) skillLevels = new Dictionary<string, int>();
        if (learnedOrder == null) learnedOrder = new List<string>();
        InitializeStartingSkills();
    }

    private void InitializeStartingSkills()
    {
        skillLevels["wilde_schlaege"] = 1;
        skillLevels["blitzschlag"] = 1;
        
        if (!learnedOrder.Contains("wilde_schlaege")) learnedOrder.Add("wilde_schlaege");
        if (!learnedOrder.Contains("blitzschlag")) learnedOrder.Add("blitzschlag");
    }

    public int GetSkillLevel(BattleSkill skill)
    {
        if (skill == null) return 0;
        return GetSkillLevelById(skill.skillId);
    }

    public int GetSkillLevelById(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        if (skillLevels.ContainsKey(id)) return skillLevels[id];
        return 0;
    }

    public bool CanLearnOrUpgrade(BattleSkill skill)
    {
        if (skill == null) return false;
        
        int currentLvl = GetSkillLevel(skill);
        if (currentLvl >= skill.maxLevel) return false;
        
        int nextLvl = currentLvl + 1;
        if (skillPoints < nextLvl) return false;

        if (PlayerStats.Instance != null && PlayerStats.Instance.level < skill.levelRequirement)
            return false;

        if (skill.prerequisiteSkill != null)
        {
            if (GetSkillLevel(skill.prerequisiteSkill) < 1)
                return false;
        }

        return true;
    }

    public void LearnOrUpgrade(BattleSkill skill)
    {
        if (CanLearnOrUpgrade(skill))
        {
            int currentLvl = GetSkillLevel(skill);
            int nextLvl = currentLvl + 1;
            
            if (skill.category == SkillCategory.Verflucht && skill.isPassiveCurse && currentLvl >= 1)
            {
                Debug.LogWarning("SkillManager: Passive curse skills can only be learned once.");
                return;
            }

            skillPoints -= nextLvl;

            if (skillLevels.ContainsKey(skill.skillId)) skillLevels[skill.skillId]++;
            else
            {
                skillLevels[skill.skillId] = 1;
                if (!learnedOrder.Contains(skill.skillId)) learnedOrder.Add(skill.skillId);
            }

            if (skill.isCurseUnlocker && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.isCurseSystemUnlocked = true;
                Debug.Log("SkillManager: CURSE SYSTEM UNLOCKED!");
                PlayerStats.Instance.UpdateUI();
            }

            Debug.Log($"SkillManager: {skill.skillName} upgraded to Lvl {skillLevels[skill.skillId]}");
            
            var ui = FindFirstObjectByType<SkillUI>();
            if (ui != null) ui.RefreshUI();
        }
    }

    public void AddPoints(int amount)
    {
        skillPoints += amount;
    }
}
