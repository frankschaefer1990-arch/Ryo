using UnityEngine;

public enum SkillCategory { Basic, Zauber, Verflucht }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Battle/Skill")]
public class BattleSkill : ScriptableObject
{
    public string skillId; // Unique ID
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;
    public SkillCategory category;
    
    [Header("Requirements")]
    public int levelRequirement = 1;
    public BattleSkill prerequisiteSkill;
    public int maxLevel = 5;

    [Header("Base Battle Stats")]
    public int hitCount = 1;
    public float damageMultiplier = 1f;
    public bool isSpell = false;
    public int manaCost = 0;
    public Color effectColor = Color.white;
    
    [Header("Custom Visual Settings")]
    public Sprite customSlashSprite; // New: Custom image for the slash effect
    public float slashDuration = 0.2f; // New: Duration for the custom effect
    public Vector3 visualOffset = Vector3.zero; // New: Manual offset from target
    public Vector3 visualScale = Vector3.one; // New: Manual scale for the sprite
    public bool randomRotation = true; // New: Whether to rotate the sprite randomly
    
    public AudioClip skillSound;
    public bool hasCombo = false;
    public float healMultiplier = 0f; // New: Multiplier for healing based on damage

    [Header("Special Effects")]
    public bool canStun = false;
    public float baseStunChance = 0.1f;
    public float stunChancePerLevel = 0.05f;
    
    [Header("QTE Settings")]
    public float baseTimeLimit = 1.2f; // Time for the QTE ring to shrink
    public float timeLimitReductionPerLevel = 0.05f; // Makes it harder per level

    // Scaling Methods
    public int GetHitCount(int level) 
    {
        if (skillId == "wilde_schlaege") return hitCount + (level - 1);
        if (skillId == "rage") return hitCount + (level - 1);
        return hitCount;
    }

    public int GetManaCost(int level)
    {
        if (skillId == "blitzschlag") return manaCost + (level - 1) * 10;
        if (skillId == "soulreap") return manaCost + (level - 1) * 10;
        return manaCost;
    }

    public float GetDamageMultiplier(int level)
    {
        if (skillId == "blitzschlag") return damageMultiplier + (level - 1) * 0.3f; 
        if (skillId == "rage") return damageMultiplier + (level - 1) * 0.1f;
        return damageMultiplier;
    }

    public float GetHealMultiplier(int level)
    {
        if (skillId == "soulreap") return healMultiplier + (level - 1) * 0.1f;
        return healMultiplier;
    }

    public float GetStunChance(int level)
    {
        if (!canStun) return 0f;
        return baseStunChance + (level - 1) * stunChancePerLevel;
    }

    public float GetQTETimeLimit(int level)
    {
        return Mathf.Max(0.4f, baseTimeLimit - (level - 1) * timeLimitReductionPerLevel);
    }

    public string GetTooltipInfo(int level)
    {
        string info = $"<color=#FFD700>{skillName}</color> (Lvl {level})\n";
        info += $"{description}\n\n";
        
        if (isSpell) info += $"Mana: {GetManaCost(level)}\n";
        
        if (skillId == "wilde_schlaege" || skillId == "rage" || skillId == "soulreap") info += $"Schläge: {GetHitCount(level)}\n";
        else info += $"Schaden: x{GetDamageMultiplier(level):F1}\n";

        if (GetHealMultiplier(level) > 0)
        {
            info += $"Heilung: {GetHealMultiplier(level) * 100:F0}% des Schadens\n";
        }

        if (canStun)
        {
            info += $"Stun Chance: {GetStunChance(level) * 100:F0}%\n";
        }

        if (level < maxLevel)
        {
            int nextLevelCost = level + 1;
            info += $"\n<color=#00FF00>Nächste Stufe (Kosten: {nextLevelCost}):</color>\n";
            
            if (skillId == "wilde_schlaege" || skillId == "rage") 
                info += $"Schläge: {GetHitCount(level + 1)}\n";
            
            if (skillId == "blitzschlag") 
                info += $"Schaden: x{GetDamageMultiplier(level + 1):F1} (Mana: {GetManaCost(level + 1)})\n";
            else if (skillId == "soulreap") 
                info += $"Heilung: {GetHealMultiplier(level + 1) * 100:F0}% (Mana: {GetManaCost(level + 1)})\n";
            else if (!isSpell) 
                info += $"Schaden: x{GetDamageMultiplier(level + 1):F1}\n";

            if (canStun)
                info += $"Stun Chance: {GetStunChance(level + 1) * 100:F0}%\n";
            
            if (hasCombo && skillId == "rage")
                info += $"QTE Tempo: +{(1.0f - GetQTETimeLimit(level+1)/GetQTETimeLimit(level))*100:F0}%\n";
        }
        else
        {
            info += "\n<color=#FF4500>Maximalstufe erreicht</color>";
        }

        return info;
        }
        }
