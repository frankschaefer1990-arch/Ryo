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
    }

    private void Start()
    {
        InitializePersistentSystems();
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

        if (questManager == null) {
            QuestManager qm = FindAnyObjectByType<QuestManager>();
            if (qm != null) questManager = qm.gameObject;
        }
        if (questManager != null) {
            questManager.transform.SetParent(null);
            DontDestroyOnLoad(questManager);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try {
            Debug.Log($"GameManager: Scene Loaded [{scene.name}]. Pruning duplicates...");
            isSceneLoading = false;

            // 1. Ensure only ONE EventSystem exists.
            var allEventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var es in allEventSystems)
            {
                if (eventSystem != null && es.gameObject != eventSystem)
                {
                    Debug.Log($"GameManager: Destroying duplicate EventSystem on {es.gameObject.name}");
                    Destroy(es.gameObject);
                }
            }

            // 2. Ensure only ONE AudioListener is active
            var allListeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool listenerSet = false;
            foreach (var l in allListeners)
            {
                if (!listenerSet && l.enabled && l.gameObject.activeInHierarchy) listenerSet = true;
                else if (listenerSet) l.enabled = false;
            }

            // 3. Player State Reset
            if (player != null)
            {
                var pm = player.GetComponent<PlayerMovement>();
                if (pm != null) {
                    pm.canMove = !scene.name.Contains("Battle");
                    pm.ResetMovementState();
                }
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            ReconnectSystems();
            
            // Cleanup duplicate Players in the scene
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in allPlayers)
            {
                if (player != null && p != player)
                {
                    Debug.Log($"GameManager: Destroying duplicate Player object '{p.name}' in scene '{scene.name}'");
                    Destroy(p);
                }
            }

            MovePlayerToSpawn();

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
        }
    }

    void MovePlayerToSpawn()
    {
        if (string.IsNullOrEmpty(spawnPointName) || player == null) return;
        GameObject spawn = GameObject.Find(spawnPointName);
        if (spawn != null) player.transform.position = spawn.transform.position;
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
}