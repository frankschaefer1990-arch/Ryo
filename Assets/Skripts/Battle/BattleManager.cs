using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BattleState { START, PLAYERTURN, BUSY, WON, LOST }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Data")]
    public EnemyData currentEnemy;
    public BattleSkill wildeSchlaege;
    public BattleSkill blitzstrahl;

    [Header("References")]
    public Transform playerPos;
    public Transform enemyPos;
    public ProceduralSlash slashEffect;
    public ProceduralSlash lightningEffect;
    public AudioSource audioSource;
    public TMPro.TMP_FontAsset damageFont;

    [Header("New Visuals")]
    public GameObject[] comboStrikeObjects;
    public GameObject blitzAnimationObject;
    public GameObject enemyAttackVisual;
    public GameObject playerAura; // New: Aura for Curse Form 1
    public Sprite shinigamiSprite; // New: Sprite for Curse Form 3
    public Vector3 shinigamiOffset = new Vector3(0, -0.5f, 0); // Offset to lower the sprite
    public Sprite shinigamiSlashSprite; // New: Special slash for Shinigami
    public AudioClip shinigamiSlashSound; // New: Ghostly slash sound
    public GameObject blockVisual; // New: Shield visual
    public AudioClip blockSound; // New: Block sound effect
    private Sprite humanSprite; // Store original
    private Vector3 originalRendererLocalPos;
    private Vector3 originalPlayerWorldPos;
    private bool playerActionTakenInTurn = false;
    private bool isBlocking = false;
    private int shinigamiTurnsLeft = 0;

    public BattleState state;
    private int enemyCurrentHP;
    private bool enemyIsStunned = false;

    // New: Debuff and DoT tracking
    private int enemyDefenseMod = 0;
    private int enemyDefenseTimer = 0;
    private float enemyDmgTakenMult = 1f;
    private int enemyDmgTakenTimer = 0;
    private int activeDotDamage = 0;
    private int activeDotTurns = 0;
    private float enemyHealReductionMult = 1f;
    private int enemyHealReductionTimer = 0;

    // New: Basic Skill tracking
    private int rageStacks = 0;
    private float qteSpeedMult = 1.0f;
    private int activeBleedDamage = 0;
    private int activeBleedTurns = 0;

    [Header("Audio")]
    public AudioClip battleMusic;

    private void Awake()
    {
        Instance = this;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        // Hide ALL Labyrinth Colliders if present
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("ColliderPainter"))
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) r.enabled = false;

                var tilemap = obj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (tilemap != null)
                {
                    Color c = tilemap.color;
                    c.a = 0f;
                    tilemap.color = c;
                }
            }
        }

        Debug.Log("BattleManager: Start called. Initializing visuals...");
        state = BattleState.START;
        
        // Immediately fetch enemy data to avoid visual lag/glitch
        if (QuestManager.Instance != null && QuestManager.Instance.nextBattleEnemy != null)
        {
            currentEnemy = QuestManager.Instance.nextBattleEnemy;
        }

        if (currentEnemy != null && enemyPos != null)
        {
            enemyPos.gameObject.SetActive(true);
            SpriteRenderer esr = enemyPos.GetComponentInChildren<SpriteRenderer>();
            if (esr != null)
            {
                esr.enabled = true;
                if (currentEnemy.enemySprite != null)
                {
                    esr.sprite = currentEnemy.enemySprite;
                    Debug.Log($"BattleManager (Start): Applied sprite {currentEnemy.enemySprite.name} to {currentEnemy.enemyName}");
                }
                else
                {
                    Debug.LogWarning($"BattleManager (Start): Enemy {currentEnemy.enemyName} has no sprite assigned!");
                }
            }
        }

        // IMMEDIATELY HIDE PERSISTENT PLAYER
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            var renderers = GameManager.Instance.player.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = false;
            
            // Move out of view just in case
            GameManager.Instance.player.transform.position = new Vector3(-1000, -1000, 0);
        }

        if (playerPos != null)
        {
            originalPlayerWorldPos = playerPos.position;
            SpriteRenderer sr = playerPos.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) 
            {
                humanSprite = sr.sprite;
                originalRendererLocalPos = sr.transform.localPosition;
            }
        }
