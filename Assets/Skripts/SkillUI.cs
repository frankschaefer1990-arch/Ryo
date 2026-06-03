using UnityEngine;
using TMPro;
using System.Linq;

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

            SortSlots();
        }
    }

    private void SortSlots()
    {
        if (slots == null || slots.Length == 0) return;

        // Sort by level (descending)
        var sortedSlots = slots
            .Where(s => s != null)
            .OrderByDescending(s => s.skill != null ? SkillManager.Instance.GetSkillLevel(s.skill) : -1)
            .ToList();

        // Reorder in hierarchy if they have the same parent
        for (int i = 0; i < sortedSlots.Count; i++)
        {
            sortedSlots[i].transform.SetSiblingIndex(i);
        }
    }
}
