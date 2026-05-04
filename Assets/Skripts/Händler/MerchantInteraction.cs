using UnityEngine;

public class MerchantInteraction : MonoBehaviour
{
    [Header("Merchant Settings")]
    public string speakerName = "Händler";

    [TextArea]
    public string merchantMessage = "Schau dir meine Waren an...";

    [Header("Shop UI")]
    public GameObject shopPanel;

    private bool playerInside = false;
    private bool isTalking = false;

    private void Start()
    {
        // Shop am Anfang geschlossen
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Cursor standardmäßig im Spiel verstecken
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
    }

    private void Update()
    {
        // Spieler steht beim Händler + drückt R
        if (playerInside && Input.GetKeyDown(KeyCode.R))
        {
            // Verhindert mehrfaches Triggern
            if (!isTalking)
            {
                isTalking = true;

                // Händler Dialog
                if (DialogueUI.Instance != null)
                {
                    DialogueUI.Instance.ShowMessage(speakerName, merchantMessage, 1.2f);
                }
                else
                {
                    Debug.LogError("DialogueUI fehlt!");
                }

                // Nach Dialog Shop öffnen (Delay reduziert von 2.2f auf 1.2f)
                Invoke(nameof(OpenShop), 1.2f);
                }
                }
                }

    private void OpenShop()
    {
        // Den ShopManager suchen und dort OpenShop aufrufen
        ShopManager shopManager = FindAnyObjectByType<ShopManager>();

        if (shopManager != null)
        {
            shopManager.OpenShopFromMerchant();
        }
        else
        {
            // Fallback falls kein ShopManager da ist
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) player.canMove = false;
        }

        isTalking = false;
    }
}