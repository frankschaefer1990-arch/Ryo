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

        // GANZ WICHTIG:
        // SCRIPT OBJEKT SELBST AKTIV LASSEN
        // NUR UI ELEMENTE AUSBLENDEN
        gameObject.SetActive(true);

        ReconnectUI();

        HideAllImmediate();
    }

    // =========================
    // START
    // =========================
    private void Start()
    {
        ReconnectUI();

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
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        // FRAME
        if (DialogueFrameNew == null)
        {
            Transform frame = canvas.transform.Find("LockedDoorPopup/DialogueFrameNew");

            if (frame == null)
                frame = canvas.transform.Find("DialogueFrameNew");

            if (frame != null)
                DialogueFrameNew = frame.gameObject;
        }

        // POPUP TEXT
        if (popupTextObject == null && DialogueFrameNew != null)
        {
            Transform popup = DialogueFrameNew.transform.Find("PopupText");

            if (popup != null)
                popupTextObject = popup.gameObject;
        }

        if (popupText == null && popupTextObject != null)
        {
            popupText = popupTextObject.GetComponent<TextMeshProUGUI>();
        }

        // SPEAKER
        if (speakerNameObject == null && DialogueFrameNew != null)
        {
            Transform speaker = DialogueFrameNew.transform.Find("SpeakerNameText");

            if (speaker == null)
                speaker = DialogueFrameNew.transform.Find("SpeakerName");

            if (speaker != null)
                speakerNameObject = speaker.gameObject;
        }

        if (speakerNameText == null && speakerNameObject != null)
        {
            speakerNameText = speakerNameObject.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    // =========================
    // STANDARD MESSAGE
    // =========================
    public void ShowMessage(string message)
    {
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
        // FALLS OBJECT DEAKTIVIERT WURDE
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        ReconnectUI();

        // WICHTIG:
        // Wenn Frame fehlt -> kein Coroutine Start
        if (DialogueFrameNew == null)
        {
            Debug.LogWarning("DialogueFrameNew nicht gefunden!");
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