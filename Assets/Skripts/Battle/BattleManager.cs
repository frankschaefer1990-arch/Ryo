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

    [Header("New Visuals")]
    public GameObject[] comboStrikeObjects;
    public GameObject blitzAnimationObject;
    public GameObject enemyAttackVisual; // New

    private BattleState state;
    private int enemyCurrentHP;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        state = BattleState.START;
        
        // Hide new visual objects at start
        if (blitzAnimationObject != null) blitzAnimationObject.SetActive(false);
        if (enemyAttackVisual != null) enemyAttackVisual.SetActive(false);
        if (comboStrikeObjects != null)
        {
            foreach (var obj in comboStrikeObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

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
        enemyCurrentHP = currentEnemy.maxHP;
        BattleUI.Instance.SetEnemyName(currentEnemy.enemyName);
        BattleUI.Instance.UpdateEnemyHP(1f, enemyCurrentHP, currentEnemy.maxHP);
        if (PlayerStats.Instance != null)
        {
            BattleUI.Instance.UpdatePlayerHP((float)PlayerStats.Instance.currentHealth / PlayerStats.Instance.maxHealth, PlayerStats.Instance.currentHealth, PlayerStats.Instance.maxHealth);
        }
        else
        {
            Debug.LogWarning("PlayerStats.Instance ist null. HP-Leiste konnte nicht initialisiert werden.");
        }

        // NEU: Buttons sofort sichtbar machen
        BattleUI.Instance.ToggleCommandPanel(true);
        BattleUI.Instance.SetupSubButtons(this);
        
        state = BattleState.PLAYERTURN; // Enter player turn state immediately to allow interaction

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(currentEnemy.enemyName + " erscheint!");
        }
        else
        {
            Debug.LogWarning("DialogueUI.Instance ist null. Nachricht konnte nicht angezeigt werden.");
        }
        
        yield return new WaitForSeconds(1.2f); // Reduced from 2f
        
        PlayerTurn();
        }

    private void PlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        Debug.Log("BattleManager: PlayerTurn started.");
        BattleUI.Instance.ToggleCommandPanel(true);
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowAttackPanel();
    }

    public void OnSpellButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowSpellPanel();
    }

    public void OnItemButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        BattleUI.Instance.ShowItemPanel();
    }

    // Call this from the actual skill buttons inside panels
    public void UseSkill(BattleSkill skill)
    {
        if (state != BattleState.PLAYERTURN) return;
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
            BattleUI.Instance.HideItemPanel();
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
            bool hitSuccess = true;

            if (skill.hasCombo)
            {
                yield return new WaitForSeconds(0.2f);

                bool qteResult = false;
                bool waiting = true;
                ComboSystem.Instance.StartQTE((result) => {
                    qteResult = result;
                    waiting = false;
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
                int playerStrength = (PlayerStats.Instance != null) ? PlayerStats.Instance.strength : 15;
                int baseDamage = (int)(playerStrength * skill.damageMultiplier);
                int totalDamage = Mathf.Max(1, baseDamage - currentEnemy.defense);
                
                enemyCurrentHP -= totalDamage;
                
                // Visuals
                if (skill.isSpell && blitzAnimationObject != null)
                {
                    StartCoroutine(ShowEffectBriefly(blitzAnimationObject, 0.3f));
                }
                else if (skill.hasCombo && comboStrikeObjects != null && i < comboStrikeObjects.Length)
                {
                    StartCoroutine(ShowEffectBriefly(comboStrikeObjects[i], 0.3f));
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
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.TakeDamage(damage);
            StartCoroutine(PlayHurtAnimation(playerPos)); 
            BattleUI.Instance.UpdatePlayerHP((float)PlayerStats.Instance.currentHealth / PlayerStats.Instance.maxHealth, PlayerStats.Instance.currentHealth, PlayerStats.Instance.maxHealth);
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
            ShowBattleMessage("Sieg! " + currentEnemy.xpReward + " XP erhalten.");
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.GainXP(currentEnemy.xpReward);
            }
            yield return new WaitForSeconds(2f);
            if (GameManager.Instance != null) GameManager.Instance.LoadScene("Legend of Ryo"); 
        }
        else
        {
            ShowBattleMessage("Niederlage...");
            yield return new WaitForSeconds(2f);
            if (GameManager.Instance != null) GameManager.Instance.LoadScene("Legend of Ryo");
        }
    }

    private void ShowBattleMessage(string message)
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", message, 0.4f);
        }
        else
        {
            Debug.Log("BATTLE MSG: " + message);
        }
    }
    }