if (playerAura != null) playerAura.SetActive(false);
        if (blockVisual != null) blockVisual.SetActive(false);

        if (slashEffect == null) slashEffect = GetComponent<ProceduralSlash>();
        if (lightningEffect == null) lightningEffect = GetComponent<ProceduralSlash>();
        
        if (slashEffect == null)
        {
            slashEffect = gameObject.AddComponent<ProceduralSlash>();
            if (lightningEffect == null) lightningEffect = slashEffect;
        }

        if (blitzAnimationObject != null) blitzAnimationObject.SetActive(false);
        if (enemyAttackVisual != null) enemyAttackVisual.SetActive(false);

        if (slashEffect != null && slashEffect.gameObject != gameObject) slashEffect.gameObject.SetActive(false);
        if (lightningEffect != null && lightningEffect.gameObject != gameObject) lightningEffect.gameObject.SetActive(false);

        if (comboStrikeObjects != null)
        {
            foreach (var obj in comboStrikeObjects)
            {
                if (obj != null && obj != gameObject) obj.SetActive(false);
            }
        }

        Invoke(nameof(StartSetup), 0.5f);
    }

    private void StartSetup() { StartCoroutine(SetupBattle()); }

    private IEnumerator ShowEffectBriefly(GameObject effect, float duration)
    {
        if (effect == null || effect == gameObject) yield break;
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    private IEnumerator SetupBattle()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllMusic();
        yield return null;

        // Reset Curse at start of every battle
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.curseValue = 0;
            shinigamiTurnsLeft = 0;
            Debug.Log("BattleManager: Curse reset to 0 for new battle.");
        }

        // Double check persistent player is HIDDEN but ACTIVE for stats
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            var renderers = GameManager.Instance.player.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = false;
            
            GameManager.Instance.player.transform.SetParent(null);
            DontDestroyOnLoad(GameManager.Instance.player);
            GameManager.Instance.player.transform.position = new Vector3(-1000, -1000, 0);
        }

        // Initialize battle visuals using the scene's marker (PlayerBattlePos)
        if (playerPos != null)
        {
            SpriteRenderer sr = playerPos.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) 
            {
                humanSprite = sr.sprite;
                sr.enabled = true;
                sr.color = Color.white;
            }
            Debug.Log($"BattleManager: Using scene visual '{playerPos.name}' for battle representation.");
        }

        try {
            if (QuestManager.Instance != null && QuestManager.Instance.nextBattleEnemy != null)
            {
                currentEnemy = QuestManager.Instance.nextBattleEnemy;
                QuestManager.Instance.nextBattleEnemy = null;
            }

            if (currentEnemy == null) yield break;

            if (enemyPos != null)
            {
                SpriteRenderer esr = enemyPos.GetComponentInChildren<SpriteRenderer>();
                if (esr != null && currentEnemy.enemySprite != null)
                {
                    esr.sprite = currentEnemy.enemySprite;
                }
            }

            enemyCurrentHP = currentEnemy.startHP > 0 ? currentEnemy.startHP : currentEnemy.maxHP;
            
            if (BattleUI.Instance != null) {
                var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
                BattleUI.Instance.SetEnemyName(currentEnemy.enemyName);
                float enemyRatio = currentEnemy.maxHP > 0 ? (float)enemyCurrentHP / currentEnemy.maxHP : 1f;
                BattleUI.Instance.UpdateEnemyHP(enemyRatio, enemyCurrentHP, currentEnemy.maxHP);
                
                if (stats != null)
                {
                    stats.RecalculateStats();
                    BattleUI.Instance.UpdatePlayerHP((float)stats.currentHealth / stats.maxHealth, stats.currentHealth, stats.maxHealth);
                    BattleUI.Instance.UpdatePlayerMana((float)stats.currentMana / stats.maxMana, stats.currentMana, stats.maxMana);
                    ApplyCurseVisuals();
                }
                
                BattleUI.Instance.ToggleCommandPanel(true);
                BattleUI.Instance.SetupSubButtons(this);
            }

            if (audioSource != null && battleMusic != null)
            {
                audioSource.clip = battleMusic;
                audioSource.loop = true;
                audioSource.Play();
            }
            
            state = BattleState.PLAYERTURN; 
            if (DialogueUI.Instance != null) DialogueUI.Instance.ShowMessage(currentEnemy.enemyName, "erscheint!");
        }
        catch (System.Exception e) { Debug.LogError("BattleManager ERROR: " + e.Message); }
        
        yield return new WaitForSeconds(1.2f);
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        isBlocking = false;
        if (blockVisual != null) blockVisual.SetActive(false);
        
        // Handle Shinigami Duration
        if (shinigamiTurnsLeft > 0)
        {
            shinigamiTurnsLeft--;
            Debug.Log($"Shinigami Form: {shinigamiTurnsLeft} turns remaining.");
            
            if (shinigamiTurnsLeft <= 0)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.curseValue = 0; // Reset to 0 after 3 rounds
                    Debug.Log("Shinigami Form ended. Curse reset to 0.");
                }
                ApplyCurseVisuals();
            }
        }
        else if (PlayerStats.Instance != null && PlayerStats.Instance.curseValue >= 100)
        {
            // If we just reached 100, set the turns
            shinigamiTurnsLeft = 3;
            Debug.Log("Shinigami Form aktiviert für 3 Züge!");
        }

        // Reset playerPos to original world position to prevent drift from animations
        if (playerPos != null) playerPos.position = originalPlayerWorldPos;

        // Decrement timers
