using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Curse Bar")]
    public GameObject curseBarRoot;
    public Image curseFill;
    public TMP_Text curseValueText;
    public Gradient curseGradient;

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

    [Header("Smooth Animation")]
    public float lerpSpeed = 2f;
    public float targetPlayerHP = 1f;
    public float targetEnemyHP = 1f;
    public float targetPlayerMana = 1f;

    [Header("GameOver")]
    public GameObject gameOverPanel;

    [Header("Skill Assets")]
    public List<BattleSkill> allSkills;

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
            if (show)
            {
                HideAllSubPanels();
                if (commandPanel != null) commandPanel.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void OnRetryButton()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            PlayerStats.Instance.currentMana = PlayerStats.Instance.maxMana;
            PlayerStats.Instance.UpdateUI();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuButton()
    {
        Application.Quit();
    }

    private void Awake() { Instance = this; }

    private void Start() 
    { 
        ResetAllUI(); 
    }

    private void Update()
    {
        if (playerHPFill != null) 
        {
            playerHPFill.fillAmount = Mathf.Lerp(playerHPFill.fillAmount, targetPlayerHP, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(playerHPFill.fillAmount - targetPlayerHP) < 0.001f) playerHPFill.fillAmount = targetPlayerHP;
        }
        
        if (enemyHPFill != null) 
        {
            enemyHPFill.fillAmount = Mathf.Lerp(enemyHPFill.fillAmount, targetEnemyHP, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(enemyHPFill.fillAmount - targetEnemyHP) < 0.001f) enemyHPFill.fillAmount = targetEnemyHP;
        }
        
        if (playerManaFill != null) 
        {
            playerManaFill.fillAmount = Mathf.Lerp(playerManaFill.fillAmount, targetPlayerMana, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(playerManaFill.fillAmount - targetPlayerMana) < 0.001f) playerManaFill.fillAmount = targetPlayerMana;
        }
    }

    public void UpdateCurseBar()
    {
        var stats = PlayerStats.Instance;
        if (stats == null) return;

        if (curseBarRoot != null)
            curseBarRoot.SetActive(stats.isCurseSystemUnlocked);

        if (curseFill != null)
        {
            float ratio = (float)stats.curseValue / stats.maxCurseValue;
            curseFill.fillAmount = ratio;
            
            if (curseGradient != null)
            {
                curseFill.color = curseGradient.Evaluate(ratio);
            }
            else
            {
                if (ratio < 0.5f)
                    curseFill.color = Color.Lerp(new Color(0.3f, 0f, 0.5f), Color.red, ratio * 2f);
                else
                    curseFill.color = Color.Lerp(Color.red, Color.black, (ratio - 0.5f) * 2f);
            }
        }

        if (curseValueText != null)
            curseValueText.text = stats.curseValue + " / " + stats.maxCurseValue;
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
            foreach(var b in commandPanel.GetComponentsInChildren<Button>()) b.interactable = true;
        }

        var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            targetPlayerHP = stats.maxHealth > 0 ? (float)stats.currentHealth / stats.maxHealth : 1f;
            targetPlayerMana = stats.maxMana > 0 ? (float)stats.currentMana / stats.maxMana : 1f;
            
            if (playerHPFill != null) playerHPFill.fillAmount = targetPlayerHP;
            if (playerManaFill != null) playerManaFill.fillAmount = targetPlayerMana;
            
            if (playerHPText != null) playerHPText.text = stats.currentHealth + " / " + stats.maxHealth;
            if (playerManaText != null) playerManaText.text = stats.currentMana + " / " + stats.maxMana;
            
            UpdateCurseBar();
        }
        
        if (enemyHPFill != null) targetEnemyHP = enemyHPFill.fillAmount;
    }

    public void SetEnemyName(string name) { 
        if (enemyNameText != null) {
            enemyNameText.gameObject.SetActive(true);
            enemyNameText.text = name; 
        }
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
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.HideTooltip();
    }

    public void SetupSubButtons(BattleManager manager)
    {
        Dictionary<string, BattleSkill> uniqueLearnedSkills = new Dictionary<string, BattleSkill>();
        
        if (SkillManager.Instance != null && allSkills != null)
        {
            foreach (var s in allSkills)
            {
                if (s == null || string.IsNullOrEmpty(s.skillId)) continue;
                if (SkillManager.Instance.GetSkillLevel(s) > 0 && !s.isPassiveCurse)
                {
                    if (!uniqueLearnedSkills.ContainsKey(s.skillId))
                    {
                        uniqueLearnedSkills.Add(s.skillId, s);
                    }
                }
            }
        }

        List<BattleSkill> learnedSkillsList = uniqueLearnedSkills.Values.ToList();

        if (SkillManager.Instance != null && SkillManager.Instance.learnedOrder != null)
        {
            learnedSkillsList = learnedSkillsList.OrderBy(s => {
                int index = SkillManager.Instance.learnedOrder.IndexOf(s.skillId);
                return index >= 0 ? index : int.MaxValue;
            }).ToList();
        }

        List<BattleSkill> attacks = learnedSkillsList.Where(s => !s.isSpell).ToList();
        List<BattleSkill> spells = learnedSkillsList.Where(s => s.isSpell).ToList();

        PopulatePanel(attackPanel, attacks, manager);
        PopulatePanel(spellPanel, spells, manager);
    }

    private void PopulatePanel(GameObject panel, List<BattleSkill> skills, BattleManager manager)
    {
        if (panel == null) return;
        Transform content = panel.transform.Find("Viewport/Content");
        if (content == null) content = panel.transform;

        foreach (Transform child in content) child.gameObject.SetActive(false);

        List<Button> buttons = new List<Button>();
        foreach (Transform child in content)
        {
            Button b = child.GetComponent<Button>();
            if (b != null) buttons.Add(b);
        }
        
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < skills.Count)
            {
                BattleSkill currentSkill = skills[i];
                buttons[i].gameObject.SetActive(true);
                buttons[i].interactable = true;
                
                var text = buttons[i].GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = currentSkill.skillName;

                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => {
                    if (manager != null) manager.UseSkill(currentSkill);
                });

                BattleSkillTooltip tooltip = buttons[i].gameObject.GetComponent<BattleSkillTooltip>();
                if (tooltip == null) tooltip = buttons[i].gameObject.AddComponent<BattleSkillTooltip>();
                tooltip.skill = currentSkill;
            }
        }
    }

    public void UpdatePlayerHP(float ratio, int curr, int max) { 
        targetPlayerHP = ratio; 
        if (playerHPFill != null) playerHPFill.fillAmount = ratio; 
        if (playerHPText != null) playerHPText.text = curr + " / " + max; 
    }

    public void UpdatePlayerMana(float ratio, int curr, int max) { 
        targetPlayerMana = ratio; 
        if (playerManaFill != null) playerManaFill.fillAmount = ratio; 
        if (playerManaText != null) playerManaText.text = curr + " / " + max; 
        UpdateCurseBar();
    }

    public void UpdateEnemyHP(float ratio, int curr, int max) { 
        targetEnemyHP = ratio; 
        if (enemyHPFill != null) enemyHPFill.fillAmount = ratio; 
        if (enemyHPText != null) enemyHPText.text = curr + " / " + max; 
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
            if (qteShrinkRing != null) { 
                qteShrinkRing.gameObject.SetActive(true); 
                qteShrinkRing.rectTransform.localScale = Vector3.one * 5.0f; 
                Color c = qteShrinkRing.color;
                c.r = 1f; c.g = 1f; c.b = 1f;
                qteShrinkRing.color = c; 
            }
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
