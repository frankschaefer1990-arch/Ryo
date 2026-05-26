using UnityEngine;

public class IdolPuzzleEntryTrigger : MonoBehaviour
{
    public string message = "Welch ein seltsamer Ort";
    public string speaker = "Ryo";
    private bool hasTriggered = false;

    private void Start()
    {
        // Simple entry logic: if we are in this scene and haven't triggered, show message
        Invoke("ShowDialogue", 1f);
    }

    private void ShowDialogue()
    {
        if (hasTriggered) return;
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speaker, message);
            hasTriggered = true;
        }
    }
}

public class IdolPuzzleWallTrigger : MonoBehaviour
{
    public string message = "Wer den Wächter der Wasserfälle will sehen, muss alle Statuen zum Brunnen drehen.";
    public string speaker = "???";
    private bool isPlayerInside = false;
    private bool popupShown = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !popupShown)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(speaker, message);
                popupShown = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            popupShown = false;
        }
    }
}
