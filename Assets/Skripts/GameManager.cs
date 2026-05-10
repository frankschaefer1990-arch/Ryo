using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Core")]
    public GameObject player;
    public GameObject canvas;
    public GameObject eventSystem;
    public GameObject mainCamera;

    [Header("Optional Systems")]
    public GameObject questManager;

    [Header("Spawn System")]
    public string spawnPointName = "";

    private bool isSceneLoading = false;

    public static System.Action OnSystemsReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Root logic: If we are in the persistent root, don't detach. 
        // If we are loose, become persistent.
        if (transform.parent == null || transform.parent.name != "PersistentSystems")
        {
            DontDestroyOnLoad(gameObject);
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializePersistentSystems();
        ReconnectSystems();
        OnSystemsReady?.Invoke();
    }

    private void InitializePersistentSystems()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            player.transform.SetParent(null);
            DontDestroyOnLoad(player);
        }

        if (canvas == null) {
            Canvas c = FindAnyObjectByType<Canvas>();
            if (c != null && c.name != "SoftwareCursorCanvas") canvas = c.gameObject;
        }
        if (canvas != null) {
            canvas.transform.SetParent(null);
            DontDestroyOnLoad(canvas);
        }

        if (eventSystem == null) {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null) eventSystem = es.gameObject;
        }
        if (eventSystem != null) {
            eventSystem.transform.SetParent(null);
            DontDestroyOnLoad(eventSystem);
        }

        if (questManager == null) {
            QuestManager qm = FindAnyObjectByType<QuestManager>();
            if (qm != null) questManager = qm.gameObject;
        }
        if (questManager != null) {
            questManager.transform.SetParent(null);
            DontDestroyOnLoad(questManager);
        }

        if (mainCamera == null) {
            Camera cam = Camera.main;
            if (cam != null) mainCamera = cam.gameObject;
        }
        if (mainCamera != null) {
            mainCamera.transform.SetParent(null);
            DontDestroyOnLoad(mainCamera);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try {
            Debug.Log($"GameManager: Scene Loaded [{scene.name}].");
            isSceneLoading = false;

            // Start a safe, delayed cleanup for builds
            StartCoroutine(SafeBuildCleanupRoutine(scene.name));
            
            // Refind Player
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null && player != null && foundPlayer != player)
            {
                if (foundPlayer.scene.name != "DontDestroyOnLoad") Destroy(foundPlayer);
            }
            else if (foundPlayer != null) player = foundPlayer;

            ReconnectSystems();
            MovePlayerToSpawn();

            if (player != null)
            {
                var pm = player.GetComponent<PlayerMovement>();
                if (pm != null) pm.ResetMovementState();
            }
            
            Invoke(nameof(NotifySystemsReady), 0.2f);
        }
        catch (System.Exception e) {
            Debug.LogError("GameManager: Error in OnSceneLoaded: " + e.Message);
        }
    }

    private IEnumerator SafeBuildCleanupRoutine(string sceneName)
    {
        // Wait one frame to ensure Unity's internal scene management is stable
        yield return null;

        // Cleanup EventSystems
        var allEventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var es in allEventSystems)
        {
            if (eventSystem != null && es.gameObject != eventSystem)
            {
                Destroy(es.gameObject);
            }
        }

        // Handle Camera for BattleScene
        if (sceneName.Contains("Battle"))
        {
            if (mainCamera != null) 
            {
                var cam = mainCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;
                var listener = mainCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
        }
        else
        {
            if (mainCamera != null) 
            {
                mainCamera.SetActive(true);
                var cam = mainCamera.GetComponent<Camera>();
                if (cam != null) cam.enabled = true;
            }
            CleanupDuplicates();
        }
    }

    private void NotifySystemsReady() {
        OnSystemsReady?.Invoke();
    }

    void CleanupDuplicates()
    {
        try {
            // Camera cleanup - Critical for Unity 6
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                if (mainCamera != null && cam.gameObject != mainCamera && cam.CompareTag("MainCamera"))
                {
                    Debug.Log($"GameManager: Neutralizing duplicate Main Camera on [{cam.gameObject.name}].");
                    cam.tag = "Untagged"; // Crucial: Remove tag first to stop Unity/Cinemachine tracking
                    cam.enabled = false;
                    Destroy(cam.gameObject);
                }
                else if (mainCamera == null && cam.CompareTag("MainCamera"))
                {
                    mainCamera = cam.gameObject;
                    mainCamera.transform.SetParent(null);
                    DontDestroyOnLoad(mainCamera);
                }
            }

            // EventSystem - Absolute control
            EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EventSystem e in systems) {
                if (eventSystem != null && e.gameObject != eventSystem) {
                    Debug.Log($"GameManager: Nuking duplicate EventSystem on [{e.gameObject.name}].");
                    e.enabled = false;
                    Destroy(e.gameObject);
                }
                else if (eventSystem == null) { 
                    eventSystem = e.gameObject; 
                    eventSystem.transform.SetParent(null); 
                    DontDestroyOnLoad(eventSystem); 
                }
            }

            // Manager Cleanup
            var uis = FindObjectsByType<MyUIManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var u in uis) if (MyUIManager.Instance != null && u != MyUIManager.Instance) Destroy(u.gameObject);
            
            var stats = FindObjectsByType<PlayerStats>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var s in stats) {
                if (PlayerStats.Instance != null && s != PlayerStats.Instance) {
                    if (s.gameObject.tag == "Player" && s.gameObject.scene.name == "DontDestroyOnLoad") {
                        // Never destroy the persistent player!
                        continue;
                    }
                    Destroy(s.gameObject);
                }
            }

            var invs = FindObjectsByType<InventoryManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var i in invs) {
                if (InventoryManager.Instance != null && i != InventoryManager.Instance) {
                    if (i.gameObject.scene.name == "DontDestroyOnLoad") continue;
                    Destroy(i.gameObject);
                }
            }
            
            // Canvas logic
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases) {
                if (canvas != null && c.gameObject != canvas) {
                    if (c.name == "SoftwareCursorCanvas") continue;
                    if (c.name.Contains("Battle") || c.name.Contains("Kampf")) continue;
                    if (c.gameObject.scene.name != "DontDestroyOnLoad" && c.gameObject.scene.name != null) {
                        Destroy(c.gameObject);
                    }
                }
            }
        } catch (System.Exception ex) {
            Debug.LogError("GameManager: Error in CleanupDuplicates: " + ex.Message);
        }
    }

    void ReconnectSystems()
    {
        CameraFollow follow = FindAnyObjectByType<CameraFollow>();
        if (follow == null && mainCamera != null) follow = mainCamera.GetComponent<CameraFollow>();

        if (follow != null && player != null) {
            follow.player = player.transform;
            GameObject bounds = GameObject.Find("CameraBounds");
            if (bounds != null) {
                BoxCollider2D b = bounds.GetComponent<BoxCollider2D>();
                if (b != null) { follow.boundsCollider = b; follow.UpdateBounds(); }
            }
        }
    }

    void MovePlayerToSpawn()
    {
        if (string.IsNullOrEmpty(spawnPointName) || player == null) return;
        GameObject spawn = GameObject.Find(spawnPointName);
        if (spawn != null) player.transform.position = spawn.transform.position;
    }

    public void LoadScene(string sceneName, string newSpawnPoint) {
        if (isSceneLoading) return;
        spawnPointName = newSpawnPoint;
        StartCoroutine(LoadSceneDelayed(sceneName));
    }

    public void LoadScene(string sceneName) { 
        if (isSceneLoading) return;
        StartCoroutine(LoadSceneDelayed(sceneName)); 
    }

    private IEnumerator LoadSceneDelayed(string sceneName)
    {
        isSceneLoading = true;
        Debug.Log($"GameManager: Triggering SceneManager.LoadScene [{sceneName}]...");
        
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
}