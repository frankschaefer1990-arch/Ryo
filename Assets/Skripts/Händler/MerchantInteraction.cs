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
                    DialogueUI.Instance.ShowMessage(speakerName, merchantMessage);
                }
                else
                {
                    Debug.LogError("DialogueUI fehlt!");
                }

                // Nach Dialog Shop öffnen
                Invoke(nameof(OpenShop), 2.2f);
            }
        }
    }

    private void OpenShop()
    {
        // Shop sichtbar
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        // Maus aktivieren
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Spieler Bewegung sperren
        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if (player != null)
        {
            player.canMove = false;
        }

        isTalking = false;
    }
}