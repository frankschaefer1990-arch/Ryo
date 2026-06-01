using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlacksmithManager : MonoBehaviour
{
    public static BlacksmithManager Instance;

    [Header("UI Panels")]
    public GameObject smithPanel;
    
    [Header("Slots")]
    public Image inputSlotIcon;
    public Image materialSlotIcon;
    public Image outputSlotIcon;

    [Header("Texts")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI forgeCostText;
    public TextMeshProUGUI bonusChanceText;

    [Header("Buttons")]
    public Button forgeButton;
    public Button cancelButton;
    public Button addBonusButton;

    [Header("Sprites")]
    public Sprite skeletonBoneSprite; // material ID 100

    private int inputItemId = 0;
    private int inputSourceIdx = -1;
    private EqType? inputSourceEq = null;

    private int materialItemId = 0;
    private int materialSourceIdx = -1;
    private EqType? materialSourceEq = null;

    private int outputItemId = 0;

    private float baseSuccessRate = 70f;
    private float bonusSuccessRate = 0f;
    private int baseForgeCost = 200;
    private int bonusForgeCost = 0;

    private void Awake()
    {
        Instance = this;
        if (smithPanel != null) smithPanel.SetActive(false);
    }

    private void Start()
    {
        if (forgeButton != null) forgeButton.onClick.AddListener(ForgeItem);
        if (cancelButton != null) cancelButton.onClick.AddListener(ClosePanel);
        if (addBonusButton != null) addBonusButton.onClick.AddListener(AddBonusChance);
    }

    public void OpenPanel()
    {
        smithPanel.SetActive(true);
        if (MyUIManager.Instance != null) {
            MyUIManager.Instance.inventoryPanel.SetActive(true);
            MyUIManager.Instance.backpackPanel.SetActive(true);
            MyUIManager.Instance.isLocked = true;
        }
        PlayerMovement pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ClearSlots();
        UpdateUI();
    }

    public int GetSlotItemId(SmithSlotType type)
    {
        return type == SmithSlotType.Input ? inputItemId : materialItemId;
    }

    public void RemoveItemFromSlot(SmithSlotType type)
    {
        if (type == SmithSlotType.Input) inputItemId = 0;
        else materialItemId = 0;
        UpdateOutputPreview();
        UpdateUI();
    }

    public void SetInputItem(int id, int backpackIdx = -1, EqType? eqType = null)
{
        ReturnItemToSource(inputItemId, inputSourceIdx, inputSourceEq);
        inputItemId = id; inputSourceIdx = backpackIdx; inputSourceEq = eqType;
        RemoveFromInventory(id, backpackIdx, eqType);
        UpdateOutputPreview(); UpdateUI();
    }

    public void SetMaterialItem(int id, int backpackIdx = -1, EqType? eqType = null)
    {
        ReturnItemToSource(materialItemId, materialSourceIdx, materialSourceEq);
        materialItemId = id; materialSourceIdx = backpackIdx; materialSourceEq = eqType;
        RemoveFromInventory(id, backpackIdx, eqType);
        UpdateOutputPreview(); UpdateUI();
    }

    private void RemoveFromInventory(int id, int backpackIdx, EqType? eqType)
    {
        if (eqType != null) SetEquippedId(eqType.Value, 0);
        else if (backpackIdx != -1) InventoryManager.Instance.GetSlotItemTypes()[backpackIdx] = 0;
        InventoryManager.Instance.RefreshInventory();
    }

    private void ReturnItemToSource(int id, int idx, EqType? eq)
    {
        if (id == 0) return;
        if (eq != null) SetEquippedId(eq.Value, id);
        else if (idx != -1) InventoryManager.Instance.GetSlotItemTypes()[idx] = id;
        else InventoryManager.Instance.AddItem(id);
        InventoryManager.Instance.RefreshInventory();
    }

    private void SetEquippedId(EqType type, int id)
    {
        if (PlayerStats.Instance == null) return;
        switch (type) {
            case EqType.Weapon: PlayerStats.Instance.equippedWeapon = id; break;
            case EqType.Helm: PlayerStats.Instance.equippedHelm = id; break;
            case EqType.Armor: PlayerStats.Instance.equippedArmor = id; break;
            case EqType.Ring1: PlayerStats.Instance.equippedRing1 = id; break;
            case EqType.Ring2: PlayerStats.Instance.equippedRing2 = id; break;
            case EqType.Boots: PlayerStats.Instance.equippedBoots = id; break;
        }
    }

    public void ClosePanel()
    {
        ReturnItemToSource(inputItemId, inputSourceIdx, inputSourceEq);
        ReturnItemToSource(materialItemId, materialSourceIdx, materialSourceEq);
        inputItemId = materialItemId = 0;
        smithPanel.SetActive(false);
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        PlayerMovement pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateOutputPreview()
    {
        outputItemId = 0; baseForgeCost = 0;
        if (inputItemId == 0) return;
        if (inputItemId == 3 && materialItemId == 100) { outputItemId = 15; baseForgeCost = 500; }
        else {
            if (inputItemId >= 3 && inputItemId <= 8) { outputItemId = inputItemId + 6; baseForgeCost = 200; }
            else if (inputItemId >= 9 && inputItemId <= 14) { outputItemId = GetEpicId(inputItemId); baseForgeCost = 500; }
            else if (IsEpic(inputItemId)) { outputItemId = GetLegendaryId(inputItemId); baseForgeCost = 1000; }
        }
    }

    private int GetEpicId(int rare) {
        if (rare == 9) return 21; if (rare == 10) return 16; if (rare == 11) return 17;
        if (rare == 12) return 18; if (rare == 13) return 19; if (rare == 14) return 26;
        return 0;
    }
    private int GetLegendaryId(int epic) {
        if (epic == 21) return 27; if (epic == 16) return 22; if (epic == 17) return 23;
        if (epic == 18) return 24; if (epic == 19) return 25; if (epic == 26) return 28;
        return 0;
    }
    private bool IsEpic(int id) => id == 21 || (id >= 16 && id <= 19) || id == 26;

    private void AddBonusChance() {
        if (inputItemId != 0 && bonusSuccessRate + baseSuccessRate < 100f) {
            bonusSuccessRate += 10f; bonusForgeCost += 100; UpdateUI();
        }
    }

    private void ForgeItem()
    {
        if (inputItemId == 0) return;
        int totalCost = baseForgeCost + bonusForgeCost;
        if (PlayerGold.GetInstance() == null || !PlayerGold.GetInstance().SpendGold(totalCost)) return;
        
        float chance = baseSuccessRate + bonusSuccessRate;
        if (Random.Range(0f, 100f) <= chance) {
            InventoryManager.Instance.AddItem(outputItemId);
            InventoryManager.Instance.RefreshInventory();
            Debug.Log("Forge Success!");
            inputItemId = materialItemId = 0; // Consumed only on success
            ClearSlots();
        } else {
            Debug.Log("Forge Failed! Only gold was lost.");
            // Do NOT clear items on failure, so they stay in slots
        }
        UpdateUI();
    }

    private void ClearSlots() { inputItemId = materialItemId = outputItemId = 0; bonusSuccessRate = 0f; bonusForgeCost = 0; }

    private void Update() {
        if (smithPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape)) ClosePanel();
        if (Input.GetKeyDown(KeyCode.Alpha6)) { PlayerGold.GetInstance()?.AddGold(10000); UpdateUI(); }
    }

    private void UpdateUI() {
        if (PlayerGold.GetInstance() != null) goldText.text = PlayerGold.GetInstance().currentGold.ToString();
        if (inputItemId == 0) { successRateText.text = "0%"; bonusChanceText.text = "+0%"; forgeCostText.text = "0"; }
        else { successRateText.text = $"{baseSuccessRate + bonusSuccessRate}%"; bonusChanceText.text = $"+{bonusSuccessRate}%"; forgeCostText.text = $"{baseForgeCost + bonusForgeCost}"; }
        
        SetIcon(inputSlotIcon, inputItemId); 
        SetIcon(materialSlotIcon, materialItemId); 
        SetIcon(outputSlotIcon, outputItemId);
        
        if (outputSlotIcon != null) {
            var parent = outputSlotIcon.transform.parent;
            GeneralTooltipTrigger tt = parent.GetComponent<GeneralTooltipTrigger>() ?? parent.gameObject.AddComponent<GeneralTooltipTrigger>();
            tt.content = (outputItemId == 0) ? "Verbesserungsvorschau" : InventoryManager.Instance.GetTooltipForId(outputItemId);
            tt.RefreshTooltipIfHovered();
        }
        if (inputSlotIcon != null) {
            var parent = inputSlotIcon.transform.parent;
            GeneralTooltipTrigger tt = parent.GetComponent<GeneralTooltipTrigger>() ?? parent.gameObject.AddComponent<GeneralTooltipTrigger>();
            tt.content = (inputItemId == 0) ? "Item einlegen" : InventoryManager.Instance.GetTooltipForId(inputItemId);
            tt.pivotOverride = new Vector2(0f, 0f); // Expand to the Right
            tt.RefreshTooltipIfHovered();
        }
    }

    private void SetIcon(Image img, int id) {
        if (img == null) return;
        if (id == 0) { img.sprite = null; img.color = new Color(0,0,0,0); img.enabled = false; return; }
        
        InventoryManager inv = InventoryManager.Instance;
        if (inv == null) {
            GameObject ps = GameObject.Find("[PERSISTENT_SYSTEMS]");
            if (ps != null) inv = ps.GetComponentInChildren<InventoryManager>();
        }

        Sprite s = (id == 100) ? skeletonBoneSprite : (inv != null ? inv.GetSpriteForId(id) : null);
        
        if (s != null) {
            img.sprite = s;
            img.color = Color.white;
            img.enabled = true;
            img.preserveAspect = true;
        } else {
            img.sprite = null;
            img.color = new Color(0,0,0,0);
            img.enabled = false;
        }
    }
}