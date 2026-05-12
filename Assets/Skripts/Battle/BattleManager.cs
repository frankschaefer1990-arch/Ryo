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
    public GameObject enemyAttackVisual; // New

    public BattleState state;
    private int enemyCurrentHP;

    [Header("Audio")]
    public AudioClip battleMusic;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Debug.Log("BattleManager: Start called. Waiting for scene stabilization...");
        state = BattleState.START;
        
        // Ensure ALL visual effect objects are hidden at start
        if (blitzAnimationObject != null) blitzAnimationObject.SetActive(false);
        if (enemyAttackVisual != null) enemyAttackVisual.SetActive(false);
        
        // Fix: Check if the effect is on the same GameObject to prevent self-deactivation
        if (slashEffect != null && slashEffect.gameObject != gameObject) slashEffect.gameObject.SetActive(false);
        if (lightningEffect != null && lightningEffect.gameObject != gameObject) lightningEffect.gameObject.SetActive(false);

        if (comboStrikeObjects != null)
        {
            foreach (var obj in comboStrikeObjects)
            {
                if (obj != null && obj != gameObject) obj.SetActive(false);
            }
        }

        Debug.Log("BattleManager: Starting SetupBattle in 0.5s...");
        Invoke(nameof(StartSetup), 0.5f);
    }

    private void StartSetup()
    {
        StartCoroutine(SetupBattle());
    }

    private IEnumerator ShowEffectBriefly(GameObject effect, float duration)
    {
        if (effect == null) yield break;
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    private IEnumerator SetupBattle()
    {
        Debug.Log("BattleManager: Setting up battle...");
        
        // Wait a frame to ensure all Awake/Starts are done
        yield return null;
        
        try {
            // Use boss data from QuestManager if it exists
            if (QuestManager.Instance != null && QuestManager.Instance.nextBattleEnemy != null)
            {
                currentEnemy = QuestManager.Instance.nextBattleEnemy;
                QuestManager.Instance.nextBattleEnemy = null; // Clear after use
                Debug.Log("BattleManager: Loaded enemy from QuestManager: " + (currentEnemy != null ? currentEnemy.enemyName : "NULL"));
            }

            if (currentEnemy == null)
            {
                Debug.LogError("BattleManager: No enemy data available! Cannot start battle.");
                yield break;
            }

            enemyCurrentHP = currentEnemy.startHP > 0 ? currentEnemy.startHP : currentEnemy.maxHP;
            
            if (BattleUI.Instance != null) {
                var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
                Debug.Log($"BattleManager: Initializing UI with {enemyCurrentHP} Enemy HP and {(stats != null ? stats.currentHealth.ToString() : "NULL")} Player HP.");
                BattleUI.Instance.SetEnemyName(currentEnemy.enemyName);
                float enemyRatio = currentEnemy.maxHP > 0 ? (float)enemyCurrentHP / currentEnemy.maxHP : 1f;
                BattleUI.Instance.UpdateEnemyHP(enemyRatio, enemyCurrentHP, currentEnemy.maxHP);
                
                if (stats != null)
                {
                    stats.RecalculateStats(); // Ensure latest values
                    float pHP = stats.maxHealth > 0 ? (float)stats.currentHealth / stats.maxHealth : 1f;
                    float pMana = stats.maxMana > 0 ? (float)stats.currentMana / stats.maxMana : 1f;
                    
                    BattleUI.Instance.UpdatePlayerHP(pHP, stats.currentHealth, stats.maxHealth);
                    BattleUI.Instance.UpdatePlayerMana(pMana, stats.currentMana, stats.maxMana);
                }
                
                BattleUI.Instance.ToggleCommandPanel(true);
                BattleUI.Instance.SetupSubButtons(this);
            }else {
                Debug.LogError("BattleManager: BattleUI.Instance is missing!");
            }

            // Start Music
            if (audioSource != null && battleMusic != null)
            {
                audioSource.clip = battleMusic;
                audioSource.loop = true;
                audioSource.Play();
                Debug.Log("BattleManager: Started music " + battleMusic.name);
            }
            
            state = BattleState.PLAYERTURN; 

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(currentEnemy.enemyName, "erscheint!");
            }
        }
        catch (System.Exception e) {
            Debug.LogError("BattleManager: CRITICAL ERROR during SetupBattle: " + e.Message + "\n" + e.StackTrace);
        }
        
        yield return new WaitForSeconds(1.2f);
        
        Debug.Log("BattleManager: Setup complete. Transitioning to PlayerTurn.");
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        Debug.Log("BattleManager: PlayerTurn started.");

        // Mana Regeneration: +5 per turn
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.RestoreMana(5);
            BattleUI.Instance.UpdatePlayerMana((float)PlayerStats.Instance.currentMana / PlayerStats.Instance.maxMana, PlayerStats.Instance.currentMana, PlayerStats.Instance.maxMana);
        }

        BattleUI.Instance.ToggleCommandPanel(true);
    }

    public void OnAttackButton()
    {
        Debug.Log("BattleManager: OnAttackButton clicked.");
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowAttackPanel();
    }

    public void OnSpellButton()
    {
        Debug.Log("BattleManager: OnSpellButton clicked.");
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowSpellPanel();
    }

    public void OnItemButton()
    {
        Debug.Log("BattleManager: OnItemButton clicked.");
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowItemPanel();
    }

    // Call this from the actual skill buttons inside panels
    public void UseSkill(BattleSkill skill)
    {
        Debug.Log("BattleManager: UseSkill called for " + (skill != null ? skill.skillName : "null"));
        if (state != BattleState.PLAYERTURN) return;

        // Mana check
        if (skill.isSpell && PlayerStats.Instance != null && PlayerStats.Instance.currentMana < skill.manaCost)
        {
            ShowBattleMessage("Nicht genug Mana!");
            return;
        }

        BattleUI.Instance.ToggleCommandPanel(false); // Hides all
        StartCoroutine(ExecuteSkill(skill));
    }

    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(TryRun());
    }

    private IEnumerator TryRun()
    {
        state = BattleState.BUSY;
        BattleUI.Instance.ToggleCommandPanel(false);
        ShowBattleMessage("Ryo versucht zu flüchten...");
        yield return new WaitForSeconds(1f);

        // 50% chance to run
        if (Random.value > 0.5f)
        {
            ShowBattleMessage("Flucht erfolgreich!");
            yield return new WaitForSeconds(1.5f);
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
            BattleUI.Instance.HideAllSubPanels();
            BattleUI.Instance.ToggleCommandPanel(false);
            
            int healAmount = 30;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(healAmount);
                InventoryManager.Instance.RemoveOnePotion();
                BattleUI.Instance.UpdatePlayerHP((float)PlayerStats.Instance.currentHealth / PlayerStats.Instance.maxHealth, PlayerStats.Instance.currentHealth, PlayerStats.Instance.maxHealth);
            }
            
            ShowBattleMessage("Ryo verwendet einen Trank!");
            StartCoroutine(EnemyTurnAfterDelay(1.2f)); // Reduced from 2f
            }
        else if (InventoryManager.Instance == null)
        {
            ShowBattleMessage("Kein Inventar gefunden!");
        }
        else
        {
            ShowBattleMessage("Keine Tränke mehr!");
        }
    }

    private IEnumerator EnemyTurnAfterDelay(float delay)
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(delay);
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator ExecuteSkill(BattleSkill skill)
    {
        state = BattleState.BUSY;

        // Mana Deduction
        if (skill.isSpell && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.UseMana(skill.manaCost);
            BattleUI.Instance.UpdatePlayerMana((float)PlayerStats.Instance.currentMana / PlayerStats.Instance.maxMana, PlayerStats.Instance.currentMana, PlayerStats.Instance.maxMana);
        }

        BattleUI.Instance.ShowActionMessage("Ryo", "setzt " + skill.skillName + " ein!");
        
        yield return new WaitForSeconds(1.0f);
        BattleUI.Instance.HideActionMessage();

        Vector3 originalPos = playerPos.position;
        playerPos.position += new Vector3(0.5f, 0.5f, 0);
        
        // Initial sound for non-combo skills
        if (audioSource != null && skill.skillSound != null && !skill.hasCombo)
        {
            audioSource.PlayOneShot(skill.skillSound);
        }

        for (int i = 0; i < skill.hitCount; i++)
        {
            Debug.Log($"BattleManager: Starting hit {i+1} of {skill.hitCount} for {skill.skillName}");
            bool hitSuccess = true;

            if (skill.hasCombo)
            {
                yield return new WaitForSeconds(0.2f);

                bool qteResult = false;
                bool waiting = true;
                ComboSystem.Instance.StartQTE((result) => {
                    qteResult = result;
                    waiting = false;
                    Debug.Log($"BattleManager: QTE Result for hit {i+1}: {result}");
                });

                while (waiting) yield return null;
                hitSuccess = qteResult;
            }

            if (hitSuccess)
            {
                // Play sound for every hit
                if (audioSource != null && skill.skillSound != null)
                {
                    audioSource.PlayOneShot(skill.skillSound);
                }

                // Damage calculation
                int playerStrength = (PlayerStats.Instance != null) ? PlayerStats.Instance.strength : 1;
                int baseDamage = skill.hasCombo ? 15 : 30; 
                int totalDamage = baseDamage + playerStrength;
                
                Debug.Log($"BattleManager: Hit {i+1} Success! Damage: {totalDamage}");
                
                enemyCurrentHP -= totalDamage;
                if (enemyCurrentHP < 0) enemyCurrentHP = 0;

                // Spawn Damage Popup
DamagePopup.Create(enemyPos.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0), baseDamage, playerStrength, damageFont);
                
                // Visuals
                if (skill.isSpell && blitzAnimationObject != null)
                {
                    StartCoroutine(ShowEffectBriefly(blitzAnimationObject, 0.3f));
                }
                else if (skill.hasCombo && comboStrikeObjects != null && i < comboStrikeObjects.Length)
                {
                    GameObject strikeObj = comboStrikeObjects[i];
                    if (strikeObj != null)
                    {
                        // Always activate the object so children (animations) are visible
                        StartCoroutine(ShowEffectBriefly(strikeObj, 0.5f));
                        
                        ProceduralSlash ps = strikeObj.GetComponent<ProceduralSlash>();
                        if (ps != null) ps.PlaySlash(enemyPos.position, skill.effectColor);
                    }
                }
                else
                {
                    ProceduralSlash effectToUse = skill.isSpell ? lightningEffect : slashEffect;
                    if (effectToUse != null)
                    {
                        effectToUse.PlaySlash(enemyPos.position, skill.effectColor);
                    }
                }

                StartCoroutine(PlayHurtAnimation(enemyPos)); 
                BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);

                if (enemyCurrentHP <= 0) break;
            }
            else
            {
                ShowBattleMessage("Combo unterbrochen!");
                break;
            }

            yield return new WaitForSeconds(0.3f);
        }

        playerPos.position = originalPos;
        yield return new WaitForSeconds(1.0f);

        if (enemyCurrentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(1.0f);

        BattleUI.Instance.ShowActionMessage(currentEnemy.enemyName, "greift an!");
        yield return new WaitForSeconds(1.0f);
        BattleUI.Instance.HideActionMessage();

        Vector3 enemyOriginalPos = enemyPos.position;
        enemyPos.position -= new Vector3(0.5f, 0, 0);

        if (enemyAttackVisual != null) StartCoroutine(ShowEffectBriefly(enemyAttackVisual, 0.4f));

        // Enemy Sound
        AudioSource enemyAudio = enemyPos.GetComponent<AudioSource>();
        if (enemyAudio != null)
        {
            AudioClip soundToPlay = currentEnemy.attackSound != null ? currentEnemy.attackSound : wildeSchlaege.skillSound;
            if (soundToPlay != null) enemyAudio.PlayOneShot(soundToPlay);
        }

        int damage = currentEnemy.attack;
        var stats = PlayerStats.Instance ?? FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            // Calculate final damage after Ryo's defense to match what's actually subtracted
            int finalDamage = Mathf.Max(damage - stats.defense, 1);
            
            stats.TakeDamage(damage);
            DamagePopup.Create(playerPos.position + Vector3.up, finalDamage, damageFont);
            StartCoroutine(PlayHurtAnimation(playerPos)); 
            BattleUI.Instance.UpdatePlayerHP((float)stats.currentHealth / stats.maxHealth, stats.currentHealth, stats.maxHealth);
        }

        yield return new WaitForSeconds(0.5f); 
        enemyPos.position = enemyOriginalPos;
        yield return new WaitForSeconds(0.5f); 

        if (PlayerStats.Instance != null && PlayerStats.Instance.currentHealth <= 0)
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    private IEnumerator PlayHurtAnimation(Transform target)
    {
        Vector3 originalPos = target.position;
        Vector3 originalScale = target.localScale;
        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        
        // Intense shake, flash and slight scale pulse
        for (int i = 0; i < 4; i++)
        {
            float shakeX = Random.Range(-0.15f, 0.15f);
            target.position = originalPos + new Vector3(shakeX, 0, 0);
            target.localScale = originalScale * 1.1f; // Slight pop
            
            if (sr != null) sr.color = new Color(1, 0.3f, 0.3f, 1); // Bright red-ish
            
            yield return new WaitForSeconds(0.04f);
            
            target.position = originalPos;
            target.localScale = originalScale;
            
            if (sr != null) sr.color = Color.white;
            
            yield return new WaitForSeconds(0.04f);
        }
        
        target.position = originalPos;
        target.localScale = originalScale;
    }

    private IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            // Immediately hide enemy visual
            if (enemyPos != null) enemyPos.gameObject.SetActive(false);

            ShowBattleMessage("Sieg! " + currentEnemy.xpReward + " XP und 50 Gold erhalten.");
            
            // Story Progress: Unlock bridge if boss was defeated
            if (currentEnemy.isBoss && QuestManager.Instance != null)
            {
                QuestManager.Instance.defeatedTempleBoss = true;
            }

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.GainXP(currentEnemy.xpReward);
                // Reward 50 Gold
                PlayerGold goldMgr = PlayerGold.GetInstance();
                if (goldMgr != null)
                {
                    Debug.Log($"BattleManager: Gold before reward: {goldMgr.currentGold}");
                    goldMgr.AddGold(50);
                    Debug.Log($"BattleManager: Gained 50 Gold. Total: {goldMgr.currentGold}");
                }
                else
                {
                    Debug.LogError("BattleManager: PlayerGold instance not found!");
                }
            }
            yield return new WaitForSeconds(3f); // Increased wait time for dialogue
            
            if (BattleUI.Instance != null) BattleUI.Instance.HideActionMessage();

            if (GameManager.Instance != null) GameManager.Instance.LoadScene("Temple", "BossDefeatedSpawn"); 
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Temple");
        }
        else
        {
            ShowBattleMessage("Niederlage...");
            yield return new WaitForSeconds(1f);
            
            // Show Game Over UI instead of direct reload
            if (BattleUI.Instance != null)
            {
                BattleUI.Instance.ShowGameOver(true);
            }
            else
            {
                // Fallback
                if (GameManager.Instance != null) GameManager.Instance.LoadScene("Legend of Ryo");
                else UnityEngine.SceneManagement.SceneManager.LoadScene("Legend of Ryo");
            }
        }
    }

    private void ShowBattleMessage(string message)
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", message, 1.5f); // Increased visibility
        }
        else if (BattleUI.Instance != null)
        {
            // Fallback to BattleUI if DialogueUI is missing from scene
            BattleUI.Instance.ShowActionMessage("Ryo", message);
            Debug.Log("BATTLE MSG (BattleUI): " + message);
        }
        else
        {
            Debug.Log("BATTLE MSG (Console Only): " + message);
        }
    }
    }