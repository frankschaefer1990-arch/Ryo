using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string speakerName = "NPC";
    public Sprite portrait;
    
    [TextArea]
    public string[] randomDialogues;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2.0f;
    public KeyCode interactKey = KeyCode.R;
    
    private Transform player;
    private bool playerInRange = false;

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

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
            {
                ShowRandomDialogue();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
