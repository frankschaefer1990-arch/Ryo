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
        questManager = QuestManager.Instance;
        if (questManager == null) questManager = FindAnyObjectByType<QuestManager>();

        if (questManager != null)
        {
            // Initial state based on quest progress
            if (bridgeWall != null)
            {
                bridgeWall.SetActive(!questManager.defeatedTempleBoss);
            }
            if (questManager.defeatedTempleBoss) this.enabled = false;
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
        // QuestManager holen falls noch nicht vorhanden
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
            if (questManager == null) return;
        }

        // Boss besiegt -> Brücke freigeben
        if (questManager.defeatedTempleBoss)
        {
            if (bridgeWall != null && bridgeWall.activeSelf)
            {
                bridgeWall.SetActive(false);
                Debug.Log("BridgeBlocker: Boss besiegt. Brücke geöffnet.");
            }
            this.enabled = false;
        }
        else
        {
            // Boss noch da -> Brücke MUSS gesperrt sein
            if (bridgeWall != null && !bridgeWall.activeSelf)
            {
                bridgeWall.SetActive(true);
                Debug.Log("BridgeBlocker: Boss lebt noch. Brücke gesperrt.");
            }
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
            questManager = FindAnyObjectByType<QuestManager>();
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
                bridgeMessage = "Ich muss zuerst den Skelettkrieger im Tempel besiegen!";
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