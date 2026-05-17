using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public BattleSkill skill;
    public Image skillIcon;
    public TextMeshProUGUI levelText;
    public GameObject lockOverlay;

    [Header("Inspector Info")]
    [TextArea(3, 10)] public string skillDescriptionPreview;

    private Button button;

    private void OnValidate()
    {
        if (skill != null)
        {
            string summary = "";
            if (skill.isCurseUnlocker) summary = "Unlock";
            else if (skill.isPassiveCurse) summary = "Passive";
            else if (skill.isSpell) summary = skill.manaCost + " MP (+" + (PlayerStats.Instance != null ? PlayerStats.Instance.defense * 2 : 0) + " Int-Dmg)";
            else summary = "x" + skill.damageMultiplier + " Dmg (+" + (PlayerStats.Instance != null ? PlayerStats.Instance.strength : 0) + " Str-Dmg)";

            // Get a short snippet of the description
            string descSnippet = skill.description;
            if (descSnippet.Length > 25) descSnippet = descSnippet.Substring(0, 22) + "...";
            
            if (string.IsNullOrEmpty(descSnippet)) descSnippet = "Keine Beschreibung";

            gameObject.name = string.Format("[{0}] {1} | {2} | {3}", skill.category, skill.skillName, summary, descSnippet);
            
            // Preview the full description in the inspector
            skillDescriptionPreview = skill.description;
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnSlotClicked);
    }

    public void Setup(BattleSkill skillData)
    {
        skill = skillData;
        Refresh();
    }

    public void Refresh()
    {
        UnityEngine.UI.Image bg = GetComponent<UnityEngine.UI.Image>();

        // Don't hide the slot anymore, just clear visuals if no skill is assigned
        if (skill == null) 
        {
            if (skillIcon != null) {
                skillIcon.sprite = null;
                skillIcon.color = new Color(0, 0, 0, 0); // Hide icon
            }
            if (levelText != null) levelText.text = "";
            if (lockOverlay != null) lockOverlay.SetActive(false);
            
            // Empty slot: very dark and transparent
            if (bg != null) bg.color = new Color(0.05f, 0.05f, 0.05f, 0.4f);
            
            return;
        }

        gameObject.SetActive(true);
        int level = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(skill) : 0;

        if (skillIcon != null) 
        {
            skillIcon.sprite = skill.icon;
            
            // Icon color: Full color if learned, dark grayscale if not
            if (level > 0) {
                skillIcon.color = Color.white; // Full color
            } else {
                skillIcon.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark/De-saturated
            }
        }

        // Background color based on status
        if (bg != null) {
            if (level > 0) {
                bg.color = new Color(0, 0, 0, 0.1f); // Almost clear so icon pops
            } else {
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // Dark gray
            }
        }

        if (levelText != null) levelText.text = level > 0 ? $"Lvl {level}" : "";

        bool canUpgrade = SkillManager.Instance != null && SkillManager.Instance.CanLearnOrUpgrade(skill);
        int playerLevel = PlayerStats.Instance != null ? PlayerStats.Instance.level : 1;
        bool isUnlocked = level > 0 || canUpgrade || (skill.prerequisiteSkill == null && playerLevel >= skill.levelRequirement);
        
        if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
    }

    public void OnSlotClicked()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.LearnOrUpgrade(skill);
            Refresh();
        }
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && skill != null)
        {
            int level = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(skill) : 0;
            string info = skill.GetTooltipInfo(level);
            
            // Add requirement warning if level too low
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < skill.levelRequirement)
            {
                info += $"\n<color=#FF0000>Benötigt Stufe {skill.levelRequirement}</color>";
            }

            TooltipManager.Instance.ShowTooltip(info);
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}
