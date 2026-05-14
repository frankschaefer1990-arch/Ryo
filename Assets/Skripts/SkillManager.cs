using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public int skillPoints = 0;
    
    // Tracks current level of each skill. Skill ID -> Level
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

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
        InitializeStartingSkills();
    }

    private void InitializeStartingSkills()
    {
        // Ryo starts with these two
        skillLevels["wilde_schlaege"] = 1;
        skillLevels["blitzschlag"] = 1;
    }

    private void Update()
    {
        // DEBUG: Press 'K' to add 5 skill points
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddPoints(5);
            var ui = FindFirstObjectByType<SkillUI>();
            if (ui != null) ui.RefreshUI();
            Debug.Log("SkillManager: Debug - Added 5 skill points.");
        }
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
        if (skillPoints <= 0) return false;

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
            skillPoints--;
            if (skillLevels.ContainsKey(skill.skillId))
                skillLevels[skill.skillId]++;
            else
                skillLevels[skill.skillId] = 1;

            Debug.Log($"SkillManager: {skill.skillName} upgraded to Lvl {skillLevels[skill.skillId]}");
            
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
