using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // =========================
    // SINGLETON + PERSISTENT
    // =========================
    private void Awake()
    {
        // Duplicate Schutz
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Zwischen Szenen behalten
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // CLEANUP
    // =========================
    private void OnDestroy()
    {
        // Keine Listener mehr nötig
    }

    // =========================
    // START
    // =========================
    private void Start()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        Debug.Log("Inventar Refresh gestartet...");
        ReconnectBackpackPanel();
        InitializeInventorySlots();
        RestoreInventoryVisuals();
        UpdateSlotHighlights();
        Debug.Log("Inventar Refresh abgeschlossen. Pots: " + GetPotionCount());
    }

    // =========================
    // BACKPACK PANEL RECONNECT
    // =========================
    private void ReconnectBackpackPanel()
    {
        // WICHTIG: Nutze den Canvas vom GameManager falls vorhanden
        GameObject targetCanvas = null;
        if (GameManager.Instance != null && GameManager.Instance.canvas != null)
        {
            targetCanvas = GameManager.Instance.canvas;
        }
        else
        {
            Canvas c = FindAnyObjectByType<Canvas>();
            if (c != null) targetCanvas = c.gameObject;
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("InventoryManager: Kein Canvas gefunden!");
            return;
        }

        Transform canvasTransform = targetCanvas.transform;
        
        // Suche BackpackPanel unter dem korrekten Canvas
        Transform foundBackpack = canvasTransform.Find("InventoryPanel/BackpackPanel");
        
        if (foundBackpack == null)
            foundBackpack = canvasTransform.Find("Inventory/Backpack");

        if (foundBackpack == null)
        {
            // Suche tief im Canvas falls Deaktiviert oder verschoben
            foreach (Transform t in canvasTransform.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "BackpackPanel") { foundBackpack = t; break; }
            }
        }

        if (foundBackpack != null)
        {
            backpackPanel = foundBackpack;
            Debug.Log("BackpackPanel verbunden unter: " + targetCanvas.name);
        }
        else
        {
            Debug.LogWarning("BackpackPanel konnte nicht im Canvas gefunden werden!");
        }
    }

    // =========================
    // SLOT SYSTEM AUFBAUEN
    // =========================
    private void InitializeInventorySlots()
    {
        if (backpackPanel == null)
            return;

        int slotCount = backpackPanel.childCount;
        if (slotCount == 0) 
        {
            Debug.LogWarning("BackpackPanel hat keine Slots!");
            return;
        }

        // Wir erzwingen eine Neuinitialisierung der Arrays pro Szene, 
        // da die Image-Komponenten in der Szene neu sind.
        inventorySlots = new Image[slotCount];
        slotBackgrounds = new Image[slotCount];
        
        // Aber die Daten (slotOccupied) müssen erhalten bleiben
        if (slotOccupied == null)
        {
            slotOccupied = new bool[slotCount];
        }
        else if (slotOccupied.Length != slotCount)
        {
            bool[] newOccupied = new bool[slotCount];
            int copyLength = Mathf.Min(slotOccupied.Length, slotCount);
            for (int i = 0; i < copyLength; i++)
            {
                newOccupied[i] = slotOccupied[i];
            }
            slotOccupied = newOccupied;
        }

        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = backpackPanel.GetChild(i);
            int index = i; // Closure fix
            
            // Hintergrund für Highlight speichern
            slotBackgrounds[i] = slot.GetComponent<Image>();
            if (slotBackgrounds[i] != null) 
            {
                slotBackgrounds[i].enabled = true;
                slotBackgrounds[i].color = new Color(1f, 1f, 1f, 0f);
            }

            // NEU: Click Handler für den Slot (Hintergrund)
            InventoryClickHandler slotHandler = slot.GetComponent<InventoryClickHandler>();
            if (slotHandler == null) slotHandler = slot.gameObject.AddComponent<InventoryClickHandler>();
            slotHandler.slotIndex = index;

            Transform itemTransform = slot.Find("Item");
            if (itemTransform == null && slot.childCount > 0) itemTransform = slot.GetChild(0);

            if (itemTransform == null)
            {
                Debug.LogWarning("Kein Item Child gefunden in Slot: " + slot.name);
                continue;
            }

            Image itemImage = itemTransform.GetComponent<Image>();
            if (itemImage == null) itemImage = itemTransform.gameObject.AddComponent<Image>();

            inventorySlots[i] = itemImage;
            inventorySlots[i].preserveAspect = true;
            
            // NEU: Click Handler auch für das Item selbst (damit Tränke rechtsklickbar sind)
            InventoryClickHandler itemHandler = itemTransform.GetComponent<InventoryClickHandler>();
            if (itemHandler == null) itemHandler = itemTransform.gameObject.AddComponent<InventoryClickHandler>();
            itemHandler.slotIndex = index;

            // Die Tränke sollen die Klicks NICHT blockieren für den SlotHandler, 
            // aber sie brauchen RaycastTarget=true für den eigenen IPointerClickHandler.
            inventorySlots[i].raycastTarget = true;
        }

        Debug.Log("Inventory erfolgreich initialisiert.");
    }

    // =========================
    // SELEKTION
    // =========================
    public void SelectSlot(int index)
    {
        // Nur belegte Slots auswählbar machen
        if (index < 0 || index >= slotOccupied.Length || !slotOccupied[index])
        {
            selectedSlotIndex = -1;
        }
        else
        {
            selectedSlotIndex = index;
            Debug.Log("Slot " + index + " selektiert.");
            
            // Shop-Auswahl aufheben
            ShopManager shop = FindAnyObjectByType<ShopManager>();
            if (shop != null)
            {
                shop.DeselectShopItem();
            }
        }
        UpdateSlotHighlights();
    }

    public void DeselectSlot()
    {
        selectedSlotIndex = -1;
        UpdateSlotHighlights();
    }

    private void UpdateSlotHighlights()
    {
        if (slotBackgrounds == null) return;

        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;
            
            // Wichtig: enabled muss TRUE bleiben, damit der Slot klickbar bleibt!
            slotBackgrounds[i].enabled = true;

            if (i == selectedSlotIndex)
            {
                slotBackgrounds[i].color = new Color(0f, 0.5f, 1f, 0.8f); // Blaues Highlight
            }
            else
            {
                // Nur transparent machen
                slotBackgrounds[i].color = new Color(1f, 1f, 1f, 0f); 
            }
        }
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

    // =========================
    // VISUALS NACH SZENENWECHSEL
    // =========================
    private void RestoreInventoryVisuals()
    {
        if (inventorySlots == null || slotOccupied == null)
            return;

        int count = 0;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
                continue;

            if (i < slotOccupied.Length && slotOccupied[i])
            {
                inventorySlots[i].sprite = potionSprite;
                inventorySlots[i].color = Color.white;
                count++;
            }
            else
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].color = new Color(1f, 1f, 1f, 0f);
            }
        }
        Debug.Log("Visuals wiederhergestellt. Aktive Tränke: " + count);
    }

    // =========================
    // POTION HINZUFÜGEN
    // =========================
    public bool AddPotion()
    {
        if (potionSprite == null)
        {
            Debug.LogError("Potion Sprite fehlt!");
            return false;
        }

        if (inventorySlots == null || slotOccupied == null)
        {
            Debug.LogError("Inventar nicht initialisiert!");
            return false;
        }

        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (!slotOccupied[i])
            {
                slotOccupied[i] = true;
                RestoreInventoryVisuals(); // Update UI
                Debug.Log("Potion hinzugefügt in Slot: " + (i + 1));
                return true;
            }
        }

        Debug.Log("Inventar voll!");
        return false;
    }

    // =========================
    // POTION ENTFERNEN (EINEN)
    // =========================
    public bool RemoveOnePotion()
    {
        if (slotOccupied == null) return false;

        // Wenn etwas selektiert ist, nimm das zuerst
        if (selectedSlotIndex != -1 && slotOccupied[selectedSlotIndex])
        {
            return RemoveSelectedPotion();
        }

        // Entferne den letzten Trank in der Liste
        for (int i = slotOccupied.Length - 1; i >= 0; i--)
        {
            if (slotOccupied[i])
            {
                slotOccupied[i] = false;
                RestoreInventoryVisuals();
                return true;
            }
        }
        return false;
    }

    // =========================
    // POTION ENTFERNEN (SPEZIFISCH)
    // =========================
    public void RemovePotion(Image usedSlot)
    {
        if (usedSlot == null || inventorySlots == null)
            return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == usedSlot)
            {
                slotOccupied[i] = false;
                
                // Auswahl aufheben, wenn der gelöschte Slot selektiert war
                if (selectedSlotIndex == i)
                {
                    selectedSlotIndex = -1;
                }

                RestoreInventoryVisuals(); // Update UI
                UpdateSlotHighlights();
                Debug.Log("Potion entfernt aus Slot: " + (i + 1));
                return;
            }
        }
    }

    // =========================
    // INVENTAR CHECK
    // =========================
    public bool HasFreeSlot()
    {
        if (slotOccupied == null)
            return false;

        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (!slotOccupied[i])
                return true;
        }

        return false;
    }

    // =========================
    // SLOT COUNT
    // =========================
    public int GetPotionCount()
    {
        if (slotOccupied == null)
            return 0;

        int count = 0;

        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (slotOccupied[i])
                count++;
        }

        return count;
    }
}