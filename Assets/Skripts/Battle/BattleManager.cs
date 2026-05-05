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

    private BattleState state;
    private int enemyCurrentHP;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
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
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(currentEnemy.enemyName + " erscheint!");
        }
        else
        {
            Debug.LogWarning("DialogueUI.Instance ist null. Nachricht konnte nicht angezeigt werden.");
        }
        
        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    private void PlayerTurn()
    {
        Debug.Log("BattleManager: PlayerTurn started.");
        // Panel ist bereits aktiv vom Start her
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

        if (InventoryManager.Instance.GetPotionCount() > 0)
        {
            BattleUI.Instance.HideItemPanel();
            BattleUI.Instance.ToggleCommandPanel(false);
            
            // Reusing existing Potion logic indirectly or directly
            // For now, let's do it directly to control the turn flow
            int healAmount = 30; // Default potion value
            PlayerStats.Instance.Heal(healAmount);
            InventoryManager.Instance.RemoveOnePotion();
            
            ShowBattleMessage("Ryo verwendet einen Trank!");
            BattleUI.Instance.UpdatePlayerHP((float)PlayerStats.Instance.currentHealth / PlayerStats.Instance.maxHealth, PlayerStats.Instance.currentHealth, PlayerStats.Instance.maxHealth);
            
            StartCoroutine(EnemyTurnAfterDelay(2f));
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
        ShowBattleMessage("Ryo setzt " + skill.skillName + " ein!");
        yield return new WaitForSeconds(1f);

        // Move Player slightly forward
        Vector3 originalPos = playerPos.position;
        playerPos.position += new Vector3(0.5f, 0.5f, 0);

        for (int i = 0; i < skill.hitCount; i++)
        {
            bool hitSuccess = true;

            if (skill.hasCombo)
            {
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
                // Visuals
                slashEffect.PlaySlash(enemyPos.position, skill.effectColor);
                
                // Damage calculation
                int damage = Mathf.Max(1, (int)(PlayerStats.Instance.strength * skill.damageMultiplier) - currentEnemy.defense);
                enemyCurrentHP -= damage;
                BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);
                Debug.Log("Hit " + (i+1) + ": " + damage + " Schaden!");

                if (enemyCurrentHP <= 0) break;
            }
            else
            {
                ShowBattleMessage("Combo unterbrochen!");
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        playerPos.position = originalPos;
        yield return new WaitForSeconds(1f);

        if (enemyCurrentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.BUSY;
            StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator EnemyTurn()
    {
        ShowBattleMessage(currentEnemy.enemyName + " greift an!");
        yield return new WaitForSeconds(1f);

        int damage = currentEnemy.attack;
        PlayerStats.Instance.TakeDamage(damage);

        yield return new WaitForSeconds(1f);

        if (PlayerStats.Instance.currentHealth <= 0)
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

    private IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            ShowBattleMessage("Sieg! " + currentEnemy.xpReward + " XP erhalten.");
            PlayerStats.Instance.GainXP(currentEnemy.xpReward);
            yield return new WaitForSeconds(2f);
            // Return to world
            GameManager.Instance.LoadScene("Legend of Ryo"); 
        }
        else
        {
            ShowBattleMessage("Niederlage...");
            yield return new WaitForSeconds(2f);
            // Return to world or Game Over
            GameManager.Instance.LoadScene("Legend of Ryo");
        }
    }

    private void ShowBattleMessage(string message)
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(message);
        }
        else
        {
            Debug.Log("BATTLE MSG: " + message);
        }
    }
    }
