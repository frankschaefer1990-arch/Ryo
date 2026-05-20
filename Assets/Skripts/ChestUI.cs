using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance;

    [Header("UI References")]
    public GameObject chestPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;
    public Button takeAllButton;

    private Chest currentChest;

    private void Awake()
    {
        Instance = this;
        if (chestPanel != null) chestPanel.SetActive(false);
    }

    public void Open(Chest chest)
    {
        currentChest = chest;
        chestPanel.SetActive(true);
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
            if (itemName == "Heiltrank")
            {
                if (InventoryManager.Instance.AddPotion())
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
    }

    public void TakeAll()
    {
        if (currentChest == null) return;
        
        List<string> itemsToTake = new List<string>(currentChest.items);
        foreach (var item in itemsToTake)
        {
            if (item == "Heiltrank")
            {
                if (InventoryManager.Instance != null && InventoryManager.Instance.AddPotion())
                {
                    currentChest.items.Remove(item);
                }
                else
                {
                    break; // Full
                }
            }
        }
        
        RefreshUI();
        if (currentChest.items.Count == 0) Close();
    }
}
