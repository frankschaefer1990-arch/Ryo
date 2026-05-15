using UnityEngine;

public class TooltipTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string content;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.ShowTooltip(content);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.HideTooltip();
    }

    private void OnDestroy()
    {
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.HideTooltip();
    }
}