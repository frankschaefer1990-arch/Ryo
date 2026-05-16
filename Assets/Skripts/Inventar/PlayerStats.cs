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
    public int strength = 1;
    public int vitality = 1;
    public int defense = 1; // Used as Intelligence
    public int agility = 1; // Used as Curse

    [Header("Health / Mana")]
    public int baseHealth = 100;
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
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI agilityText;

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
            defenseText = attributeUI.armorText;
            agilityText = attributeUI.speedText;
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

    public void RecalculateStats()
    {
        maxHealth = baseHealth + ((vitality - 1) * 10);
        maxMana = 50 + (defense * 10);
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentMana > maxMana) currentMana = maxMana;
    }

    public void UpdateUI()
    {
        if (levelText != null) levelText.text = level.ToString();
        if (healthText != null) healthText.text = currentHealth + " / " + maxHealth;
        if (manaText != null) manaText.text = currentMana + " / " + maxMana;
        if (expText != null) expText.text = currentXP + " / " + xpToNextLevel;
        if (attributePointsText != null) attributePointsText.text = attributePoints.ToString();
        if (strengthText != null) strengthText.text = strength.ToString();
        if (vitalityText != null) vitalityText.text = vitality.ToString();
        if (defenseText != null) defenseText.text = defense.ToString();
        if (agilityText != null) agilityText.text = agility.ToString();
    }

    public bool HasCursePassive(int skillIndex)
    {
        if (SkillManager.Instance == null) return false;
        return SkillManager.Instance.GetSkillLevelById("verflucht_" + skillIndex) > 0;
    }

    public void ChangeCurseValue(int amount)
    {
        if (!isCurseSystemUnlocked) return;
        
        // Fluchfokus (Skill 3): Increase gain by 50%
        if (amount > 0 && HasCursePassive(3)) amount = (int)(amount * 1.5f);

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
        // Finsterschritt (Skill 9): 15% Dodge chance if curse active
        if (IsCursePassiveActive() && HasCursePassive(9))
        {
            if (Random.value < 0.15f)
            {
                Debug.Log("DODGED! (Finsterschritt)");
                return;
            }
        }

        int defValue = defense;
        int finalDamage = Mathf.Max(amount - defValue, 1);
        
        // Schattengunst (Skill 2): 20% Damage reduction if curse active
        if (IsCursePassiveActive() && HasCursePassive(2))
        {
            finalDamage = (int)(finalDamage * 0.8f);
            if (finalDamage < 1) finalDamage = 1;
        }

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