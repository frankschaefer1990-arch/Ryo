using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;
    
    private int slotIndex;
    private bool isLoadMode;

    public void Setup(int index, bool loadMode)
    {
        slotIndex = index;
        isLoadMode = loadMode;
        
        if (SaveSystem.Instance != null)
        {
            infoText.text = $"Slot {index + 1}\n" + SaveSystem.Instance.GetSaveInfo(index);
            
            bool hasSave = SaveSystem.Instance.HasSave(index);
            if (loadMode)
            {
                actionButton.interactable = hasSave;
                buttonText.text = "Laden";
            }
            else
            {
                actionButton.interactable = true;
                buttonText.text = hasSave ? "Überschreiben" : "Speichern";
            }
        }
    }

    public void OnClick()
    {
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotSelected(slotIndex);
        }
    }
}