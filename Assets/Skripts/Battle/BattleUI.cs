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

    [Header("Mana Bar")]
    public Image playerManaFill;
    public TMP_Text playerManaText;

    [Header("Enemy Info")]
    public TMP_Text enemyNameText;

    [Header("QTE (Legend of Dragoon Style)")]
    public GameObject qteRoot;
    public TMP_Text qteKeyText;
    public Image qteOuterRing;
    public Image qteShrinkRing;
    public Image qteButtonCore;
    public QTERingController qteController;

    [Header("Skill Display")]
    public GameObject battleInfoPanel;
    public TMP_Text skillNameText;

    [Header("Panels")]
    public GameObject commandPanel;
    public GameObject attackPanel;
    public GameObject spellPanel;
    public GameObject itemPanel;
    
    [Header("Texts")]
    public TMP_Text itemButtonText;

    public Color qteSuccessColor = Color.green;
    public Color qteFailColor = Color.red;

    private void Awake() { Instance = this; }

    private void Start() 
    { 
        ResetAllUI(); 
    }

    public void ResetAllUI()
    {
        if (qteRoot != null) qteRoot.SetActive(false);
        HideAllSubPanels();
        HideActionMessage();
        HideSkillName();
        if (commandPanel != null) 
        { 
            commandPanel.SetActive(true); 
            // Ensure interactable is true
            foreach(var b in commandPanel.GetComponentsInChildren<Button>()) b.interactable = true;
        }
    }

    public void SetEnemyName(string name) { 
        if (enemyNameText != null) enemyNameText.text = name; 
        else Debug.LogWarning("BattleUI: enemyNameText is not assigned!");
    }

    public void ShowAttackPanel() { HideAllSubPanels(); if (attackPanel != null) attackPanel.SetActive(true); }
    public void ShowSpellPanel() { HideAllSubPanels(); if (spellPanel != null) spellPanel.SetActive(true); }
    public void ShowItemPanel()
    {
        HideAllSubPanels();
        if (itemPanel != null) {
            itemPanel.SetActive(true);
            int count = 0;
            if (InventoryManager.Instance != null) count = InventoryManager.Instance.GetPotionCount();
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

    public void SetupSubButtons(BattleManager manager)
    {
        Debug.Log("BattleUI: Setting up sub buttons...");
        if (attackPanel != null) {
            Button[] buttons = attackPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) {
                var text = b.GetComponentInChildren<TMP_Text>();
                if (text != null && text.text.Contains("Wilde Schläge")) {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        Debug.Log("BattleUI: Wilde Schläge button clicked!");
                        manager.UseSkill(manager.wildeSchlaege);
                    });
                    Debug.Log("BattleUI: Linked Wilde Schläge button.");
                }
            }
        }
        if (spellPanel != null) {
            Button[] buttons = spellPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) {
                var text = b.GetComponentInChildren<TMP_Text>();
                if (text != null && text.text.Contains("Blitzstrahl")) {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        Debug.Log("BattleUI: Blitzstrahl button clicked!");
                        manager.UseSkill(manager.blitzstrahl);
                    });
                    Debug.Log("BattleUI: Linked Blitzstrahl button.");
                }
            }
        }
        if (itemPanel != null) {
            Button[] buttons = itemPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) {
                var text = b.GetComponentInChildren<TMP_Text>();
                if (text != null && text.text.Contains("Heiltrank")) {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(() => {
                        Debug.Log("BattleUI: Potion button clicked!");
                        manager.UsePotionInBattle();
                    });
                    Debug.Log("BattleUI: Linked Potion button.");
                }
            }
        }
    }

    public void UpdatePlayerHP(float ratio, int curr, int max) { 
        if (playerHPFill != null) playerHPFill.fillAmount = ratio; 
        if (playerHPText != null) {
            playerHPText.text = curr + " / " + max; 
            Debug.Log($"BattleUI: UpdatePlayerHP text to {playerHPText.text}");
        }
    }

    public void UpdatePlayerMana(float ratio, int curr, int max) { 
        if (playerManaFill != null) playerManaFill.fillAmount = ratio; 
        if (playerManaText != null) {
            playerManaText.text = curr + " / " + max; 
        }
    }

    public void UpdateEnemyHP(float ratio, int curr, int max) { 
        if (enemyHPFill != null) enemyHPFill.fillAmount = ratio; 
        if (enemyHPText != null) {
            enemyHPText.text = curr + " / " + max; 
            Debug.Log($"BattleUI: UpdateEnemyHP text to {enemyHPText.text}");
        }
    }

    public void ShowSkillName(string name) { if (battleInfoPanel != null) { battleInfoPanel.SetActive(true); if (skillNameText != null) { skillNameText.text = name; skillNameText.alignment = TextAlignmentOptions.Center; } } }
    public void ShowActionMessage(string speaker, string action) { if (battleInfoPanel != null) { battleInfoPanel.SetActive(true); if (skillNameText != null) { skillNameText.text = "<color=#FFD700>" + speaker + "</color> " + action; skillNameText.alignment = TextAlignmentOptions.Left; } } }
    public void ShowActionMessage(string msg) { ShowActionMessage("", msg); }
    public void HideActionMessage() { if (battleInfoPanel != null) battleInfoPanel.SetActive(false); }
    public void HideSkillName() { if (battleInfoPanel != null) battleInfoPanel.SetActive(false); }

    public void ShowComboPrompt(string key)
    {
        if (qteRoot != null) {
            qteRoot.SetActive(true);
            if (qteKeyText != null) qteKeyText.text = key;
            if (qteShrinkRing != null) { qteShrinkRing.gameObject.SetActive(true); qteShrinkRing.rectTransform.localScale = Vector3.one * 5.0f; qteShrinkRing.color = Color.white; }
            if (qteOuterRing != null) qteOuterRing.color = Color.white;
            if (qteButtonCore != null) qteButtonCore.color = Color.white;
        }
    }

    public void HideComboPrompt() { if (qteRoot != null) qteRoot.SetActive(false); }

    public void SetQTEFeedback(bool success)
    {
        Color c = success ? qteSuccessColor : qteFailColor;
        if (qteOuterRing != null) qteOuterRing.color = c;
        if (qteButtonCore != null) qteButtonCore.color = c;
        if (qteShrinkRing != null) qteShrinkRing.gameObject.SetActive(false);
    }

    public void ToggleCommandPanel(bool show)
    {
        if (show) { HideAllSubPanels(); if (commandPanel != null) commandPanel.SetActive(true); }
        else { if (commandPanel != null) commandPanel.SetActive(false); HideAllSubPanels(); }
        if (commandPanel != null) { foreach(var b in commandPanel.GetComponentsInChildren<Button>()) b.interactable = show; }
    }
}