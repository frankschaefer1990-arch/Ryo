using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EchoPanelUI : MonoBehaviour
{
    public static EchoPanelUI Instance;

    public GameObject panel;
    public Image monsterImage;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI statsText;
    
    [Header("Level Selection")]
    public Transform levelButtonContainer;
    public GameObject levelButtonTemplate;
    public TMP_FontAsset medievalFont;
    
    public Button fightButton;
public Button cancelButton;

    private EchoStatue currentStatue;
    private int selectedLevel = 1;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (levelButtonTemplate != null) levelButtonTemplate.SetActive(false);
    }

    private void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    public void Open(EchoStatue statue)
    {
        Debug.Log($"EchoPanelUI: Opening for {statue.bossName}. Panel ref: {panel != null}");
        currentStatue = statue;
        
        if (goldText != null && PlayerGold.Instance != null)
        {
            goldText.text = PlayerGold.Instance.currentGold.ToString();
        }

        if (panel != null) panel.SetActive(true);
        
        if (MyUIManager.Instance != null)
        {
            MyUIManager.Instance.isLocked = true;
            // Ensure cursor is visible for the UI
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (monsterImage != null && statue.bossData != null)
            monsterImage.sprite = statue.bossData.enemySprite;

        // Default to highest unlocked level
        if (QuestManager.Instance != null && statue.slotIndex >= 0 && statue.slotIndex < QuestManager.Instance.echoLevels.Length)
        {
            selectedLevel = QuestManager.Instance.echoLevels[statue.slotIndex];
        }
        else
        {
            selectedLevel = 1;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentStatue == null || levelButtonContainer == null || levelButtonTemplate == null) return;

        // Clear existing buttons (except template)
        foreach (Transform child in levelButtonContainer)
        {
            if (child.gameObject != levelButtonTemplate)
                Destroy(child.gameObject);
        }

        int maxLevel = 1;
        if (QuestManager.Instance != null && currentStatue.slotIndex >= 0 && currentStatue.slotIndex < QuestManager.Instance.echoLevels.Length)
        {
            maxLevel = QuestManager.Instance.echoLevels[currentStatue.slotIndex];
        }

        // Normalize Name for display
        string displayName = currentStatue.bossName;
        if (displayName == "Skelettkrieger") displayName = "Skelett Krieger";

        for (int i = maxLevel; i >= 1; i--)
        {
            int level = i;
            GameObject btnObj = Instantiate(levelButtonTemplate, levelButtonContainer);
            btnObj.SetActive(true);
            btnObj.name = "LevelBtn_" + level; // Store level in name for easy finding
            
            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{displayName} Echo Stufe {level}";
                if (medievalFont != null) text.font = medievalFont;
            }

            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectLevel(level));
            }
        }

        RefreshButtonVisuals();
        ShowStats(selectedLevel);
    }

    public void SelectLevel(int level)
    {
        selectedLevel = level;
        RefreshButtonVisuals();
        ShowStats(selectedLevel);
    }

    private void RefreshButtonVisuals()
    {
        if (levelButtonContainer == null) return;
        
        foreach (Transform child in levelButtonContainer)
        {
            if (child.gameObject == levelButtonTemplate) continue;
            
            Button btn = child.GetComponent<Button>();
            if (btn == null) continue;
            
            // Extract level from name or component
            int btnLevel = 1;
            if (child.name.StartsWith("LevelBtn_")) 
                int.TryParse(child.name.Replace("LevelBtn_", ""), out btnLevel);

            var colors = btn.colors;
            if (btnLevel == selectedLevel)
            {
                // Selected: Brighter/More opaque milky white
                colors.normalColor = new Color(1f, 1f, 1f, 0.8f);
                colors.selectedColor = new Color(1f, 1f, 1f, 0.8f);
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
                colors.pressedColor = new Color(1f, 1f, 1f, 1.0f);
            }
            else
            {
                // Unselected: Fainter milky white
                colors.normalColor = new Color(1f, 1f, 1f, 0.3f);
                colors.selectedColor = new Color(1f, 1f, 1f, 0.3f);
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.6f);
            }
            btn.colors = colors;
}
    }

    private void ShowStats(int level)
    {
        if (currentStatue == null || currentStatue.bossData == null || statsText == null) return;

        float multiplier = 1f + (level - 1) * 1.0f; // Increased from 0.2 for better scaling challenge
        int hp = Mathf.RoundToInt(currentStatue.bossData.maxHP * multiplier);
        int mana = Mathf.RoundToInt(currentStatue.bossData.maxMana * multiplier);
        int atk = Mathf.RoundToInt(currentStatue.bossData.attack * multiplier);
        int def = Mathf.RoundToInt(currentStatue.bossData.defense * multiplier);
        
        int xpReward = Mathf.RoundToInt(currentStatue.bossData.xpReward * multiplier);
        int goldReward = Mathf.RoundToInt(50 * multiplier);

        statsText.text = $"HP: {hp}\nMana: {mana}\nATK: {atk}\nDEF: {def}\n\n" +
                         $"<color=#FFD700>Belohnung: {xpReward} XP und {goldReward} Gold</color>";
    }

    public void OnFight()
    {
        if (currentStatue == null || currentStatue.bossData == null) return;

        int level = selectedLevel;
        float multiplier = 1f + (level - 1) * 1.0f; // Increased from 0.2 for better scaling challenge

        EnemyData scaledEnemy = Instantiate(currentStatue.bossData);
        scaledEnemy.maxHP = Mathf.RoundToInt(currentStatue.bossData.maxHP * multiplier);
        scaledEnemy.startHP = scaledEnemy.maxHP; // Force full HP for Echoes
        scaledEnemy.maxMana = Mathf.RoundToInt(currentStatue.bossData.maxMana * multiplier);
        scaledEnemy.startMana = scaledEnemy.maxMana;
        scaledEnemy.attack = Mathf.RoundToInt(scaledEnemy.attack * multiplier);
        scaledEnemy.defense = Mathf.RoundToInt(scaledEnemy.defense * multiplier);
        scaledEnemy.xpReward = Mathf.RoundToInt(currentStatue.bossData.xpReward * multiplier);
        scaledEnemy.goldReward = Mathf.RoundToInt(50 * multiplier);
        
        scaledEnemy.enemyName = currentStatue.bossData.enemyName + " (Echo Lvl " + level + ")";

        if (QuestManager.Instance != null && GameManager.Instance != null)
        {
            QuestManager.Instance.nextBattleEnemy = scaledEnemy;
            
            // Store which echo we are fighting to increase level on win
            // We can use PlayerPrefs or a static var in QuestManager
            PlayerPrefs.SetInt("ActiveEchoSlot", currentStatue.slotIndex);
            PlayerPrefs.SetInt("ActiveEchoLevel", level);
            
            GameManager.Instance.LoadScene("BattleScene");
        }

        Close();
    }

    public void OnCancel()
    {
        Close();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
    }
}
