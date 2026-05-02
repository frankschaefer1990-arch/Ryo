using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("Main UI Elements")]
    public GameObject DialogueFrameNew;          // Neues großes Dialogfenster
    public GameObject popupTextObject;           // PopupText Objekt
    public TextMeshProUGUI popupText;            // Haupttext

    [Header("Speaker UI")]
    public GameObject speakerNameObject;         // NameBackground / Namebox
    public TextMeshProUGUI speakerNameText;      // Sprechername Text

    [Header("Default Speaker")]
    public string defaultSpeakerName = "Ryo";

    [Header("Typewriter Settings")]
    public float letterDelay = 0.04f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;

        // Alles am Anfang verstecken
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null)
            speakerNameObject.SetActive(false);
    }

    // Standard: Nutzt automatisch den Default Speaker aus dem Inspector
    public void ShowMessage(string message)
    {
        ShowMessage(defaultSpeakerName, message);
    }

    // Optional: Eigener Speaker direkt im Script
    public void ShowMessage(string speakerName, string message)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowPopup(speakerName, message));
    }

    private IEnumerator ShowPopup(string speakerName, string message)
    {
        // UI aktivieren
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(true);

        if (popupTextObject != null)
            popupTextObject.SetActive(true);

        // Sprecher anzeigen
        if (speakerNameObject != null)
            speakerNameObject.SetActive(true);

        if (speakerNameText != null)
            speakerNameText.text = speakerName;

        // Text leeren
        if (popupText != null)
            popupText.text = "";

        // Typewriter Effekt
        foreach (char letter in message)
        {
            if (popupText != null)
            {
                popupText.text += letter;
            }

            yield return new WaitForSeconds(letterDelay);
        }

        // Sichtbar bleiben
        yield return new WaitForSeconds(2f);

        // UI ausblenden
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null)
            popupTextObject.SetActive(false);

        if (speakerNameObject != null)
            speakerNameObject.SetActive(false);
    }
}