using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("Main UI Elements")]
    public GameObject DialogueFrameNew;
    public GameObject popupTextObject;
    public TextMeshProUGUI popupText;
    public UnityEngine.UI.Text legacyPopupText; 

    [Header("Speaker UI")]
    public GameObject speakerNameObject;
    public TextMeshProUGUI speakerNameText;

    [Header("Default Speaker")]
    public string defaultSpeakerName = "Ryo";

    [Header("Typewriter Settings")]
    public float letterDelay = 0.05f; 

    private Coroutine currentRoutine;
    private bool isShowing = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;

        // Try to find a CanvasGroup on the frame's parent instead of the root
        LocalReconnect();
        HideAll();
    }

    // ...

    public void LocalReconnect()
    {
        // Force finding the DialogueFrameNew child if not assigned
        if (DialogueFrameNew == null) {
            DialogueFrameNew = transform.Find("DialogueFrameNew")?.gameObject;
            if (DialogueFrameNew == null) DialogueFrameNew = transform.Find("Chatbox")?.gameObject;
            if (DialogueFrameNew == null) DialogueFrameNew = transform.Find("LockedDoorPopup/DialogueFrameNew")?.gameObject;
        }

        if (DialogueFrameNew != null) {
            Transform frame = DialogueFrameNew.transform;
            
            // Get CanvasGroup from the popup parent (LockedDoorPopup)
            canvasGroup = frame.GetComponentInParent<CanvasGroup>();

            if (popupTextObject == null) {
                Transform p = frame.Find("PopupText");
                if (p != null) {
                    popupTextObject = p.gameObject;
                    popupText = p.GetComponent<TextMeshProUGUI>();
                }
            }

            if (speakerNameObject == null) {
                Transform s = frame.Find("TextPlayerName");
                if (s == null) s = frame.Find("SpeakerNameText");
                if (s != null) {
                    speakerNameObject = s.gameObject;
                    speakerNameText = s.GetComponent<TextMeshProUGUI>();
                }
            }
        }
    }

    public void ShowMessage(string speakerName, string message, float visibleDuration = 0.8f)
    {
        // Ensure local references are set
        LocalReconnect();

        if (currentRoutine != null) 
        {
            StopCoroutine(currentRoutine);
        }
        
        isShowing = true;
        Debug.Log($"DialogueUI: ShowMessage for {speakerName}");
        currentRoutine = StartCoroutine(ShowPopup(speakerName, message, visibleDuration));
    }

    public void ShowMessage(string message)
    {
        ShowMessage(defaultSpeakerName, message);
    }

    private void OnDisable()
    {
        isShowing = false;
    }

    private IEnumerator ShowPopup(string speakerName, string message, float visibleDuration)
    {
        isShowing = true;
        Debug.Log($"DialogueUI: ShowPopup started for '{speakerName}': {message}");
        
        // ALLES ERZWINGEN
        if (DialogueFrameNew != null) {
            DialogueFrameNew.SetActive(true);
            if (DialogueFrameNew.transform.parent != null) 
                DialogueFrameNew.transform.parent.gameObject.SetActive(true);
        }
        
        gameObject.SetActive(true);

        if (canvasGroup != null) {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (DialogueFrameNew != null) {
            foreach (Transform t in DialogueFrameNew.transform) {
                if (t.name == "Name" || (t.name.Contains("Name") && t.gameObject != speakerNameObject))
                    t.gameObject.SetActive(false);
            }
        }

        if (popupTextObject != null) popupTextObject.SetActive(true);
        if (popupText != null) popupText.text = "";
        if (legacyPopupText != null) legacyPopupText.text = "";

        if (speakerNameObject != null) {
            speakerNameObject.SetActive(true);
            if (speakerNameText != null) {
                speakerNameText.text = speakerName;
                speakerNameText.color = new Color(1f, 0.84f, 0f); // Gold Color
                speakerNameText.fontSize = 28f; 
            }
        }

        if (!string.IsNullOrEmpty(message)) {
            foreach (char letter in message) {
                if (popupText != null) popupText.text += letter;
                if (legacyPopupText != null) legacyPopupText.text += letter;
                yield return new WaitForSeconds(letterDelay);
            }
        }

        yield return new WaitForSeconds(visibleDuration);
        
        HideAll();
    }

    public void HideAll()
    {
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null) popupTextObject.SetActive(false);
        
        // NEVER hide the EnemyNameDisplay automatically, as it belongs to the BattleUI
        if (speakerNameObject != null && speakerNameObject.name != "EnemyNameDisplay")
            speakerNameObject.SetActive(false);
            
        if (canvasGroup != null && canvasGroup.gameObject != gameObject) {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
            
        isShowing = false;
    }

    public bool IsDialogueActive() => isShowing;
}