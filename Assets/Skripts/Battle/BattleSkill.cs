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
    public Sprite customSlashSprite; 
    public float slashDuration = 0.2f; 
    public Vector3 visualOffset = Vector3.zero; 
    public Vector3 visualScale = Vector3.one; 
    public bool randomRotation = true; 
    
    public AudioClip skillSound;
    public bool hasCombo = false;
    public float healMultiplier = 0f; 
    public int cooldownTurns = 0; // For enemy/boss skills

    [Header("Curse System")]
public bool isCurseUnlocker = false; // "Dunkler Keim"
    public bool isPassiveCurse = false;  // Skills 2-11

    [Header("Inspector Preview")]
    [TextArea(5, 10)]
    public string effectSummary;

    private void OnValidate()
    {
        string summary = "CATEGORY: " + category + "\n";
        if (isPassiveCurse) summary += "TYPE: Passive\n";
        else if (isCurseUnlocker) summary += "TYPE: System Unlock\n";
        else
        {
            summary += "TYPE: Active\n";
            summary += "Base Hits: " + hitCount + "\n";
            summary += "Dmg Multiplier: x" + damageMultiplier + "\n";
            if (isSpell) summary += "Mana Cost: " + manaCost + "\n";
            
            if (category == SkillCategory.Basic) summary += "Scaling: +1 Damage per Strength point\n";
            else summary += "Scaling: +2 Damage per Intelligence point\n";
        }
        
        if (canStun) summary += "Stun Chance: " + (baseStunChance * 100) + "% (+" + (stunChancePerLevel * 100) + "% per Level)\n";
        
        effectSummary = summary;
    }

    [Header("Special Effects")]
    public bool canStun = false;
    public float baseStunChance = 0.1f;
    public float stunChancePerLevel = 0.05f;
    
    [Header("QTE Settings")]
    public float baseTimeLimit = 1.2f; 
    public float timeLimitReductionPerLevel = 0.05f; 

    // Scaling Methods
    public int GetHitCount(int level) 
    {
        if (skillId == "wilde_schlaege") return 3 + (level - 1);
        if (skillId == "rage") return 5; // Fixed: Rage now has 5 hits to match its 5 stack potential
        if (skillId == "klingenwirbel") return 4 + (level - 1);
        return hitCount;
    }

    public int GetManaCost(int level)
    {
        if (category == SkillCategory.Basic) return 0;

        if (skillId == "blitzschlag") return manaCost + (level - 1) * 10;
        if (skillId == "soulreap") return manaCost + (level - 1) * 10;
        if (skillId == "donnerspeer") return 30 + (level - 1) * 5;
        if (skillId == "schattenklinge") return 25 + (level - 1) * 5;
        if (skillId == "seelenbrand") return 35 + (level - 1) * 5;
        if (skillId == "blutpakt") return 40 + (level - 1) * 10;
        if (skillId == "astralbruch") return 45 + (level - 1) * 5;
        if (skillId == "finstermal") return 30 + (level - 1) * 5;
        if (skillId == "leerenstoss") return 50 + (level - 1) * 10;
        if (skillId == "nachtkralle") return 40 + (level - 1) * 5;
        if (skillId == "hollow_judgment") return 80 + (level - 1) * 15;
        if (skillId == "boss_soul_eruption") return 60;
        return manaCost;
}

    public float GetDamageMultiplier(int level)
    {
        if (skillId == "blitzschlag") return damageMultiplier + (level - 1) * 0.4f; // Increased scaling per level
        if (skillId == "rage") return 1.0f + (level - 1) * 0.05f; // Added level scaling to base multiplier
        
        // Scaling for spells
        if (skillId == "donnerspeer") return 1.2f + (level - 1) * 0.2f; // Buffed base and scaling
        if (skillId == "schattenklinge") return 1.1f + (level - 1) * 0.15f;
        if (skillId == "seelenbrand") return 1.0f + (level - 1) * 0.15f;
        if (skillId == "blutpakt") return 1.3f + (level - 1) * 0.2f;
        if (skillId == "astralbruch") return 1.0f + (level - 1) * 0.15f;
        if (skillId == "finstermal") return 1.0f + (level - 1) * 0.08f;
        if (skillId == "leerenstoss") return 1.2f + (level - 1) * 0.18f;
        if (skillId == "nachtkralle") return 1.1f + (level - 1) * 0.15f;
        if (skillId == "hollow_judgment") return 1.5f + (level - 1) * 0.25f;

        // Scaling for Basic Skills
        if (skillId == "wilde_schlaege") return 1.0f + (level - 1) * 0.1f; // Now scales damage per hit too
        if (skillId == "schaedelbrecher") return 1.0f + (level - 1) * 0.15f;
        if (skillId == "konterklinge") return 1.1f + (level - 1) * 0.18f;
        if (skillId == "blutschnitt") return 1.0f + (level - 1) * 0.14f;
        if (skillId == "aufwaertshieb") return 1.0f + (level - 1) * 0.12f;
        if (skillId == "klingenwirbel") return 1.0f + (level - 1) * 0.08f;
        if (skillId == "eisenbrecher") return 1.1f + (level - 1) * 0.16f;
        if (skillId == "schattenhieb") return 1.0f + (level - 1) * 0.15f;
        if (skillId == "zornspalter") return 1.2f + (level - 1) * 0.2f;
        if (skillId == "seelenklinge") return 1.2f + (level - 1) * 0.22f;

        return damageMultiplier;
    }

    public int GetBaseDamage(int str, int intel, int curse)
    {
        switch (skillId)
        {
            // Spells
            case "blitzschlag": return 150 + (int)(intel * 2.2f);
            case "donnerspeer": return 120 + (int)(intel * 1.6f);
            case "schattenklinge": return 100 + (int)(intel * 1.3f);
            case "seelenbrand": return 35 + (int)(curse * 0.7f);
            case "blutpakt": return 150 + (int)(intel * 1.8f);
            case "astralbruch": return 70 + (int)(intel * 1.0f);
            case "finstermal": return 40 + (int)(curse * 0.5f);
            case "leerenstoss": return 110 + (int)(intel * 1.5f);
            case "nachtkralle": return 105 + (int)(intel * 1.4f);
            case "hollow_judgment": return 180 + (int)(intel * 2.2f);
            
            // Basic Skills
            case "wilde_schlaege": return 30 + (int)(str * 0.8f);
            case "rage": return 25 + (int)(str * 0.6f);
            case "schaedelbrecher": return 90 + (int)(str * 1.4f);
            case "konterklinge": return 75 + (int)(str * 1.2f);
            case "blutschnitt": return 60 + (int)(str * 1.0f);
            case "aufwaertshieb": return 70 + (int)(str * 1.1f);
            case "klingenwirbel": return 18 + (int)(str * 0.4f);
            case "eisenbrecher": return 100 + (int)(str * 1.5f);
            case "schattenhieb": return 85 + (int)(str * 1.2f);
            case "zornspalter": return 130 + (int)(str * 1.8f);
            case "seelenklinge": return 150 + (int)(str * 1.7f + intel * 0.8f);

            default: return -1; 
        }
    }

    public string GetAttributeBonusInfo(int str, int intel, int curse)
    {
        switch (skillId)
        {
            case "donnerspeer": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.6f)} von Intelligenz</color>";
            case "schattenklinge": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.3f)} von Intelligenz</color>";
            case "seelenbrand": return $"Bonus: <color=#800080>+{(int)(curse * 0.7f)} von Fluch</color>";
            case "blutpakt": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.8f)} von Intelligenz</color>";
            case "astralbruch": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.0f)} von Intelligenz</color>";
            case "finstermal": return $"Bonus: <color=#800080>+{(int)(curse * 0.5f)} von Fluch</color>";
            case "leerenstoss": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.5f)} von Intelligenz</color>";
            case "nachtkralle": return $"Bonus: <color=#00FF00>+{(int)(intel * 1.4f)} von Intelligenz</color>";
            case "hollow_judgment": return $"Bonus: <color=#00FF00>+{(int)(intel * 2.2f)} von Intelligenz</color>";
            case "blitzschlag": 
            case "soulreap": return $"Bonus: <color=#00FF00>+{(int)(intel * 2)} von Intelligenz</color>";
            
            // Basic Skills
            case "wilde_schlaege": return $"Bonus: <color=#00FF00>+{(int)(str * 0.7f)} von Stärke</color>";
            case "rage": return $"Bonus: <color=#00FF00>+{(int)(str * 0.5f)} von Stärke</color>";
            case "schaedelbrecher": return $"Bonus: <color=#00FF00>+{(int)(str * 1.4f)} von Stärke</color>";
            case "konterklinge": return $"Bonus: <color=#00FF00>+{(int)(str * 1.2f)} von Stärke</color>";
            case "blutschnitt": return $"Bonus: <color=#00FF00>+{(int)(str * 1.0f)} von Stärke</color>";
            case "aufwaertshieb": return $"Bonus: <color=#00FF00>+{(int)(str * 1.1f)} von Stärke</color>";
            case "klingenwirbel": return $"Bonus: <color=#00FF00>+{(int)(str * 0.4f)} von Stärke</color>";
            case "eisenbrecher": return $"Bonus: <color=#00FF00>+{(int)(str * 1.5f)} von Stärke</color>";
            case "schattenhieb": return $"Bonus: <color=#00FF00>+{(int)(str * 1.2f)} von Stärke</color>";
            case "zornspalter": return $"Bonus: <color=#00FF00>+{(int)(str * 1.8f)} von Stärke</color>";
            case "seelenklinge": return $"Bonus: <color=#00FF00>+{(int)(str * 1.7f)}</color> / <color=#00FF00>+{(int)(intel * 0.8f)} (Str/Int)</color>";

            default: return "";
        }
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

    private string GetPassiveDescription()
    {
        int curseLvl = PlayerStats.Instance != null ? PlayerStats.Instance.agility : 1;
        
        if (skillId == "verflucht_2") return "Reduziert erlittenen Schaden um 15%.\n<color=#00FF00>Bonus:</color> Reduziert Schaden zusätzlich um " + (curseLvl * 0.5f).ToString("F1") + " (fest).";
        if (skillId == "verflucht_3") return "Erhöht Fluch-Gewinn um 40%.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 1.0f).ToString("F1") + "% zusätzlicher Gewinn.";
        if (skillId == "verflucht_4") return "Heilt dich bei Treffern um 5% des Schadens.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.2f).ToString("F1") + "% zusätzliche Heilung.";
        if (skillId == "verflucht_5") return "Erhöht Stun-Chance um 5%.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.3f).ToString("F1") + "% Chance.";
        if (skillId == "verflucht_6") return "Schattenangriff verursacht 20% Zusatzschaden.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.5f).ToString("F1") + "% Schaden.";
        if (skillId == "verflucht_7") return "Erhöht Stun-Chance um weitere 10%.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.5f).ToString("F1") + "% Chance.";
        if (skillId == "verflucht_8") return "Erhöht Zauber- & Fluchschaden um 20%.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.8f).ToString("F1") + "% Kraft.";
        if (skillId == "verflucht_9") return "Gewährt 10% Ausweichchance.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 0.3f).ToString("F1") + "% Chance.";
        if (skillId == "verflucht_10") return "Reduziert Fluch-Verfall pro Runde.\n<color=#00FF00>Bonus:</color> Verfall sinkt um " + (curseLvl * 0.3f).ToString("F1") + ".";
        if (skillId == "verflucht_11") return "Shinigami-Form verursacht 50% mehr Gesamtschaden.\n<color=#00FF00>Bonus:</color> +" + (curseLvl * 1.5f).ToString("F1") + "% Schaden in Form.";
        
        return "Keine speziellen Passiv-Daten gefunden.";
    }

    public string GetTooltipInfo(int level)
    {
        string info = $"<color=#FFD700>{skillName}</color>";
        if (!isPassiveCurse) info += $" (Lvl {level})";
        info += "\n";
        
        info += $"{description}\n\n";
        
        if (isCurseUnlocker)
        {
            info += "<color=#800080>SYSTEM-FREISCHALTUNG</color>\n";
            info += "Schaltet das Fluchsystem frei.\n";
            info += "Fluch steigt durch Aktionen im Kampf.\n";
            info += "25: Passiva aktiv | 50: Aura\n75: Tint | 100: Shinigami-Form\n";
            return info;
        }

        if (isPassiveCurse)
        {
            info += "<color=#FF4500>Passiver Fluch-Effekt</color>\n";
            info += GetPassiveDescription();
            return info;
        }

        if (isSpell) info += $"Mana: {GetManaCost(level)}\n";
        
        int bonusDmg = 0;
        int str = 1;
        int intel = 1;
        int curse = 1;
        if (PlayerStats.Instance != null)
        {
            str = PlayerStats.Instance.strength;
            intel = PlayerStats.Instance.defense;
            curse = PlayerStats.Instance.agility;
            if (category == SkillCategory.Basic) bonusDmg = PlayerStats.Instance.strength;
            else bonusDmg = PlayerStats.Instance.defense * 2; 
        }

        int customBase = GetBaseDamage(str, intel, curse);
        if (customBase > 0)
        {
            float mult = GetDamageMultiplier(level);
            info += $"Schaden: {(int)(customBase * mult)}";
            if (skillId == "seelenbrand") info += " (pro Runde)";
            if (skillId == "wilde_schlaege" || skillId == "klingenwirbel") info += " (pro Schlag)";
            info += "\n";
            
            string bonusLine = GetAttributeBonusInfo(str, intel, curse);
            if (!string.IsNullOrEmpty(bonusLine)) info += bonusLine + "\n";

            // Effect specific info
            if (skillId == "wilde_schlaege") info += "QTE: Perfekt = +1 Schlag (max +3)\n";
            if (skillId == "rage") info += $"Effekt: Jeder Treffer +{8 + (level-1)*2}% Schaden (max 5 Stacks)\nQTE: Treffer beschleunigen Folgetreffer\n";
            if (skillId == "schaedelbrecher") info += $"Stun: 25% (Perfekt: 50%)\nScaling: +10% Dmg / +3% Stun\n";
            if (skillId == "konterklinge") info += "QTE: Kurzes Fenster | Erfolg: +100% Kritchance\n";
            if (skillId == "blutschnitt") info += "Effekt: Blutung 3 Rd. (Perfekt = Doppelt)\n";
            if (skillId == "aufwaertshieb") info += "Effekt: -15% Def (Perfekt: -25% Def)\n";
            if (skillId == "klingenwirbel") info += "QTE: Schnelle Eingaben erhöhen Trefferzahl\n";
            if (skillId == "eisenbrecher") info += "Effekt: Ignoriert 30% Def (Perfekt: 50%)\n";
            if (skillId == "schattenhieb") info += "Effekt: +35% Krit (Perfekt: Garantierter Krit)\n";
            if (skillId == "zornspalter") info += "Effekt: Verbraucht Rage Stacks (+12% Dmg/Stack)\nQTE: Perfekt = Kein Verlust\n";
            if (skillId == "seelenklinge") info += "Effekt: 20% Lifesteal | Perfekt: +10% Dmg\n";

            if (skillId == "donnerspeer") info += "Effekt: -20% Verteidigung (2 Rd.) | Perfekt: -35%\n";
            if (skillId == "schattenklinge") info += $"Krit-Bonus: +{30 + (level-1)*5}% | Perfekt: Garantierter Krit\n";
            if (skillId == "seelenbrand") info += "Effekt: -40% Heilung | Perfekt: +20% Schaden\n";
            if (skillId == "blutpakt") info += "Effekt: -15% eigene HP | Perfekt: +20% Schaden\n";
            if (skillId == "astralbruch") info += $"Mana Burn: {25 + (level-1)*5} | Perfekt: +50%\n";
            if (skillId == "finstermal") info += $"Schadens-Bonus: +{25 + (level - 1) * 5}% (2 Rd.) | Perfekt: +40% Bonus\n";
            if (skillId == "leerenstoss") info += "Effekt: Ignoriert 35% Def | Perfekt: Ignoriert 100%\n";
            if (skillId == "nachtkralle") info += "Effekt: Entfernt Gegner-Buff | Perfekt: +20% Schaden\n";
            if (skillId == "hollow_judgment") info += "Bonus: +50% DMG wenn Ziel <30% HP | Perfekt: +20% Schaden\n";
            if (skillId == "blitzschlag") info += "Perfekt: +20% Schaden\n";
            if (skillId == "soulreap") info += "Perfekt: +20% Schaden & Heilung\n";
            }
else
        {
            string bonusStr = bonusDmg > 0 ? $" <color=#00FF00>+{bonusDmg}</color>" : "";
            if (skillId == "wilde_schlaege" || skillId == "rage" || skillId == "soulreap") info += $"Schläge: {GetHitCount(level)}\n";
            else info += $"Schaden: x{GetDamageMultiplier(level):F1}{bonusStr}\n";

            string bonusInfo = GetAttributeBonusInfo(str, intel, curse);
if (!string.IsNullOrEmpty(bonusInfo)) info += bonusInfo + "\n";

            if (category == SkillCategory.Basic && bonusDmg > 0)
{
                 if (skillId == "wilde_schlaege" || skillId == "rage")
                     info += $"Bonus Schaden pro Schlag: <color=#00FF00>+{bonusDmg}</color>\n";
            }
            else if (bonusDmg > 0)
            {
                 info += $"Bonus Schaden: <color=#00FF00>+{bonusDmg}</color>\n";
            }
        }

        if (GetHealMultiplier(level) > 0) info += $"Heilung: {GetHealMultiplier(level) * 100:F0}% des Schadens\n";
        if (canStun) info += $"Stun Chance: {GetStunChance(level) * 100:F0}%\n";

        if (level < maxLevel && !isPassiveCurse)
        {
            int nextLevelCost = level + 1;
            info += $"\n<color=#00FF00>Nächste Stufe (Kosten: {nextLevelCost}):</color>\n";
            
            if (customBase > 0)
            {
                float nextMult = GetDamageMultiplier(level + 1);
                info += $"Schaden: {(int)(customBase * nextMult)}";
                if (skillId == "seelenbrand") info += " (pro Runde)";
                info += $" (Mana: {GetManaCost(level + 1)})\n";

                // Detailed Scaling Info for Next Level
                if (skillId == "donnerspeer") info += "Scaling: +15% Schaden\n";
                if (skillId == "schattenklinge") info += $"Krit-Bonus: +{30 + level * 5}% (+12% Dmg)\n";
                if (skillId == "seelenbrand") info += "Scaling: +15% DoT\n";
                if (skillId == "blutpakt") info += "Scaling: +18% Schaden\n";
                if (skillId == "astralbruch") info += $"Mana Burn: {25 + level * 5} (+10% Dmg)\n";
                if (skillId == "finstermal") info += $"Schadens-Bonus: +{25 + level * 5}% (+5% Dmg)\n";
                if (skillId == "leerenstoss") info += "Scaling: +14% Schaden\n";
                if (skillId == "nachtkralle") info += "Scaling: +13% Schaden\n";
                if (skillId == "hollow_judgment") info += "Scaling: +20% Schaden\n";
                if (skillId == "blitzschlag") info += "Scaling: +40% Schaden\n";
                if (skillId == "soulreap") info += "Scaling: +10% Heilung\n";
                }
else
            {
                if (skillId == "wilde_schlaege" || skillId == "rage") info += $"Schläge: {GetHitCount(level + 1)}\n";
                if (skillId == "blitzschlag") info += $"Schaden: x{GetDamageMultiplier(level + 1):F1} (Mana: {GetManaCost(level + 1)})\n";
                else if (skillId == "soulreap") info += $"Heilung: {GetHealMultiplier(level + 1) * 100:F0}% (Mana: {GetManaCost(level + 1)})\n";
                else if (!isSpell) info += $"Schaden: x{GetDamageMultiplier(level + 1):F1}\n";
                else if (isSpell) info += $"Mana: {GetManaCost(level + 1)}\n";
            }

            if (canStun) info += $"Stun Chance: {GetStunChance(level + 1) * 100:F0}%\n";
            if (hasCombo && skillId == "rage") info += $"QTE Tempo: +{(1.0f - GetQTETimeLimit(level+1)/GetQTETimeLimit(level))*100:F0}%\n";
        }
else if (isPassiveCurse) info += "\n<color=#FF4500>Passiver Effekt</color>";
        else info += "\n<color=#FF4500>Maximalstufe erreicht</color>";

        return info;
    }
}
