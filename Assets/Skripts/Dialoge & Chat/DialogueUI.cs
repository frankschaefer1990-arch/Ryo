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

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        gameObject.SetActive(true);
        ReconnectUI();
        HideAll();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnSystemsReady += ReconnectUI;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnSystemsReady -= ReconnectUI;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // WICHTIG: Nach dem Szenenwechsel müssen wir die UI neu verbinden
        ReconnectUI();
    }

    public void ReconnectUI()
    {
        GameObject targetCanvas = null;
        
        // 1. Suche nach dem Canvas in der aktuellen Szene
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases) {
            if (c.name != "SoftwareCursorCanvas" && c.name != "SoftwareCursor") {
                if (c.gameObject.scene == SceneManager.GetActiveScene()) {
                    targetCanvas = c.gameObject;
                    
                    // Sichtbarkeit des Canvas erzwingen
                    if (!targetCanvas.activeSelf) targetCanvas.SetActive(true);
                    
                    // Kamera-Link sicherstellen
                    if (c.renderMode == RenderMode.ScreenSpaceCamera && c.worldCamera == null) {
                        c.worldCamera = Camera.main;
                    }
                    break;
                }
                targetCanvas = c.gameObject;
            }
        }

        if (targetCanvas == null && GameManager.Instance != null && GameManager.Instance.canvas != null) {
            targetCanvas = GameManager.Instance.canvas;
        }

        if (targetCanvas == null) {
            Debug.LogError("DialogueUI: KEIN CANVAS GEFUNDEN!");
            return;
        }

        // 2. Suche den DialogueFrame im Canvas
        Transform frame = targetCanvas.transform.Find("DialogueFrameNew");
        if (frame == null) {
            // Tiefensuche
            foreach (Transform t in targetCanvas.GetComponentsInChildren<Transform>(true)) {
                if (t.name == "DialogueFrameNew") { frame = t; break; }
            }
        }

        if (frame != null) {
            DialogueFrameNew = frame.gameObject;
            
            // Parent (LockedDoorPopup) finden
            if (frame.parent != null) {
                // CanvasGroup auf dem Parent?
                CanvasGroup parentCG = frame.parent.GetComponent<CanvasGroup>();
                if (parentCG != null) {
                    canvasGroup = parentCG;
                }
            }

            // Textfelder finden
            Transform popup = frame.Find("PopupText");
            if (popup != null) {
                popupTextObject = popup.gameObject;
                popupText = popup.GetComponent<TextMeshProUGUI>();
            }

            // Namensfeld finden
            Transform speakerT = frame.Find("TextPlayerName");
            if (speakerT == null) speakerT = frame.Find("SpeakerNameText");
            if (speakerT != null) {
                speakerNameObject = speakerT.gameObject;
                speakerNameText = speakerT.GetComponent<TextMeshProUGUI>();
            }
            
            Debug.Log($"DialogueUI: Verbunden mit {targetCanvas.name} -> {DialogueFrameNew.name}. Speaker: {speakerNameObject?.name}");
        } else {
            Debug.LogWarning("DialogueUI: DialogueFrameNew konnte im Canvas nicht gefunden werden!");
        }
    }

    public void ShowMessage(string speakerName, string message, float visibleDuration = 0.8f)
    {
        isShowing = true;
        Debug.Log($"DialogueUI: ShowMessage starting. Speaker: {speakerName}, Msg: {message}");
        
        ReconnectUI();
        if (DialogueFrameNew == null) {
            Debug.LogError("DialogueUI: DialogueFrameNew ist null!");
            isShowing = false;
            return;
        }

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowPopup(speakerName, message, visibleDuration));
    }

    public void ShowMessage(string message)
    {
        ShowMessage(defaultSpeakerName, message);
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
            Debug.Log($"DialogueUI: CanvasGroup Alpha set to {canvasGroup.alpha}");
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

        Debug.Log("DialogueUI: Typewriter finished. Waiting duration: " + visibleDuration);
        yield return new WaitForSeconds(visibleDuration);
        
        Debug.Log("DialogueUI: ShowPopup calling HideAll.");
        HideAll();
    }

    public void HideAll()
    {
        Debug.Log("DialogueUI: HideAll called.");
        if (DialogueFrameNew != null)
            DialogueFrameNew.SetActive(false);

        if (popupTextObject != null) popupTextObject.SetActive(false);
        if (speakerNameObject != null && speakerNameObject.name != "EnemyNameDisplay")
            speakerNameObject.SetActive(false);
            
        if (canvasGroup != null) {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
            
        isShowing = false;
    }

    public bool IsDialogueActive() => isShowing;
}