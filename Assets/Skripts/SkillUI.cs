using UnityEngine;
using TMPro;

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI skillPointsText;
    public SkillSlotUI[] slots;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (SkillManager.Instance != null && skillPointsText != null)
        {
            skillPointsText.text = SkillManager.Instance.skillPoints.ToString();
        }

        // Refresh all slots regardless of skill presence
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot != null) slot.Refresh();
            }
        }
    }
}
