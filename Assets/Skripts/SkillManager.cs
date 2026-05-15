using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public int skillPoints = 0;
    
    // Tracks current level of each skill. Skill ID -> Level
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    // Tracks the order in which skills were learned
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
        // Ryo starts with these two
        skillLevels["wilde_schlaege"] = 1;
        skillLevels["blitzschlag"] = 1;
        
        if (!learnedOrder.Contains("wilde_schlaege")) learnedOrder.Add("wilde_schlaege");
        if (!learnedOrder.Contains("blitzschlag")) learnedOrder.Add("blitzschlag");
    }

    private void Update()
    {
        // Debug functionality removed to avoid conflict with UI keys
    }

    public int GetSkillLevel(BattleSkill skill)
{
        if (skill == null) return 0;
        if (skillLevels.ContainsKey(skill.skillId)) return skillLevels[skill.skillId];
        return 0;
    }

    public bool CanLearnOrUpgrade(BattleSkill skill)
    {
        if (skill == null) return false;
        
        int currentLvl = GetSkillLevel(skill);
        if (currentLvl >= skill.maxLevel) return false;
        
        int nextLvl = currentLvl + 1;
        if (skillPoints < nextLvl) return false;

        // Level Requirement
        if (PlayerStats.Instance != null && PlayerStats.Instance.level < skill.levelRequirement)
            return false;

        // Prerequisite: Previous skill must be at least lvl 1
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
            int nextLvl = GetSkillLevel(skill) + 1;
            skillPoints -= nextLvl;

            if (skillLevels.ContainsKey(skill.skillId))
            {
                skillLevels[skill.skillId]++;
            }
            else
            {
                skillLevels[skill.skillId] = 1;
                if (!learnedOrder.Contains(skill.skillId))
                    learnedOrder.Add(skill.skillId);
            }

            Debug.Log($"SkillManager: {skill.skillName} upgraded to Lvl {skillLevels[skill.skillId]} (Cost: {nextLvl})");
            
            // Refresh UI
            var ui = FindFirstObjectByType<SkillUI>();
            if (ui != null) ui.RefreshUI();
        }
    }

    public void AddPoints(int amount)
    {
        skillPoints += amount;
    }
}
