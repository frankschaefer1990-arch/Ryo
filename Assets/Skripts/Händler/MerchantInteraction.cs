using UnityEngine;

public class MerchantInteraction : MonoBehaviour
{
    [Header("Merchant Settings")]
    public string speakerName = "Händler";
    public float interactionRadius = 2.5f;

    [TextArea]
    public string merchantMessage = "Schau dir meine Waren an...";

    [Header("Shop UI")]
    public GameObject shopPanel;
    public Sprite speakerPortrait;

    private bool playerInside = false;
    private bool isTalking = false;

    private void Start()
    {
        // Shop am Anfang geschlossen
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
    }

    private void OnTriggerStay2D(Collider2D other)
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
            StartTalking();
            Invoke(nameof(OpenShop), 1.2f);
        }
    }

    public void StartTalking()
    {
        if (isTalking) return;
        isTalking = true;
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speakerName, merchantMessage, speakerPortrait, 1.2f);
        }
        Invoke(nameof(ResetTalkState), 1.2f);
    }

    private void ResetTalkState()
    {
        isTalking = false;
    }

    private void OpenShop()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShopFromMerchant();
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
    }

    private void OnValidate()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.radius = interactionRadius;
            circle.isTrigger = true;
            circle.offset = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    }