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
            int count = InventoryManager.Instance.GetPotionCount();
            if (itemButtonText != null) itemButtonText.text = count + "x Heiltrank";
        }
    }

    public void HideAllSubPanels()
    {
        if (attackPanel != null) attackPanel.SetActive(false);
        if (spellPanel != null) spellPanel.SetActive(false);
        if (itemPanel != null) itemPanel.SetActive(false);
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
            Button btn = attackPanel.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.UseSkill(manager.wildeSchlaege));
            }
        }

        // Spell Panel
        if (spellPanel != null)
        {
            Button btn = spellPanel.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.UseSkill(manager.blitzstrahl));
            }
        }

        // Item Panel
        if (itemPanel != null)
        {
            Button btn = itemPanel.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.UsePotionInBattle());
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

    public void ShowComboPrompt(string keyName)
    {
        if (qteText != null) qteText.text = keyName;
    }

    public void HideComboPrompt()
    {
        if (qteText != null) qteText.text = "";
    }

    public void ToggleCommandPanel(bool show)
    {
        if (commandPanel != null) commandPanel.SetActive(show);
        if (!show) HideAllSubPanels();
    }
}
