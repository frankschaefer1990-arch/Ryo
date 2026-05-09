using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AttributeUI : MonoBehaviour
{
    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("Main Text")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI attributePointsText;

    [Header("Attribute Text")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI speedText;

    [Header("Bars")]
    public Slider hpBar;
    public Slider expBar;

    [Header("Attribute Buttons")]
    public Button strengthButton;
    public Button vitalityButton;
    public Button armorButton;
    public Button speedButton;

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectAndSetup;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectAndSetup;
    }

    private void ReconnectAndSetup()
    {
        ReconnectPlayerStats();
        SetupAllButtons();
    }

    private void Start()
    {
        ReconnectAndSetup();
    }

    private void OnDestroy()
    {
        // Keine Listener mehr nötig
    }

    // =========================
    // PLAYERSTATS IMMER NEU FINDEN
    // =========================
    private void ReconnectPlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats konnte nicht gefunden werden!");
        }
        else
        {
            Debug.Log("PlayerStats erfolgreich verbunden.");
        }
    }

    // =========================
    // BUTTONS KOMPLETT NEU VERBINDEN
    // =========================
    public void SetupAllButtons()
    {
        SetupButton(strengthButton);
        SetupButton(vitalityButton);
        SetupButton(armorButton);
        SetupButton(speedButton);

        if (strengthButton != null)
        {
            strengthButton.onClick.RemoveAllListeners();
            strengthButton.onClick.AddListener(AddStrength);
        }

        if (vitalityButton != null)
        {
            vitalityButton.onClick.RemoveAllListeners();
            vitalityButton.onClick.AddListener(AddVitality);
        }

        if (armorButton != null)
        {
            armorButton.onClick.RemoveAllListeners();
            armorButton.onClick.AddListener(AddArmor);
        }

        if (speedButton != null)
        {
            speedButton.onClick.RemoveAllListeners();
            speedButton.onClick.AddListener(AddSpeed);
        }
    }

    private void Update()
    {
        if (playerStats == null)
        {
            ReconnectPlayerStats();
            return;
        }

        // =========================
        // MAIN TEXT
        // =========================
        if (levelText != null)
            levelText.text = playerStats.level.ToString();

        if (hpText != null)
            hpText.text = playerStats.currentHealth + " / " + playerStats.maxHealth;

        if (expText != null)
            expText.text = playerStats.currentXP + " / " + playerStats.xpToNextLevel;

        if (attributePointsText != null)
            attributePointsText.text = playerStats.attributePoints.ToString();

        // =========================
        // ATTRIBUTE TEXT
        // =========================
        if (strengthText != null)
            strengthText.text = playerStats.strength.ToString();

        if (vitalityText != null)
            vitalityText.text = playerStats.vitality.ToString();

        if (armorText != null)
            armorText.text = playerStats.defense.ToString();

        if (speedText != null)
            speedText.text = playerStats.agility.ToString();

        // =========================
        // HP BAR
        // =========================
        if (hpBar != null)
        {
            hpBar.minValue = 0;
            hpBar.maxValue = playerStats.maxHealth;
            hpBar.value = playerStats.currentHealth;
        }

        // =========================
        // EXP BAR
        // =========================
        if (expBar != null)
        {
            expBar.minValue = 0;
            expBar.maxValue = playerStats.xpToNextLevel;
            expBar.value = playerStats.currentXP;
        }

        // =========================
        // BUTTON VISUALS
        // =========================
        UpdateButtonVisual(strengthButton);
        UpdateButtonVisual(vitalityButton);
        UpdateButtonVisual(armorButton);
        UpdateButtonVisual(speedButton);
    }

    // =========================
    // BUTTON SETUP
    // =========================
    private void SetupButton(Button button)
    {
        if (button == null) return;

        // ColorTint für visuelles Feedback beim Klicken
        button.transition = Selectable.Transition.ColorTint;
        
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;
    }

    // =========================
    // BUTTON VISUAL UPDATE
    // =========================
    private void UpdateButtonVisual(Button button)
    {
        if (button == null || playerStats == null) return;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) return;

        bool hasPoints = playerStats.attributePoints > 0;
        button.interactable = hasPoints;

        if (hasPoints)
        {
            buttonImage.color = Color.white;
        }
        else
        {
            buttonImage.color = new Color(1f, 1f, 1f, 0.3f);
        }
    }

    // =========================
    // STRENGTH
    // =========================
    public void AddStrength()
    {
        if (playerStats == null) return;
        if (!playerStats.UseAttributePoint()) return;

        playerStats.strength += 1;

        Debug.Log("STR erhöht auf: " + playerStats.strength);

        playerStats.UpdateUI();
    }

    // =========================
    // VITALITY
    // =========================
    public void AddVitality()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats fehlt!");
            return;
        }

        if (!playerStats.UseAttributePoint())
        {
            Debug.Log("Keine Attributpunkte!");
            return;
        }

        int oldMax = playerStats.maxHealth;
        playerStats.vitality += 1;
        playerStats.RecalculateStats();
        
        // Die Differenz (HP Gewinn) auch heilen
        int healthGain = playerStats.maxHealth - oldMax;
        playerStats.currentHealth += healthGain;

        if (playerStats.currentHealth > playerStats.maxHealth)
        {
            playerStats.currentHealth = playerStats.maxHealth;
        }

        Debug.Log("VIT erhöht auf: " + playerStats.vitality);
        Debug.Log("HP: " + playerStats.currentHealth + " / " + playerStats.maxHealth);

        playerStats.UpdateUI();
    }

    // =========================
    // ARMOR
    // =========================
    public void AddArmor()
    {
        if (playerStats == null) return;
        if (!playerStats.UseAttributePoint()) return;

        playerStats.defense += 1;

        Debug.Log("DEF erhöht auf: " + playerStats.defense);

        playerStats.UpdateUI();
    }

    // =========================
    // SPEED
    // =========================
    public void AddSpeed()
    {
        if (playerStats == null) return;
        if (!playerStats.UseAttributePoint()) return;

        playerStats.agility += 1;

        Debug.Log("AGI erhöht auf: " + playerStats.agility);

        playerStats.UpdateUI();
    }

    // =========================
    // PANEL SCHLIESSEN
    // =========================
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // =========================
    // PANEL TOGGLE
    // =========================
    public void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}