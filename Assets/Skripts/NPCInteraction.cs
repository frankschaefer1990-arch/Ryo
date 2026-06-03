using UnityEngine;
using System.Collections;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string speakerName = "NPC";
    public Sprite portrait;
    public bool isSequence = false; // If true, shows all dialogues in order. If false, picks one at random.
    
    [TextArea]
    public string[] randomDialogues;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2.0f;
    public KeyCode interactKey = KeyCode.R;
    
    private Transform player;
    private bool playerInRange = false;
    private bool isDialogueRunning = false;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueRunning)
        {
            if (DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
            {
                if (isSequence) ShowSequenceDialogue();
                else ShowRandomDialogue();
            }
        }
    }

    private void FindPlayer()
    {
        GameObject pObj = GameObject.FindWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    private void ShowRandomDialogue()
    {
        if (randomDialogues == null || randomDialogues.Length == 0) return;
        
        string msg = randomDialogues[Random.Range(0, randomDialogues.Length)];
        DialogueUI.Instance.ShowMessage(speakerName, msg, portrait, 0.8f);
    }

    private void ShowSequenceDialogue()
    {
        if (randomDialogues == null || randomDialogues.Length == 0) return;
        StartCoroutine(DialogueSequenceRoutine());
    }

    private IEnumerator DialogueSequenceRoutine()
    {
        isDialogueRunning = true;
        
        foreach (string msg in randomDialogues)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(speakerName, msg, portrait, 0.8f);
                while (DialogueUI.Instance.IsDialogueActive()) yield return null;
                yield return new WaitForSeconds(0.1f); // Tiny gap between windows
            }
        }
        
        isDialogueRunning = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
