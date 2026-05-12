using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

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
    private CanvasGroup frameCanvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // If we are under PersistentSystems, we take over.
            if (transform.parent != null && transform.parent.name == "PersistentSystems")
            {
                Debug.Log("DialogueUI: Persistent instance taking over.");
                if (Instance != null && Instance.gameObject != null) Destroy(Instance.gameObject);
                Instance = this;
            }
            else
            {
                // We are a local duplicate (likely from a scene instance while a persistent one already exists)
                Debug.Log("DialogueUI: Local duplicate found, destroying GameObject.");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Instance = this;
        }

        // Only persist if we are a root object
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        LocalReconnect();
        if (!isShowing) HideAll();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"DialogueUI: Scene '{scene.name}' loaded. Resetting UI state.");
        StopAllCoroutines();
        currentRoutine = null;
        isShowing = false;
        LocalReconnect();
        HideAll(); 
    }

    public void LocalReconnect()
    {
        // Only search for a new frame if our current one is missing or destroyed
        if (DialogueFrameNew != null && DialogueFrameNew.gameObject != null && DialogueFrameNew.scene.isLoaded)
        {
            // Already connected to a valid, loaded frame. No need to search.
            return;
        }

        var allFrames = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject foundFrame = null;
        foreach(var obj in allFrames) {
            if ((obj.name == "DialogueFrameNew" || obj.name == "Chatbox") && obj.scene.isLoaded) {
                foundFrame = obj;
                break;
            }
        }

        if (foundFrame != null) {
            DialogueFrameNew = foundFrame;
        }

        if (DialogueFrameNew != null) {
            frameCanvasGroup = DialogueFrameNew.GetComponent<CanvasGroup>();
            if (frameCanvasGroup == null) frameCanvasGroup = DialogueFrameNew.AddComponent<CanvasGroup>();

            popupTextObject = FindDeepChild(DialogueFrameNew.transform, "PopupText");
            if (popupTextObject == null) popupTextObject = FindDeepChild(DialogueFrameNew.transform, "Text");
            if (popupTextObject == null) popupTextObject = FindDeepChild(DialogueFrameNew.transform, "DialogueText");
            
            if (popupTextObject != null) {
                popupText = popupTextObject.GetComponent<TextMeshProUGUI>();
            }

            speakerNameObject = FindDeepChild(DialogueFrameNew.transform, "TextPlayerName");
            if (speakerNameObject == null) speakerNameObject = FindDeepChild(DialogueFrameNew.transform, "SpeakerNameText");
            if (speakerNameObject == null) speakerNameObject = FindDeepChild(DialogueFrameNew.transform, "Name");

            if (speakerNameObject != null) {
                speakerNameText = speakerNameObject.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private GameObject FindDeepChild(Transform parent, string name) {
        foreach(Transform child in parent.GetComponentsInChildren<Transform>(true)) {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }

    public void ShowMessage(string speakerName, string message, float visibleDuration = 0.8f)
    {
        if (Instance == null) Instance = this;
        Debug.Log($"DialogueUI: ShowMessage called by '{speakerName}': {message}");
        
        LocalReconnect();
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        isShowing = true;
        currentRoutine = StartCoroutine(ShowPopup(speakerName, message, visibleDuration));
    }

    public void ShowMessage(string message) { ShowMessage(defaultSpeakerName, message); }

    private IEnumerator ShowPopup(string speakerName, string message, float visibleDuration)
    {
        isShowing = true;
        LocalReconnect();

        if (DialogueFrameNew != null) {
            DialogueFrameNew.SetActive(true);
            if (frameCanvasGroup != null) {
                frameCanvasGroup.alpha = 1f;
                frameCanvasGroup.blocksRaycasts = true;
                frameCanvasGroup.interactable = true;
            }
            
            foreach (Transform t in DialogueFrameNew.transform) {
                if ((t.name.Contains("Name") || t.name == "Name") && t.gameObject != speakerNameObject)
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
                speakerNameText.color = new Color(1f, 0.84f, 0f);
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
        if (currentRoutine != null) { StopCoroutine(currentRoutine); currentRoutine = null; }
        
        if (DialogueFrameNew != null) {
            DialogueFrameNew.SetActive(false);
            if (frameCanvasGroup != null) {
                frameCanvasGroup.alpha = 0f;
                frameCanvasGroup.blocksRaycasts = false;
                frameCanvasGroup.interactable = false;
            }
        }
        if (popupTextObject != null) popupTextObject.SetActive(false);
        if (speakerNameObject != null && speakerNameObject.name != "EnemyNameDisplay") speakerNameObject.SetActive(false);
        isShowing = false;
    }

    public bool IsDialogueActive() => isShowing;
}
