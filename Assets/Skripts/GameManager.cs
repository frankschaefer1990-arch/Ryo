using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Core")]
    public GameObject player;
    public GameObject canvas;
    public GameObject eventSystem;

    [Header("Optional Systems")]
    public GameObject questManager;

    [Header("Spawn System")]
    public string spawnPointName = "";

    // Das zentrale Signal für alle Sub-Systeme
    public static System.Action OnSystemsReady;

    private void Awake()
    {
        // =========================
        // SINGLETON
        // =========================
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // =========================
        // PLAYER FINDEN
        // =========================
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer;
        }

        // =========================
        // CANVAS FINDEN
        // =========================
        if (canvas == null)
        {
            Canvas foundCanvas = FindAnyObjectByType<Canvas>();

            if (foundCanvas != null)
                canvas = foundCanvas.gameObject;
        }

        // =========================
        // EVENT SYSTEM FINDEN
        // =========================
        if (eventSystem == null)
        {
            EventSystem foundEventSystem = FindAnyObjectByType<EventSystem>();

            if (foundEventSystem != null)
                eventSystem = foundEventSystem.gameObject;
        }

        // =========================
        // QUEST MANAGER OPTIONAL
        // =========================
        if (questManager == null)
        {
            QuestManager foundQuest = FindAnyObjectByType<QuestManager>();

            if (foundQuest != null)
                questManager = foundQuest.gameObject;
        }

        // =========================
        // PERSISTENT
        // =========================
        if (player != null)
            DontDestroyOnLoad(player);

        if (canvas != null)
            DontDestroyOnLoad(canvas);

        if (eventSystem != null)
            DontDestroyOnLoad(eventSystem);

        if (questManager != null)
            DontDestroyOnLoad(questManager);

        // =========================
        // SYSTEME VERBINDEN
        // =========================
        ReconnectSystems();

        // Signal auch beim ersten Start feuern
        OnSystemsReady?.Invoke();
        }

        // =========================
        // SCENE LOADED
        // =========================
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
        Debug.Log("GameManager: Szene geladen, starte Cleanup und Reconnect...");
        
        CleanupDuplicates();

        ReconnectCoreReferences();

        // Erst ReconnectSystems nach dem Cleanup
        ReconnectSystems();

        MovePlayerToSpawn();

        // SIGNAL SENDEN - Jetzt sind alle Sub-Systeme dran
        Debug.Log("GameManager: Sende Signal OnSystemsReady...");
        OnSystemsReady?.Invoke();
        }

    // =========================
    // CORE REFERENCES AKTUALISIEREN
    // =========================
    void ReconnectCoreReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        // WICHTIG: Prüfen, ob der aktuelle Canvas überhaupt Gameplay-UI enthält
        bool currentCanvasIsInvalid = canvas == null || 
                                     (canvas.transform.Find("InventoryPanel") == null && 
                                      canvas.transform.Find("DialogueFrameNew") == null &&
                                      canvas.transform.Find("LockedDoorPopup") == null);

        if (currentCanvasIsInvalid)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                // Suche einen Canvas, der tatsächliche UI-Elemente hat
                if (c.transform.Find("InventoryPanel") != null || 
                    c.transform.Find("DialogueFrameNew") != null ||
                    c.transform.Find("LockedDoorPopup") != null)
                {
                    canvas = c.gameObject;
                    DontDestroyOnLoad(canvas);
                    Debug.Log("Gültigen Gameplay-Canvas gefunden: " + canvas.name);
                    break;
                }
            }
        }

        if (eventSystem == null)
        {
            EventSystem foundEventSystem = FindAnyObjectByType<EventSystem>();
            if (foundEventSystem != null)
            {
                eventSystem = foundEventSystem.gameObject;
                DontDestroyOnLoad(eventSystem);
            }
        }
    }

    // =========================
    // DOPPELTE OBJEKTE ENTFERNEN
    // =========================
    void CleanupDuplicates()
    {
        // PLAYER
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (player != null && p != player)
            {
                Destroy(p);
            }
            else if (player == null)
            {
                player = p;
                DontDestroyOnLoad(player);
            }
        }

        // CANVAS
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            // Wenn wir einen Canvas haben, lösche alle anderen (die NICHT unser Haupt-Canvas sind)
            if (canvas != null && c.gameObject != canvas)
            {
                // Aber nur löschen, wenn der neue Canvas keine "bessere" UI hat
                bool newHasUI = c.transform.Find("InventoryPanel") != null || 
                               c.transform.Find("DialogueFrameNew") != null;
                
                bool oldHasUI = canvas.transform.Find("InventoryPanel") != null || 
                               canvas.transform.Find("DialogueFrameNew") != null;

                if (newHasUI && !oldHasUI)
                {
                    Debug.Log("Tausche leeren persistenten Canvas gegen neuen Gameplay-Canvas aus.");
                    GameObject oldCanvas = canvas;
                    canvas = c.gameObject;
                    DontDestroyOnLoad(canvas);
                    Destroy(oldCanvas);
                }
                else
                {
                    Debug.Log("Lösche redundanten Canvas: " + c.gameObject.name);
                    Destroy(c.gameObject);
                }
            }
            else if (canvas == null)
            {
                canvas = c.gameObject;
                DontDestroyOnLoad(canvas);
            }
        }

        // EVENT SYSTEM
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EventSystem e in systems)
        {
            if (eventSystem != null && e.gameObject != eventSystem)
            {
                Destroy(e.gameObject);
            }
        }

        // QUEST MANAGER
        QuestManager[] quests = FindObjectsByType<QuestManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (QuestManager q in quests)
        {
            if (QuestManager.Instance != null && q != QuestManager.Instance)
            {
                Destroy(q.gameObject);
            }
            else if (QuestManager.Instance == null)
            {
                // QuestManager.Instance wird in seiner eigenen Awake-Methode gesetzt
                DontDestroyOnLoad(q.gameObject);
            }
        }
        }

    // =========================
    // SYSTEME NEU VERBINDEN
    // =========================
    void ReconnectSystems()
    {
        // Wir rufen nur noch Systeme auf, die (noch) nicht auf das Signal hören
        // Alle UI-Systeme (Inventar, Shop, Stats) erledigen das jetzt selbstständig!
        
        ReconnectOtherSystems();
    }

    void ReconnectOtherSystems()
    {
        // CAMERA FOLLOW
        CameraFollow cameraFollow = FindAnyObjectByType<CameraFollow>();
        if (cameraFollow != null && player != null)
        {
            cameraFollow.player = player.transform;
            GameObject boundsObject = GameObject.Find("CameraBounds");
            if (boundsObject != null)
            {
                BoxCollider2D bounds = boundsObject.GetComponent<BoxCollider2D>();
                if (bounds != null)
                {
                    cameraFollow.boundsCollider = bounds;
                    cameraFollow.UpdateBounds();
                }
            }
        }

        // TEMPLE INTRO
        TempleCameraIntro templeIntro = FindAnyObjectByType<TempleCameraIntro>();
        if (templeIntro != null && player != null)
        {
            templeIntro.player = player.transform;
        }

        Debug.Log("Alle Systeme erfolgreich synchronisiert.");
    }

    // =========================
    // PLAYER ZUM SPAWN
    // =========================
    void MovePlayerToSpawn()
    {
        if (string.IsNullOrEmpty(spawnPointName))
            return;

        if (player == null)
            return;

        GameObject spawn = GameObject.Find(spawnPointName);

        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;

            Debug.Log("Player gespawnt bei: " + spawnPointName);
        }
        else
        {
            Debug.LogWarning("SpawnPoint nicht gefunden: " + spawnPointName);
        }
    }

    // =========================
    // SCENE LOAD
    // =========================
    public void LoadScene(string sceneName, string newSpawnPoint)
    {
        spawnPointName = newSpawnPoint;

        SceneManager.LoadScene(sceneName);
    }

    // OPTIONAL
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // =========================
    // CLEANUP
    // =========================
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}