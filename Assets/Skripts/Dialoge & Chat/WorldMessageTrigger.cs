using UnityEngine;

public class WorldMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Scheint verschlossen zu seien";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
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