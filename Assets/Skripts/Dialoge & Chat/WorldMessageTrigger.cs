using UnityEngine;

public class WorldMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Scheint abgeschlossen zu sein...";

    [Header("Interaction Settings")]
    public float interactionDistance = 2.0f;

    private bool playerInside = false;

    void Update()
    {
        // Manueller Entfernungscheck zum Player (100% verlässlich)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            float dist = Vector2.Distance(transform.position, playerObj.transform.position);
            bool isClose = dist <= interactionDistance;

            // Logge Statusänderung
            if (isClose && !playerInside)
            {
                Debug.Log("--- HAUS-INTERAKTION BEREIT --- Objekt: " + gameObject.name + " | Distanz: " + dist);
            }
            else if (!isClose && playerInside)
            {
                Debug.Log("--- HAUS-INTERAKTION VERLASSEN --- Objekt: " + gameObject.name);
            }

            playerInside = isClose;
        }

        // Interaktion auslösen
        if (playerInside && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("HAUS-INTERAKTION: Taste R gedrückt bei: " + gameObject.name);

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