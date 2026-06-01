using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string content;

    public Vector2 pivotOverride = new Vector2(1f, 0f); // Default to Top-Left

    private bool isHovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (string.IsNullOrEmpty(content)) return;
        
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(content, pivotOverride);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    public void RefreshTooltipIfHovered()
    {
        if (isHovering && !string.IsNullOrEmpty(content) && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(content, pivotOverride);
        }
    }

    private void OnDisable()
    {
        isHovering = false;
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
