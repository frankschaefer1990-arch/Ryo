using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;

    [Header("Buttons")]
    public Button buyButton;
    public Button sellButton;
    public Button leaveButton;
    public Button healthSlotButton;
    public Button manaSlotButton;

    [Header("Highlights")]
    public GameObject healthHighlight;
    public GameObject manaHighlight;

    [Header("Prices")]
    public int healthPrice = 10;
    public int manaPrice = 10;
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI manaPriceText;

    [Header("Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.R;
    public float dialogueDuration = 1.2f;

    private Transform player;
    private bool playerInRange = false;
    private int selectedShopItem = 0; // 0=none, 1=health, 2=mana
    private bool isShopOpen = false;
    private bool isOpeningShop = false;

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectShop;
        PlayerGold.OnGoldChanged += UpdateGoldUI;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectShop;
        PlayerGold.OnGoldChanged -= UpdateGoldUI;
    }

    private void Start()
    {
        ReconnectShop();
        if (shopPanel != null) shopPanel.SetActive(false);
        DeselectShopItem();
        UpdateGoldUI();
        SetupButtonsPublic();
    }

    private void Update()
    {
        FindPlayer();
        if (player == null) return;
        CheckRange();

        if (playerInRange && Input.GetKeyDown(interactKey) && !isShopOpen && !isOpeningShop)
        {
            OpenShop();
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null) player = GameManager.Instance.player.transform;
            else
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
                if (foundPlayer != null) player = foundPlayer.transform;
            }
        }
    }

    private void CheckRange()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
    }

    public void ReconnectShop()
    {
        Canvas targetCanvas = null;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") { targetCanvas = c; break; }
        }

        if (targetCanvas == null) return;

        if (shopPanel == null) shopPanel = FindChildRecursive(targetCanvas.transform, "ShopPanel")?.gameObject;

        if (shopPanel != null)
        {
            buyButton = FindChildRecursive(shopPanel.transform, "BuyButton")?.GetComponent<Button>();
            sellButton = FindChildRecursive(shopPanel.transform, "SellButton")?.GetComponent<Button>();
            leaveButton = FindChildRecursive(shopPanel.transform, "LeaveButton")?.GetComponent<Button>();
            
            healthSlotButton = FindChildRecursive(shopPanel.transform, "PotionSlot")?.GetComponent<Button>();
            if (healthSlotButton != null) {
                healthHighlight = FindChildRecursive(healthSlotButton.transform, "SelectionHighlight")?.gameObject;
                healthPriceText = FindChildRecursive(healthSlotButton.transform, "PotionPriceText")?.GetComponent<TextMeshProUGUI>();
            }

            manaSlotButton = FindChildRecursive(shopPanel.transform, "ManaSlot")?.GetComponent<Button>();
            if (manaSlotButton != null) {
                manaHighlight = FindChildRecursive(manaSlotButton.transform, "SelectionHighlight")?.gameObject;
                manaPriceText = FindChildRecursive(manaSlotButton.transform, "ManaPriceText")?.GetComponent<TextMeshProUGUI>();
            }

            goldText = FindChildRecursive(shopPanel.transform, "GoldText")?.GetComponent<TextMeshProUGUI>();
        }
        SetupButtonsPublic();
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    public void SetupButtonsPublic()
    {
        if (buyButton != null) { buyButton.onClick.RemoveAllListeners(); buyButton.onClick.AddListener(BuyItem); }
        if (sellButton != null) { sellButton.onClick.RemoveAllListeners(); sellButton.onClick.AddListener(SellItem); }
        if (leaveButton != null) { leaveButton.onClick.RemoveAllListeners(); leaveButton.onClick.AddListener(CloseShop); }
        if (healthSlotButton != null) { healthSlotButton.onClick.RemoveAllListeners(); healthSlotButton.onClick.AddListener(() => SelectShopItem(1)); }
        if (manaSlotButton != null) { manaSlotButton.onClick.RemoveAllListeners(); manaSlotButton.onClick.AddListener(() => SelectShopItem(2)); }
    }

    public void OpenShopFromMerchant()
    {
        if (isShopOpen) return;
        ReconnectShop();
        UpdateGoldUI();
        DeselectShopItem();
        if (shopPanel != null) shopPanel.SetActive(true);
        isShopOpen = true;
        isOpeningShop = false;
        if (MyUIManager.Instance != null) MyUIManager.Instance.SetShopLayout(true);
        LockPlayerMovement(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OpenShop()
    {
        isOpeningShop = true;
        if (DialogueUI.Instance != null) DialogueUI.Instance.ShowMessage("Händler", "Schau dir meine Waren an!", dialogueDuration);
        Invoke(nameof(OpenShopAfterDialogue), dialogueDuration);
    }

    private void OpenShopAfterDialogue() => OpenShopFromMerchant();

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (MyUIManager.Instance != null) MyUIManager.Instance.SetShopLayout(false);
        DeselectShopItem();
        isShopOpen = false;
        isOpeningShop = false;
        LockPlayerMovement(false);
    }

    private void LockPlayerMovement(bool locked)
    {
        if (player == null) FindPlayer();
        if (player != null) {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.canMove = !locked;
        }
    }

    public void SelectShopItem(int type)
    {
        selectedShopItem = type;
        if (healthHighlight != null) healthHighlight.SetActive(type == 1);
        if (manaHighlight != null) manaHighlight.SetActive(type == 2);
        if (InventoryManager.Instance != null) InventoryManager.Instance.DeselectSlot();
    }

    public void DeselectShopItem()
    {
        selectedShopItem = 0;
        if (healthHighlight != null) healthHighlight.SetActive(false);
        if (manaHighlight != null) manaHighlight.SetActive(false);
    }

    public void BuyItem()
    {
        if (selectedShopItem == 0) return;
        int price = (selectedShopItem == 1) ? healthPrice : manaPrice;
        PlayerGold gold = PlayerGold.GetInstance();
        if (gold == null) return;

        if (gold.SpendGold(price))
        {
            bool success = false;
            if (selectedShopItem == 1) success = InventoryManager.Instance.AddPotion();
            else if (selectedShopItem == 2) success = InventoryManager.Instance.AddManaPotion();

            if (!success) gold.AddGold(price);
        }
        UpdateGoldUI();
    }

    public void SellItem()
    {
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) return;
        int idx = inv.GetSelectedSlotIndex();
        if (idx == -1) return;

        int[] types = inv.GetSlotItemTypes();
        int type = (types != null && idx < types.Length) ? types[idx] : 0;
        if (type == 0) return;

        int price = (type == 1) ? healthPrice : manaPrice;

        if (inv.RemoveSelected())
        {
            PlayerGold gold = PlayerGold.GetInstance();
            if (gold != null) gold.AddGold(price / 2);
        }
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        PlayerGold gold = PlayerGold.GetInstance();
        if (goldText != null && gold != null) goldText.text = gold.currentGold.ToString();
        if (healthPriceText != null) healthPriceText.text = healthPrice.ToString();
        if (manaPriceText != null) manaPriceText.text = manaPrice.ToString();
    }
}
