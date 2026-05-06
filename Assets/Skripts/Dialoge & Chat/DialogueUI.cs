using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("Main UI Elements")]
    public GameObject DialogueFrameNew;
    public GameObject popupTextObject;
    public TextMeshProUGUI popupText;
    public UnityEngine.UI.Text legacyPopupText; // Added fallback for legacy Text

    [Header("Speaker UI")]
    public GameObject speakerNameObject;
    public TextMeshProUGUI speakerNameText;

    [Header("Default Speaker")]
    public string defaultSpeakerName = "Ryo";

    [Header("Typewriter Settings")]
    public float letterDelay = 0.05f; // Slower text speed

    private Coroutine currentRoutine;
    private bool isShowing = false;

    // =========================
    // AWAKE
    // =========================
    private void Awake()
    {
        // SINGLETON
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // GANZ WICHTIG: Persistent machen
        DontDestroyOnLoad(gameObject);

        gameObject.SetActive(true);

        ReconnectUI();

        HideAllImmediate();
    }

    private void OnEnable()
    {
        GameManager.OnSystemsReady += ReconnectUI;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= ReconnectUI;
    }

    // =========================
    // START
    // =========================
    private void Start()
    {
        // ReconnectUI() wird bereits in Awake und vom GameManager aufgerufen
        
        if (popupText != null)
        {
            popupText.enableAutoSizing = true;
            popupText.fontSizeMin = 18; // Larger minimum font size
            popupText.fontSizeMax = 32;
            popupText.alignment = TextAlignmentOptions.TopLeft;
        }

        HideAllImmediate();
    }

    // =========================
    // UI RECONNECT
    // =========================
    public void ReconnectUI()
    {
        // Priorisiere den persistenten Canvas vom GameManager
        GameObject targetCanvas = null;
        if (GameManager.Instance != null && GameManager.Instance.canvas != null)
        {
            targetCanvas = GameManager.Instance.canvas;
        }
        else
        {
            Canvas c = FindAnyObjectByType<Canvas>();
            if (c != null) targetCanvas = c.gameObject;
        }

        if (targetCanvas == null)
            return;

        Transform canvasTransform = targetCanvas.transform;

        // FRAME suchen wenn nötig
        if (DialogueFrameNew == null || DialogueFrameNew.scene.name == null)
        {
            Transform frame = canvasTransform.Find("LockedDoorPopup/DialogueFrameNew");
            if (frame == null) frame = canvasTransform.Find("DialogueFrameNew");
            if (frame == null) frame = canvasTransform.Find("Chatbox"); // Fallback to Chatbox
            
            if (frame == null)
            {
                foreach (Transform t in canvasTransform.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "DialogueFrameNew" || t.name == "Chatbox") { frame = t; break; }
                }
            }

            if (frame != null) DialogueFrameNew = frame.gameObject;
        }

        // Kinder IMMER neu verknüpfen wenn sie fehlen oder ungültig sind
        if (DialogueFrameNew != null)
        {
            // Layout erzwingen: Links unten bündig (nur wenn es der DialogFrame ist)
            if (DialogueFrameNew.name == "DialogueFrameNew")
            {
                RectTransform rt = DialogueFrameNew.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(0, 0);
                    rt.pivot = new Vector2(0, 0);
                    rt.anchoredPosition = new Vector2(20, 20);
                }
            }

            if (popupTextObject == null || popupTextObject.scene.name == null)
            {
                Transform popup = DialogueFrameNew.transform.Find("PopupText");
                if (popup == null) popup = DialogueFrameNew.transform.Find("LogText");
                if (popup != null) popupTextObject = popup.gameObject;
            }

            if (popupTextObject != null)
            {
                popupText = popupTextObject.GetComponent<TextMeshProUGUI>();
                legacyPopupText = popupTextObject.GetComponent<UnityEngine.UI.Text>();
            }

            if (speakerNameObject == null || speakerNameObject.scene.name == null)
            {
                Transform speaker = DialogueFrameNew.transform.Find("SpeakerNameText");
                if (speaker == null) speaker = DialogueFrameNew.transform.Find("SpeakerName");
                if (speaker != null) speakerNameObject = speaker.gameObject;
            }

            if (speakerNameText == null && speakerNameObject != null)
            {
                speakerNameText = speakerNameObject.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    // =========================
    // STANDARD MESSAGE
    // =========================
    public void ShowMessage(string message)
    {
        Debug.Log("DialogueUI: ShowMessage aufgerufen: " + message);
        ShowMessage(defaultSpeakerName, message, 0.8f);
    }

    // =========================
    // SPEAKER MESSAGE
    // =========================
    public void ShowMessage(string speakerName, string message)
    {
        ShowMessage(speakerName, message, 0.8f);
    }

    // =========================
    // CUSTOM MESSAGE
    // =========================
    public void ShowMessage(string speakerName, string message, float visibleDuration)
    {
        Debug.Log($"DialogueUI: Request to show message from {speakerName}");
        
        // Vor jedem Anzeigen sicherstellen, dass die UI-Elemente verknüpft sind
        ReconnectUI();

        if (DialogueFrameNew == null)
        {
            Debug.LogWarning("DialogueFrameNew fehlt! Suche Ersatz...");
            GameObject found = GameObject.Find("DialogueFrameNew");
            if (found == null) found = GameObject.Find("Chatbox");
            if (found == null) found = GameObject.Find("Kampfinformation");
            if (found != null) DialogueFrameNew = found;
        }

        if (DialogueFrameNew == null)
        {
            Debug.LogError("KRITISCH: DialogueFrameNew konnte nicht gefunden werden!");
            isShowing = false;
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowPopup(speakerName, message, visibleDuration));
    }

    // =========================
    // POPUP
    // =========================
    private IEnumerator ShowPopup(string speakerName, string message, float visibleDuration)
    {
        isShowing = true;

        // ROOT AKTIV
        gameObject.SetActive(true);

        // FRAME
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(true);

        // TEXT
        if (popupTextObject != null)
            popupTextObject.SetActive(true);

        // SPEAKER
        if (speakerNameObject != null)
            speakerNameObject.SetActive(true);

        // SPEAKER TEXT
        if (speakerNameText != null)
            speakerNameText.text = speakerName;

        // CLEAR
        if (popupText != null) popupText.text = "";
        if (legacyPopupText != null) legacyPopupText.text = "";

        // TYPEWRITER
        foreach (char letter in message)
        {
            if (popupText != null) popupText.text += letter;
            if (legacyPopupText != null) legacyPopupText.text += letter;

            yield return new WaitForSeconds(letterDelay);
        }

        // SICHTBAR
        yield return new WaitForSeconds(visibleDuration);

        HideAll();

        Debug.Log("DialogueUI: Message finished.");
        isShowing = false;
        }

    // =========================
    // HIDE
    // =========================
    public void HideAll()
    {
        // Clear text content
        if (popupText != null) popupText.text = "";
        if (legacyPopupText != null) legacyPopupText.text = "";

        // Only hide the frame if it's NOT the permanent Battle Chatbox
        if (DialogueFrameNew != null && DialogueFrameNew.name != "Chatbox")
        {
            DialogueFrameNew.SetActive(false);
        }

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null && speakerNameObject.name != "EnemyNameDisplay")
            speakerNameObject.SetActive(false);

        // SCRIPT ROOT BLEIBT AKTIV
        gameObject.SetActive(true);
    }

    // =========================
    // SOFORT HIDE
    // =========================
    private void HideAllImmediate()
    {
        if (popupText != null) popupText.text = "";
        if (legacyPopupText != null) legacyPopupText.text = "";

        if (DialogueFrameNew != null && DialogueFrameNew.name != "Chatbox")
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null && speakerNameObject.name != "EnemyNameDisplay")
            speakerNameObject.SetActive(false);
    }

    // =========================
    // STATUS
    // =========================
    public bool IsDialogueActive()
    {
        return isShowing;
    }
}