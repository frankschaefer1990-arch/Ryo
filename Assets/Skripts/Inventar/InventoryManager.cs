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
    private bool[] slotOccupied;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (backpackPanel == null)
        {
            Debug.LogError("BackpackPanel fehlt!");
            return;
        }

        int slotCount = backpackPanel.childCount;

        inventorySlots = new Image[slotCount];
        slotOccupied = new bool[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = backpackPanel.GetChild(i);

            // Child Item suchen
            Transform itemTransform = slot.Find("Item");

            // FALLBACK:
            // Falls dein Child anders heißt, erstes Child nehmen
            if (itemTransform == null && slot.childCount > 0)
            {
                itemTransform = slot.GetChild(0);
            }

            if (itemTransform == null)
            {
                Debug.LogWarning("Kein Item Child gefunden in Slot: " + slot.name);
                continue;
            }

            Image itemImage = itemTransform.GetComponent<Image>();

            // Falls kein Image vorhanden → automatisch hinzufügen
            if (itemImage == null)
            {
                itemImage = itemTransform.gameObject.AddComponent<Image>();
            }

            inventorySlots[i] = itemImage;

            // Start leer
            inventorySlots[i].sprite = null;
            inventorySlots[i].color = new Color(1f, 1f, 1f, 0f);
            inventorySlots[i].preserveAspect = true;

            slotOccupied[i] = false;

            Debug.Log("Inventar Slot bereit: " + slot.name);
        }
    }

    // =========================
    // POTION HINZUFÜGEN
    // =========================
    public void AddPotion()
    {
        if (potionSprite == null)
        {
            Debug.LogError("Potion Sprite fehlt im InventoryManager!");
            return;
        }

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("Keine Inventory Slots gefunden!");
            return;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // Sicherheitscheck
            if (inventorySlots[i] == null)
            {
                Debug.LogWarning("Slot " + i + " ist NULL");
                continue;
            }

            // Freier Slot
            if (!slotOccupied[i])
            {
                inventorySlots[i].sprite = potionSprite;
                inventorySlots[i].color = Color.white;
                inventorySlots[i].preserveAspect = true;

                slotOccupied[i] = true;

                Debug.Log("Trank hinzugefügt in Slot: " + (i + 1));

                return;
            }
        }

        Debug.Log("Inventar voll!");
    }

    // =========================
    // SLOT FREIGEBEN
    // =========================
    public void RemovePotion(Image usedSlot)
    {
        if (usedSlot == null) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == usedSlot)
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].color = new Color(1f, 1f, 1f, 0f);

                slotOccupied[i] = false;

                Debug.Log("Slot freigegeben: " + (i + 1));

                return;
            }
        }
    }
}