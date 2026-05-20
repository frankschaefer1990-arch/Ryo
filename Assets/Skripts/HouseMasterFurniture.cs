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

    private void Start()
    {
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (sleepButton != null) sleepButton.onClick.AddListener(StartSleep);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseUI);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (interactionPanel != null && interactionPanel.activeSelf)
            {
                if (type == FurnitureType.Desk) AdvanceDeskText();
                else CloseUI();
            }
            else
            {
                OpenUI();
            }
        }
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
