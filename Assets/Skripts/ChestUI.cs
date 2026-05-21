using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ChestUI : MonoBehaviour
{
    private static ChestUI _instance;
    public static ChestUI Instance
    {
        get
        {
            if (_instance == null || _instance.gameObject == null)
            {
                if (MyUIManager.Instance != null && MyUIManager.Instance.chestUI != null)
                    _instance = MyUIManager.Instance.chestUI;
                else {
                    var all = Object.FindObjectsByType<ChestUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    _instance = all.OrderByDescending(x => x.gameObject.scene.name == "DontDestroyOnLoad").FirstOrDefault();
                }
            }
            return _instance;
        }
    }

    [Header("UI References")]
    public GameObject chestPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;
    public GameObject slotTemplate; // New: Internal template
    public Button takeAllButton;
    public Button closeButton;

    private Chest currentChest;

    private void Awake()
    {
        if (_instance != null && _instance != this && _instance.gameObject != null)
        {
            if (_instance.gameObject.scene.name == "DontDestroyOnLoad" || _instance.transform.root.gameObject.scene.name == "DontDestroyOnLoad")
            {
                Destroy(gameObject);
                return;
            }
        }
        _instance = this;
        if (MyUIManager.Instance != null) MyUIManager.Instance.chestUI = this;

        // Auto-find references if missing
        if (chestPanel == null) chestPanel = transform.Find("ChestPanel")?.gameObject;
        
        if (slotContainer == null && chestPanel != null)
            slotContainer = chestPanel.transform.Find("Slots") ?? chestPanel.transform.Find("Viewport/Content") ?? chestPanel.transform.Find("Content");

        if (slotTemplate == null && slotContainer != null)
            slotTemplate = slotContainer.Find("SlotTemplate")?.gameObject;

        if (takeAllButton == null && chestPanel != null)
            takeAllButton = chestPanel.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "TakeAllButton");

        if (closeButton == null && chestPanel != null)
            closeButton = chestPanel.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.ToLower().Contains("close") || b.name == "CloseButton" || b.name == "X");

        if (chestPanel != null) chestPanel.SetActive(false);
        if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(Close); }
        if (takeAllButton != null) { takeAllButton.onClick.RemoveAllListeners(); takeAllButton.onClick.AddListener(TakeAll); }
    }

    public void Open(Chest chest)
    {
        if (chest == null) 
        {
            Debug.LogError("ChestUI: Attempted to open with NULL chest reference!");
            return;
        }

        Debug.Log($"ChestUI: Opening chest '{chest.name}' with {chest.items.Count} items.");
        currentChest = chest;
        
        gameObject.SetActive(true);
        if (chestPanel == null) chestPanel = transform.Find("ChestPanel")?.gameObject;
        
        if (chestPanel != null) 
        {
            chestPanel.SetActive(true);
            // Center the panel just in case
            RectTransform rt = chestPanel.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            Debug.LogError("ChestUI: ChestPanel GameObject NOT found!");
            return;
        }

        RefreshUI();

        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = false;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        if (chestPanel != null) chestPanel.SetActive(false);
        
        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = true;
        
        if (MyUIManager.Instance != null && !MyUIManager.Instance.IsAnyPanelOpen())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (currentChest != null && currentChest.items.Count == 0)
            currentChest.OnChestEmptied();
        
        currentChest = null;
    }

    public void RefreshUI()
    {
        if (slotContainer == null) 
        {
            slotContainer = transform.Find("ChestPanel/Slots") ?? transform.Find("ChestPanel/Viewport/Content") ?? transform.Find("ChestPanel/Content");
            if (slotContainer == null) return;
        }
        
        // Clear existing slots safely (except template)
        foreach (Transform child in slotContainer)
        {
            if (child.name == "SlotTemplate") continue;
            Destroy(child.gameObject);
        }

        if (currentChest == null || currentChest.items == null) return;

        Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        foreach (var item in currentChest.items)
        {
            if (string.IsNullOrEmpty(item)) continue;
            if (itemCounts.ContainsKey(item)) itemCounts[item]++;
            else itemCounts[item] = 1;
        }

        GameObject prefabToUse = slotPrefab != null ? slotPrefab : slotTemplate;
        if (prefabToUse == null)
        {
            Debug.LogError("ChestUI: No slot prefab or template found!");
            return;
        }

        foreach (var kvp in itemCounts)
        {
            GameObject slot = Instantiate(prefabToUse, slotContainer);
            slot.name = "Slot_" + kvp.Key;
            slot.SetActive(true);
            
            var texts = slot.GetComponentsInChildren<TextMeshProUGUI>(true);
            var itemText = texts.FirstOrDefault(t => t.name == "ItemText") ?? texts.FirstOrDefault();
            if (itemText != null)
            {
                itemText.text = $"{kvp.Key} {kvp.Value}x";
                itemText.color = new Color(1f, 0.84f, 0f, 1f);
            }

            var btn = slot.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string itemName = kvp.Key;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => TakeItem(itemName));
            }
        }
        
        Canvas.ForceUpdateCanvases();
        }

        public void TakeItem(string itemName)
        {
        if (currentChest == null || InventoryManager.Instance == null) return;
        
        bool success = false;
        if (itemName == "Heiltrank") success = InventoryManager.Instance.AddPotion();
        else if (itemName == "Manatrank" || itemName == "Mana Trank") success = InventoryManager.Instance.AddManaPotion();

        if (success)
        {
            Debug.Log($"ChestUI: Successfully took {itemName}");
            currentChest.items.Remove(itemName);
            RefreshUI();
            if (currentChest.items.Count == 0) Close();
        }
        else
        {
            Debug.LogWarning("ChestUI: Inventory is full! Item remained in chest.");
        }
    }

    public void TakeAll()
    {
        if (currentChest == null) return;
        List<string> itemsToTake = new List<string>(currentChest.items);
        foreach (var item in itemsToTake) {
            bool success = false;
            if (item == "Heiltrank") success = InventoryManager.Instance != null && InventoryManager.Instance.AddPotion();
            else if (item == "Manatrank" || item == "Mana Trank") success = InventoryManager.Instance != null && InventoryManager.Instance.AddManaPotion();
            if (success) currentChest.items.Remove(item);
            else break;
        }
        RefreshUI();
        if (currentChest.items.Count == 0) Close();
    }
}

