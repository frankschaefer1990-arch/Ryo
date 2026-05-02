using UnityEngine;

public class WorldMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Scheint abgeschlossen zu sein...";

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Player im Trigger");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Player raus");
        }
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R funktioniert");

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage(message);
            }
            else
            {
                Debug.LogError("DialogueUI Instance fehlt!");
            }
        }
    }
}