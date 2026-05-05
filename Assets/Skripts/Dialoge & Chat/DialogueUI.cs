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

    [Header("Speaker UI")]
    public GameObject speakerNameObject;
    public TextMeshProUGUI speakerNameText;

    [Header("Default Speaker")]
    public string defaultSpeakerName = "Ryo";

    [Header("Typewriter Settings")]
    public float letterDelay = 0.04f;

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

    // =========================
    // START
    // =========================
    private void Start()
    {
        // ReconnectUI() wird bereits in Awake und vom GameManager aufgerufen
        
        if (popupText != null)
        {
            popupText.enableAutoSizing = true;
            popupText.fontSizeMin = 12;
            popupText.fontSizeMax = 32;
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
            
            if (frame == null)
            {
                foreach (Transform t in canvasTransform.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "DialogueFrameNew") { frame = t; break; }
                }
            }

            if (frame != null) DialogueFrameNew = frame.gameObject;
        }

        // Kinder IMMER neu verknüpfen wenn sie fehlen oder ungültig sind
        if (DialogueFrameNew != null)
        {
            if (popupTextObject == null || popupTextObject.scene.name == null)
            {
                Transform popup = DialogueFrameNew.transform.Find("PopupText");
                if (popup != null) popupTextObject = popup.gameObject;
            }

            if (popupText == null && popupTextObject != null)
            {
                popupText = popupTextObject.GetComponent<TextMeshProUGUI>();
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
        ShowMessage(defaultSpeakerName, message, 2f);
    }

    // =========================
    // SPEAKER MESSAGE
    // =========================
    public void ShowMessage(string speakerName, string message)
    {
        ShowMessage(speakerName, message, 2f);
    }

    // =========================
    // CUSTOM MESSAGE
    // =========================
    public void ShowMessage(string speakerName, string message, float visibleDuration)
    {
        // Vor jedem Anzeigen sicherstellen, dass die UI-Elemente verknüpft sind
        ReconnectUI();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // WICHTIG: Wenn Frame immer noch fehlt -> Notfall-Suche im gesamten Projekt
        if (DialogueFrameNew == null)
        {
            Debug.LogWarning("DialogueFrameNew fehlt! Suche Ersatz...");
            GameObject found = GameObject.Find("DialogueFrameNew");
            if (found != null) DialogueFrameNew = found;
        }

        if (DialogueFrameNew == null)
        {
            Debug.LogError("KRITISCH: DialogueFrameNew konnte nicht gefunden werden!");
            return;
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
        if (popupText != null)
            popupText.text = "";

        // TYPEWRITER
        foreach (char letter in message)
        {
            if (popupText != null)
                popupText.text += letter;

            yield return new WaitForSeconds(letterDelay);
        }

        // SICHTBAR
        yield return new WaitForSeconds(visibleDuration);

        HideAll();

        isShowing = false;
    }

    // =========================
    // HIDE
    // =========================
    public void HideAll()
    {
        // NUR UI ELEMENTE
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null)
            speakerNameObject.SetActive(false);

        // SCRIPT ROOT BLEIBT AKTIV
        gameObject.SetActive(true);
    }

    // =========================
    // SOFORT HIDE
    // =========================
    private void HideAllImmediate()
    {
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null)
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