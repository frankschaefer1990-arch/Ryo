using UnityEngine;

public class BridgeBlocker : MonoBehaviour
{
    [Header("Bridge Wall")]
    public GameObject bridgeWall;

    [Header("Speaker Name")]
    public string speakerName = "Ryo";

    [Header("Popup Message")]
    [TextArea]
    public string bridgeMessage = "Ich muss unbedingt zuerst in den Tempel...";

    private bool popupShown = false;

    private void Start()
    {
        if (bridgeWall == null)
        {
            Debug.LogError("BridgeWall fehlt!");
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager fehlt!");
        }

        if (DialogueUI.Instance == null)
        {
            Debug.LogError("DialogueUI fehlt!");
        }
    }

    private void Update()
    {
        // Wenn Tempel besucht -> Brücke freigeben
        if (QuestManager.Instance != null && QuestManager.Instance.visitedTemple)
        {
            if (bridgeWall != null)
            {
                bridgeWall.SetActive(false);
            }

            // Trigger deaktivieren
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Nur solange Tempel NICHT besucht
        if (QuestManager.Instance != null && !QuestManager.Instance.visitedTemple)
        {
            if (!popupShown)
            {
                if (DialogueUI.Instance != null)
                {
                    // Speaker + Nachricht
                    DialogueUI.Instance.ShowMessage(speakerName, bridgeMessage);
                }
                else
                {
                    Debug.LogError("DialogueUI Instance fehlt!");
                }

                popupShown = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        popupShown = false;
    }
}