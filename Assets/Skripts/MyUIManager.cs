using UnityEngine;

public class MyUIManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject backpackPanel;
    public GameObject attributePanel;

    void Start()
    {
        inventoryPanel.SetActive(false);
        backpackPanel.SetActive(false);
        attributePanel.SetActive(false);
    }

    void Update()
    {
        // INVENTORY
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isOpen = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isOpen);

            if (isOpen)
                backpackPanel.SetActive(false);

            UpdateCursor();
        }

        // ATTRIBUTES
        if (Input.GetKeyDown(KeyCode.T))
        {
            attributePanel.SetActive(!attributePanel.activeSelf);
            UpdateCursor();
        }

        // ESC CLOSE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventoryPanel.SetActive(false);
            backpackPanel.SetActive(false);
            attributePanel.SetActive(false);

            UpdateCursor();
        }
    }

    public void ToggleBackpack()
    {
        backpackPanel.SetActive(!backpackPanel.activeSelf);
        UpdateCursor();
    }

    void UpdateCursor()
    {
        bool anyOpen = inventoryPanel.activeSelf || backpackPanel.activeSelf || attributePanel.activeSelf;

        Cursor.visible = anyOpen;
        Cursor.lockState = anyOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}