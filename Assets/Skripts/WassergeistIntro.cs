using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class WassergeistIntro : MonoBehaviour
{
    public CinemachineCamera vcam;
    public Transform bossTransform;
    public float panDuration = 3.0f;
    public float walkToBossDistance = 1.5f;
    
    private GameObject player;
    private PlayerMovement pm;
    private Animator playerAnim;

    private IEnumerator Start()
    {
        if (QuestManager.Instance != null && (QuestManager.Instance.defeatedWassergeist || QuestManager.Instance.returningFromWassergeist))
        {
            this.enabled = false;
            yield break;
        }

        // Wait for player to spawn from GameManager
        float timeout = 5f;
        while (player == null && timeout > 0)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Ryo") ?? GameObject.Find("Player");
            timeout -= Time.deltaTime;
            yield return null;
        }
        
        if (player == null) {
            Debug.LogError("WassergeistIntro: Player not found!");
            yield break;
        }
        
        pm = player.GetComponent<PlayerMovement>();
        playerAnim = player.GetComponent<Animator>();
        if (playerAnim == null) playerAnim = player.GetComponentInChildren<Animator>();

        StartCoroutine(IntroSequence());
    }
    
    private IEnumerator IntroSequence()
    {
        if (pm != null) pm.canMove = false;
        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = true;
        
        // Ensure camera starts on Boss
        vcam.Follow = bossTransform;
        vcam.Priority = 20; 
        
        // Wait at boss for a moment
        yield return new WaitForSeconds(1.0f);
        
        // Pan to Player
        float elapsed = 0;
        Vector3 startPos = vcam.transform.position;
        vcam.Follow = player.transform; // Cinemachine handles the pan if damping is set, but we want a "slow" pan.
        
        // We can use a lower damping or just wait for the transition
        yield return new WaitForSeconds(panDuration);

        // --- NEW DIALOGUE SEQUENCE ---
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage("Ryo", "Diese Präsenz... Was bist du?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Ich bin der Wächter der Quelle.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Bist du für die Überschwemmung im Dorf verantwortlich?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Nein. Das Wasser folgt meinem Willen nicht mehr wie einst.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Dann weißt du, was dahintersteckt?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Nein... Doch seit die Fluten ihren Lauf veränderten, spüre ich eine fremde Macht.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Wovon sprichst du?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Von dir.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "...Was?");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Eine Seele, die nicht existieren dürfte.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Ryo", "Ich verstehe nicht, wovon du redest.");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
            
            DialogueUI.Instance.ShowMessage("Wassergeist", "Dann zeige mir deine wahre Natur!");
            while (DialogueUI.Instance.IsDialogueActive()) yield return null;
        }
        
        // Walk to Boss
        if (pm != null && playerAnim != null)
        {
            pm.isCutsceneMoving = true;
            playerAnim.SetBool("isMoving", true);
            
            while (Vector2.Distance(player.transform.position, bossTransform.position) > walkToBossDistance)
            {
                Vector2 diff = (bossTransform.position - player.transform.position);
                Vector2 dir = diff.normalized;
                
                // Update Animator
                playerAnim.SetFloat("MoveX", dir.x);
                playerAnim.SetFloat("MoveY", dir.y);
                
                player.transform.position = Vector2.MoveTowards(player.transform.position, bossTransform.position, pm.baseMoveSpeed * Time.deltaTime);
                yield return null;
            }
            
            playerAnim.SetBool("isMoving", false);
            pm.isCutsceneMoving = false;
        }

        if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
        
        // Trigger Battle
        EnemyTrigger trigger = bossTransform.GetComponent<EnemyTrigger>();
        if (trigger != null)
        {
            if (QuestManager.Instance != null && GameManager.Instance != null)
            {
                GameManager.Instance.lastEnemyTriggerID = "Bossraum_Wassergeist";
                QuestManager.Instance.nextBattleEnemy = trigger.enemyData;
                GameManager.Instance.LoadScene(trigger.battleScene);
            }
        }
    }
}