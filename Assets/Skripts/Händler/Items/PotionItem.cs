using UnityEngine;
using UnityEngine.UI;

public class PotionItem : MonoBehaviour
{
    [Header("Potion Settings")]
    public int healAmount = 30;

    private Button button;
    private Image itemImage;

    private void Start()
    {
        // Erst eigenes Image versuchen
        itemImage = GetComponent<Image>();

        // Falls keins oder kein Sprite -> Child prüfen
        if (itemImage == null)
        {
            itemImage = GetComponentInChildren<Image>();
        }
    }

    // =========================
    // TRANK BENUTZEN
    // =========================
    public void UsePotion()
    {
        // Sicherheitscheck
        if (itemImage == null)
        {
            Debug.Log("Kein Item Image gefunden.");
            return;
        }

        // Kein Sprite = Kein Trank
        if (itemImage.sprite == null)
        {
            Debug.Log("Kein Trank vorhanden");
            return;
        }

        // Player prüfen
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats fehlt!");
            return;
        }

        // HP voll?
        if (PlayerStats.Instance.currentHealth >= PlayerStats.Instance.maxHealth)
        {
            Debug.Log("HP bereits voll!");
            return;
        }

        // Heilen
        PlayerStats.Instance.Heal(healAmount);
        Debug.Log("Trank benutzt! +" + healAmount + " HP");

        // WICHTIG: Über InventoryManager entfernen, damit der State (slotOccupied) stimmt!
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemovePotion(itemImage);
        }
        else
        {
            // Fallback falls Manager nicht da
            itemImage.sprite = null;
            itemImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}