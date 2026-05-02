using UnityEngine;
using UnityEngine.SceneManagement;

public class MyUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject inventoryPanel;
    public GameObject backpackPanel;
    public GameObject attributePanel;

    // =========================
    // START
    // =========================
    void Start()
    {
        ReconnectUI();

        // Standardmäßig alles schließen
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (backpackPanel != null)
            backpackPanel.SetActive(false);

        if (attributePanel != null)
            attributePanel.SetActive(false);

        UpdateCursor();
    }

    // =========================
    // SCENE EVENTS
    // =========================
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReconnectUI();
        UpdateCursor();
    }

    // =========================
    // UI AUTOMATISCH NEU VERBINDEN
    // =========================
    void ReconnectUI()
    {
        // Alte Referenzen löschen
        inventoryPanel = null;
        backpackPanel = null;
        attributePanel = null;

        // Aktives Canvas finden
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas nicht gefunden!");
            return;
        }

        // Panels suchen
        Transform inventory = canvas.transform.Find("InventoryPanel");
        Transform backpack = canvas.transform.Find("InventoryPanel/BackpackPanel");
        Transform attribute = canvas.transform.Find("AttributePanel");

        if (inventory != null)
            inventoryPanel = inventory.gameObject;

        if (backpack != null)
            backpackPanel = backpack.gameObject;

        if (attribute != null)
            attributePanel = attribute.gameObject;

        Debug.Log("UI neu verbunden.");
    }

    // =========================
    // UPDATE
    // =========================
    void Update()
    {
        if (inventoryPanel == null || backpackPanel == null || attributePanel == null)
            return;

        // =========================
        // INVENTORY (I)
        // =========================
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool inventoryWasOpen = inventoryPanel.activeSelf;

            inventoryPanel.SetActive(!inventoryWasOpen);

            // Inventory schließen = Backpack auch schließen
            if (inventoryWasOpen)
            {
                backpackPanel.SetActive(false);
            }

            UpdateCursor();
        }

        // =========================
        // BACKPACK (B)
        // =========================
        if (Input.GetKeyDown(KeyCode.B))
        {
            // Inventory öffnen falls geschlossen
            if (!inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(true);
            }

            // Backpack togglen
            backpackPanel.SetActive(!backpackPanel.activeSelf);

            UpdateCursor();
        }

        // =========================
        // ATTRIBUTE (T)
        // =========================
        if (Input.GetKeyDown(KeyCode.T))
        {
            attributePanel.SetActive(!attributePanel.activeSelf);

            UpdateCursor();
        }

        // =========================
        // ESC
        // =========================
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
            attributePanel.SetActive(false);

            UpdateCursor();
        }
    }

    // =========================
    // BUTTON BACKPACK
    // =========================
    public void ToggleBackpack()
    {
        if (inventoryPanel == null || backpackPanel == null)
            return;

        // Inventory öffnen falls nötig
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
        }

        // Backpack togglen
        backpackPanel.SetActive(!backpackPanel.activeSelf);

        UpdateCursor();
    }

    // =========================
    // BUTTON ATTRIBUTE
    // =========================
    public void ToggleAttributePanel()
    {
        if (attributePanel == null)
            return;

        attributePanel.SetActive(!attributePanel.activeSelf);

        UpdateCursor();
    }

    // =========================
    // CLOSE ALL
    // =========================
    public void CloseAllPanels()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (backpackPanel != null)
            backpackPanel.SetActive(false);

        if (attributePanel != null)
            attributePanel.SetActive(false);

        UpdateCursor();
    }

    // =========================
    // CURSOR
    // =========================
    void UpdateCursor()
    {
        bool anyOpen = false;

        if (inventoryPanel != null && inventoryPanel.activeSelf)
            anyOpen = true;

        if (backpackPanel != null && backpackPanel.activeSelf)
            anyOpen = true;

        if (attributePanel != null && attributePanel.activeSelf)
            anyOpen = true;

        Cursor.visible = anyOpen;
        Cursor.lockState = anyOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}