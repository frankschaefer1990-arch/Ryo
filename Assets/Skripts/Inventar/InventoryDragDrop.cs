using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    
    private int startSlotIndex = -1;
    private EqType? startEqType = null;
    private SmithSlotType? startSmithType = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryClickHandler backpackHandler = GetComponentInParent<InventoryClickHandler>();
        if (backpackHandler != null) { startSlotIndex = backpackHandler.slotIndex; startEqType = null; startSmithType = null; }
        else {
            EquipmentSlot eq = GetComponentInParent<EquipmentSlot>();
            if (eq != null) { startEqType = eq.slotType; startSlotIndex = -1; startSmithType = null; }
            else {
                BlacksmithSlot smith = GetComponentInParent<BlacksmithSlot>();
                if (smith != null) { startSmithType = smith.slotType; startSlotIndex = -1; startEqType = null; }
            }
        }
        
        InventoryManager.Instance?.DeselectSlot();
        startPosition = rectTransform.localPosition;
        startParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) { rectTransform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f; canvasGroup.blocksRaycasts = true;
        GameObject target = eventData.pointerCurrentRaycast.gameObject;

        if (target != null)
        {
            // 1. Target is a Backpack Slot
            InventoryClickHandler targetBackpack = target.GetComponentInParent<InventoryClickHandler>();
            if (targetBackpack != null && targetBackpack.slotIndex != -1)
            {
                if (startSlotIndex != -1) { InventoryManager.Instance.MoveItem(startSlotIndex, targetBackpack.slotIndex); InventoryManager.Instance.SelectSlot(targetBackpack.slotIndex); }
                else if (startEqType != null) InventoryManager.Instance.UnequipToSlot(startEqType.Value, targetBackpack.slotIndex);
                else if (startSmithType != null && BlacksmithManager.Instance != null) {
                    int id = BlacksmithManager.Instance.GetSlotItemId(startSmithType.Value);
                    if (id != 0) {
                        InventoryManager.Instance.GetSlotItemTypes()[targetBackpack.slotIndex] = id;
                        BlacksmithManager.Instance.RemoveItemFromSlot(startSmithType.Value);
                    }
                }
            }
            
            // 2. Target is an Equipment Slot
            EquipmentSlot targetEq = target.GetComponentInParent<EquipmentSlot>();
            if (targetEq != null)
            {
                if (startSlotIndex != -1) InventoryManager.Instance.EquipFromBackpack(startSlotIndex, targetEq.slotType);
                else if (startSmithType != null && BlacksmithManager.Instance != null) {
                    int id = BlacksmithManager.Instance.GetSlotItemId(startSmithType.Value);
                    if (id != 0) {
                        InventoryManager.Instance.AddItem(id); 
                        BlacksmithManager.Instance.RemoveItemFromSlot(startSmithType.Value);
                        InventoryManager.Instance.EquipFromBackpack(InventoryManager.Instance.GetSlotItemTypes().Length-1, targetEq.slotType); 
                    }
                }
            }

            // 3. Target is a Blacksmith Slot
            BlacksmithSlot targetSmith = target.GetComponentInParent<BlacksmithSlot>();
            if (targetSmith != null && BlacksmithManager.Instance != null)
            {
                int itemId = 0;
                if (startSlotIndex != -1) itemId = InventoryManager.Instance.GetSlotItemTypes()[startSlotIndex];
                else if (startEqType != null) itemId = InventoryManager.Instance.GetEquippedId(startEqType.Value);
                else if (startSmithType != null) itemId = BlacksmithManager.Instance.GetSlotItemId(startSmithType.Value);

                if (itemId != 0 && startSmithType != targetSmith.slotType) {
                    if (targetSmith.slotType == SmithSlotType.Input) BlacksmithManager.Instance.SetInputItem(itemId, startSlotIndex, startEqType);
                    else BlacksmithManager.Instance.SetMaterialItem(itemId, startSlotIndex, startEqType);
                }
            }
            
            // 4. If dropped on nothing/invalid but was equipment, try to unequip
            if (targetBackpack == null && targetEq == null && targetSmith == null && startEqType != null)
            {
                InventoryManager.Instance.UnequipToFirstFree(startEqType.Value);
            }
        }
        else if (startEqType != null)
        {
            // Dropped on absolute nothing
            InventoryManager.Instance.UnequipToFirstFree(startEqType.Value);
        }

        transform.SetParent(startParent);
        rectTransform.localPosition = startPosition;
        InventoryManager.Instance.RefreshInventory();
    }
}