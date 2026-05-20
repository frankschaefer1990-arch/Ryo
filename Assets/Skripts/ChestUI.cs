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
            if (_instance == null)
            {
                // Fallback 1: Standard search
                _instance = Object.FindAnyObjectByType<ChestUI>(FindObjectsInactive.Include);
                
                // Fallback 2: Search by name on Canvas
                if (_instance == null)
                {
                    GameObject canvas = GameObject.Find("Canvas");
                    if (canvas != null)
                    {
                        var found = canvas.GetComponentsInChildren<ChestUI>(true).FirstOrDefault();
                        if (found != null) _instance = found;
                    }
                }

                // Fallback 3: Search for ANY object named ChestUI
                if (_instance == null)
                {
                    var allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach(var go in allGOs)
                    {
                        if(go.name == "ChestUI" || go.name == "ChestPanel")
                        {
                            var ui = go.GetComponent<ChestUI>();
                            if(ui != null) { _instance = ui; break; }
                        }
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("UI References")]
    public GameObject chestPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;
    public Button takeAllButton;

    private Chest currentChest;

    private void Awake()
    {
        Debug.Log($"ChestUI: Awake on {gameObject.name} (ActiveInHierarchy: {gameObject.activeInHierarchy})");
        if (_instance != null && _instance != this)
        {
            Debug.Log($"ChestUI: Duplicate found on {gameObject.name}, destroying.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // Panel should be inactive by default
        if (chestPanel != null) chestPanel.SetActive(false);
    }

    public void Open(Chest chest)
    {
        currentChest = chest;
        // We ensure the root is active
        gameObject.SetActive(true);
        if (chestPanel != null) chestPanel.SetActive(true);
        RefreshUI();

        // Disable player movement if needed
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null) player.canMove = false;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        chestPanel.SetActive(false);
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null) player.canMove = true;
        
        if (MyUIManager.Instance != null && !MyUIManager.Instance.IsAnyPanelOpen())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (currentChest != null && currentChest.items.Count == 0)
        {
            currentChest.OnChestEmptied();
        }
    }

    public void RefreshUI()
    {
        // Clear slots
        foreach (Transform child in slotContainer) Destroy(child.gameObject);

        if (currentChest == null) return;

        // Group items for display (e.g., Heiltrank 2x)
        Dictionary<string, int> itemCounts = new Dictionary<string, int>();
        foreach (var item in currentChest.items)
        {
            if (itemCounts.ContainsKey(item)) itemCounts[item]++;
            else itemCounts[item] = 1;
        }

        foreach (var kvp in itemCounts)
        {
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            var text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"{kvp.Key} {kvp.Value}x";

            var btn = slot.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string itemName = kvp.Key;
                btn.onClick.AddListener(() => TakeItem(itemName));
            }
        }

        if (takeAllButton != null)
        {
            takeAllButton.onClick.RemoveAllListeners();
            takeAllButton.onClick.AddListener(TakeAll);
        }
    }

    public void TakeItem(string itemName)
    {
        if (currentChest == null) return;

        if (InventoryManager.Instance != null)
        {
            bool success = false;
            if (itemName == "Heiltrank")
            {
                success = InventoryManager.Instance.AddPotion();
            }
            else if (itemName == "Manatrank" || itemName == "Mana Trank")
            {
                success = InventoryManager.Instance.AddManaPotion();
            }

            if (success)
            {
                currentChest.items.Remove(itemName);
                RefreshUI();
                if (currentChest.items.Count == 0) Close();
            }
            else
            {
                Debug.Log("Inventory Full!");
            }
        }
    }

    public void TakeAll()
    {
        if (currentChest == null) return;
        
        List<string> itemsToTake = new List<string>(currentChest.items);
        foreach (var item in itemsToTake)
        {
            bool success = false;
            if (item == "Heiltrank")
            {
                success = InventoryManager.Instance != null && InventoryManager.Instance.AddPotion();
            }
            else if (item == "Manatrank")
            {
                success = InventoryManager.Instance != null && InventoryManager.Instance.AddManaPotion();
            }

            if (success)
            {
                currentChest.items.Remove(item);
            }
            else
            {
                break; // Full
            }
        }
        
        RefreshUI();
        if (currentChest.items.Count == 0) Close();
    }
}
