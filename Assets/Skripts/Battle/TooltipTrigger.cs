using UnityEngine;

public class TooltipTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string content;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.ShowTooltip(content);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDestroy()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }
}