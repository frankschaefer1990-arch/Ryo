using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI goldText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    [Header("Buttons")]
    public Button buyButton;
    public Button sellButton;
    public Button leaveButton;
    public Button healthSlotButton;
    public Button manaSlotButton;
    public Button swordSlotButton;
    public Button helmSlotButton;
    public Button armorSlotButton;
    public Button ringSlotButton;
    public Button bootsSlotButton;
    public Button wandSlotButton;

    [Header("Highlights")]
    public GameObject healthHighlight;
    public GameObject manaHighlight;
    public GameObject swordHighlight;
    public GameObject helmHighlight;
    public GameObject armorHighlight;
    public GameObject ringHighlight;
    public GameObject bootsHighlight;
    public GameObject wandHighlight;

    [Header("Prices")]
    public int healthPrice = 10;
    public int manaPrice = 10;
    public int swordPrice = 100;
    public int helmPrice = 70;
    public int armorPrice = 150;
    public int ringPrice = 80;
    public int bootsPrice = 60;
    public int wandPrice = 120;
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI manaPriceText;
    public TextMeshProUGUI swordPriceText;
    public TextMeshProUGUI helmPriceText;
    public TextMeshProUGUI armorPriceText;
    public TextMeshProUGUI ringPriceText;
    public TextMeshProUGUI bootsPriceText;
    public TextMeshProUGUI wandPriceText;

    [Header("Item Visibility")]
    public GameObject swordSlotUI;
    public GameObject helmSlotUI;
    public GameObject armorSlotUI;
    public GameObject ringSlotUI;
    public GameObject bootsSlotUI;
    public GameObject wandSlotUI;

    [Header("Settings")]
    public float interactionRange = 2f;
    public KeyCode interactKey = KeyCode.R;
    public float dialogueDuration = 1.2f;

    private Transform player;
    private bool playerInRange = false;
    private int selectedShopItem = 0; // 0=none, 1=HP, 2=MP, 3=Sword, 4=Helm, 5=Armor, 6=Ring, 7=Boots, 8=Wand
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
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (isShopOpen) return;
        StartCoroutine(OpenShopSequence());
    }

    private System.Collections.IEnumerator OpenShopSequence()
    {
        isOpeningShop = true;
        
        MerchantInteraction merchant = GetComponent<MerchantInteraction>();
        if (merchant != null)
        {
            merchant.StartTalking();
            yield return new WaitForSeconds(dialogueDuration);
        }

        OpenShopFromMerchant();
    }

    public void ReconnectShop()
    {
        if (shopPanel != null && shopPanel.activeInHierarchy) return;

        Canvas targetCanvas = null;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.name.Contains("Master") || c.name.Contains("Canvas"))
            {
                if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor")
                {
                    targetCanvas = c;
                    break;
                }
            }
        }

        if (targetCanvas == null && canvases.Length > 0) targetCanvas = canvases[0];
        if (targetCanvas == null) return;

        if (shopPanel == null)
        {
            Transform found = FindChildRecursive(targetCanvas.transform, "ShopPanel");
            if (found != null) shopPanel = found.gameObject;
        }

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

            swordSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_2")?.gameObject;
            if (swordSlotUI != null) {
                swordSlotButton = swordSlotUI.GetComponent<Button>();
                swordHighlight = FindChildRecursive(swordSlotUI.transform, "SelectionHighlight")?.gameObject;
                swordPriceText = FindChildRecursive(swordSlotUI.transform, "ItemPriceText")?.GetComponent<TextMeshProUGUI>();
            }

            helmSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_3")?.gameObject;
            if (helmSlotUI != null) {
                helmSlotButton = helmSlotUI.GetComponent<Button>();
                helmHighlight = FindChildRecursive(helmSlotUI.transform, "SelectionHighlight")?.gameObject;
                helmPriceText = FindChildRecursive(helmSlotUI.transform, "ItemPriceText")?.GetComponent<TextMeshProUGUI>();
            }

            armorSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_4")?.gameObject;
            if (armorSlotUI != null) {
                armorSlotButton = armorSlotUI.GetComponent<Button>();
                armorHighlight = FindChildRecursive(armorSlotUI.transform, "SelectionHighlight")?.gameObject;
            }

            ringSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_5")?.gameObject;
            if (ringSlotUI != null) {
                ringSlotButton = ringSlotUI.GetComponent<Button>();
                ringHighlight = FindChildRecursive(ringSlotUI.transform, "SelectionHighlight")?.gameObject;
            }

            bootsSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_6")?.gameObject;
            if (bootsSlotUI != null) {
                bootsSlotButton = bootsSlotUI.GetComponent<Button>();
                bootsHighlight = FindChildRecursive(bootsSlotUI.transform, "SelectionHighlight")?.gameObject;
            }

            wandSlotUI = FindChildRecursive(shopPanel.transform, "ShopSlot_7")?.gameObject;
            if (wandSlotUI != null) {
                wandSlotButton = wandSlotUI.GetComponent<Button>();
                wandHighlight = FindChildRecursive(wandSlotUI.transform, "SelectionHighlight")?.gameObject;
            }

            goldText = FindChildRecursive(shopPanel.transform, "GoldText")?.GetComponent<TextMeshProUGUI>();

            // Ensure shop slots show the correct sprites from InventoryManager
            UpdateShopSprites();
            }
            SetupButtonsPublic();
            }

            private void UpdateShopSprites()
            {
            if (InventoryManager.Instance == null) return;
            SetShopSlotIcon(swordSlotUI, 3);
            SetShopSlotIcon(helmSlotUI, 4);
            SetShopSlotIcon(armorSlotUI, 5);
            SetShopSlotIcon(ringSlotUI, 6);
            SetShopSlotIcon(bootsSlotUI, 7); // Basic Boots
            SetShopSlotIcon(wandSlotUI, 8);
            }

            private void SetShopSlotIcon(GameObject slotUI, int itemId)
            {
                if (slotUI == null) return;
                
                // Find or create the icon image (child named Icon or Item)
                Transform icnT = slotUI.transform.Find("Icon") ?? slotUI.transform.Find("Item");
                UnityEngine.UI.Image iconImg = icnT != null ? icnT.GetComponent<UnityEngine.UI.Image>() : null;
                
                Sprite s = (InventoryManager.Instance != null) ? InventoryManager.Instance.GetSpriteForId(itemId) : null;
                
                // AGGRESSIVELY DISABLE/CLEAR ALL GRAPHICS IN THE SLOT hierarchy
                UnityEngine.UI.Graphic[] allGraphics = slotUI.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
                foreach (var g in allGraphics)
                {
                    // Skip the icon image and any highlight objects
                    if (g != iconImg && !g.name.Contains("Highlight"))
                    {
                        if (g is UnityEngine.UI.Image img) {
                            img.sprite = null;
                            if (g.gameObject == slotUI) { g.color = Color.black; g.enabled = true; }
                            else { g.color = new Color(0,0,0,0); g.enabled = false; }
                        } else if (g is UnityEngine.UI.RawImage ri) {
                            ri.texture = null;
                            ri.color = new Color(0,0,0,0);
                            ri.enabled = false;
                        }
                    }
                }
                
                // Also check for SpriteRenderers just in case
                SpriteRenderer[] srs = slotUI.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in srs) { sr.sprite = null; sr.enabled = false; }

                if (iconImg != null)
                {
                    iconImg.sprite = s;
                    iconImg.color = s != null ? Color.white : new Color(0,0,0,0);
                    iconImg.enabled = s != null;
                    iconImg.gameObject.SetActive(s != null);
                    iconImg.preserveAspect = true;
                }
                else if (s != null)
                {
                    // Fallback: if no icon child, use base image
                    UnityEngine.UI.Image baseImg = slotUI.GetComponent<UnityEngine.UI.Image>();
                    if (baseImg != null) {
                        baseImg.sprite = s;
                        baseImg.color = Color.white;
                        baseImg.enabled = true;
                        baseImg.preserveAspect = true;
                    }
                }
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
        if (swordSlotButton != null) { swordSlotButton.onClick.RemoveAllListeners(); swordSlotButton.onClick.AddListener(() => SelectShopItem(3)); }
        if (helmSlotButton != null) { helmSlotButton.onClick.RemoveAllListeners(); helmSlotButton.onClick.AddListener(() => SelectShopItem(4)); }
        if (armorSlotButton != null) { armorSlotButton.onClick.RemoveAllListeners(); armorSlotButton.onClick.AddListener(() => SelectShopItem(5)); }
        if (ringSlotButton != null) { ringSlotButton.onClick.RemoveAllListeners(); ringSlotButton.onClick.AddListener(() => SelectShopItem(6)); }
        if (bootsSlotButton != null) { bootsSlotButton.onClick.RemoveAllListeners(); bootsSlotButton.onClick.AddListener(() => SelectShopItem(7)); }
        if (wandSlotButton != null) { wandSlotButton.onClick.RemoveAllListeners(); wandSlotButton.onClick.AddListener(() => SelectShopItem(8)); }
    }

    public void OpenShopFromMerchant()
    {
        if (isShopOpen) return;

        // Reset to original prices
        swordPrice = 100;
        wandPrice = 120;

        // Update prices if boss is defeated AND we are in the starting area
        if (QuestManager.Instance != null && QuestManager.Instance.defeatedTempleBoss && 
            SceneManager.GetActiveScene().name == "Legend of Ryo")
        {
            swordPrice = 60;
            wandPrice = 60;
        }

        ReconnectShop();
        UpdatePriceTexts();
        UpdateGoldUI();
        DeselectShopItem();
        if (shopPanel != null) shopPanel.SetActive(true);
        isShopOpen = true;
        isOpeningShop = false;

        // Item visibility based on scene
        bool isDorf = SceneManager.GetActiveScene().name == "Dorf";
        bool isStartScene = SceneManager.GetActiveScene().name == "Legend of Ryo";
        bool bossDefeated = QuestManager.Instance != null && QuestManager.Instance.defeatedTempleBoss;

        if (swordSlotUI != null) swordSlotUI.SetActive(isDorf || (isStartScene && bossDefeated));
        if (wandSlotUI != null) wandSlotUI.SetActive(isDorf || (isStartScene && bossDefeated));
        
        if (helmSlotUI != null) helmSlotUI.SetActive(isDorf);
        if (armorSlotUI != null) armorSlotUI.SetActive(isDorf);
        if (ringSlotUI != null) ringSlotUI.SetActive(isDorf);
        if (bootsSlotUI != null) bootsSlotUI.SetActive(isDorf);

        if (MyUIManager.Instance != null) MyUIManager.Instance.SetShopLayout(true);
        LockPlayerMovement(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void UpdatePriceTexts()
    {
        if (swordPriceText != null) swordPriceText.text = "Preis: " + swordPrice + " Gold";
        if (wandPriceText != null) wandPriceText.text = "Preis: " + wandPrice + " Gold";
        if (healthPriceText != null) healthPriceText.text = "Preis: " + healthPrice + " Gold";
        if (manaPriceText != null) manaPriceText.text = "Preis: " + manaPrice + " Gold";
        if (helmPriceText != null) helmPriceText.text = "Preis: " + helmPrice + " Gold";
        if (armorPriceText != null) armorPriceText.text = "Preis: " + armorPrice + " Gold";
        if (ringPriceText != null) ringPriceText.text = "Preis: " + ringPrice + " Gold";
        if (bootsPriceText != null) bootsPriceText.text = "Preis: " + bootsPrice + " Gold";

        // Update Tooltips dynamically
        UpdateSlotTooltip(swordSlotUI, "Basic Schwert", swordPrice, "+10 Dmg, +5 Stärke");
        UpdateSlotTooltip(wandSlotUI, "Basic Zauberstab", wandPrice, "+10 Zauberschaden, +10 Int");
        UpdateSlotTooltip(helmSlotUI, "Basic Helm", helmPrice, "+5 Vitalität, +4 Rüstung");
        UpdateSlotTooltip(armorSlotUI, "Basic Rüstung", armorPrice, "+12 Vitalität, +8 Rüstung");
        UpdateSlotTooltip(ringSlotUI, "Basic Ring", ringPrice, "+20 Mana, +5 Int");
        UpdateSlotTooltip(bootsSlotUI, "Basic Stiefel", bootsPrice, "+5 Fluch");

        // Potion tooltips (usually fixed but good for consistency)
        UpdateSlotTooltip(FindChildRecursive(shopPanel.transform, "PotionSlot")?.gameObject, "Heiltrank", healthPrice, "Heilt 60 HP");
        UpdateSlotTooltip(FindChildRecursive(shopPanel.transform, "ManaSlot")?.gameObject, "Manatrank", manaPrice, "Heilt 30 Mana");
    }

    private void UpdateSlotTooltip(GameObject slotObj, string itemName, int price, string stats)
    {
        if (slotObj == null) return;
        var tt = slotObj.GetComponent<GeneralTooltipTrigger>();
        if (tt != null)
        {
            tt.content = "Preis: " + price + " Gold\n" + itemName + "\n" + stats;
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        isShopOpen = false;
        if (MyUIManager.Instance != null) MyUIManager.Instance.SetShopLayout(false);
        LockPlayerMovement(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SelectShopItem(int type)
    {
        selectedShopItem = type;
        if (healthHighlight != null) healthHighlight.SetActive(type == 1);
        if (manaHighlight != null) manaHighlight.SetActive(type == 2);
        if (swordHighlight != null) swordHighlight.SetActive(type == 3);
        if (helmHighlight != null) helmHighlight.SetActive(type == 4);
        if (armorHighlight != null) armorHighlight.SetActive(type == 5);
        if (ringHighlight != null) ringHighlight.SetActive(type == 6);
        if (bootsHighlight != null) bootsHighlight.SetActive(type == 7);
        if (wandHighlight != null) wandHighlight.SetActive(type == 8);
        if (InventoryManager.Instance != null) InventoryManager.Instance.DeselectSlot();
    }

    public void DeselectShopItem()
    {
        selectedShopItem = 0;
        if (healthHighlight != null) healthHighlight.SetActive(false);
        if (manaHighlight != null) manaHighlight.SetActive(false);
        if (swordHighlight != null) swordHighlight.SetActive(false);
        if (helmHighlight != null) helmHighlight.SetActive(false);
        if (armorHighlight != null) armorHighlight.SetActive(false);
        if (ringHighlight != null) ringHighlight.SetActive(false);
        if (bootsHighlight != null) bootsHighlight.SetActive(false);
        if (wandHighlight != null) wandHighlight.SetActive(false);
    }

    public void BuyItem()
    {
        if (selectedShopItem == 0) return;
        int price = GetPrice(selectedShopItem);

        PlayerGold gold = PlayerGold.Instance;
        if (gold == null) return;

        if (gold.HasEnoughGold(price))
        {
            bool success = false;
            if (selectedShopItem == 1) success = InventoryManager.Instance.AddPotion();
            else if (selectedShopItem == 2) success = InventoryManager.Instance.AddManaPotion();
            else success = InventoryManager.Instance.AddItem(selectedShopItem);

            if (success) gold.SpendGold(price);
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

        int price = GetPrice(type);

        if (inv.RemoveSelected())
        {
            PlayerGold gold = PlayerGold.Instance;
            if (gold != null) gold.AddGold(price / 2);
        }
        UpdateGoldUI();
    }

    private int GetPrice(int type)
    {
        switch(type) {
            case 1: return healthPrice;
            case 2: return manaPrice;
            case 3: return swordPrice;
            case 4: return helmPrice;
            case 5: return armorPrice;
            case 6: return ringPrice;
            case 7: return bootsPrice;
            case 8: return wandPrice;
            default: return 0;
        }
    }

    public void UpdateGoldUI()
    {
        PlayerGold gold = PlayerGold.Instance;
        if (goldText != null && gold != null) goldText.text = gold.currentGold.ToString();
    }

    private void LockPlayerMovement(bool locked)
    {
        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = !locked;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = locked;
    }
}