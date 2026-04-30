using UnityEngine;

public class ShopDoor : MonoBehaviour
{
    public GameObject shopOverlay;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playerInside)
            {
                // Shop öffnen
                shopOverlay.SetActive(true);
                playerInside = true;
            }
            else
            {
                // Shop schließen
                shopOverlay.SetActive(false);
                playerInside = false;
            }
        }
    }
}