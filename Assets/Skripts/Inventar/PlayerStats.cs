using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Level System")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;
    public int attributePoints = 0;

    [Header("Core Attributes")]
    [Tooltip("Increases Damage of Basic skills.")]
    public int strength = 1;
    [Tooltip("Increases Max Health.")]
    public int vitality = 1;
    [Tooltip("Intelligence: Increases Max Mana and Mana Regen per turn.")]
    public int defense = 1; // Used as Intelligence
    [Tooltip("Curse: Increases Curse effectiveness/gain (No longer gives speed).")]
    public int agility = 1; // Used as Curse

    [Header("Stat Info (Inspector Only)")]
    [TextArea(5, 10)]
    public string statDescription = "Strength: +1 Dmg per point to basic attacks\n" +
                                    "Vitality: +10 Max HP per point\n" +
                                    "Intelligence: +10 Max Mana, +1 Mana Regen per turn & +2 Spell Damage per point\n" +
                                    "Curse: Increases the effectiveness and scaling of all passive curse skills.";

    [Header("Health / Mana")]
    public int maxHealth;
    public int currentHealth;
    public int maxMana = 50;
    public int currentMana = 50;

    [Header("Curse System")]
    public bool isCurseSystemUnlocked = false;
    public int curseValue = 0;
    public int maxCurseValue = 100;

    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI attributePointsText;

    [Header("Attribute UI Text")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI curseText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectAndUpdateUI;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectAndUpdateUI;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ReconnectAndUpdateUI()
    {
        ReconnectUI();
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
        }
        UpdateUI();
    }

    private void Start()
    {
        RecalculateStats();
        if (currentHealth <= 0) currentHealth = maxHealth;
        UpdateUI();
    }

    private void ReconnectUI()
    {
        AttributeUI attributeUI = FindFirstObjectByType<AttributeUI>();
        if (attributeUI != null)
        {
            levelText = attributeUI.levelText;
            healthText = attributeUI.hpText;
            expText = attributeUI.expText;
            attributePointsText = attributeUI.attributePointsText;
            strengthText = attributeUI.strengthText;
            vitalityText = attributeUI.vitalityText;
            intelligenceText = attributeUI.intelligenceText;
            curseText = attributeUI.curseText;
            }
            }

    private void Update()
    {
        // Debug Keys
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            UpdateUI();
            
            // Also update Battle UI bars if present
            if (BattleUI.Instance != null)
            {
                BattleUI.Instance.UpdatePlayerHP(1f, currentHealth, maxHealth);
                BattleUI.Instance.UpdatePlayerMana(1f, currentMana, maxMana);
            }
            
            Debug.Log("Debug: Spieler voll geheilt (HP/Mana)");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            LevelUp();
            Debug.Log("Debug: Level Up ausgelöst");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.AddPoints(5);
                var ui = FindAnyObjectByType<SkillUI>();
                if (ui != null) ui.RefreshUI();
                Debug.Log("Debug: +5 Skillpunkte erhalten");
            }
        }
    }

    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseMana = 50;
    public int bonusArmor = 0;
    public int bonusSpellDamage = 0;
    public int bonusPhysicalDamage = 0;

    [Header("Equipment")]
    public int equippedWeapon = 0; // Sword (3) or Wand (8)
    public int equippedHelm = 0;   // Helm (4)
    public int equippedArmor = 0;  // Armor (5)
    public int equippedRing1 = 0;  // Ring (6)
    public int equippedRing2 = 0;  // Ring (6)
    public int equippedBoots = 0;  // Boots (7)

    public void RecalculateStats()
    {
        int bonusVitality = 0;
        int bonusDefense = 0; // Intelligence
        int bonusStrength = 0;
        int bonusAgility = 0; // Curse
        int bonusMana = 0;
        bonusArmor = 0;
        bonusSpellDamage = 0;
        bonusPhysicalDamage = 0;

        // Apply equipment bonuses
        ApplyItemBonuses(equippedWeapon, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);
        ApplyItemBonuses(equippedHelm, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);
        ApplyItemBonuses(equippedArmor, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);
        ApplyItemBonuses(equippedRing1, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);
        ApplyItemBonuses(equippedRing2, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);
        ApplyItemBonuses(equippedBoots, ref bonusStrength, ref bonusVitality, ref bonusDefense, ref bonusAgility, ref bonusMana);

        maxHealth = baseHealth + ((vitality + bonusVitality - 1) * 10);
        maxMana = baseMana + ((defense + bonusDefense) * 10) + bonusMana;
    }

    private void ApplyItemBonuses(int itemId, ref int str, ref int vit, ref int def, ref int agi, ref int mana)
    {
        switch (itemId)
        {
            case 3: // Basic Sword
                str += 5; bonusPhysicalDamage += 10; break;
            case 4: // Basic Helm
                vit += 20; bonusArmor += 5; break;
            case 5: // Basic Armor
                vit += 40; bonusArmor += 10; break;
            case 6: // Basic Ring
                def += 5; mana += 20; break;
            case 7: // Basic Boots
                agi += 5; break;
            case 8: // Basic Wand
                def += 10; bonusSpellDamage += 10; break;
            
            case 9: // Rare Sword
                str += 12; bonusPhysicalDamage += 30; break;
            case 10: // Rare Helm
                vit += 45; bonusArmor += 12; break;
            case 11: // Rare Armor
                vit += 80; bonusArmor += 25; break;
            case 12: // Rare Ring
                def += 12; mana += 50; break;
            case 13: // Rare Boots
                agi += 12; break;
            case 14: // Rare Wand
                def += 25; bonusSpellDamage += 25; break;

            case 15: // Undead Sword (Special)
                str += 20; bonusSpellDamage += 15; bonusPhysicalDamage += 25; break;

            case 16: // Epic Helm
                vit += 100; bonusArmor += 25; break;
            case 17: // Epic Armor
                vit += 150; bonusArmor += 40; break;
            case 18: // Epic Ring
                def += 25; mana += 100; break;
            case 19: // Epic Boots
                agi += 25; break;
            case 21: // Epic Sword
                str += 30; bonusPhysicalDamage += 60; break;
            case 26: // Epic Wand
                def += 50; bonusSpellDamage += 60; break;

            case 22: // Legendary Helm
                vit += 250; bonusArmor += 60; break;
            case 23: // Legendary Armor
                vit += 400; bonusArmor += 100; break;
            case 24: // Legendary Ring
                def += 60; mana += 250; break;
            case 25: // Legendary Boots
                agi += 60; break;
            case 27: // Legendary Sword
                str += 75; bonusPhysicalDamage += 150; break;
            case 28: // Legendary Wand
                def += 120; bonusSpellDamage += 150; break;
        }
    }

    public int GetTotalStrength() { int s=0, v=0, d=0, a=0, m=0; ApplyItemBonuses(equippedWeapon, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedHelm, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedArmor, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing1, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing2, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedBoots, ref s, ref v, ref d, ref a, ref m); return strength + s; }
    public int GetTotalVitality() { int s=0, v=0, d=0, a=0, m=0; ApplyItemBonuses(equippedWeapon, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedHelm, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedArmor, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing1, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing2, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedBoots, ref s, ref v, ref d, ref a, ref m); return vitality + v; }
    public int GetTotalIntelligence() { int s=0, v=0, d=0, a=0, m=0; ApplyItemBonuses(equippedWeapon, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedHelm, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedArmor, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing1, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing2, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedBoots, ref s, ref v, ref d, ref a, ref m); return defense + d; }
    public int GetTotalCurse() { int s=0, v=0, d=0, a=0, m=0; ApplyItemBonuses(equippedWeapon, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedHelm, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedArmor, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing1, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedRing2, ref s, ref v, ref d, ref a, ref m); ApplyItemBonuses(equippedBoots, ref s, ref v, ref d, ref a, ref m); return agility + a; }

    public void SetStats(int lvl, int xp, int pts, int str, int vit, int def, int agi, bool curseUnlocked, int curseVal)
{
        level = lvl;
        currentXP = xp;
        attributePoints = pts;
        strength = str;
        vitality = vit;
        defense = def;
        agility = agi;
        isCurseSystemUnlocked = curseUnlocked;
        curseValue = curseVal;
        
        RecalculateStats();
        UpdateUI();
    }

    public void RestoreHPAndMana(int hp, int mana)
    {
        currentHealth = hp;
        currentMana = mana;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (levelText != null) levelText.text = level.ToString();
        if (healthText != null) healthText.text = currentHealth + " / " + maxHealth;
        if (manaText != null) manaText.text = currentMana + " / " + maxMana;
        if (expText != null) expText.text = currentXP + " / " + xpToNextLevel;
        if (attributePointsText != null) attributePointsText.text = attributePoints.ToString();
        
        if (strengthText != null) strengthText.text = GetTotalStrength().ToString();
        if (vitalityText != null) vitalityText.text = GetTotalVitality().ToString();
        if (intelligenceText != null) intelligenceText.text = GetTotalIntelligence().ToString();
        if (curseText != null) curseText.text = GetTotalCurse().ToString();
    }

    public bool HasCursePassive(int skillIndex)
    {
        if (SkillManager.Instance == null) return false;
        return SkillManager.Instance.GetSkillLevelById("verflucht_" + skillIndex) > 0;
    }

    public void ChangeCurseValue(int amount)
    {
        if (!isCurseSystemUnlocked) return;
        
        // Fluchfokus (Skill 3): +40% gain + Scaling
        if (amount > 0 && HasCursePassive(3)) 
        {
            float bonusMult = 1.40f + (agility * 0.01f);
            amount = (int)(amount * bonusMult);
        }

        curseValue = Mathf.Clamp(curseValue + amount, 0, maxCurseValue);
UpdateUI();
        if (BattleUI.Instance != null) BattleUI.Instance.UpdateCurseBar();
    }

    public int GetCurseForm()
    {
        if (!isCurseSystemUnlocked) return 0;
        if (curseValue >= 100) return 3; 
        if (curseValue >= 75) return 2;  
        if (curseValue >= 50) return 1;  
        return 0;
    }

    public bool IsCursePassiveActive()
    {
        return isCurseSystemUnlocked && curseValue >= 25;
    }

    public void TakeDamage(int amount)
    {
        // Finsterschritt (Skill 9): 10% Dodge chance + Bonus if curse active
        if (IsCursePassiveActive() && HasCursePassive(9))
        {
            float dodgeChance = 0.10f + (agility * 0.003f); // Agility is Curse attribute
            if (Random.value < dodgeChance)
            {
                Debug.Log("DODGED! (Finsterschritt)");
                return;
            }
        }

        int finalDamage = amount;
        
        // Armor Reduction
        finalDamage -= (bonusArmor / 2);
        
        // Schattengunst (Skill 2): 15% Damage reduction + Fixed Bonus if curse active
        if (IsCursePassiveActive() && HasCursePassive(2))
        {
            finalDamage = (int)(finalDamage * 0.85f);
            finalDamage -= (int)(agility * 0.5f);
        }

        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;
if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;
        UpdateUI();
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;
        UpdateUI();
    }

    public void GainXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel) LevelUp();
        UpdateUI();
    }

    public void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        attributePoints += 3;
        if (SkillManager.Instance != null) SkillManager.Instance.AddPoints(1);
        xpToNextLevel += 5;
        UpdateUI();
    }

    public bool UseAttributePoint()
    {
        if (attributePoints > 0)
        {
            attributePoints--;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void Die() { Debug.Log("Spieler ist gestorben!"); }
}