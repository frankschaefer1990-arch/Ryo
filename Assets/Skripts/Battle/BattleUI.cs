using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    [Header("HP Bars")]
    // ... rest of header ...
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
                // Ensure cursor is visible for the menu
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void OnRetryButton()
    {
        // Reset player health and mana before reloading
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            PlayerStats.Instance.currentMana = PlayerStats.Instance.maxMana;
            PlayerStats.Instance.UpdateUI();
        }
        
        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuButton()
    {
        Debug.Log("Spiel wird beendet...");
        Application.Quit();
        // In the editor, this doesn't quit, so we could also load a menu scene if one existed
    }

    private void Awake() { Instance = this; }

    private void Start() 
    { 
        Debug.Log("BattleUI: Start called.");
        ResetAllUI(); 
    }

    private void Update()
    {
        // Use a more robust interpolation that ensures we eventually reach the target
        if (playerHPFill != null) 
        {
            float prev = playerHPFill.fillAmount;
            playerHPFill.fillAmount = Mathf.Lerp(playerHPFill.fillAmount, targetPlayerHP, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(playerHPFill.fillAmount - targetPlayerHP) < 0.001f) playerHPFill.fillAmount = targetPlayerHP;
            
            if (Mathf.Abs(prev - playerHPFill.fillAmount) > 0.0001f && Time.frameCount % 60 == 0)
                Debug.Log($"BattleUI: Animating Player HP {prev:F3} -> {playerHPFill.fillAmount:F3} (Target: {targetPlayerHP:F3})");
        }
        
        if (enemyHPFill != null) 
        {
            float prev = enemyHPFill.fillAmount;
            enemyHPFill.fillAmount = Mathf.Lerp(enemyHPFill.fillAmount, targetEnemyHP, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(enemyHPFill.fillAmount - targetEnemyHP) < 0.001f) enemyHPFill.fillAmount = targetEnemyHP;
            
            if (Mathf.Abs(prev - enemyHPFill.fillAmount) > 0.0001f && Time.frameCount % 60 == 0)
                Debug.Log($"BattleUI: Animating Enemy HP {prev:F3} -> {enemyHPFill.fillAmount:F3} (Target: {targetEnemyHP:F3})");
        }
        
        if (playerManaFill != null) 
        {
            float prev = playerManaFill.fillAmount;
            playerManaFill.fillAmount = Mathf.Lerp(playerManaFill.fillAmount, targetPlayerMana, Time.deltaTime * lerpSpeed);
            if (Mathf.Abs(playerManaFill.fillAmount - targetPlayerMana) < 0.001f) playerManaFill.fillAmount = targetPlayerMana;
            
            if (Mathf.Abs(prev - playerManaFill.fillAmount) > 0.0001f && Time.frameCount % 60 == 0)
                Debug.Log($"BattleUI: Animating Player Mana {prev:F3} -> {playerManaFill.fillAmount:F3} (Target: {targetPlayerMana:F3})");
        }
    }

    public void ResetAllUI()
    {
        Debug.Log("BattleUI: Resetting All UI.");
        if (qteRoot != null) qteRoot.SetActive(false);
        HideAllSubPanels();
        HideActionMessage();
        HideSkillName();
        if (commandPanel != null) 
        { 
            commandPanel.SetActive(true); 
            foreach(var b in commandPanel.GetComponentsInChildren<Button>()) b.interactable = true;
        }

        // Initialize targets and SNAP bars to current player state immediately
        var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            targetPlayerHP = stats.maxHealth > 0 ? (float)stats.currentHealth / stats.maxHealth : 1f;
            targetPlayerMana = stats.maxMana > 0 ? (float)stats.currentMana / stats.maxMana : 1f;
            
            if (playerHPFill != null) playerHPFill.fillAmount = targetPlayerHP;
            if (playerManaFill != null) playerManaFill.fillAmount = targetPlayerMana;
            
            if (playerHPText != null) playerHPText.text = stats.currentHealth + " / " + stats.maxHealth;
            if (playerManaText != null) playerManaText.text = stats.currentMana + " / " + stats.maxMana;
            
            Debug.Log($"BattleUI: Initialized from stats. HP: {targetPlayerHP}, Mana: {targetPlayerMana}");
        }
        else
        {
            Debug.LogWarning("BattleUI: PlayerStats not found during ResetAllUI.");
            if (playerHPFill != null) targetPlayerHP = playerHPFill.fillAmount;
            if (playerManaFill != null) targetPlayerMana = playerManaFill.fillAmount;
        }
        
        if (enemyHPFill != null) targetEnemyHP = enemyHPFill.fillAmount;
    }

    public void SetEnemyName(string name) { 
        if (enemyNameText != null) {
            enemyNameText.gameObject.SetActive(true);
            enemyNameText.text = name; 
            Debug.Log($"BattleUI: Enemy name set to '{name}' on object '{enemyNameText.gameObject.name}'");
        }
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
        if (BattleTooltipManager.Instance != null) BattleTooltipManager.Instance.HideTooltip();
    }

    public void SetupSubButtons(BattleManager manager)
    {
        Debug.Log("BattleUI: Setting up sub buttons dynamically...");

        // Get all learned skills and filter by unique ID to avoid duplicates appearing multiple times
        Dictionary<string, BattleSkill> uniqueLearnedSkills = new Dictionary<string, BattleSkill>();
        
        if (SkillManager.Instance != null && allSkills != null)
        {
            foreach (var s in allSkills)
            {
                if (s == null || string.IsNullOrEmpty(s.skillId)) continue;
                if (SkillManager.Instance.GetSkillLevel(s) > 0)
                {
                    if (!uniqueLearnedSkills.ContainsKey(s.skillId))
                    {
                        uniqueLearnedSkills.Add(s.skillId, s);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("BattleUI: SkillManager or allSkills list is missing!");
            // Fallback: use skills currently in manager if they exist
            if (manager.wildeSchlaege != null) uniqueLearnedSkills[manager.wildeSchlaege.skillId] = manager.wildeSchlaege;
            if (manager.blitzstrahl != null) uniqueLearnedSkills[manager.blitzstrahl.skillId] = manager.blitzstrahl;
        }

        List<BattleSkill> learnedSkillsList = uniqueLearnedSkills.Values.ToList();

        // Sort by learned order from SkillManager
        if (SkillManager.Instance != null && SkillManager.Instance.learnedOrder != null)
        {
            learnedSkillsList = learnedSkillsList.OrderBy(s => {
                int index = SkillManager.Instance.learnedOrder.IndexOf(s.skillId);
                return index >= 0 ? index : int.MaxValue;
            }).ToList();
        }

        // Separate learned skills into Basic/Verflucht (Attack) and Zauber (Spell)
        List<BattleSkill> attacks = learnedSkillsList.Where(s => !s.isSpell).ToList();
        List<BattleSkill> spells = learnedSkillsList.Where(s => s.isSpell).ToList();

        PopulatePanel(attackPanel, attacks, manager);
        PopulatePanel(spellPanel, spells, manager);

        // ... Item Panel ...
    }

    private void PopulatePanel(GameObject panel, List<BattleSkill> skills, BattleManager manager)
    {
        if (panel == null) return;
        
        Transform content = panel.transform.Find("Viewport/Content");
        if (content == null) content = panel.transform;

        // Hide ALL children first to ensure "only learned are visible"
        foreach (Transform child in content)
        {
            child.gameObject.SetActive(false);
        }

        // Get only direct children buttons
        List<Button> buttons = new List<Button>();
        foreach (Transform child in content)
        {
            Button b = child.GetComponent<Button>();
            if (b != null) buttons.Add(b);
        }
        
        Debug.Log($"BattleUI: Populating {panel.name} with {skills.Count} skills. Available buttons: {buttons.Count}");

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < skills.Count)
            {
                // Local copy to avoid closure issues in lambda
                BattleSkill currentSkill = skills[i];
                Debug.Log($"BattleUI: Assigning skill {currentSkill.skillName} to button {i} in {panel.name}");
                buttons[i].gameObject.SetActive(true);
                buttons[i].interactable = true; // Ensure button is interactable
                
                // Update text
                var text = buttons[i].GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = currentSkill.skillName;

                // Update listener
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => {
                    Debug.Log($"BattleUI: Button clicked for {currentSkill.skillName} (ID: {currentSkill.skillId}) in {panel.name}");
                    if (manager != null) manager.UseSkill(currentSkill);
                });

                // Setup Tooltip
                BattleSkillTooltip tooltip = buttons[i].gameObject.GetComponent<BattleSkillTooltip>();
                if (tooltip == null) tooltip = buttons[i].gameObject.AddComponent<BattleSkillTooltip>();
                tooltip.skill = currentSkill;
            }
        }
    }

    public void UpdatePlayerHP(float ratio, int curr, int max) { 
        Debug.Log($"BattleUI: Updating Player HP to {ratio} ({curr}/{max})");
        targetPlayerHP = ratio; 
        if (playerHPFill != null) playerHPFill.fillAmount = ratio; // Direct update
        if (playerHPText != null) {
            playerHPText.text = curr + " / " + max; 
        }
    }

    public void UpdatePlayerMana(float ratio, int curr, int max) { 
        Debug.Log($"BattleUI: Updating Player Mana to {ratio} ({curr}/{max})");
        targetPlayerMana = ratio; 
        if (playerManaFill != null) playerManaFill.fillAmount = ratio; // Direct update
        if (playerManaText != null) {
            playerManaText.text = curr + " / " + max; 
        }
    }

    public void UpdateEnemyHP(float ratio, int curr, int max) { 
        Debug.Log($"BattleUI: Updating Enemy HP to {ratio} ({curr}/{max})");
        targetEnemyHP = ratio; 
        if (enemyHPFill != null) enemyHPFill.fillAmount = ratio; // Direct update
        if (enemyHPText != null) {
            enemyHPText.text = curr + " / " + max; 
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
            if (qteShrinkRing != null) { 
                qteShrinkRing.gameObject.SetActive(true); 
                qteShrinkRing.rectTransform.localScale = Vector3.one * 5.0f; 
                // Preserve existing alpha from the inspector
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
