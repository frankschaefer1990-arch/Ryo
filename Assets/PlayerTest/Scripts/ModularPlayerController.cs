using UnityEngine;

/// <summary>
/// Handles the modular visual parts of the player character.
/// It syncs equipment from PlayerStats and ensures all layers animate together.
/// </summary>
public class ModularPlayerController : MonoBehaviour
{
    [Header("Modular Parts")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer armorRenderer;
    public SpriteRenderer weaponRenderer;
    public SpriteRenderer capeRenderer;

    [Header("References")]
    private Animator animator;
    private PlayerStats playerStats;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        playerStats = PlayerStats.Instance;
        UpdateEquipmentVisuals();
    }

    /// <summary>
    /// Call this whenever equipment changes in the inventory.
    /// </summary>
    public void UpdateEquipmentVisuals()
    {
        if (playerStats == null) return;

        // In a real implementation, you would load sprites from a database or Resources
        // based on the IDs: playerStats.equippedWeapon, playerStats.equippedArmor, etc.
        
        Debug.Log($"ModularPlayerController: Updating visuals. Weapon ID: {playerStats.equippedWeapon}, Armor ID: {playerStats.equippedArmor}");
        
        // Example logic:
        // if (playerStats.equippedWeapon == 3) weaponRenderer.sprite = swordSprite;
        // else if (playerStats.equippedWeapon == 8) weaponRenderer.sprite = wandSprite;
    }

    void Update()
    {
        // For testing/preview purposes:
        // In the final version, this should be called by the Inventory System.
        if (Input.GetKeyDown(KeyCode.U)) 
        {
            UpdateEquipmentVisuals();
        }
    }
}
