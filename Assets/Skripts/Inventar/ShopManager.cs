using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;

    [Header("Buttons")]
    public Button buyButton;
    public Button sellButton;
    public Button leaveButton;
    public Button potionSlotButton;

    [Header("Potion UI")]
    public GameObject selectionHighlight;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI potionPriceText;

    [Header("Settings")]
    public int potionPrice = 10;
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.R;
    public float dialogueDuration = 1.2f;

    private Transform player;
    private bool playerInRange = false;
    private bool potionSelected = false;
    private bool isShopOpen = false;
    private bool isOpeningShop = false;

    // =========================
    // START
    // =========================
    private void Start()
    {
        ReconnectShop();

        // Shop zu Beginn schließen
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Auswahl aus
        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);

        UpdateGoldUI();
    }

    // =========================
    // UPDATE
    // =========================
    private void Update()
    {
        FindPlayer();

        if (player == null)
            return;

        CheckRange();

        // =========================
        // SHOP ÖFFNEN
        // =========================
        if (playerInRange &&
            Input.GetKeyDown(interactKey) &&
            !isShopOpen &&
            !isOpeningShop)
        {
            OpenShop();
        }

        // =========================
        // SHOP MIT ESC SCHLIESSEN
        // =========================
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    // =========================
    // PLAYER FINDEN
    // =========================
    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    // =========================
    // RANGE CHECK
    // =========================
    private void CheckRange()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
    }

    // =========================
    // SHOP UI RECONNECT
    // =========================
    public void ReconnectShop()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ShopManager: Canvas nicht gefunden!");
            return;
        }

        // Suche ShopPanel unter dem Canvas (auch wenn deaktiviert)
        Transform spTransform = canvas.transform.Find("ShopPanel");
        if (spTransform != null)
        {
            shopPanel = spTransform.gameObject;
        }
        else
        {
            // Suche tiefer falls nötig
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "ShopPanel")
                {
                    shopPanel = t.gameObject;
                    break;
                }
            }
        }

        if (shopPanel != null)
        {
            buyButton = shopPanel.transform.Find("BuyButton")?.GetComponent<Button>();
            sellButton = shopPanel.transform.Find("SellButton")?.GetComponent<Button>();
            leaveButton = shopPanel.transform.Find("LeaveButton")?.GetComponent<Button>();
            potionSlotButton = shopPanel.transform.Find("PotionSlot")?.GetComponent<Button>();
            selectionHighlight = shopPanel.transform.Find("PotionSlot/SelectionHighlight")?.gameObject;
            goldText = shopPanel.transform.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
            potionPriceText = shopPanel.transform.Find("PotionSlot/PotionPriceText")?.GetComponent<TextMeshProUGUI>();
        }

        SetupButtonsPublic();
    }

    // =========================
    // BUTTONS VERBINDEN
    // =========================
    public void SetupButtonsPublic()
    {
        // BUY
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyPotion);
        }

        // SELL
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellPotion);
        }

        // LEAVE
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(CloseShop);
        }

        // POTION SLOT
        if (potionSlotButton != null)
        {
            potionSlotButton.onClick.RemoveAllListeners();
            potionSlotButton.onClick.AddListener(SelectPotion);
        }
    }

    // =========================
    // EXTERNE ÖFFNUNG (Vom Händler)
    // =========================
    public void OpenShopFromMerchant()
    {
        if (isShopOpen) return;

        // Sicherstellen, dass Referenzen da sind (nach Szenenwechsel)
        ReconnectShop();

        // Auswahl reset
        potionSelected = false;
        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);

        // UI direkt öffnen ohne erneuten Dialog (der kam schon vom Händler)
        if (shopPanel != null)
            shopPanel.SetActive(true);

        isShopOpen = true;
        isOpeningShop = false;

        // Rucksack miteröffnen und Layout anpassen
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.SetShopLayout(true);
        }

        // Bewegung sperren
        LockPlayerMovement(true);

        // Maus zeigen
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UpdateGoldUI();
    }

    // =========================
    // SHOP STARTEN (Eigene Interaktion)
    // =========================
    private void OpenShop()
    {
        isOpeningShop = true;
        potionSelected = false;
        if (selectionHighlight != null) selectionHighlight.SetActive(false);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Händler", "Schau dir meine Waren an!", dialogueDuration);
        }

        Invoke(nameof(OpenShopAfterDialogue), dialogueDuration);
    }

    private void OpenShopAfterDialogue()
    {
        OpenShopFromMerchant();
    }

    // =========================
    // SHOP SCHLIESSEN
    // =========================
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Rucksack schließen und Layout reset
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.SetShopLayout(false);
        }

        // Auswahl reset
        potionSelected = false;

        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);

        isShopOpen = false;
        isOpeningShop = false;

        // Bewegung zurück
        LockPlayerMovement(false);

        // Cursor wieder verstecken
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // =========================
    // PLAYER MOVEMENT LOCK
    // =========================
    private void LockPlayerMovement(bool locked)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            // canMove = false -> Stop
            movement.canMove = !locked;
        }
    }

    // =========================
    // POTION AUSWÄHLEN
    // =========================
    public void SelectPotion()
    {
        potionSelected = true;

        if (selectionHighlight != null)
            selectionHighlight.SetActive(true);

        // Inventar-Auswahl aufheben
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.DeselectSlot();
        }
    }

    public void DeselectShopItem()
    {
        potionSelected = false;
        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);
    }

    // =========================
    // KAUFEN
    // =========================
    public void BuyPotion()
    {
        // Erst auswählen
        if (!potionSelected)
        {
            Debug.Log("Kein Item im Shop ausgewählt!");
            return;
        }

        // PlayerGold prüfen
        if (PlayerGold.Instance == null)
        {
            Debug.LogError("PlayerGold Instance fehlt!");
            return;
        }

        // Inventory prüfen
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("InventoryManager fehlt!");
            return;
        }

        // Gold ausgeben
        if (!PlayerGold.Instance.SpendGold(potionPrice))
        {
            Debug.Log("Nicht genug Gold!");
            return;
        }

        // Potion hinzufügen
        inventory.AddPotion();

        UpdateGoldUI();

        Debug.Log("Potion gekauft für: " + potionPrice + " Gold.");
    }

    // =========================
    // VERKAUFEN
    // =========================
    public void SellPotion()
    {
        // 1. Prüfen ob überhaupt etwas selektiert ist (im Shop oder im Backpack)
        if (!potionSelected)
        {
            Debug.Log("Kein Item ausgewählt!");
            return;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory != null)
        {
            // Versuche den selektierten Trank zu entfernen
            if (inventory.RemoveSelectedPotion())
            {
                if (PlayerGold.Instance != null)
                {
                    PlayerGold.Instance.AddGold(potionPrice / 2);
                }
                UpdateGoldUI();
                Debug.Log("Selektierte Potion verkauft!");
            }
            else if (inventory.GetPotionCount() > 0)
            {
                // Fallback: Wenn nichts selektiert war aber Tränke da sind, nimm einen beliebigen
                if (inventory.RemoveOnePotion())
                {
                    if (PlayerGold.Instance != null)
                    {
                        PlayerGold.Instance.AddGold(potionPrice / 2);
                    }
                    UpdateGoldUI();
                    Debug.Log("Potion verkauft (Fallback)!");
                }
            }
            else
            {
                Debug.Log("Keine Tränke zum Verkaufen vorhanden!");
            }
        }
    }

    // =========================
    // UI UPDATE
    // =========================
    private void UpdateGoldUI()
    {
        if (goldText != null && PlayerGold.Instance != null)
            goldText.text = PlayerGold.Instance.currentGold.ToString();

        if (potionPriceText != null)
            potionPriceText.text = potionPrice.ToString();
    }
}