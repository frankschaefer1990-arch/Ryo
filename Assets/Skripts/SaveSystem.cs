using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    // Stats
    public int level;
    public int currentXP;
    public int attributePoints;
    public int strength, vitality, defense, agility;
    public int currentHealth, currentMana;
    public bool isCurseSystemUnlocked;
    public int curseValue;
    
    // Gold
    public int gold;
    
    // Inventory
    public bool[] inventorySlots;
    
    // Quests
    public bool introSeen;
    public bool visitedTemple;
    public bool defeatedTempleBoss;
    public bool finishedTempleSequence;
    
    // Skills
    public int skillPoints;
    public List<string> learnedSkillIds = new List<string>();
    public List<int> learnedSkillLevels = new List<int>();
    
    // Scene & Position
    public string sceneName;
    public float posX, posY, posZ;
    
    // Metadata
    public string saveTime;
    public float playTimeSeconds;
    }

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    private string savePath;
    private float currentSessionTime;
    private float loadedPlayTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        savePath = Application.persistentDataPath + "/savegame.json";
    }

    private void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene != "MainMenu" && scene != "SplashScreen")
        {
            currentSessionTime += Time.deltaTime;
        }
    }

    public float GetTotalPlayTime() => loadedPlayTime + currentSessionTime;

    public void Save()
    {
        if (PlayerStats.Instance == null) return;

        SaveData data = new SaveData();
        
        // Stats
        data.level = PlayerStats.Instance.level;
        data.currentXP = PlayerStats.Instance.currentXP;
        data.attributePoints = PlayerStats.Instance.attributePoints;
        data.strength = PlayerStats.Instance.strength;
        data.vitality = PlayerStats.Instance.vitality;
        data.defense = PlayerStats.Instance.defense;
        data.agility = PlayerStats.Instance.agility;
        data.currentHealth = PlayerStats.Instance.currentHealth;
        data.currentMana = PlayerStats.Instance.currentMana;
        data.isCurseSystemUnlocked = PlayerStats.Instance.isCurseSystemUnlocked;
        data.curseValue = PlayerStats.Instance.curseValue;
        
        // Gold
        if (PlayerGold.Instance != null) data.gold = PlayerGold.Instance.currentGold;
        
        // Inventory
        if (InventoryManager.Instance != null) data.inventorySlots = InventoryManager.Instance.GetSlotData();
        
        // Quests
        if (QuestManager.Instance != null)
        {
            data.introSeen = QuestManager.Instance.introSeen;
            data.visitedTemple = QuestManager.Instance.visitedTemple;
            data.defeatedTempleBoss = QuestManager.Instance.defeatedTempleBoss;
            data.finishedTempleSequence = QuestManager.Instance.finishedTempleSequence;
        }
        
        // Skills
        if (SkillManager.Instance != null)
        {
            data.skillPoints = SkillManager.Instance.skillPoints;
            var levels = SkillManager.Instance.GetSkillLevels();
            foreach (var kvp in levels)
            {
                data.learnedSkillIds.Add(kvp.Key);
                data.learnedSkillLevels.Add(kvp.Value);
            }
        }
        
        // Scene & Pos
        data.sceneName = SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            Vector3 p = GameManager.Instance.player.transform.position;
            data.posX = p.x; data.posY = p.y; data.posZ = p.z;
        }
        
        data.saveTime = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        data.playTimeSeconds = GetTotalPlayTime();

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("SaveSystem: Spiel gespeichert. Spielzeit: " + FormatTime(data.playTimeSeconds));
        }

    public void Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("SaveSystem: Kein Spielstand gefunden.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        loadedPlayTime = data.playTimeSeconds;
        currentSessionTime = 0;

        // Load Scene first
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(data.sceneName);
            StartCoroutine(ApplyDataAfterSceneLoad(data));
        }
        else
        {
            SceneManager.LoadScene(data.sceneName);
        }
    }

    public void ResetForNewGame()
    {
        loadedPlayTime = 0;
        currentSessionTime = 0;
        
        if (PlayerStats.Instance != null) PlayerStats.Instance.SetStats(1, 0, 0, 1, 1, 1, 1, false, 0);
        if (PlayerGold.Instance != null) PlayerGold.Instance.SetGold(10); // Start with 10 Gold
        if (InventoryManager.Instance != null) InventoryManager.Instance.SetSlotData(new bool[10]);
        if (QuestManager.Instance != null) QuestManager.Instance.SetQuestData(false, false, false, false);
        if (SkillManager.Instance != null) SkillManager.Instance.LoadSkillData(new List<string>(), new List<int>(), 0);
    }

    private string FormatTime(float seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours + t.Days * 24, t.Minutes, t.Seconds);
    }

    private System.Collections.IEnumerator ApplyDataAfterSceneLoad(SaveData data)
    {
        // Wait for GameManager to finish setup
        yield return new WaitForSeconds(0.2f);
        
        // Stats
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetStats(data.level, data.currentXP, data.attributePoints, data.strength, data.vitality, data.defense, data.agility, data.isCurseSystemUnlocked, data.curseValue);
            PlayerStats.Instance.RestoreHPAndMana(data.currentHealth, data.currentMana);
        }
        
        // Gold
        if (PlayerGold.Instance != null) PlayerGold.Instance.SetGold(data.gold);
        
        // Inventory
        if (InventoryManager.Instance != null && data.inventorySlots != null) InventoryManager.Instance.SetSlotData(data.inventorySlots);
        
        // Quests
        if (QuestManager.Instance != null) QuestManager.Instance.SetQuestData(data.introSeen, data.visitedTemple, data.defeatedTempleBoss, data.finishedTempleSequence);
        
        // Skills
        if (SkillManager.Instance != null) SkillManager.Instance.LoadSkillData(data.learnedSkillIds, data.learnedSkillLevels, data.skillPoints);
        
        // Position
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            GameManager.Instance.player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
        }
        
        Debug.Log("SaveSystem: Spielstand erfolgreich geladen.");
    }

    public bool HasSave() => File.Exists(savePath);
    
    public string GetSaveInfo()
    {
        if (!HasSave()) return "Kein Spielstand";
        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return $"Level {data.level} - {FormatTime(data.playTimeSeconds)}\nGespeichert: {data.saveTime}";
    }
}