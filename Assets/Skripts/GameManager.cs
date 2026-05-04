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
    }

    // =========================
    // SCENE LOADED
    // =========================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDuplicates();

        ReconnectCoreReferences();

        ReconnectSystems();

        MovePlayerToSpawn();
    }

    // =========================
    // CORE REFERENCES AKTUALISIEREN
    // =========================
    void ReconnectCoreReferences()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (canvas == null)
        {
            Canvas foundCanvas = FindAnyObjectByType<Canvas>();

            if (foundCanvas != null)
            {
                canvas = foundCanvas.gameObject;
                DontDestroyOnLoad(canvas); // Sicherstellen, dass er bleibt!
                Debug.Log("Neuer Canvas gefunden und persistent gemacht: " + canvas.name);
            }
        }

        if (eventSystem == null)
        {
            EventSystem foundEventSystem = FindAnyObjectByType<EventSystem>();

            if (foundEventSystem != null)
            {
                eventSystem = foundEventSystem.gameObject;
                DontDestroyOnLoad(eventSystem);
                Debug.Log("Neues EventSystem persistent gemacht.");
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
                // Wenn das Duplikat in DDOL ist, aber wir ein anderes wollen? 
                // Normalerweise behalten wir die Instanz-Variable 'player'
                Destroy(p);
            }
            else if (player == null)
            {
                player = p;
            }
        }

        // CANVAS
        // WICHTIG: Inaktive einschließen!
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Canvas c in canvases)
        {
            // Wenn wir bereits einen persistenten Canvas haben, lösche alle anderen
            if (canvas != null && c.gameObject != canvas)
            {
                // Sicherheit Check: Wenn der andere Canvas mehr Kinder hat? 
                // Das Risiko ist zu hoch, wir bleiben bei der etablierten Instanz.
                Destroy(c.gameObject);
            }
            else if (canvas == null)
            {
                // Erster Canvas wird der Chef
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
            else if (eventSystem == null)
            {
                eventSystem = e.gameObject;
                DontDestroyOnLoad(eventSystem);
            }
        }

        // QUEST MANAGER
        QuestManager[] quests = FindObjectsByType<QuestManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (QuestManager q in quests)
        {
            if (questManager != null && q.gameObject != questManager)
            {
                Destroy(q.gameObject);
            }
            else if (questManager == null)
            {
                questManager = q.gameObject;
                DontDestroyOnLoad(questManager);
            }
        }
    }

    // =========================
    // SYSTEME NEU VERBINDEN
    // =========================
    void ReconnectSystems()
    {
        // =========================
        // UI MANAGER (I / B / T)
        // =========================
        MyUIManager uiManager = FindAnyObjectByType<MyUIManager>();

        if (uiManager != null)
        {
            uiManager.ReconnectUIFromGameManager();
        }

        // =========================
        // DIALOGUE UI (LOCKEDDOORPOPUP)
        // ROOT MUSS AKTIV BLEIBEN
        // =========================
        DialogueUI dialogue = FindAnyObjectByType<DialogueUI>();

        if (dialogue != null)
        {
            dialogue.gameObject.SetActive(true);
        }

        // =========================
        // SHOP MANAGER
        // =========================
        ShopManager[] shops = FindObjectsByType<ShopManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (ShopManager shop in shops)
        {
            if (shop != null)
            {
                shop.ReconnectShop();
                shop.SetupButtonsPublic();
            }
        }

        // =========================
        // CAMERA FOLLOW
        // =========================
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

        // =========================
        // TEMPLE INTRO
        // =========================
        TempleCameraIntro templeIntro = FindAnyObjectByType<TempleCameraIntro>();

        if (templeIntro != null && player != null)
        {
            templeIntro.player = player.transform;
        }

        Debug.Log("Systeme erfolgreich neu verbunden.");
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