if (enemyDefenseTimer > 0) { enemyDefenseTimer--; if (enemyDefenseTimer == 0) enemyDefenseMod = 0; }
        if (enemyDmgTakenTimer > 0) { enemyDmgTakenTimer--; if (enemyDmgTakenTimer == 0) enemyDmgTakenMult = 1f; }
        if (enemyHealReductionTimer > 0) { enemyHealReductionTimer--; if (enemyHealReductionTimer == 0) enemyHealReductionMult = 1f; }

        if (PlayerStats.Instance != null)
{
            // Always decay at start of turn
            int decay = PlayerStats.Instance.HasCursePassive(10) ? -5 : -15; // Slightly faster decay to ensure it ends
            if (shinigamiTurnsLeft > 0 || PlayerStats.Instance.GetCurseForm() == 3) decay -= 5; // Extra decay in Shinigami form
            
            PlayerStats.Instance.ChangeCurseValue(decay);
            ApplyCurseVisuals();

            playerActionTakenInTurn = false;
            
            int baseManaRegen = 5;
            int intBonus = PlayerStats.Instance.defense;
            PlayerStats.Instance.RestoreMana(baseManaRegen + intBonus);
            
            BattleUI.Instance.UpdatePlayerMana((float)PlayerStats.Instance.currentMana / PlayerStats.Instance.maxMana, PlayerStats.Instance.currentMana, PlayerStats.Instance.maxMana);
            }

        BattleUI.Instance.ToggleCommandPanel(true);
    }

    public void OnAttackButton() { if (state == BattleState.PLAYERTURN) BattleUI.Instance.ShowAttackPanel(); }
    public void OnSpellButton() { if (state == BattleState.PLAYERTURN) BattleUI.Instance.ShowSpellPanel(); }
    public void OnItemButton() { if (state == BattleState.PLAYERTURN) BattleUI.Instance.ShowItemPanel(); }

    public void OnBlockButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        
        state = BattleState.BUSY;
        isBlocking = true;
        playerActionTakenInTurn = true;
        
        if (blockVisual != null) blockVisual.SetActive(true);
        if (audioSource != null && blockSound != null) audioSource.PlayOneShot(blockSound);

        BattleUI.Instance.ToggleCommandPanel(false);
        ShowBattleMessage("Ryo geht in Verteidigungshaltung!");
        StartCoroutine(EnemyTurnAfterDelay(1.2f));
    }

    public void UseSkill(BattleSkill skill)
    {
        if (skill == null || state != BattleState.PLAYERTURN) return;

        int level = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(skill) : 1;
        if (level < 1) level = 1;

        int manaCost = skill.GetManaCost(level);
        if (skill.isSpell && PlayerStats.Instance != null && PlayerStats.Instance.currentMana < manaCost)
        {
            ShowBattleMessage("Nicht genug Mana!");
            return;
        }

        playerActionTakenInTurn = true;
        BattleUI.Instance.ToggleCommandPanel(false);
        StartCoroutine(ExecuteSkill(skill, level));
    }

    public void OnRunButton() { if (state == BattleState.PLAYERTURN) StartCoroutine(TryRun()); }

    private IEnumerator TryRun()
    {
        state = BattleState.BUSY;
        BattleUI.Instance.ToggleCommandPanel(false);
        ShowBattleMessage("Ryo versucht zu flüchten...");
        yield return new WaitForSeconds(1f);

        if (Random.value > 0.5f)
        {
            ShowBattleMessage("Flucht erfolgreich!");
            yield return new WaitForSeconds(1.5f);
            
            // Fix: If escaping from temple boss, allow re-triggering the sequence
            if (currentEnemy != null && currentEnemy.isBoss && QuestManager.Instance != null)
            {
                QuestManager.Instance.visitedTemple = false;
            }

            RestorePlayerVisibility();
            GameManager.Instance.LoadScene("Legend of Ryo"); 
        }
        else
        {
            ShowBattleMessage("Flucht fehlgeschlagen!");
            yield return new WaitForSeconds(1f);
            StartCoroutine(EnemyTurn());
        }
    }

    public void UsePotionInBattle()
    {
        if (state != BattleState.PLAYERTURN) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.GetPotionCount() > 0)
        {
            playerActionTakenInTurn = true;
            BattleUI.Instance.HideAllSubPanels();
            BattleUI.Instance.ToggleCommandPanel(false);
            
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(30);
                InventoryManager.Instance.RemoveOnePotion();
                BattleUI.Instance.UpdatePlayerHP((float)PlayerStats.Instance.currentHealth / PlayerStats.Instance.maxHealth, PlayerStats.Instance.currentHealth, PlayerStats.Instance.maxHealth);
            }
            
            ShowBattleMessage("Ryo verwendet einen Heiltrank!");
            StartCoroutine(EnemyTurnAfterDelay(1.2f));
        }
    }

    public void UseManaPotionInBattle()
    {
        if (state != BattleState.PLAYERTURN) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemCount(2) > 0)
        {
            playerActionTakenInTurn = true;
            BattleUI.Instance.HideAllSubPanels();
            BattleUI.Instance.ToggleCommandPanel(false);
            
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.RestoreMana(30);
                // We need a method to remove mana potion
                RemoveItemFromInventory(2);
                BattleUI.Instance.UpdatePlayerMana((float)PlayerStats.Instance.currentMana / PlayerStats.Instance.maxMana, PlayerStats.Instance.currentMana, PlayerStats.Instance.maxMana);
            }
            
            ShowBattleMessage("Ryo verwendet einen Manatrank!");
            StartCoroutine(EnemyTurnAfterDelay(1.2f));
        }
    }

    private void RemoveItemFromInventory(int type)
    {
        if (InventoryManager.Instance == null) return;
        var types = InventoryManager.Instance.GetSlotItemTypes();
        for (int i = types.Length - 1; i >= 0; i--)
        {
            if (types[i] == type)
            {
                types[i] = 0;
                InventoryManager.Instance.RefreshInventory();
                return;
            }
        }
    }

    private IEnumerator EnemyTurnAfterDelay(float delay)
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(delay);
        StartCoroutine(EnemyTurn());
    }

    private Vector3 GetEffectCenter(Transform target, bool isSpell = false)
    {
        if (target == null) return Vector3.zero;
        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            if (isSpell)
            {
                // Use the SpriteRenderer's position (Pivot) for the Y coordinate.
                // If the sprite pivot is at the feet, this is the ground level.
                // We still use bounds.center.x for the horizontal middle.
                return new Vector3(sr.bounds.center.x, sr.transform.position.y, sr.bounds.center.z);
            }
            return sr.bounds.center;
        }
        return target.position + (isSpell ? Vector3.zero : Vector3.up);
    }

    private IEnumerator ExecuteSkill(BattleSkill skill, int level)
    {
        state = BattleState.BUSY;
        Vector3 effectCenter = GetEffectCenter(enemyPos, skill.isSpell);
        var stats = PlayerStats.Instance;
        int currentForm = stats != null ? stats.GetCurseForm() : 0;
        if (shinigamiTurnsLeft > 0) currentForm = 3;

        if (stats != null && skill.isSpell)
        {
            stats.ChangeCurseValue(5);
            ApplyCurseVisuals();
            stats.UseMana(skill.GetManaCost(level));
            BattleUI.Instance.UpdatePlayerMana((float)stats.currentMana / stats.maxMana, stats.currentMana, stats.maxMana);
        }

        BattleUI.Instance.ShowActionMessage("Ryo", "setzt " + skill.skillName + " ein!");
        yield return new WaitForSeconds(1.0f);
        BattleUI.Instance.HideActionMessage();

        // Reset local QTE speed multiplier for the skill execution
        qteSpeedMult = 1.0f; 

        int actualHitCount = skill.GetHitCount(level);
        int extraHits = 0;

        for (int i = 0; i < actualHitCount + extraHits; i++)
        {
            // CHECK FORM MID-COMBO
            currentForm = stats != null ? stats.GetCurseForm() : 0;
            if (shinigamiTurnsLeft > 0) currentForm = 3;
            
            bool hitSuccess = true;
            QTEResult qteResult = QTEResult.SUCCESS;

            if (skill.hasCombo)
            {
                yield return new WaitForSeconds(0.2f);
                if (ComboSystem.Instance != null)
                {
                    bool waiting = true;
                    float baseLimit = skill.GetQTETimeLimit(level);
                    float hitSpecificLimit = baseLimit * qteSpeedMult;

                    // Rage gets faster with every hit in the combo
                    if (skill.skillId == "rage")
                    {
                        hitSpecificLimit = Mathf.Max(baseLimit * Mathf.Pow(0.85f, i), 0.3f);
                    }
                    else if (skill.skillId == "klingenwirbel")
                    {
                        hitSpecificLimit = Mathf.Max(baseLimit * Mathf.Pow(0.92f, i), 0.4f);
                    }

                    if (skill.skillId == "konterklinge") hitSpecificLimit *= 0.7f;

                    ComboSystem.Instance.StartQTE((result) => {
                        qteResult = result;
                        waiting = false;
                        if (result != QTEResult.FAIL && stats != null) 
                        {
                            stats.ChangeCurseValue(10);
                            ApplyCurseVisuals();
                        }
                    }, hitSpecificLimit);

                    while (waiting) yield return null;
                    hitSuccess = qteResult != QTEResult.FAIL;
                }
            }

            if (hitSuccess)
            {
                // Wilde Schläge: Perfect = +1 extra hit
                if (skill.skillId == "wilde_schlaege" && qteResult == QTEResult.PERFECT && extraHits < 3)
                {
                    extraHits++;
                    ShowBattleMessage("Extrahieb!");
                }

                // Klingenwirbel: increases hits
                if (skill.skillId == "klingenwirbel" && qteResult != QTEResult.FAIL && i >= actualHitCount - 1 && extraHits < 4)
                {
                    extraHits++;
                }

                // Dynamic strike animation: diagonal move forward and back
                if (playerPos != null) StartCoroutine(PlayStrikeAnimation(playerPos));

                // Stun Logic
                float stunChance = skill.GetStunChance(level);
                if (skill.skillId == "schaedelbrecher")
                {
                    stunChance = 0.25f + (level - 1) * 0.03f;
                    if (qteResult == QTEResult.PERFECT) stunChance = 0.50f + (level - 1) * 0.03f;
                }
                
                // Geisterzwang (Skill 5): +5% stun chance if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(5))
                    stunChance += 0.05f;
                
                // Seelenlast (Skill 7): +10% stun chance if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(7))
                    stunChance += 0.10f;

                if (Random.value <= stunChance) enemyIsStunned = true;

                AudioClip hitSound = (currentForm == 3 && skill.category == SkillCategory.Basic && shinigamiSlashSound != null) ? shinigamiSlashSound : skill.skillSound;
                if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

                int playerStrength = stats != null ? stats.strength : 1;
                int playerIntelligence = stats != null ? stats.defense : 1;
                int playerCurse = stats != null ? stats.agility : 1;
                
                int customBase = skill.GetBaseDamage(playerStrength, playerIntelligence, playerCurse);
                int baseDamage = customBase > 0 ? customBase : (skill.hasCombo ? 15 : 30); 
                float totalMultiplier = skill.GetDamageMultiplier(level);
                
                if (skill.skillId == "rage")
                {
                    if (qteResult != QTEResult.FAIL)
                    {
                        rageStacks = Mathf.Min(rageStacks + 1, 5);
                    }
                    // Damage increases significantly with stacks
                    totalMultiplier *= (1.0f + (rageStacks * (0.10f + (level - 1) * 0.02f)));
                }

                if (skill.skillId == "zornspalter")
                {
                    totalMultiplier *= (1.0f + (rageStacks * 0.15f)); // Higher bonus for consumption
                    if (qteResult != QTEResult.PERFECT) rageStacks = 0;
                }

                // Abgrundruf (Skill 8): +25% bonus dmg to spells/curse if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(8))
                {
                    if (skill.category != SkillCategory.Basic) 
                    {
                        if (customBase > 0) baseDamage = (int)(baseDamage * 1.25f);
                    }
                }

                // Todeshauch (Skill 11): +50% total damage in Shinigami form
                if (stats != null && currentForm == 3 && stats.HasCursePassive(11))
                {
                    totalMultiplier *= 1.5f;
                }

                // Mechanics
                float critChance = 0f;
                if (skill.skillId == "schattenklinge")
                {
                    critChance = 0.3f + (level - 1) * 0.05f;
                    if (qteResult == QTEResult.PERFECT) critChance = 1.0f; // Perfect Schattenklinge = Crit
                }
                if (skill.skillId == "schattenhieb") critChance = 0.35f;
                if (skill.skillId == "konterklinge" && qteResult != QTEResult.FAIL) critChance = 1.0f;
                if (skill.skillId == "schattenhieb" && qteResult == QTEResult.PERFECT) critChance = 1.0f;

                float defIgnore = 0f;
                if (skill.skillId == "leerenstoss")
                {
                    defIgnore = (qteResult == QTEResult.PERFECT) ? 1.0f : 0.35f; // Perfect Leerenstoß ignores all def
                }
                if (skill.skillId == "eisenbrecher") defIgnore = (qteResult == QTEResult.PERFECT) ? 0.5f : 0.3f;

                if (skill.skillId == "hollow_judgment" && (float)enemyCurrentHP / currentEnemy.maxHP < 0.3f)
                    totalMultiplier *= 1.5f;
                
                if (skill.skillId == "seelenklinge" && qteResult == QTEResult.PERFECT)
                    totalMultiplier *= 1.15f;

                bool isCrit = Random.value < critChance;
                if (isCrit) totalMultiplier *= 2f;

                int bonusDmg = customBase > 0 ? 0 : (skill.category == SkillCategory.Basic ? playerStrength : playerIntelligence * 2);

                int finalEnemyDef = Mathf.Max(0, (int)(currentEnemy.defense * (1.0f - defIgnore)) + enemyDefenseMod);
                int totalDamage = (int)((baseDamage + bonusDmg) * totalMultiplier * enemyDmgTakenMult);
                
                // Minimum damage 1
                totalDamage = Mathf.Max(1, totalDamage - (finalEnemyDef / 2));

                enemyCurrentHP -= totalDamage;
                if (enemyCurrentHP < 0) enemyCurrentHP = 0;

                // Effects
                if (skill.skillId == "donnerspeer")
                {
                    float reduction = (qteResult == QTEResult.PERFECT) ? 0.35f : 0.2f;
                    enemyDefenseMod = -(int)(currentEnemy.defense * reduction);
                    enemyDefenseTimer = 2;
                }
                if (skill.skillId == "aufwaertshieb")
                {
                    float red = (qteResult == QTEResult.PERFECT) ? 0.25f : 0.15f;
                    enemyDefenseMod = -(int)(currentEnemy.defense * red);
                    enemyDefenseTimer = 2;
                }
                if (skill.skillId == "seelenbrand")
                {
                    activeDotDamage = (int)(customBase * totalMultiplier);
                    if (qteResult == QTEResult.PERFECT) activeDotDamage = (int)(activeDotDamage * 1.2f);
                    activeDotTurns = 3;
                    enemyHealReductionMult = 0.6f;
                    enemyHealReductionTimer = 3;
                }
                if (skill.skillId == "blutschnitt")
                {
                    activeBleedDamage = (int)(15 + playerStrength * 0.3f);
                    if (qteResult == QTEResult.PERFECT) activeBleedDamage *= 2;
                    activeBleedTurns = 3;
                }
                if (skill.skillId == "blutpakt" && stats != null)
                {
                    int selfDmg = (int)(stats.maxHealth * 0.15f);
                    stats.TakeDamage(selfDmg);
                    BattleUI.Instance.UpdatePlayerHP((float)stats.currentHealth / stats.maxHealth, stats.currentHealth, stats.maxHealth);
                }
                if (skill.skillId == "astralbruch")
                {
                    int burn = 25 + (level - 1) * 5;
                    if (qteResult == QTEResult.PERFECT) burn = (int)(burn * 1.5f);
                    ShowBattleMessage($"{burn} Mana des Gegners verbrannt!");
                }
                if (skill.skillId == "finstermal")
                {
                    float bonus = (qteResult == QTEResult.PERFECT) ? 0.4f : 0.25f;
                    enemyDmgTakenMult = 1.0f + bonus + (level - 1) * 0.05f;
                    enemyDmgTakenTimer = 2;
                }
                if (skill.skillId == "nachtkralle")
                {
                    ShowBattleMessage("Gegner-Vorteile entfernt!");
                }

                if (skill.category == SkillCategory.Basic && stats != null)
                {
                    stats.ChangeCurseValue(10);
                    ApplyCurseVisuals();
                }

                if (skill.skillId == "seelenklinge" && stats != null)
                {
                    float lifeSteal = (qteResult == QTEResult.PERFECT) ? 0.35f : 0.2f;
                    stats.Heal((int)(totalDamage * lifeSteal));
                }

                // Blutzoll (Skill 4): Heal 5% of damage if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(4))
                {
                    int curseHeal = (int)(totalDamage * 0.05f);
                    if (curseHeal > 0) stats.Heal(curseHeal);
                }

                float healMult = skill.GetHealMultiplier(level);
                if (healMult > 0 && stats != null)
                {
                    stats.Heal((int)(totalDamage * healMult));
                    BattleUI.Instance.UpdatePlayerHP((float)stats.currentHealth / stats.maxHealth, stats.currentHealth, stats.maxHealth);
                }

                DamagePopup.Create(enemyPos.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0), totalDamage, damageFont);
                
                // Nachtmahr (Skill 6): Shadow Attack (+20% extra hit)
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(6))
                {
                    int shadowDmg = (int)(totalDamage * 0.2f);
                    if (shadowDmg > 0)
                    {
                        enemyCurrentHP -= shadowDmg;
                        DamagePopup.Create(enemyPos.position + new Vector3(Random.Range(-0.5f, 0.5f), 2.5f, 0), shadowDmg, damageFont, Color.gray);
                    }
                }

                // Visual Overrides for Shinigami
                Sprite slashSpriteToUse = (currentForm == 3 && skill.category == SkillCategory.Basic && shinigamiSlashSprite != null) ? shinigamiSlashSprite : skill.customSlashSprite;
                Color effectColorToUse = skill.effectColor;
                Vector3 scaleOverride = skill.visualScale;

                if (currentForm == 3 && skill.isSpell)
                {
                    effectColorToUse = new Color(0.6f, 0f, 1f, 1f); // Vibrant Purple
                    scaleOverride *= 1.3f;
                }

                ProceduralSlash effectToUse = skill.isSpell ? lightningEffect : slashEffect;
                
                if (slashSpriteToUse != null)
                {
                    if (skill.hasCombo && comboStrikeObjects != null && comboStrikeObjects.Length > 0)
                    {
                        GameObject strikeObj = comboStrikeObjects[i % comboStrikeObjects.Length];
                        if (strikeObj != null)
                        {
                            StartCoroutine(ShowEffectBriefly(strikeObj, Mathf.Max(0.5f, skill.slashDuration + 0.1f)));
                            ProceduralSlash ps = strikeObj.GetComponent<ProceduralSlash>();
                            if (ps != null) effectToUse = ps;
                        }
                    }
                    if (effectToUse != null) effectToUse.PlaySlash(effectCenter, effectColorToUse, slashSpriteToUse, skill.slashDuration, skill.visualOffset, scaleOverride, skill.randomRotation);
                }
                else if (skill.isSpell && blitzAnimationObject != null) StartCoroutine(ShowEffectBriefly(blitzAnimationObject, 0.3f));
                else if (effectToUse != null)
                {
                    effectToUse.PlaySlash(effectCenter, effectColorToUse, slashSpriteToUse, skill.slashDuration, skill.visualOffset, scaleOverride, skill.randomRotation);
                }

                StartCoroutine(PlayHurtAnimation(enemyPos)); 
                BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);
                if (enemyCurrentHP <= 0) break;
            }
            else { ShowBattleMessage("Combo unterbrochen!"); break; }
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1.0f);
        if (enemyCurrentHP <= 0) { state = BattleState.WON; StartCoroutine(EndBattle()); }
        else StartCoroutine(EnemyTurn());
    }

    private IEnumerator PlayStrikeAnimation(Transform target)
    {
        if (target == null) yield break;
        Vector3 startPos = target.position;
        Vector3 jumpPos = startPos + new Vector3(0.5f, 0.5f, 0);
        
        float duration = 0.08f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.position = Vector3.Lerp(startPos, jumpPos, elapsed / duration);
            yield return null;
        }
        
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.position = Vector3.Lerp(jumpPos, startPos, elapsed / duration);
            yield return null;
        }
        target.position = startPos;
    }

    private IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(1.0f);

        // DoT Logic (Curse)
        if (activeDotTurns > 0)
        {
            enemyCurrentHP -= activeDotDamage;
            DamagePopup.Create(enemyPos.position + Vector3.up * 2f, activeDotDamage, damageFont, new Color(0.6f, 0f, 1f));
            BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);
            activeDotTurns--;
            if (activeDotTurns == 0) activeDotDamage = 0;
            
            if (enemyCurrentHP <= 0)
            {
                state = BattleState.WON;
                StartCoroutine(EndBattle());
                yield break;
            }
            yield return new WaitForSeconds(0.6f);
        }

        // Bleed Logic (Physical)
        if (activeBleedTurns > 0)
        {
            enemyCurrentHP -= activeBleedDamage;
            DamagePopup.Create(enemyPos.position + Vector3.up * 1.5f, activeBleedDamage, damageFont, Color.red);
            BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);
            activeBleedTurns--;
            if (activeBleedTurns == 0) activeBleedDamage = 0;

            if (enemyCurrentHP <= 0)
            {
                state = BattleState.WON;
                StartCoroutine(EndBattle());
                yield break;
            }
            yield return new WaitForSeconds(0.6f);
        }

        if (enemyIsStunned)
{
            enemyIsStunned = false;
            ShowBattleMessage(currentEnemy.enemyName + " ist betäubt!");
            yield return new WaitForSeconds(1.5f);
            PlayerTurn();
            yield break;
        }

        BattleUI.Instance.ShowActionMessage(currentEnemy.enemyName, "greift an!");
        yield return new WaitForSeconds(1.0f);
        BattleUI.Instance.HideActionMessage();

        Vector3 enemyOriginalPos = enemyPos.position;
        enemyPos.position -= new Vector3(0.5f, 0, 0);
        if (enemyAttackVisual != null) StartCoroutine(ShowEffectBriefly(enemyAttackVisual, 0.4f));

        var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.ChangeCurseValue(3);
            ApplyCurseVisuals();

            int damage = currentEnemy.attack;
            if (isBlocking)
            {
                damage = Mathf.Max(damage / 2, 1);
                Debug.Log("BattleManager: Player BLOCKED. Damage reduced.");
            }

            stats.TakeDamage(damage);
            DamagePopup.Create(playerPos.position + Vector3.up, Mathf.Max(damage, 1), damageFont);
            StartCoroutine(PlayHurtAnimation(playerPos)); 
            BattleUI.Instance.UpdatePlayerHP((float)stats.currentHealth / stats.maxHealth, stats.currentHealth, stats.maxHealth);
        }

        yield return new WaitForSeconds(0.5f); 
        enemyPos.position = enemyOriginalPos;
        yield return new WaitForSeconds(0.5f); 

        if (stats != null && stats.currentHealth <= 0) { state = BattleState.LOST; StartCoroutine(EndBattle()); }
        else PlayerTurn();
    }

    private IEnumerator PlayHurtAnimation(Transform target)
    {
        Vector3 originalPos = target.position;
        Vector3 originalScale = target.localScale;
        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            target.position = originalPos + new Vector3(Random.Range(-0.15f, 0.15f), 0, 0);
            target.localScale = originalScale * 1.1f;
            if (sr != null) sr.color = new Color(1, 0.3f, 0.3f, 1);
            yield return new WaitForSeconds(0.04f);
            target.position = originalPos;
            target.localScale = originalScale;
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.04f);
        }
    }

    private IEnumerator EndBattle()
    {
        RestorePlayerVisibility();

        if (state == BattleState.WON)
        {
            if (enemyPos != null) enemyPos.gameObject.SetActive(false);
            
            // Record defeat for per-visit persistence
            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.lastEnemyTriggerID))
            {
                GameManager.Instance.defeatedEnemiesInCurrentScene.Add(GameManager.Instance.lastEnemyTriggerID);
                Debug.Log($"BattleManager: Enemy {GameManager.Instance.lastEnemyTriggerID} recorded as defeated.");
            }

            ShowBattleMessage("Sieg! " + currentEnemy.xpReward + " XP und 50 Gold erhalten.");
            if (currentEnemy.isBoss && QuestManager.Instance != null) QuestManager.Instance.defeatedTempleBoss = true;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.GainXP(currentEnemy.xpReward);
                PlayerGold goldMgr = PlayerGold.GetInstance();
                if (goldMgr != null) goldMgr.AddGold(50);
            }
            yield return new WaitForSeconds(3f);
            
            if (GameManager.Instance != null)
            {
                // Specifically return to Temple for the boss sequence
                if (currentEnemy.isBoss && currentEnemy.enemyName == "Skelettkrieger")
                {
                    GameManager.Instance.LoadScene("Temple", "BossDefeatedSpawn");
                }
                else
                {
                    // Generic return for other enemies like Sensenmann
                    string lastScene = !string.IsNullOrEmpty(GameManager.Instance.lastGameplayScene) 
                        ? GameManager.Instance.lastGameplayScene 
                        : "Legend of Ryo";
                    GameManager.Instance.LoadScene(lastScene, "ReturnFromBattle");
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Temple");
            }
            }
        else { 
            ShowBattleMessage("Niederlage..."); 
            yield return new WaitForSeconds(1f); 
            if (BattleUI.Instance != null) BattleUI.Instance.ShowGameOver(true); 
        }
    }

    private void RestorePlayerVisibility()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            GameManager.Instance.player.transform.SetParent(null);
            DontDestroyOnLoad(GameManager.Instance.player);
            
            var renderers = GameManager.Instance.player.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = true;
            
            Debug.Log("BattleManager: Persistent player visibility restored.");
        }
    }

    private void ShowBattleMessage(string message)
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.ShowMessage("Ryo", message, 1.0f);
        else if (BattleUI.Instance != null) BattleUI.Instance.ShowActionMessage("Ryo", message);
    }

    private void ApplyCurseVisuals()
    {
        var stats = PlayerStats.Instance;
        if (stats == null || playerPos == null) return;
        SpriteRenderer sr = playerPos.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        int form = stats.GetCurseForm();
        
        // Duration Logic: Set to 3 when entering form 3 (if not already in it)
        if (form == 3 && shinigamiTurnsLeft <= 0)
        {
            shinigamiTurnsLeft = 3;
            Debug.Log("Shinigami Form aktiviert für 3 Züge!");
        }

        // Use shinigamiTurnsLeft to force form 3 visuals even if curseValue decayed
        bool isShinigami = shinigamiTurnsLeft > 0;

        if (playerAura != null) playerAura.SetActive((form >= 1 || isShinigami) && !isShinigami);
        
        if (isShinigami)
        {
            sr.color = Color.white;
            if (shinigamiSprite != null) 
            {
                sr.sprite = shinigamiSprite;
                sr.transform.localPosition = originalRendererLocalPos + shinigamiOffset;
            }
        }
        else
        {
            if (form == 2) sr.color = new Color(0.5f, 0f, 0.5f, 1f);
            else sr.color = Color.white;

            if (humanSprite != null) sr.sprite = humanSprite;
            sr.transform.localPosition = originalRendererLocalPos;
        }
    }
}
