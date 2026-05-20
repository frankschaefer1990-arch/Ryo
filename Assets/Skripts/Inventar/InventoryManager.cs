using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Backpack Panel")]
    public Transform backpackPanel;

    [Header("Item Sprites")]
    public Sprite potionSprite;
    public Sprite manaPotionSprite;

    private Image[] inventorySlots;
    private Image[] slotBackgrounds;
    private int[] slotItemType; // 0 = empty, 1 = health, 2 = mana
    private int selectedSlotIndex = -1;

    // Legacy Support
    public bool[] GetSlotData() 
    {
        if (slotItemType == null) return new bool[0];
        bool[] data = new bool[slotItemType.Length];
        for (int i = 0; i < data.Length; i++) data[i] = slotItemType[i] > 0;
        return data;
    }

    public int[] GetSlotItemTypes() => slotItemType;
    
    public void SetSlotData(int[] data) 
    { 
        slotItemType = data; 
        RefreshInventory(); 
    }

    public int GetSelectedSlotIndex() => selectedSlotIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            if (Instance.potionSprite == null && this.potionSprite != null) Instance.potionSprite = this.potionSprite;
            if (Instance.manaPotionSprite == null && this.manaPotionSprite != null) Instance.manaPotionSprite = this.manaPotionSprite;
            Destroy(this);
            return; 
        }
        
        Instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
        
        if (slotItemType == null || slotItemType.Length == 0)
        {
            slotItemType = new int[10]; // Default
        }
    }

    private void OnEnable() { GameManager.OnSystemsReady += RefreshInventory; }
    private void OnDisable() { GameManager.OnSystemsReady -= RefreshInventory; }
    private void Start() { RefreshInventory(); }

    public void RefreshInventory()
    {
        ReconnectBackpackPanel();
        InitializeInventorySlots();
        RestoreInventoryVisuals();
        UpdateSlotHighlights();
    }

    private void ReconnectBackpackPanel()
    {
        if (backpackPanel != null && backpackPanel.gameObject.scene.name == "DontDestroyOnLoad") return;

        GameObject target = null;
        if (GameManager.Instance != null && GameManager.Instance.canvas != null) target = GameManager.Instance.canvas;
        
        if (target == null) {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in canvases) {
                if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") {
                    target = c.gameObject;
                    break;
                }
            }
        }
        
        if (target == null) return;

        backpackPanel = FindChildRecursive(target.transform, "BackpackPanel");
        if (backpackPanel == null) backpackPanel = FindChildRecursive(target.transform, "Backpack");
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    private void InitializeInventorySlots()
    {
        if (potionSprite == null) {
            var allPotions = FindObjectsByType<PotionItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach(var p in allPotions) {
                var img = p.GetComponent<UnityEngine.UI.Image>();
                if (img != null && img.sprite != null && !img.sprite.name.Contains("Mana")) { potionSprite = img.sprite; break; }
            }
        }
        
        if (manaPotionSprite == null) {
#if UNITY_EDITOR
            manaPotionSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Händler/Mana Trank.png");
#endif
        }

        if (backpackPanel == null) return;
        
        int count = backpackPanel.childCount;
        inventorySlots = new Image[count];
        slotBackgrounds = new Image[count];
        
        if (slotItemType == null || slotItemType.Length == 0) 
            slotItemType = new int[count];
        else if (slotItemType.Length != count) {
            int[] newType = new int[count];
            System.Array.Copy(slotItemType, newType, Mathf.Min(slotItemType.Length, count));
            slotItemType = newType;
        }

        for (int i = 0; i < count; i++) {
            Transform slot = backpackPanel.GetChild(i);
            slotBackgrounds[i] = slot.GetComponent<Image>();
            if (slotBackgrounds[i] != null) { slotBackgrounds[i].enabled = true; slotBackgrounds[i].color = new Color(1,1,1,0); }
            
            InventoryClickHandler handler = slot.GetComponent<InventoryClickHandler>();
            if (handler == null) handler = slot.gameObject.AddComponent<InventoryClickHandler>();
            handler.slotIndex = i;

            Transform item = slot.Find("Item");
            if (item == null && slot.childCount > 0) item = slot.GetChild(0);
            if (item != null) {
                inventorySlots[i] = item.GetComponent<Image>();
                if (inventorySlots[i] == null) inventorySlots[i] = item.gameObject.AddComponent<Image>();
                inventorySlots[i].preserveAspect = true;
                inventorySlots[i].raycastTarget = true;
                InventoryClickHandler itemH = item.GetComponent<InventoryClickHandler>();
                if (itemH == null) itemH = item.gameObject.AddComponent<InventoryClickHandler>();
                itemH.slotIndex = i;
            }
        }
    }

    public void SelectSlot(int idx)
    {
        if (idx < 0 || idx >= slotItemType.Length || slotItemType[idx] == 0) selectedSlotIndex = -1;
        else {
            selectedSlotIndex = idx;
            ShopManager shop = FindFirstObjectByType<ShopManager>();
            if (shop != null) shop.DeselectShopItem();
        }
        UpdateSlotHighlights();
    }

    public void DeselectSlot() { selectedSlotIndex = -1; UpdateSlotHighlights(); }

    private void UpdateSlotHighlights()
    {
        if (slotBackgrounds == null) return;
        for (int i = 0; i < slotBackgrounds.Length; i++) {
            if (slotBackgrounds[i] != null) slotBackgrounds[i].color = (i == selectedSlotIndex) ? new Color(0, 0.5f, 1, 0.8f) : new Color(1,1,1,0);
        }
    }

    public bool AddItem(int type)
    {
        if (slotItemType == null || slotItemType.Length == 0) {
            int count = (backpackPanel != null) ? backpackPanel.childCount : 10;
            slotItemType = new int[count];
        }

        for (int i = 0; i < slotItemType.Length; i++) {
            if (slotItemType[i] == 0) { 
                slotItemType[i] = type; 
                RestoreInventoryVisuals(); 
                return true; 
            }
        }
        return false;
    }

    public bool AddPotion() => AddItem(1);
    public bool AddManaPotion() => AddItem(2);

    public bool RemoveSelected()
    {
        if (selectedSlotIndex == -1 || slotItemType[selectedSlotIndex] == 0) return false;
        slotItemType[selectedSlotIndex] = 0;
        selectedSlotIndex = -1;
        RestoreInventoryVisuals();
        UpdateSlotHighlights();
        return true;
    }

    public bool RemoveSelectedPotion() => RemoveSelected();

    public bool RemoveOnePotion() {
        for (int i = slotItemType.Length - 1; i >= 0; i--) if (slotItemType[i] == 1) { slotItemType[i] = 0; RestoreInventoryVisuals(); return true; }
        return false;
    }

    public void RemovePotion(Image usedSlot) {
        if (usedSlot == null || inventorySlots == null) return;
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i] == usedSlot) { slotItemType[i] = 0; if (selectedSlotIndex == i) selectedSlotIndex = -1; RestoreInventoryVisuals(); UpdateSlotHighlights(); return; }
        }
    }

    public void UseSelectedItem()
    {
        if (selectedSlotIndex == -1) return;
        int type = slotItemType[selectedSlotIndex];
        if (type == 1) {
            if (PlayerStats.Instance != null) { PlayerStats.Instance.Heal(50); RemoveSelected(); }
        } else if (type == 2) {
            if (PlayerStats.Instance != null) { PlayerStats.Instance.RestoreMana(30); RemoveSelected(); }
        }
    }

    private void RestoreInventoryVisuals()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            ReconnectBackpackPanel();
            InitializeInventorySlots();
        }
        RestoreVisualsInternal();
    }

    private void RestoreVisualsInternal()
    {
        if (inventorySlots == null || slotItemType == null) return;
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i] == null) continue;
            int type = (i < slotItemType.Length) ? slotItemType[i] : 0;
            if (type == 1) { 
                inventorySlots[i].sprite = potionSprite; 
                inventorySlots[i].color = Color.white; 
                inventorySlots[i].gameObject.SetActive(true);
            } else if (type == 2) {
                inventorySlots[i].sprite = manaPotionSprite; 
                inventorySlots[i].color = Color.white; 
                inventorySlots[i].gameObject.SetActive(true);
            }
            else { 
                inventorySlots[i].sprite = null; 
                inventorySlots[i].color = new Color(1,1,1,0); 
            }
        }
    }

    public int GetPotionCount() => GetItemCount(1);
    public int GetItemCount(int type)
    {
        if (slotItemType == null) return 0;
        int count = 0;
        foreach (int t in slotItemType) if (t == type) count++;
        return count;
    }
}


