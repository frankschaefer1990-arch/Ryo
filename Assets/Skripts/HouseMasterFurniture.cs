using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HouseMasterFurniture : MonoBehaviour
{
    public enum FurnitureType { Bed, Desk }
    public FurnitureType type;

    [Header("UI References")]
    public GameObject interactionPanel; // The LadePanel
    public TextMeshProUGUI textDisplay;
    public GameObject choiceButtons; // Buttons for sleep/cancel
    public Button sleepButton;
    public Button cancelButton;

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.R;

    [Header("Desk Content")]
    [TextArea(5, 10)]
    public string deskMessage = "Der Seelenverschlinger wird einst alles verschlingen.\nKeine Seele bleibt verschont.\nNicht die der Menschen.\nNicht die der Hollow.";
    public string deskMessage2 = "Ich hätte den Jungen töten sollen, bevor der Fluch erwacht.\nDoch mein Herz wurde schwach…";
    public string deskMessage3 = "Er war doch bloß ein Kind.";

    private bool playerInRange = false;
    private int deskPage = 0;
    private bool isDialogueRunning = false;

    private void Start()
    {
        FindUIReferences();
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }

    private void FindUIReferences()
    {
        // Try to find the FurnitureUIConnector in the scene
        var connector = Object.FindAnyObjectByType<FurnitureUIConnector>(FindObjectsInactive.Include);
        if (connector != null)
        {
            if (interactionPanel == null) interactionPanel = connector.panel;
            if (textDisplay == null) textDisplay = connector.textDisplay;
            if (choiceButtons == null) choiceButtons = connector.choiceButtons;
            if (sleepButton == null) sleepButton = connector.sleepButton;
            if (cancelButton == null) cancelButton = connector.cancelButton;
        }

        // Wire buttons if found
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Furniture: Player entered range of {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log($"Furniture: Player left range of {gameObject.name}");
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueRunning)
        {
            Debug.Log($"Furniture: Interacting with {gameObject.name}");

            if (type == FurnitureType.Desk)
            {
                StartCoroutine(DeskDialogueRoutine());
            }
            else
            {
                if (interactionPanel == null) FindUIReferences();

                if (interactionPanel != null && interactionPanel.activeSelf)
                {
                    CloseUI();
                }
                else
                {
                    OpenUI();
                }
            }
        }
    }

    private IEnumerator DeskDialogueRoutine()
    {
        if (isDialogueRunning) yield break;
        isDialogueRunning = true;

        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null)
        {
            pm.canMove = false;
            pm.ResetMovementState();
        }

        if (DialogueUI.Instance != null)
        {
            // Duration calculation: typewriter time + visible time + buffer
            float letterTime = 0.04f;
            
            DialogueUI.Instance.ShowMessage("Meister", deskMessage, 3.5f);
            yield return new WaitForSeconds(deskMessage.Length * letterTime + 3.5f + 0.5f);
            
            DialogueUI.Instance.ShowMessage("Meister", deskMessage2, 3.5f);
            yield return new WaitForSeconds(deskMessage2.Length * letterTime + 3.5f + 0.5f);
            
            DialogueUI.Instance.ShowMessage("Meister", deskMessage3, 3.0f);
            yield return new WaitForSeconds(deskMessage3.Length * letterTime + 3.0f + 0.5f);
        }

        if (pm != null) pm.canMove = true;
        isDialogueRunning = false;
    }

    private void OpenUI()
    {
        if (interactionPanel == null) return;
        interactionPanel.SetActive(true);
        
        if (type == FurnitureType.Bed)
        {
            if (textDisplay != null) textDisplay.text = "Möchtest du schlafen?";
            if (choiceButtons != null) choiceButtons.SetActive(true);
        }
        else
        {
            deskPage = 0;
            if (choiceButtons != null) choiceButtons.SetActive(false);
            ShowDeskPage();
        }

        var pm = Object.FindAnyObjectByType<PlayerMovement>();
        if (pm != null) pm.canMove = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowDeskPage()
    {
        if (textDisplay == null) return;
        if (deskPage == 0) textDisplay.text = deskMessage;
        else if (deskPage == 1) textDisplay.text = deskMessage2;
        else if (deskPage == 2) textDisplay.text = deskMessage3;
    }

    private void AdvanceDeskText()
    {
        deskPage++;
        if (deskPage > 2) CloseUI();
        else ShowDeskPage();
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
            
            // Restore HP/Mana
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
}
