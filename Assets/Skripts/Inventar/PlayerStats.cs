using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Level System")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int attributePoints = 0;

    [Header("Health")]
    public int vitality = 5;
    public int maxHP = 100;
    public int currentHP = 75;

    [Header("Attributes")]
    public int strength = 3;
    public int armor = 3;
    public int speed = 4;

    [Header("Combat")]
    public int baseDamage = 5;

    [Header("Speed Settings")]
    public float movementSpeed = 5f;

    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        ApplySpeed();

        ClampValues();
    }

    void Update()
    {
        ClampValues();

        // =========================
        // DEBUG TEST BUTTONS
        // =========================

        // Taste 1 = Spieler nimmt 10 Schaden
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamage(10);
            Debug.Log("Spieler nimmt 10 Schaden | HP: " + currentHP + "/" + maxHP);
        }

        // Taste 2 = Spieler heilt 10 HP
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Heal(10);
            Debug.Log("Spieler heilt 10 HP | HP: " + currentHP + "/" + maxHP);
        }

        // Taste 3 = Zeigt deinen aktuellen Schaden
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Dein Schaden: " + GetDamage());
        }

        // Taste 4 = +50 EXP
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GainXP(50);
            Debug.Log("EXP erhalten | XP: " + currentXP + "/" + xpToNextLevel);
        }
    }

    void ClampValues()
    {
        if (vitality < 1)
            vitality = 1;

        if (maxHP < 1)
            maxHP = 1;

        if (currentHP > maxHP)
            currentHP = maxHP;

        if (currentHP < 0)
            currentHP = 0;

        if (currentXP < 0)
            currentXP = 0;

        if (xpToNextLevel < 1)
            xpToNextLevel = 1;
    }

    // =========================
    // DAMAGE OUTPUT
    // =========================
    public int GetDamage()
    {
        // Base Damage + Strength
        return baseDamage + strength;
    }

    // =========================
    // XP SYSTEM
    // =========================
    public void GainXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;

        // Pro Level 3 Attributpunkte
        attributePoints += 3;

        // Nächstes Level schwerer
        xpToNextLevel += 50;

        Debug.Log("LEVEL UP! Level: " + level);
    }

    // =========================
    // DAMAGE INPUT
    // =========================
    public void TakeDamage(int enemyDamage)
    {
        // Armor reduziert 0.5 Schaden pro Punkt
        float reducedDamage = enemyDamage - (armor * 0.5f);

        // Mindestens 1 Schaden
        int finalDamage = Mathf.Max(Mathf.RoundToInt(reducedDamage), 1);

        currentHP -= finalDamage;

        ClampValues();
    }

    // =========================
    // HEAL
    // =========================
    public void Heal(int amount)
    {
        currentHP += amount;

        ClampValues();
    }

    // =========================
    // SPEED
    // =========================
    void ApplySpeed()
    {
        if (playerMovement != null)
        {
            playerMovement.speed = movementSpeed;
        }
    }

    // =========================
    // ATTRIBUTE BUTTONS
    // =========================
    public void IncreaseStrength()
    {
        if (attributePoints > 0)
        {
            strength++;
            attributePoints--;

            Debug.Log("Strength erhöht: " + strength);
        }
    }

    public void IncreaseVitality()
    {
        if (attributePoints > 0)
        {
            vitality++;
            attributePoints--;

            // +5 Max HP
            maxHP += 5;

            // +5 Current HP
            currentHP += 5;

            ClampValues();

            Debug.Log("Vitality erhöht: " + vitality);
        }
    }

    public void IncreaseArmor()
    {
        if (attributePoints > 0)
        {
            armor++;
            attributePoints--;

            Debug.Log("Armor erhöht: " + armor);
        }
    }

    public void IncreaseSpeed()
    {
        if (attributePoints > 0)
        {
            speed++;
            attributePoints--;

            // Nur minimal schneller
            movementSpeed += 0.1f;

            ApplySpeed();

            Debug.Log("Speed erhöht: " + speed + " | Movement Speed: " + movementSpeed);
        }
    }
}