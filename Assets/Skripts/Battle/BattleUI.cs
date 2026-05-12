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

    [Header("Smooth Animation")]
    public float lerpSpeed = 2f;
    public float targetPlayerHP = 1f;
    public float targetEnemyHP = 1f;
    public float targetPlayerMana = 1f;

    [Header("GameOver")]
    public GameObject gameOverPanel;

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