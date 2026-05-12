using UnityEngine;

public class WorldMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Scheint verschlossen zu seien";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // NEW: Only allow in Scene 1 (Overworld)
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Temple") return;

            // Prevent triggering during cutscenes or when UI is locked
            if (MyUIManager.Instance != null && MyUIManager.Instance.isLocked) return;
            if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive()) return;

            Debug.Log("HAUS-TRIGGER (Collider): Spieler betritt Bereich von " + gameObject.name);

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(message);
            }
            else
            {
                Debug.LogError("DialogueUI.Instance fehlt im Haus-Trigger!");
            }
        }
    }
}