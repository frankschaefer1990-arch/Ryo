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
    public int defense = 1;
    public int agility = 1;

    [Header("Health / Mana")]
    public int baseHealth = 100;
    public int maxHealth;
    public int currentHealth;

    public int maxMana = 50;
    public int currentMana = 50;

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

    // =========================
    // SINGLETON + PERSISTENT
    // =========================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log($"PlayerStats: Duplicate script on {gameObject.name} removed.");
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
        
        // Reset health if dead when switching scenes or reloading
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
        }
        
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Keine Listener mehr nötig
    }

    // =========================
    // START
    // =========================
    private void Start()
    {
        RecalculateStats();

        if (currentHealth <= 0)
            currentHealth = maxHealth;

        UpdateUI();
    }

    // ... (rest of the file)


    // =========================
    // UI AUTOMATISCH NEU VERBINDEN
    // =========================
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

        Debug.Log("PlayerStats UI neu verbunden.");
    }

    private void Update()
    {
        // =========================
        // DEBUG KEYS
        // =========================

        // 1 = Stärke +
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            strength++;
            Debug.Log("STR: " + strength);
            UpdateUI();
        }

        // 2 = Schaden
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TakeDamage(10);
        }

        // 3 = EXP
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GainXP(5);
        }
    }

    // =========================
    // STATS BERECHNEN
    // =========================
    public void RecalculateStats()
    {
        // Vitality 1 = 100 HP
        maxHealth = baseHealth + ((vitality - 1) * 10);

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    // =========================
    // UI UPDATE
    // =========================
    public void UpdateUI()
    {
        // MAIN
        if (levelText != null)
            levelText.text = level.ToString();

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;

        if (manaText != null)
            manaText.text = currentMana + " / " + maxMana;

        if (expText != null)
            expText.text = currentXP + " / " + xpToNextLevel;

        if (attributePointsText != null)
            attributePointsText.text = attributePoints.ToString();

        // ATTRIBUTES
        if (strengthText != null)
            strengthText.text = strength.ToString();

        if (vitalityText != null)
            vitalityText.text = vitality.ToString();

        if (defenseText != null)
            defenseText.text = defense.ToString();

        if (agilityText != null)
            agilityText.text = agility.ToString();
    }

    // =========================
    // DAMAGE
    // =========================
    public void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(amount - defense, 1);

        currentHealth -= finalDamage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Damage: -" + finalDamage);

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    // =========================
    // HEAL
    // =========================
    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log("Heal: +" + amount);

        UpdateUI();
    }

    // =========================
    // MANA
    // =========================
    public void UseMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
            currentMana = 0;

        UpdateUI();
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;

        if (currentMana > maxMana)
            currentMana = maxMana;

        UpdateUI();
    }

    // =========================
    // XP
    // =========================
    public void GainXP(int amount)
    {
        currentXP += amount;

        Debug.Log("XP: +" + amount);

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    // =========================
    // LEVEL UP
    // =========================
    public void LevelUp()
    {
        currentXP -= xpToNextLevel;

        level++;

        attributePoints += 3;

        xpToNextLevel += 5;

        Debug.Log("LEVEL UP!");

        UpdateUI();
    }

    // =========================
    // ATTRIBUTE POINT USE
    // =========================
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

    // =========================
    // DEATH
    // =========================
    private void Die()
    {
        Debug.Log("Spieler ist gestorben!");
    }
}