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
    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectShop;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectShop;
    }

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
        SetupButtonsPublic();
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
        Canvas targetCanvas = null;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var c in canvases)
        {
            if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor")
            {
                targetCanvas = c;
                break;
            }
        }

        if (targetCanvas == null)
        {
            Debug.LogError("ShopManager: Kein gültiges Canvas gefunden!");
            return;
        }

        // Suche ShopPanel rekursiv im Canvas
        if (shopPanel == null)
        {
            shopPanel = FindChildRecursive(targetCanvas.transform, "ShopPanel")?.gameObject;
        }

        if (shopPanel != null)
        {
            buyButton = shopPanel.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton == null) buyButton = shopPanel.transform.Find("KaufenButton")?.GetComponent<Button>();
            if (buyButton == null) buyButton = shopPanel.transform.Find("Buy")?.GetComponent<Button>();

            sellButton = shopPanel.transform.Find("SellButton")?.GetComponent<Button>();
            if (sellButton == null) sellButton = shopPanel.transform.Find("VerkaufenButton")?.GetComponent<Button>();

            leaveButton = shopPanel.transform.Find("LeaveButton")?.GetComponent<Button>();
            if (leaveButton == null) leaveButton = shopPanel.transform.Find("BeendenButton")?.GetComponent<Button>();

            potionSlotButton = shopPanel.transform.Find("PotionSlot")?.GetComponent<Button>();
            selectionHighlight = shopPanel.transform.Find("PotionSlot/SelectionHighlight")?.gameObject;
            goldText = shopPanel.transform.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
            potionPriceText = shopPanel.transform.Find("PotionSlot/PotionPriceText")?.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("ShopManager: ShopPanel konnte nicht gefunden werden!");
        }

        SetupButtonsPublic();
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t;
        }
        return null;
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
        UpdateGoldUI();

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
        Debug.Log("ShopManager: Cursor aktiviert.");

        UpdateGoldUI();
        SetupButtonsPublic();
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
        if (player == null) FindPlayer();
        if (player == null) return;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = !locked;
        }
    }

    public void SelectPotion()
    {
        Debug.Log("ShopManager: Potion ausgewählt.");
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

    public void BuyPotion()
    {
        Debug.Log("ShopManager: Kaufversuch gestartet...");

        if (!potionSelected)
        {
            Debug.LogWarning("ShopManager: Kein Item im Shop ausgewählt!");
            return;
        }

        PlayerGold gold = PlayerGold.GetInstance();

        if (gold == null)
        {
            Debug.LogError("ShopManager: PlayerGold Instance fehlt!");
            return;
        }

        Debug.Log($"ShopManager: Gold vor Kauf: {gold.currentGold}, Preis: {potionPrice}");

        if (!gold.SpendGold(potionPrice))
        {
            Debug.LogWarning("ShopManager: Nicht genug Gold!");
            return;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("ShopManager: InventoryManager fehlt!");
            gold.AddGold(potionPrice);
            return;
        }

        bool success = inventory.AddPotion();
        if (success)
        {
            Debug.Log("ShopManager: Potion erfolgreich gekauft.");
        }
        else
        {
            Debug.LogError("ShopManager: Potion konnte nicht zum Inventar hinzugefügt werden!");
            gold.AddGold(potionPrice);
        }

        UpdateGoldUI();
        SetupButtonsPublic();
    }

    // =========================
    // VERKAUFEN
    // =========================
    public void SellPotion()
    {
        PlayerGold gold = PlayerGold.GetInstance();
        
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("InventoryManager nicht gefunden!");
            return;
        }

        // 1. Prüfen ob im INVENTAR etwas selektiert ist
        int selectedIdx = inventory.GetSelectedSlotIndex();

        if (selectedIdx == -1)
        {
            Debug.Log("Bitte wähle zuerst einen Trank in deinem Rucksack aus, um ihn zu verkaufen!");
            return;
        }

        // 2. Den selektierten Trank aus dem Inventar entfernen
        if (inventory.RemoveSelectedPotion())
        {
            if (gold != null)
            {
                gold.AddGold(potionPrice / 2);
            }
            UpdateGoldUI();
            Debug.Log("Trank aus Inventar verkauft für " + (potionPrice / 2) + " Gold.");
        }
        else
        {
            Debug.Log("Fehler beim Verkauf oder Slot war leer.");
        }
    }

    // =========================
    // UI UPDATE
    // =========================
    private void UpdateGoldUI()
    {
        PlayerGold gold = PlayerGold.GetInstance();
        
        if (goldText != null && gold != null)
            goldText.text = gold.currentGold.ToString();

        if (potionPriceText != null)
            potionPriceText.text = potionPrice.ToString();
    }
}