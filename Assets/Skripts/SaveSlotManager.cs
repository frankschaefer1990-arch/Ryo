using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance;

    [Header("UI References")]
    public GameObject saveSlotPanel;
    public SaveSlotUI[] slots;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI titleText;
    
    private int selectedSlot = -1;
    private bool isLoadingMode = true;

    private void Awake()
    {
        Instance = this;
        if (saveSlotPanel != null) saveSlotPanel.SetActive(false);
    }

    public void Open(bool loadMode)
    {
        isLoadingMode = loadMode;
        gameObject.SetActive(true); // Ensure the root is active
        if (saveSlotPanel != null) saveSlotPanel.SetActive(true);
        if (nameInputField != null) nameInputField.gameObject.SetActive(!loadMode);
        
        if (titleText != null)
        {
            titleText.text = loadMode ? "SPIEL LADEN" : "SPIEL SPEICHERN";
        }
        
        RefreshSlots();
    }

    public void Close()
    {
        if (saveSlotPanel != null) saveSlotPanel.SetActive(false);
    }

    private void Update()
    {
        if (saveSlotPanel != null && saveSlotPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    }

    public void RefreshSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Setup(i, isLoadingMode);
            }
        }
    }

    public void OnSlotSelected(int index)
    {
        selectedSlot = index;
        
        if (isLoadingMode)
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave(index))
            {
                SaveSystem.Instance.Load(index);
                Close();
            }
        }
        else
        {
            string customName = nameInputField != null ? nameInputField.text : "";
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Save(index, customName);
                RefreshSlots();
                if (nameInputField != null) nameInputField.text = "";
            }
        }
    }
}