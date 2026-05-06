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
    private QuestManager questManager;

    // =========================
    // START
    // =========================
    private void Start()
    {
        // BridgeWall prüfen
        if (bridgeWall == null)
        {
            Debug.LogError("BridgeWall fehlt!");
        }

        // QuestManager holen
        questManager = FindFirstObjectByType<QuestManager>();

        if (questManager == null)
        {
            Debug.LogWarning("QuestManager fehlt! Bridge bleibt vorerst aktiv.");
        }

        // DialogueUI prüfen
        if (DialogueUI.Instance == null)
        {
            Debug.LogWarning("DialogueUI fehlt!");
        }
    }

    // =========================
    // UPDATE
    // =========================
    private void Update()
    {
        // Kein QuestManager -> nichts prüfen
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>();

            if (questManager == null)
                return;
        }

        // Boss besiegt -> Brücke freigeben
        if (questManager.defeatedTempleBoss)
        {
            if (bridgeWall != null)
            {
                bridgeWall.SetActive(false);
            }

            // Trigger deaktivieren
            gameObject.SetActive(false);
        }
        }

        // =========================
        // PLAYER BETRITT BLOCKER
        // =========================
        private void OnTriggerEnter2D(Collider2D other)
        {
        if (!other.CompareTag("Player"))
            return;

        // QuestManager später erneut suchen
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>();
        }

        // Falls kein QuestManager existiert:
        // Nur Nachricht zeigen, aber kein Crash
        if (questManager == null)
        {
            ShowBridgeMessage();
            return;
        }

        // Nur solange Boss NICHT besiegt
        if (!questManager.defeatedTempleBoss)
        {
            if (questManager.visitedTemple)
            {
                bridgeMessage = "Ich muss zuerst den Knochenhollow im Tempel besiegen!";
            }
            ShowBridgeMessage();
        }
        }

    // =========================
    // MESSAGE
    // =========================
    private void ShowBridgeMessage()
    {
        if (popupShown)
            return;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speakerName, bridgeMessage);
        }
        else
        {
            Debug.LogWarning("DialogueUI Instance fehlt!");
        }

        popupShown = true;
    }

    // =========================
    // PLAYER VERLÄSST BLOCKER
    // =========================
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        popupShown = false;
    }
}