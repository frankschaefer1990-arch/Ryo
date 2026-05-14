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
    public AudioClip skillSound;
    public bool hasCombo = false;

    // Scaling Methods
    public int GetHitCount(int level) 
    {
        if (skillName == "Wilde Schläge") return hitCount + (level - 1);
        return hitCount;
    }

    public int GetManaCost(int level)
    {
        if (skillName == "Blitzschlag") return manaCost + (level - 1) * 5;
        return manaCost;
    }

    public float GetDamageMultiplier(int level)
    {
        if (skillName == "Blitzschlag") return damageMultiplier + (level - 1) * 0.2f;
        return damageMultiplier;
    }

    public string GetTooltipInfo(int level)
    {
        string info = $"<color=#FFD700>{skillName}</color> (Lvl {level})\n";
        info += $"{description}\n\n";
        
        if (isSpell) info += $"Mana: {GetManaCost(level)}\n";
        
        if (skillName == "Wilde Schläge") info += $"Schläge: {GetHitCount(level)}\n";
        else info += $"Schaden: x{GetDamageMultiplier(level):F1}\n";

        if (level < maxLevel)
        {
            info += $"\n<color=#00FF00>Nächste Stufe:</color>\n";
            if (skillName == "Wilde Schläge") info += $"Schläge: {GetHitCount(level + 1)}\n";
            if (skillName == "Blitzschlag") info += $"Schaden: x{GetDamageMultiplier(level + 1):F1} (Mana: {GetManaCost(level + 1)})\n";
        }
        else
        {
            info += "\n<color=#FF4500>Maximalstufe erreicht</color>";
        }

        return info;
    }
}
