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
    public GameObject blockVisual; // New: Shield visual
    public AudioClip blockSound; // New: Block sound effect
    private Sprite humanSprite; // Store original
    private bool playerActionTakenInTurn = false;
    private bool isBlocking = false;

    public BattleState state;
    private int enemyCurrentHP;
    private bool enemyIsStunned = false;

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
        
        if (playerPos != null)
        {
            SpriteRenderer sr = playerPos.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) humanSprite = sr.sprite;
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

        try {
            if (QuestManager.Instance != null && QuestManager.Instance.nextBattleEnemy != null)
            {
                currentEnemy = QuestManager.Instance.nextBattleEnemy;
                QuestManager.Instance.nextBattleEnemy = null;
            }

            if (currentEnemy == null) yield break;

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
        
        if (PlayerStats.Instance != null)
        {
            if (!playerActionTakenInTurn && PlayerStats.Instance.curseValue > 0)
            {
                // Ewigkeitsfluch (Skill 10): Reduced decay (-3 instead of -10)
                int decay = PlayerStats.Instance.HasCursePassive(10) ? -3 : -10;
                PlayerStats.Instance.ChangeCurseValue(decay);
                ApplyCurseVisuals();
            }
            playerActionTakenInTurn = false;
            
            PlayerStats.Instance.RestoreMana(5);
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
            
            ShowBattleMessage("Ryo verwendet einen Trank!");
            StartCoroutine(EnemyTurnAfterDelay(1.2f));
        }
    }

    private IEnumerator EnemyTurnAfterDelay(float delay)
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(delay);
        StartCoroutine(EnemyTurn());
    }

    private Vector3 GetEffectCenter(Transform target)
    {
        if (target == null) return Vector3.zero;
        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.bounds.center : target.position + Vector3.up;
    }

    private IEnumerator ExecuteSkill(BattleSkill skill, int level)
    {
        state = BattleState.BUSY;
        Vector3 effectCenter = GetEffectCenter(enemyPos);
        var stats = PlayerStats.Instance;

        if (skill.isSpell && stats != null)
        {
            stats.ChangeCurseValue(5);
            ApplyCurseVisuals();
            stats.UseMana(skill.GetManaCost(level));
            BattleUI.Instance.UpdatePlayerMana((float)stats.currentMana / stats.maxMana, stats.currentMana, stats.maxMana);
        }

        BattleUI.Instance.ShowActionMessage("Ryo", "setzt " + skill.skillName + " ein!");
        yield return new WaitForSeconds(1.0f);
        BattleUI.Instance.HideActionMessage();

        Vector3 originalPos = playerPos.position;
        playerPos.position += new Vector3(0.5f, 0.5f, 0);
        
        if (audioSource != null && skill.skillSound != null && !skill.hasCombo) audioSource.PlayOneShot(skill.skillSound);

        int actualHitCount = skill.GetHitCount(level);
        float currentQTELimit = skill.GetQTETimeLimit(level);

        for (int i = 0; i < actualHitCount; i++)
        {
            bool hitSuccess = true;
            if (skill.hasCombo)
            {
                yield return new WaitForSeconds(0.2f);
                if (ComboSystem.Instance != null)
                {
                    bool qteResult = false;
                    bool waiting = true;
                    float hitSpecificLimit = skill.skillId == "rage" ? Mathf.Max(currentQTELimit * Mathf.Pow(0.9f, i), 0.35f) : currentQTELimit;

                    ComboSystem.Instance.StartQTE((result) => {
                        qteResult = result;
                        waiting = false;
                        if (result && stats != null) 
                        {
                            stats.ChangeCurseValue(10);
                            ApplyCurseVisuals();
                        }
                    }, hitSpecificLimit);

                    while (waiting) yield return null;
                    hitSuccess = qteResult;
                }
            }

            if (hitSuccess)
            {
                // Stun Logic
                float stunChance = skill.GetStunChance(level);
                
                // Geisterzwang (Skill 5): +5% stun chance if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(5))
                    stunChance += 0.05f;
                
                // Seelenlast (Skill 7): +10% stun chance if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(7))
                    stunChance += 0.10f;

                if (Random.value <= stunChance) enemyIsStunned = true;

                if (audioSource != null && skill.skillSound != null) audioSource.PlayOneShot(skill.skillSound);

                int playerStrength = stats != null ? stats.strength : 1;
                int playerIntelligence = stats != null ? stats.defense : 1;
                int baseDamage = skill.hasCombo ? 15 : 30; 
                float totalMultiplier = skill.GetDamageMultiplier(level);
                if (skill.skillId == "rage") totalMultiplier *= (1.0f + (i * 0.15f));

                int bonusDmg = skill.category == SkillCategory.Basic ? playerStrength : playerIntelligence * 2;
                
                // Abgrundruf (Skill 8): +25% bonus dmg to spells/curse if curse active
                if (stats != null && stats.IsCursePassiveActive() && stats.HasCursePassive(8))
                {
                    if (skill.category != SkillCategory.Basic) bonusDmg = (int)(bonusDmg * 1.25f);
                }

                // Todeshauch (Skill 11): +50% total damage in Shinigami form
                if (stats != null && stats.GetCurseForm() == 3 && stats.HasCursePassive(11))
                {
                    totalMultiplier *= 1.5f;
                }

                int totalDamage = (int)((baseDamage + bonusDmg) * totalMultiplier);
                
                enemyCurrentHP -= totalDamage;
                if (enemyCurrentHP < 0) enemyCurrentHP = 0;

                if (skill.category == SkillCategory.Basic && stats != null)
                {
                    stats.ChangeCurseValue(10);
                    ApplyCurseVisuals();
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

                DamagePopup.Create(enemyPos.position + new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0), (int)(baseDamage * totalMultiplier), (int)(bonusDmg * totalMultiplier), damageFont);
                
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

                ProceduralSlash effectToUse = skill.isSpell ? lightningEffect : slashEffect;
                if (skill.customSlashSprite != null)
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
                    if (effectToUse != null) effectToUse.PlaySlash(effectCenter, skill.effectColor, skill.customSlashSprite, skill.slashDuration, skill.visualOffset, skill.visualScale, skill.randomRotation);
                }
                else if (skill.isSpell && blitzAnimationObject != null) StartCoroutine(ShowEffectBriefly(blitzAnimationObject, 0.3f));
                else if (effectToUse != null) effectToUse.PlaySlash(effectCenter, skill.effectColor, skill.customSlashSprite, skill.slashDuration, skill.visualOffset, skill.visualScale, skill.randomRotation);

                StartCoroutine(PlayHurtAnimation(enemyPos)); 
                BattleUI.Instance.UpdateEnemyHP((float)enemyCurrentHP / currentEnemy.maxHP, enemyCurrentHP, currentEnemy.maxHP);
                if (enemyCurrentHP <= 0) break;
            }
            else { ShowBattleMessage("Combo unterbrochen!"); break; }
            yield return new WaitForSeconds(0.3f);
        }

        playerPos.position = originalPos;
        yield return new WaitForSeconds(1.0f);
        if (enemyCurrentHP <= 0) { state = BattleState.WON; StartCoroutine(EndBattle()); }
        else StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(1.0f);

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
            DamagePopup.Create(playerPos.position + Vector3.up, Mathf.Max(damage - stats.defense, 1), damageFont);
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
        if (state == BattleState.WON)
        {
            if (enemyPos != null) enemyPos.gameObject.SetActive(false);
            ShowBattleMessage("Sieg! " + currentEnemy.xpReward + " XP und 50 Gold erhalten.");
            if (currentEnemy.isBoss && QuestManager.Instance != null) QuestManager.Instance.defeatedTempleBoss = true;
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.GainXP(currentEnemy.xpReward);
                PlayerGold goldMgr = PlayerGold.GetInstance();
                if (goldMgr != null) goldMgr.AddGold(50);
            }
            yield return new WaitForSeconds(3f);
            if (GameManager.Instance != null) GameManager.Instance.LoadScene("Temple", "BossDefeatedSpawn"); 
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Temple");
        }
        else { ShowBattleMessage("Niederlage..."); yield return new WaitForSeconds(1f); if (BattleUI.Instance != null) BattleUI.Instance.ShowGameOver(true); }
    }

    private void ShowBattleMessage(string message)
    {
        if (DialogueUI.Instance != null) DialogueUI.Instance.ShowMessage("Ryo", message, 1.5f);
        else if (BattleUI.Instance != null) BattleUI.Instance.ShowActionMessage("Ryo", message);
    }

    private void ApplyCurseVisuals()
    {
        var stats = PlayerStats.Instance;
        if (stats == null || playerPos == null) return;
        SpriteRenderer sr = playerPos.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        int form = stats.GetCurseForm();
        if (playerAura != null) playerAura.SetActive(form >= 1 && form < 3);
        if (form == 2) sr.color = new Color(0.5f, 0f, 0.5f, 1f);
        else sr.color = Color.white;

        if (form == 3 && shinigamiSprite != null) sr.sprite = shinigamiSprite;
        else if (humanSprite != null) sr.sprite = humanSprite;
    }
}