using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    void Start()
    {
        // Buttons klickbar vorbereiten
        SetupButton(strengthButton);
        SetupButton(vitalityButton);
        SetupButton(armorButton);
        SetupButton(speedButton);
    }

    void Update()
    {
        if (playerStats == null) return;

        // ===== MAIN TEXT =====
        if (levelText != null)
            levelText.text = playerStats.level.ToString();

        if (hpText != null)
            hpText.text = playerStats.currentHP + " / " + playerStats.maxHP;

        if (expText != null)
            expText.text = playerStats.currentXP + " / " + playerStats.xpToNextLevel;

        if (attributePointsText != null)
            attributePointsText.text = playerStats.attributePoints.ToString();

        // ===== ATTRIBUTE TEXT =====
        if (strengthText != null)
            strengthText.text = playerStats.strength.ToString();

        if (vitalityText != null)
            vitalityText.text = playerStats.vitality.ToString();

        if (armorText != null)
            armorText.text = playerStats.armor.ToString();

        if (speedText != null)
            speedText.text = playerStats.speed.ToString();

        // ===== HP BAR =====
        if (hpBar != null)
        {
            hpBar.minValue = 0;
            hpBar.maxValue = playerStats.maxHP;
            hpBar.value = playerStats.currentHP;
        }

        // ===== EXP BAR =====
        if (expBar != null)
        {
            expBar.minValue = 0;
            expBar.maxValue = playerStats.xpToNextLevel;
            expBar.value = playerStats.currentXP;
        }

        // ===== BUTTON VISUALS =====
        UpdateButtonVisual(strengthButton);
        UpdateButtonVisual(vitalityButton);
        UpdateButtonVisual(armorButton);
        UpdateButtonVisual(speedButton);
    }

    void SetupButton(Button button)
    {
        if (button == null) return;

        // Wichtig:
        // Kein Unity Abdunkeln
        button.transition = Selectable.Transition.None;
    }

    void UpdateButtonVisual(Button button)
    {
        if (button == null) return;

        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage == null) return;

        bool hasPoints = playerStats.attributePoints > 0;

        // Klickbar nur bei Punkten
        button.interactable = hasPoints;

        if (hasPoints)
        {
            // Volles sichtbares Rot
            buttonImage.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            // Unsichtbar -> Graues Hintergrund Plus sichtbar
            buttonImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}