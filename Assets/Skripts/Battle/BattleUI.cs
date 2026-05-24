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
    public Image enemyManaFill;
    public TMP_Text enemyManaText;

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
    public TMP_Text qteFeedbackText; // New

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
    public TMP_Text manaItemButtonText;

    public Color qteSuccessColor = Color.green;
public Color qteFailColor = Color.red;

    [Header("Smooth Animation")]
    public float lerpSpeed = 2f;
    public float targetPlayerHP = 1f;
    public float targetEnemyHP = 1f;
    public float targetPlayerMana = 1f;
    public float targetEnemyMana = 1f;

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

        // Safety: Unparent player before reloading scene to ensure they survive
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            GameManager.Instance.player.transform.SetParent(null);
            DontDestroyOnLoad(GameManager.Instance.player);
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

        if (enemyManaFill != null)
        {
            enemyManaFill.fillAmount = Mathf.Lerp(enemyManaFill.fillAmount, targetEnemyMana, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(enemyManaFill.fillAmount - targetEnemyMana) < 0.001f) enemyManaFill.fillAmount = targetEnemyMana;
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
                // Always Purple: Dark Purple at 0, Glowing/Bright Purple at 1
                Color darkPurple = new Color(0.15f, 0f, 0.25f); // Very dark purple
                Color brightPurple = new Color(0.8f, 0f, 1f);   // Bright, glowing purple
                curseFill.color = Color.Lerp(darkPurple, brightPurple, ratio);
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
        if (enemyManaFill != null) targetEnemyMana = enemyManaFill.fillAmount;
        }

        public void UpdateEnemyMana(float ratio, int curr, int max)
        {
            targetEnemyMana = ratio;
            if (enemyManaFill != null) enemyManaFill.fillAmount = ratio;
        
            if (enemyManaText != null)
            {
                if (max <= 0) enemyManaText.text = ""; 
                else enemyManaText.text = curr + " / " + max;
            }
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
            int healthCount = 0;
            if (InventoryManager.Instance != null) healthCount = InventoryManager.Instance.GetPotionCount();
            
            int manaCount = 0;
            if (InventoryManager.Instance != null) manaCount = InventoryManager.Instance.GetItemCount(2);

            // Wire the buttons
            Button[] buttons = itemPanel.GetComponentsInChildren<Button>(true);
            
            // Try by name first, then by index
            Button healthBtn = buttons.FirstOrDefault(b => b.name.Contains("Health") || b.name == "PotionButton" || b.name.Contains("Heil"));
            Button manaBtn = buttons.FirstOrDefault(b => b.name.Contains("Mana"));

            if (healthBtn == null && buttons.Length > 0) healthBtn = buttons[0];
            if (manaBtn == null && buttons.Length > 1) manaBtn = buttons[1];

            if (healthBtn != null)
            {
                healthBtn.onClick.RemoveAllListeners();
                healthBtn.onClick.AddListener(OnPotionButton);
                healthBtn.interactable = healthCount > 0;
                healthBtn.gameObject.SetActive(true);
                var t = healthBtn.GetComponentInChildren<TMP_Text>();
                if (t != null) t.text = healthCount + "x Heiltrank";
            }

            if (manaBtn != null)
            {
                manaBtn.onClick.RemoveAllListeners();
                manaBtn.onClick.AddListener(OnManaPotionButton);
                manaBtn.interactable = manaCount > 0;
                manaBtn.gameObject.SetActive(true);
                var t = manaBtn.GetComponentInChildren<TMP_Text>();
                if (t != null) t.text = manaCount + "x Manatrank";
            }

            // Hide extra buttons
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != healthBtn && buttons[i] != manaBtn) buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnPotionButton()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UsePotionInBattle();
            RefreshItemCounts();
        }
    }

    public void OnManaPotionButton()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UseManaPotionInBattle();
            RefreshItemCounts();
        }
    }

    private void RefreshItemCounts()
    {
        if (InventoryManager.Instance != null)
        {
            int count = InventoryManager.Instance.GetPotionCount();
            if (itemButtonText != null) itemButtonText.text = count + "x Heiltrank";
            
            int manaCount = InventoryManager.Instance.GetItemCount(2);
            if (manaItemButtonText != null) manaItemButtonText.text = manaCount + "x Manatrank";
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
                if (SkillManager.Instance.GetSkillLevel(s) > 0 && !s.isPassiveCurse && !s.isCurseUnlocker)
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

        // Collect existing buttons
        List<Button> buttons = new List<Button>();
        foreach (Transform child in content)
        {
            Button b = child.GetComponent<Button>();
            if (b != null)
            {
                buttons.Add(b);
                b.gameObject.SetActive(false); // Hide initially
            }
        }
        
        // If we don't have enough buttons, instantiate more using the first one as a template
        if (buttons.Count > 0 && buttons.Count < skills.Count)
        {
            Button template = buttons[0];
            int startCount = buttons.Count;
            int needed = skills.Count - startCount;
            for (int i = 0; i < needed; i++)
            {
                Button newButton = Instantiate(template, content);
                newButton.name = "Button_" + (startCount + i);
                newButton.gameObject.SetActive(false);
                buttons.Add(newButton);
            }
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
            else
            {
                buttons[i].gameObject.SetActive(false);
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

    public void ShowQTEFeedbackText(QTEResult result)
    {
        if (qteFeedbackText == null) return;

        if (result == QTEResult.FAIL)
        {
            qteFeedbackText.gameObject.SetActive(false);
            return;
        }

        qteFeedbackText.gameObject.SetActive(true);
        if (result == QTEResult.PERFECT)
        {
            qteFeedbackText.text = "Perfekt";
            qteFeedbackText.color = new Color(0f, 1f, 0f, 0.6f); // Semi-transparent Green
        }
        else if (result == QTEResult.SUCCESS)
        {
            qteFeedbackText.text = "Gut";
            qteFeedbackText.color = new Color(1f, 0.6f, 0f, 0.6f); // Semi-transparent Orange
        }

        // Hide after a short duration
        CancelInvoke(nameof(HideQTEFeedbackText));
        Invoke(nameof(HideQTEFeedbackText), 1.0f);
    }

    private void HideQTEFeedbackText()
    {
        if (qteFeedbackText != null) qteFeedbackText.gameObject.SetActive(false);
    }
    }
