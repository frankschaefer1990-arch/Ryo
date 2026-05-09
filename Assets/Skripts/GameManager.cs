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
            Debug.Log($"GameManager: Scene Loaded [{scene.name}]. Cleaning up duplicates...");
            CleanupDuplicates();
            
            // Critical: Always try to re-find player after scene load
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) {
                if (player != null && player != foundPlayer) {
                     // If we have a persistent player but found a scene one, transfer data if needed?
                     // Usually we prefer the persistent player.
                     if (foundPlayer.scene.name != "DontDestroyOnLoad") {
                        Debug.Log("GameManager: Scene has its own player. Preferring persistent player.");
                        Destroy(foundPlayer);
                     }
                } else {
                    player = foundPlayer;
                    player.transform.SetParent(null);
                    DontDestroyOnLoad(player);
                }
            }

            ReconnectSystems();
            MovePlayerToSpawn();
            
            Invoke(nameof(NotifySystemsReady), 0.2f);
        }
        catch (System.Exception e) {
            Debug.LogError("GameManager: Error in OnSceneLoaded: " + e.Message);
        }
    }

    private void NotifySystemsReady() {
        OnSystemsReady?.Invoke();
    }

    void CleanupDuplicates()
    {
        try {
            // EventSystem - Absolute control
            EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EventSystem e in systems) {
                if (eventSystem != null && e.gameObject != eventSystem) {
                    Debug.Log($"GameManager: Nuking duplicate EventSystem on [{e.gameObject.name}].");
                    Destroy(e.gameObject);
                }
                else if (eventSystem == null) { 
                    eventSystem = e.gameObject; 
                    eventSystem.transform.SetParent(null); 
                    DontDestroyOnLoad(eventSystem); 
                }
            }

            // Manager Cleanup
            var diags = FindObjectsByType<DialogueUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var d in diags) if (DialogueUI.Instance != null && d != DialogueUI.Instance) Destroy(d.gameObject);

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
        spawnPointName = newSpawnPoint;
        StartCoroutine(LoadSceneDelayed(sceneName));
    }

    public void LoadScene(string sceneName) { 
        StartCoroutine(LoadSceneDelayed(sceneName)); 
    }

    private IEnumerator LoadSceneDelayed(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
}