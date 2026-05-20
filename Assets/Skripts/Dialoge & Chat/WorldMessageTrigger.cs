using UnityEngine;

public class WorldMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Scheint verschlossen zu sein";

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleTrigger(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleTrigger(collision.gameObject);
    }

    private void HandleTrigger(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            // Skip if the house is already unlocked according to QuestManager
            if (QuestManager.Instance != null)
            {
                if (QuestManager.Instance.finishedTempleSequence || QuestManager.Instance.defeatedTempleBoss)
                {
                    return;
                }
            }

            // NEW: Only allow in Scene 1 (Overworld)
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Temple") return;

            // Prevent triggering during cutscenes or when UI is locked
            if (MyUIManager.Instance != null && MyUIManager.Instance.isLocked) return;
            
            // Check if dialogue is already showing to avoid spam
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive()) return;

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(message);
            }
        }
    }
}