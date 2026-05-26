using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string saveName;
    // Stats
    public int level;
    public int currentXP;
    public int xpToNextLevel;
    public int attributePoints;
    public int strength, vitality, defense, agility;
    public int currentHealth, currentMana;
    public bool isCurseSystemUnlocked;
    public int curseValue;
    
    // Gold
    public int gold;
    
    // Inventory
    public bool[] inventorySlots; // Legacy compatibility
    public int[] inventoryItemTypes; // New type storage
    
    // Quests
    public bool introSeen;
    public bool visitedTemple;
    public bool defeatedTempleBoss;
    public bool finishedTempleSequence;
    public bool labyrinthDialogueSeen;
    public bool masterHouseMessageSeen;

    // Krypta Quests
    public bool kryptaIntroSeen;
    public bool zombie1Defeated;
    public bool zombie2Defeated;
    public bool kryptaBossDefeated;
    public bool defeatedKryptaBossReturn;
    
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
    private float currentSessionTime;
    private float loadedPlayTime;
    private int currentSlot = 0;

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
    }

    private string GetSavePath(int slot)
    {
        return Application.persistentDataPath + "/savegame_" + slot + ".json";
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

    public void Save(int slot, string customName)
    {
        if (PlayerStats.Instance == null) return;

        SaveData data = new SaveData();
        data.saveName = string.IsNullOrEmpty(customName) ? "Spielstand " + slot : customName;
        
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
        if (InventoryManager.Instance != null) {
            data.inventorySlots = InventoryManager.Instance.GetSlotData();
            data.inventoryItemTypes = InventoryManager.Instance.GetSlotItemTypes();
        }

        // Quests
        if (QuestManager.Instance != null)
        {
            data.introSeen = QuestManager.Instance.introSeen;
            data.visitedTemple = QuestManager.Instance.visitedTemple;
            data.defeatedTempleBoss = QuestManager.Instance.defeatedTempleBoss;
            data.finishedTempleSequence = QuestManager.Instance.finishedTempleSequence;
            data.labyrinthDialogueSeen = QuestManager.Instance.labyrinthDialogueSeen;
            data.masterHouseMessageSeen = QuestManager.Instance.masterHouseMessageSeen;

            data.kryptaIntroSeen = QuestManager.Instance.kryptaIntroSeen;
            data.zombie1Defeated = QuestManager.Instance.zombie1Defeated;
            data.zombie2Defeated = QuestManager.Instance.zombie2Defeated;
            data.kryptaBossDefeated = QuestManager.Instance.kryptaBossDefeated;
            data.defeatedKryptaBossReturn = QuestManager.Instance.defeatedKryptaBossReturn;
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
        File.WriteAllText(GetSavePath(slot), json);
        currentSlot = slot;
        Debug.Log("SaveSystem: Spiel in Slot " + slot + " gespeichert. Name: " + data.saveName);
    }

    public void Load(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("SaveSystem: Kein Spielstand in Slot " + slot + " gefunden.");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        loadedPlayTime = data.playTimeSeconds;
        currentSessionTime = 0;
        currentSlot = slot;

        // Apply quest data immediately
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetQuestData(
                data.introSeen, 
                data.visitedTemple, 
                data.defeatedTempleBoss, 
                data.finishedTempleSequence, 
                data.labyrinthDialogueSeen, 
                data.masterHouseMessageSeen,
                data.kryptaIntroSeen,
                data.zombie1Defeated,
                data.zombie2Defeated,
                data.kryptaBossDefeated
            );
            // Manually set flags that aren't in SetQuestData if needed
            QuestManager.Instance.defeatedKryptaBossReturn = data.defeatedKryptaBossReturn;
        }

        // Prepare GameManager for save load to skip default spawn points
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isLoadingSave = true;
            GameManager.Instance.LoadScene(data.sceneName);
            StartCoroutine(ApplyDataAfterLoad(data));
        }
        else
        {
            SceneManager.LoadScene(data.sceneName);
        }
    }

    private System.Collections.IEnumerator ApplyDataAfterLoad(SaveData data)
    {
        // Wait for the scene to actually be the new scene
        while (SceneManager.GetActiveScene().name != data.sceneName) yield return null;
        
        // Wait a tiny bit for the player object to be processed by GameManager
        yield return new WaitForSeconds(0.1f);

        // 1. Position FIRST (before we become visible/unlocked)
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            GameManager.Instance.player.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            
            // Sync Camera immediately
            CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
            if (follow != null)
            {
                follow.player = GameManager.Instance.player.transform;
                follow.transform.position = new Vector3(data.posX, data.posY, follow.transform.position.z);
                follow.UpdateBounds();
            }
        }

        // 2. Stats & Attributes
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetStats(data.level, data.currentXP, data.attributePoints, data.strength, data.vitality, data.defense, data.agility, data.isCurseSystemUnlocked, data.curseValue);
            PlayerStats.Instance.RestoreHPAndMana(data.currentHealth, data.currentMana);
        }
        
        // 3. Gold & Inventory
        if (PlayerGold.Instance != null) PlayerGold.Instance.SetGold(data.gold);
        if (InventoryManager.Instance != null) {
            if (data.inventoryItemTypes != null && data.inventoryItemTypes.Length > 0) {
                InventoryManager.Instance.SetSlotData(data.inventoryItemTypes);
            } else if (data.inventorySlots != null) {
                // Legacy fallback: convert bool[] to int[] (1 for health potion, 0 for empty)
                int[] legacyTypes = new int[data.inventorySlots.Length];
                for(int i=0; i<data.inventorySlots.Length; i++) legacyTypes[i] = data.inventorySlots[i] ? 1 : 0;
                InventoryManager.Instance.SetSlotData(legacyTypes);
            }
        }

        // 4. Skills
        if (SkillManager.Instance != null) SkillManager.Instance.LoadSkillData(data.learnedSkillIds, data.learnedSkillLevels, data.skillPoints);
        
        // 5. Cleanup
        if (GameManager.Instance != null) GameManager.Instance.isLoadingSave = false;

        // 6. Force Unlock EVERYTHING
        if (DialogueUI.Instance != null) DialogueUI.Instance.HideAll();
        
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            var pm = GameManager.Instance.player.GetComponent<PlayerMovement>();
            if (pm != null) {
                pm.canMove = true;
                pm.ResetMovementState();
            }
        }
        
        if (MyUIManager.Instance != null) {
            MyUIManager.Instance.isLocked = false;
            MyUIManager.Instance.CloseAllPanels();
        }

        Debug.Log("SaveSystem: Load sequence complete. Movement & UI unlocked.");
    }

    public void Save() => Save(currentSlot, "Automatischer Save");
    public void Load() => Load(currentSlot);

    public void ResetForNewGame()
    {
        loadedPlayTime = 0;
        currentSessionTime = 0;
        
        if (PlayerStats.Instance != null) PlayerStats.Instance.SetStats(1, 0, 0, 1, 1, 1, 1, false, 0);
        if (PlayerGold.Instance != null) PlayerGold.Instance.SetGold(10); 
        if (InventoryManager.Instance != null) InventoryManager.Instance.SetSlotData(new int[10]);
        if (QuestManager.Instance != null) QuestManager.Instance.SetQuestData(false, false, false, false, false, false);
        if (SkillManager.Instance != null) SkillManager.Instance.LoadSkillData(new List<string>(), new List<int>(), 0);
    }

    private string FormatTime(float seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours + t.Days * 24, t.Minutes, t.Seconds);
    }

    public bool HasSave(int slot) => File.Exists(GetSavePath(slot));
    public bool HasSave() => HasSave(currentSlot);
    
    public string GetSaveInfo(int slot)
    {
        if (!HasSave(slot)) return "Kein Spielstand";
        string json = File.ReadAllText(GetSavePath(slot));
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return $"{data.saveName}\nLevel {data.level} - {FormatTime(data.playTimeSeconds)}\nGespeichert: {data.saveTime}";
    }

    public string GetSaveInfo() => GetSaveInfo(currentSlot);
}