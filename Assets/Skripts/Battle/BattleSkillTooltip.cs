using UnityEngine;
using UnityEngine.EventSystems;

public class BattleSkillTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BattleSkill skill;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && skill != null)
        {
            int level = 1;
            if (SkillManager.Instance != null) level = SkillManager.Instance.GetSkillLevel(skill);
            if (level < 1) level = 1;
            
            TooltipManager.Instance.ShowTooltip(skill.GetTooltipInfo(level));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }
}
