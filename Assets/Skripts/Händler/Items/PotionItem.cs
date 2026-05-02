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
        // Button holen
        button = GetComponent<Button>();

        // Erst eigenes Image versuchen
        itemImage = GetComponent<Image>();

        // Falls keins oder kein Sprite -> Child prüfen
        if (itemImage == null)
        {
            itemImage = GetComponentInChildren<Image>();
        }

        // Button Event
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(UsePotion);
        }
        else
        {
            Debug.LogError("Button fehlt auf: " + gameObject.name);
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

        // Entfernen
        itemImage.sprite = null;

        // Unsichtbar
        itemImage.color = new Color(1f, 1f, 1f, 0f);
    }
}