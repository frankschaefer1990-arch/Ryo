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
            // Try to find a potion in this slot and use it
            PotionItem potion = GetComponent<PotionItem>();
            if (potion == null) potion = GetComponentInChildren<PotionItem>();
            
            if (potion != null)
            {
                potion.UsePotion();
            }
        }
    }
}