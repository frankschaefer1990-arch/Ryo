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
            Debug.Log($"DialogueUI: Local duplicate found on {gameObject.name}, destroying.");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeComponents();
        if (!isShowing) HideAll();
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        currentRoutine = null;
        isShowing = false;
        
        bool isBattle = scene.name.ToLower().Contains("battle") || scene.name.ToLower().Contains("kampf");
        
        if (DialogueFrameNew == null || !DialogueFrameNew.activeInHierarchy && !DialogueFrameNew.transform.root.gameObject.activeInHierarchy)
        {
             LocalReconnect();
        }
        
        HideAll(); 

        // If in battle, we might want to keep it hidden or disabled as requested
        if (isBattle && DialogueFrameNew != null)
        {
            DialogueFrameNew.SetActive(false);
            Debug.Log("DialogueUI: Hidden for Battle scene.");
        }
    }

    private void InitializeComponents()
    {
        if (DialogueFrameNew != null)
        {
            frameCanvasGroup = DialogueFrameNew.GetComponent<CanvasGroup>();
            if (frameCanvasGroup == null) frameCanvasGroup = DialogueFrameNew.AddComponent<CanvasGroup>();
            
            // Re-find children if they were lost but frame is still here
            if (popupText == null) {
                foreach (Transform t in DialogueFrameNew.GetComponentsInChildren<Transform>(true)) {
                    if (t.name == "PopupText" || t.name == "Text" || t.name == "DialogueText") {
                        popupTextObject = t.gameObject;
                        popupText = t.GetComponent<TextMeshProUGUI>();
                        break;
                    }
                }
            }
            
            // Always search for speaker name text to ensure it's the correct one
            foreach (Transform t in DialogueFrameNew.GetComponentsInChildren<Transform>(true)) {
                if (t.name == "SpeakerNameText" || t.name == "TextPlayerName" || t.name == "Name") {
                    speakerNameObject = t.gameObject;
                    speakerNameText = t.GetComponent<TextMeshProUGUI>();
                    if (t.name == "SpeakerNameText") break; // Prefer SpeakerNameText
                }
            }
        }
    }

    public void LocalReconnect()
    {
        if (DialogueFrameNew != null && DialogueFrameNew.scene.isLoaded) return;

        GameObject foundFrame = null;
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (GameObject root in s.GetRootGameObjects()) {
                if (root.name == "DialogueFrameNew" || root.name == "Chatbox") { foundFrame = root; break; }
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) {
                    if (t.name == "DialogueFrameNew" || t.name == "Chatbox") { foundFrame = t.gameObject; break; }
                }
                if (foundFrame != null) break;
            }
            if (foundFrame != null) break;
        }

        if (foundFrame != null) {
            DialogueFrameNew = foundFrame;
            InitializeComponents();
        }
    }

    public void ShowMessage(string speakerName, string message, float visibleDuration = 2.5f)
    {
        // Check if we are in a battle scene
        string sceneName = SceneManager.GetActiveScene().name.ToLower();
        if (sceneName.Contains("battle") || sceneName.Contains("kampf"))
        {
            Debug.Log("DialogueUI: Message blocked in battle scene: " + message);
            return;
        }

        if (DialogueFrameNew == null) LocalReconnect();
if (DialogueFrameNew == null) return;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        isShowing = true;
        currentRoutine = StartCoroutine(ShowPopup(speakerName, message, visibleDuration));
    }

    public void ShowMessage(string message) { ShowMessage(defaultSpeakerName, message); }

    private IEnumerator ShowPopup(string speakerName, string message, float visibleDuration)
    {
        isShowing = true;
        if (DialogueFrameNew != null) {
            DialogueFrameNew.SetActive(true);
            DialogueFrameNew.transform.SetAsLastSibling(); // Ensure it's on top of other UI
            if (frameCanvasGroup != null) {
                frameCanvasGroup.alpha = 1f;
                frameCanvasGroup.blocksRaycasts = true;
                frameCanvasGroup.interactable = true;
            }
        }

        if (popupTextObject != null) popupTextObject.SetActive(true);
        if (popupText != null) popupText.text = "";

        // Update speaker name - Prioritize the gold TextPlayerName
        if (DialogueFrameNew != null) {
            bool foundGold = false;
            // First pass: find and set the gold one
            foreach (var tmp in DialogueFrameNew.GetComponentsInChildren<TextMeshProUGUI>(true)) {
                if (tmp.name == "TextPlayerName") {
                    tmp.gameObject.SetActive(true);
                    tmp.text = speakerName;
                    foundGold = true;
                }
            }
            // Second pass: hide the others (like the gray SpeakerNameText)
            foreach (var tmp in DialogueFrameNew.GetComponentsInChildren<TextMeshProUGUI>(true)) {
                if (tmp.name == "SpeakerNameText" || tmp.name == "Name") {
                    if (foundGold) tmp.gameObject.SetActive(false);
                    else {
                        tmp.gameObject.SetActive(true);
                        tmp.text = speakerName;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(message)) {
            foreach (char letter in message) {
                if (popupText != null) popupText.text += letter;
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

