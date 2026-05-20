using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryClickHandler : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex = -1;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SelectSlot(slotIndex);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (InventoryManager.Instance != null)
            {
                // Select first to ensure correct slot is used, then use
                InventoryManager.Instance.SelectSlot(slotIndex);
                InventoryManager.Instance.UseSelectedItem();
            }
        }
    }
}