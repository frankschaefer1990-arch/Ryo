using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI selectedItemText;

    [Header("Player")]
    public PlayerMovement playerMovement;

    [Header("Shop Slot Visuals")]
    // Blaues Overlay / Highlight Objekt
    public GameObject potionSelectionHighlight;

    // Aktuell gewähltes Item
    private int selectedItemID = 0;
    private string selectedItemName = "";
    private int selectedItemPrice = 0;

    private void Start()
    {
        // Player automatisch finden
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        // Highlight beim Start AUS
        if (potionSelectionHighlight != null)
        {
            potionSelectionHighlight.SetActive(false);
        }

        UpdateGoldUI();
        ClearSelection();
    }

    private void Update()
    {
        // =========================
        // ESC = SHOP SCHLIESSEN
        // =========================
        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LeaveShop();
            }
        }
    }

    // =========================
    // GENERISCHES ITEM SYSTEM
    // =========================
    public void SelectItem(int itemID, string itemName, int itemPrice)
    {
        selectedItemID = itemID;
        selectedItemName = itemName;
        selectedItemPrice = itemPrice;

        if (selectedItemText != null)
        {
            selectedItemText.text = "Gewählt: " + itemName + " (" + itemPrice + "G)";
        }

        Debug.Log("Item gewählt: " + itemName);
    }

    // =========================
    // SLOT HIGHLIGHTS RESET
    // =========================
    public void ClearSlotHighlights()
    {
        // Potion Highlight AUS
        if (potionSelectionHighlight != null)
        {
            potionSelectionHighlight.SetActive(false);
        }

        // Später:
        // swordSelectionHighlight.SetActive(false);
        // armorSelectionHighlight.SetActive(false);
    }

    // =========================
    // WRAPPER FÜR UNITY BUTTONS
    // =========================

    // Potion
    public void SelectPotion()
    {
        // Alte Auswahl entfernen
        ClearSlotHighlights();

        // Potion auswählen
        SelectItem(1, "Health Potion", 10);

        // Highlight AN
        if (potionSelectionHighlight != null)
        {
            potionSelectionHighlight.SetActive(true);
        }
    }

    // Eisenschwert
    public void SelectIronSword()
    {
        ClearSlotHighlights();
        SelectItem(2, "Iron Sword", 50);
    }

    // Lederrüstung
    public void SelectLeatherArmor()
    {
        ClearSlotHighlights();
        SelectItem(3, "Leather Armor", 40);
    }

    // Mana Potion
    public void SelectManaPotion()
    {
        ClearSlotHighlights();
        SelectItem(4, "Mana Potion", 15);
    }

    // =========================
    // BUY
    // =========================
    public void BuySelectedItem()
    {
        // Nichts gewählt
        if (selectedItemID == 0)
        {
            Debug.Log("Kein Item gewählt!");
            return;
        }

        if (PlayerGold.Instance == null)
        {
            Debug.LogError("PlayerGold fehlt!");
            return;
        }

        // Nicht genug Gold
        if (!PlayerGold.Instance.SpendGold(selectedItemPrice))
        {
            Debug.Log("Nicht genug Gold!");
            return;
        }

        // Item kaufen
        switch (selectedItemID)
        {
            // Health Potion
            case 1:
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddPotion();
                }
                break;

            // Sword
            case 2:
                Debug.Log("Iron Sword gekauft! (Equipment folgt)");
                break;

            // Armor
            case 3:
                Debug.Log("Leather Armor gekauft! (Equipment folgt)");
                break;

            // Mana Potion
            case 4:
                Debug.Log("Mana Potion gekauft! (folgt)");
                break;

            default:
                Debug.LogWarning("Unbekannte Item ID");
                break;
        }

        Debug.Log(selectedItemName + " gekauft!");

        UpdateGoldUI();
    }

    // =========================
    // SELL
    // =========================
    public void SellSelectedItem()
    {
        if (selectedItemID == 0)
        {
            Debug.Log("Kein Item gewählt!");
            return;
        }

        Debug.Log(selectedItemName + " Verkauf folgt später");
    }

    // =========================
    // CLEAR
    // =========================
    public void ClearSelection()
    {
        selectedItemID = 0;
        selectedItemName = "";
        selectedItemPrice = 0;

        if (selectedItemText != null)
        {
            selectedItemText.text = "Kein Item gewählt";
        }

        // Highlight entfernen
        ClearSlotHighlights();
    }

    // =========================
    // SHOP ÖFFNEN
    // =========================
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        // Spieler stoppen
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        // Maus sichtbar
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UpdateGoldUI();
        ClearSelection();
    }

    // =========================
    // SHOP SCHLIESSEN
    // =========================
    public void LeaveShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Spieler wieder bewegen
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }

        // Maus verstecken
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        ClearSelection();
    }

    // =========================
    // GOLD UI
    // =========================
    public void UpdateGoldUI()
    {
        if (goldText != null && PlayerGold.Instance != null)
        {
            goldText.text = "Gold: " + PlayerGold.Instance.currentGold;
        }
    }
}