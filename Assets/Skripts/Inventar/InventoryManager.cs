using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Backpack Panel")]
    public Transform backpackPanel;

    [Header("Potion Item")]
    public Sprite potionSprite;

    private Image[] inventorySlots;
    private Image[] slotBackgrounds;
    private bool[] slotOccupied;
    private int selectedSlotIndex = -1;

    public int GetSelectedSlotIndex() => selectedSlotIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            if (Instance.potionSprite == null && this.potionSprite != null)
            {
                Instance.potionSprite = this.potionSprite;
            }
            Debug.Log($"InventoryManager: Duplicate script on {gameObject.name} removed.");
            Destroy(this);
            return; 
        }
        
        Instance = this;
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
        
        if (slotOccupied == null || slotOccupied.Length == 0)
        {
            slotOccupied = new bool[10]; // Default
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
        
        if (backpackPanel != null) {
            Debug.Log($"InventoryManager: BackpackPanel found on {backpackPanel.name}");
        }
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
                if (img != null && img.sprite != null) { potionSprite = img.sprite; break; }
            }
        }

        if (backpackPanel == null) return;
        
        int count = backpackPanel.childCount;
        inventorySlots = new Image[count];
        slotBackgrounds = new Image[count];
        
        if (slotOccupied == null || slotOccupied.Length == 0) 
            slotOccupied = new bool[count];
        else if (slotOccupied.Length != count) {
            bool[] newOcc = new bool[count];
            System.Array.Copy(slotOccupied, newOcc, Mathf.Min(slotOccupied.Length, count));
            slotOccupied = newOcc;
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
        if (idx < 0 || idx >= slotOccupied.Length || !slotOccupied[idx]) selectedSlotIndex = -1;
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

    public bool AddPotion()
    {
        if (slotOccupied == null || slotOccupied.Length == 0) {
            int count = (backpackPanel != null) ? backpackPanel.childCount : 10;
            slotOccupied = new bool[count];
        }

        if (potionSprite == null) {
            var allManagers = FindObjectsByType<InventoryManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach(var mgr in allManagers) if (mgr != this && mgr.potionSprite != null) { potionSprite = mgr.potionSprite; break; }
        }

        if (potionSprite == null) return false;

        if (inventorySlots == null || inventorySlots.Length == 0 || inventorySlots[0] == null) RefreshInventory();

        for (int i = 0; i < slotOccupied.Length; i++) {
            if (!slotOccupied[i]) { 
                slotOccupied[i] = true; 
                RestoreInventoryVisuals(); 
                return true; 
            }
        }
        return false;
    }

    public bool RemoveSelectedPotion()
    {
        if (selectedSlotIndex == -1 || !slotOccupied[selectedSlotIndex]) return false;
        slotOccupied[selectedSlotIndex] = false;
        selectedSlotIndex = -1;
        RestoreInventoryVisuals();
        UpdateSlotHighlights();
        return true;
    }

    public bool RemoveOnePotion()
    {
        if (slotOccupied == null) return false;
        if (selectedSlotIndex != -1 && slotOccupied[selectedSlotIndex]) return RemoveSelectedPotion();
        for (int i = slotOccupied.Length - 1; i >= 0; i--) if (slotOccupied[i]) { slotOccupied[i] = false; RestoreInventoryVisuals(); return true; }
        return false;
    }

    public void RemovePotion(Image usedSlot)
    {
        if (usedSlot == null || inventorySlots == null) return;
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i] == usedSlot) { slotOccupied[i] = false; if (selectedSlotIndex == i) selectedSlotIndex = -1; RestoreInventoryVisuals(); UpdateSlotHighlights(); return; }
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
        if (inventorySlots == null || slotOccupied == null) return;
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i] == null) continue;
            if (i < slotOccupied.Length && slotOccupied[i]) { 
                inventorySlots[i].sprite = potionSprite; 
                inventorySlots[i].color = Color.white; 
                inventorySlots[i].gameObject.SetActive(true);
            }
            else { 
                inventorySlots[i].sprite = null; 
                inventorySlots[i].color = new Color(1,1,1,0); 
            }
        }
    }

    public int GetPotionCount()
    {
        if (slotOccupied == null) return 0;
        int count = 0;
        foreach (bool b in slotOccupied) if (b) count++;
        return count;
    }
}
