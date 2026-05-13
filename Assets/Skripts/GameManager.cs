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
        
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("GameManager: Initialized and persistent.");
    }

    private void Start()
    {
        InitializePersistentSystems();
        
        if (SceneManager.GetActiveScene().name == "SplashScreen")
        {
            Debug.Log("GameManager: Starting from SplashScreen.");
        }

        OnSystemsReady?.Invoke();
    }

    private void InitializePersistentSystems()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            player.transform.SetParent(null);
            DontDestroyOnLoad(player);
        }

        if (eventSystem == null) {
            EventSystem es = FindAnyObjectByType<EventSystem>();
            if (es != null) eventSystem = es.gameObject;
        }
        
        if (eventSystem != null) {
            eventSystem.transform.SetParent(null);
            DontDestroyOnLoad(eventSystem);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try {
            Debug.Log($"GameManager: Scene Loaded [{scene.name}]. Pruning duplicates and reconnecting...");
            isSceneLoading = false;

            // 1. EventSystem Cleanup
            var allEventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
            {
                if (eventSystem != null && es.gameObject != eventSystem)
                {
                    Debug.Log($"GameManager: Destroying duplicate EventSystem on {es.gameObject.name}");
                    Destroy(es.gameObject);
                }
            }

            // 2. AudioListener Cleanup
            var allListeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool listenerSet = false;
            foreach (var l in allListeners)
            {
                if (!listenerSet && l.enabled && l.gameObject.activeInHierarchy) listenerSet = true;
                else if (listenerSet) l.enabled = false;
            }

            // 3. Player Finding and Spawning
            GameObject scenePlayer = GameObject.FindGameObjectWithTag("Player");
            if (scenePlayer == null) {
                GameObject pObj = GameObject.Find("Player");
                if (pObj != null) scenePlayer = pObj;
            }

            if (player == null && scenePlayer != null) {
                player = scenePlayer;
                player.transform.SetParent(null); // CRITICAL: Must be root for DontDestroyOnLoad
                DontDestroyOnLoad(player);
                Debug.Log($"GameManager: Found player in scene '{scene.name}', detached and marked persistent.");
            }
            else if (player != null && scenePlayer != null && player != scenePlayer) {
                Debug.Log($"GameManager: Destroying duplicate scene player '{scenePlayer.name}' because persistent player exists.");
                Destroy(scenePlayer);
            }

            if (player != null)
            {
                var pm = player.GetComponent<PlayerMovement>();
                if (pm != null) {
                    pm.canMove = !scene.name.Contains("Battle");
                    pm.ResetMovementState();
                }
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                
                MovePlayerToSpawn();
            }
            else {
                Debug.LogWarning("GameManager: No player found in scene or persistence!");
            }

            ReconnectSystems();
            Invoke(nameof(NotifySystemsReady), 0.2f);
        }
        catch (System.Exception e) {
            Debug.LogError("GameManager: Error in OnSceneLoaded: " + e.Message);
        }
    }

    public void LoadScene(string sceneName, string newSpawnPoint) {
        if (isSceneLoading) return;
        spawnPointName = newSpawnPoint;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(string sceneName) { 
        if (isSceneLoading) return;
        StartCoroutine(LoadSceneAsync(sceneName)); 
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isSceneLoading = true;
        yield return null; 
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }

    private void NotifySystemsReady() {
        OnSystemsReady?.Invoke();
    }

    void ReconnectSystems()
    {
        CameraFollow follow = FindAnyObjectByType<CameraFollow>();
        if (follow != null && player != null) {
            follow.player = player.transform;
            GameObject bounds = GameObject.Find("CameraBounds");
            if (bounds != null) {
                BoxCollider2D b = bounds.GetComponent<BoxCollider2D>();
                if (b != null) { follow.boundsCollider = b; follow.UpdateBounds(); }
            }
            Debug.Log("GameManager: Camera reconnected to player.");
        }
    }

    void MovePlayerToSpawn()
    {
        if (player == null) return;
        
        GameObject spawn = null;
        if (!string.IsNullOrEmpty(spawnPointName))
        {
            spawn = GameObject.Find(spawnPointName);
        }
        
        if (spawn == null)
        {
            spawn = GameObject.Find("PlayerSpawn");
        }

        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
            Debug.Log($"GameManager: Player moved to {spawn.name} at {spawn.transform.position}");
        }
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
}
