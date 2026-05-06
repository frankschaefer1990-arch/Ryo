using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    [Header("HP Bars")]
    public Image playerHPFill;
    public TMP_Text playerHPText;
    public Image enemyHPFill;
    public TMP_Text enemyHPText;

    [Header("Enemy Info")]
    public TMP_Text enemyNameText;

    [Header("QTE")]
    public TMP_Text qteText;
    public TMP_Text qteFixedRing; // Changed from Image to TMP_Text for hollow '○'
    public RectTransform qteShrinkingRing; 
    public Color qteDefaultColor = new Color(1, 1, 1, 0.4f);
    public Color qteSuccessColor = Color.green;
    public Color qteFailColor = Color.red;

    [Header("Skill Display")]
    public GameObject battleInfoPanel; // Renamed from skillInfoPanel
    public TMP_Text skillNameText;

    [Header("Panels")]
    public GameObject commandPanel;
    public GameObject attackPanel;
    public GameObject spellPanel;
    public GameObject itemPanel;
    
    [Header("Texts")]
    public TMP_Text itemButtonText; // To show potion count
    public TMP_Text spellButtonText; // Optional
    public TMP_Text attackButtonText; // Optional

    private void Awake()
    {
        Instance = this;
    }

    public void SetEnemyName(string name)
    {
        if (enemyNameText != null) enemyNameText.text = name;
    }

    public void ShowAttackPanel()
    {
        HideAllSubPanels();
        if (attackPanel != null) attackPanel.SetActive(true);
    }

    public void ShowSpellPanel()
    {
        HideAllSubPanels();
        if (spellPanel != null) spellPanel.SetActive(true);
    }

    public void ShowItemPanel()
    {
        HideAllSubPanels();
        if (itemPanel != null)
        {
            itemPanel.SetActive(true);
            int count = 0;
            if (InventoryManager.Instance != null)
            {
                count = InventoryManager.Instance.GetPotionCount();
            }
            if (itemButtonText != null) itemButtonText.text = count + "x Heiltrank";
        }
    }

    public void HideAllSubPanels()
    {
        if (attackPanel != null) attackPanel.SetActive(false);
        if (spellPanel != null) spellPanel.SetActive(false);
        if (itemPanel != null) itemPanel.SetActive(false);
        
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    public void HideItemPanel()
    {
        if (itemPanel != null) itemPanel.SetActive(false);
    }

    public void SetupSubButtons(BattleManager manager)
    {
        // Attack Panel
        if (attackPanel != null)
        {
            Button[] buttons = attackPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(() => manager.UseSkill(manager.wildeSchlaege));
                buttons[0].interactable = true;
            }
        }

        // Spell Panel
        if (spellPanel != null)
        {
            Button[] buttons = spellPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(() => manager.UseSkill(manager.blitzstrahl));
                buttons[0].interactable = true;
            }
        }

        // Item Panel
        if (itemPanel != null)
        {
            Button[] buttons = itemPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                buttons[0].onClick.RemoveAllListeners();
                buttons[0].onClick.AddListener(() => manager.UsePotionInBattle());
                buttons[0].interactable = true;
            }
        }
    }

    public void UpdatePlayerHP(float ratio, int current, int max)
    {
        if (playerHPFill != null) playerHPFill.fillAmount = ratio;
        if (playerHPText != null) playerHPText.text = current + " / " + max;
    }

    public void UpdateEnemyHP(float ratio, int current, int max)
    {
        if (enemyHPFill != null) enemyHPFill.fillAmount = ratio;
        if (enemyHPText != null) enemyHPText.text = current + " / " + max;
    }

    public void ShowSkillName(string name)
    {
        if (battleInfoPanel != null)
        {
            battleInfoPanel.SetActive(true);
            if (skillNameText != null) 
            {
                skillNameText.text = name;
                // Force a bit of medieval style if not already set
                skillNameText.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    private void Start()
    {
        HideAllSubPanels();
        HideComboPrompt();
        HideActionMessage();
        HideSkillName();
    }

    public void ShowActionMessage(string speaker, string action)
    {
        if (battleInfoPanel != null)
        {
            battleInfoPanel.SetActive(true);
            if (skillNameText != null) 
            {
                // Format: Speaker in Gold, rest in White
                skillNameText.text = "<color=#FFD700>" + speaker + "</color> " + action;
                skillNameText.alignment = TextAlignmentOptions.Left;
                skillNameText.enableWordWrapping = false; // Keep it on one line
            }
        }
    }

    // Overload for backward compatibility
    public void ShowActionMessage(string fullMessage)
    {
        ShowActionMessage("", fullMessage);
    }

    public void HideActionMessage()
    {
        if (battleInfoPanel != null) battleInfoPanel.SetActive(false);
    }

    public void HideSkillName()
    {
        if (battleInfoPanel != null) battleInfoPanel.SetActive(false);
    }

    public void ShowComboPrompt(string keyName)
    {
        if (qteText != null) 
        {
            qteText.gameObject.SetActive(true);
            qteText.text = keyName;
        }
        if (qteFixedRing != null)
        {
            qteFixedRing.gameObject.SetActive(true);
            qteFixedRing.color = qteDefaultColor;
        }
        if (qteShrinkingRing != null)
        {
            qteShrinkingRing.gameObject.SetActive(true);
            qteShrinkingRing.localScale = Vector3.one * 3.0f; // Start 3x larger
        }
    }

    public void HideComboPrompt()
    {
        if (qteText != null) 
        {
            qteText.text = "";
            qteText.gameObject.SetActive(false);
        }
        if (qteFixedRing != null) qteFixedRing.gameObject.SetActive(false);
        if (qteShrinkingRing != null) qteShrinkingRing.gameObject.SetActive(false);
    }

    public void SetQTEFeedback(bool success)
    {
        if (qteFixedRing != null)
        {
            qteFixedRing.gameObject.SetActive(true);
            qteFixedRing.color = success ? qteSuccessColor : qteFailColor;
        }
        if (qteShrinkingRing != null) qteShrinkingRing.gameObject.SetActive(false);
    }

    public void ToggleCommandPanel(bool show)
    {
        if (show)
        {
            HideAllSubPanels();
            if (commandPanel != null) commandPanel.SetActive(true);
        }
        else
        {
            if (commandPanel != null) commandPanel.SetActive(false);
            HideAllSubPanels();
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        }
        
        // Ensure buttons are clickable
        if (commandPanel != null)
        {
            Button[] buttons = commandPanel.GetComponentsInChildren<Button>();
            foreach(var b in buttons) b.interactable = show;
        }
    }
}
