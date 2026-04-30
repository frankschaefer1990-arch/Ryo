using UnityEngine;

public class ShopDoor : MonoBehaviour
{
    [Header("Shop Overlay")]
    public GameObject shopOverlay;

    private void Start()
    {
        // Sicherheit: Shop am Anfang geschlossen
        if (shopOverlay != null)
        {
            shopOverlay.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Spieler betritt den Shopbereich
        if (other.CompareTag("Player"))
        {
            if (shopOverlay != null)
            {
                shopOverlay.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Spieler verlässt den Shopbereich
        if (other.CompareTag("Player"))
        {
            if (shopOverlay != null)
            {
                shopOverlay.SetActive(false);
            }
        }
    }
}