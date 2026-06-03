using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ElaraRestInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string speakerName = "Elara";
    public Sprite portrait;
    public string offerMessage = "Ryo, du siehst müde aus. Möchtest du dich bei mir ausruhen?";

    [Header("Interaction Settings")]
    public float interactionRange = 2.0f;
    public KeyCode interactKey = KeyCode.R;

    private Transform player;
    private bool playerInRange = false;
    private bool isDialogueRunning = false;

    [Header("UI References")]
    public GameObject interactionPanel;
    public TextMeshProUGUI textDisplay;
    public GameObject choiceButtons;
    public Button sleepButton;
    public Button cancelButton;

    private void Start()
    {
        FindPlayer();
        if (interactionPanel == null) FindUIReferences();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueRunning)
        {
            if (interactionPanel != null && interactionPanel.activeSelf)
            {
                CloseUI();
            }
            else
            {
                StartCoroutine(OfferRestRoutine());
            }
        }
    }

    private void FindPlayer()
    {
        GameObject pObj = GameObject.FindWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    private void FindUIReferences()
    {
        var connector = FurnitureUIConnector.Instance;
        if (connector != null)
        {
            interactionPanel = connector.panel;
            textDisplay = connector.textDisplay;
            choiceButtons = connector.choiceButtons;
            sleepButton = connector.sleepButton;
            cancelButton = connector.cancelButton;
        }
    }

    private IEnumerator OfferRestRoutine()
    {
        isDialogueRunning = true;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speakerName, offerMessage, portrait, 0.8f);
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }

        OpenUI();
        isDialogueRunning = false;
    }

    private void OpenUI()
    {
        if (interactionPanel == null) FindUIReferences();
        if (interactionPanel == null) return;

        interactionPanel.SetActive(true);
        if (textDisplay != null) textDisplay.text = "Möchtest du dich ausruhen? (Mana & HP werden wiederhergestellt)";
        if (choiceButtons != null) choiceButtons.SetActive(true);

        if (sleepButton != null)
        {
            sleepButton.onClick.RemoveAllListeners();
            sleepButton.onClick.AddListener(StartSleep);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseUI);
        }

        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
            pm.ResetMovementState();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void StartSleep()
    {
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        CloseUI();
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeOut(1f));
            yield return new WaitForSeconds(2f);

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(PlayerStats.Instance.maxHealth);
                PlayerStats.Instance.RestoreMana(PlayerStats.Instance.maxMana);
            }

            yield return StartCoroutine(FadeManager.Instance.FadeIn(1f));
        }
    }

    private void CloseUI()
    {
        if (interactionPanel != null) interactionPanel.SetActive(false);
        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = true;

        if (MyUIManager.Instance != null && !MyUIManager.Instance.IsAnyPanelOpen())
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
