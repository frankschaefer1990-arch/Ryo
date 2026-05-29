using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Core")]
    public GameObject player; 
    public static GameObject PersistentPlayer; 

    public GameObject playerPrefab; 
    public GameObject canvas;
    public GameObject eventSystem;

    [Header("Optional Systems")]
    public GameObject questManager;
    public GameObject saveSystemPrefab;

    [Header("Spawn System")]
    public string spawnPointName = "";
    public bool isLoadingSave = false; 

    [Header("Return System")]
    public string lastGameplayScene = "";
    public Vector3 lastGameplayPosition;
    public string lastEnemyTriggerID = "";
    public System.Collections.Generic.List<string> defeatedEnemiesInCurrentScene = new System.Collections.Generic.List<string>();

    private bool isSceneLoading = false;

    public static System.Action OnSystemsReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"GameManager: Duplicate instance {this.GetInstanceID()} removed. Instance {Instance.GetInstanceID()} remains.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log($"GameManager: Initialized instance {this.GetInstanceID()}.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        isLoadingSave = false; // Reset just in case
        InitializePersistentSystems();
        
        if (SceneManager.GetActiveScene().name == "SplashScreen")
        {
            Debug.Log("GameManager: Starting from SplashScreen.");
        }

        OnSystemsReady?.Invoke();
    }

    private void InitializePersistentSystems()
    {
        if (PersistentPlayer == null) 
        {
            PersistentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (PersistentPlayer != null) player = PersistentPlayer;
        }

        if (PersistentPlayer != null) {
            PersistentPlayer.transform.SetParent(null);
            DontDestroyOnLoad(PersistentPlayer);
        }

        if (eventSystem == null) {
            var es = Object.FindAnyObjectByType<EventSystem>();
            if (es != null) eventSystem = es.gameObject;
        }
        
        if (eventSystem != null) {
            eventSystem.transform.SetParent(null);
            DontDestroyOnLoad(eventSystem);
        }

        if (SaveSystem.Instance == null)
        {
            GameObject ss = new GameObject("SaveSystem");
            ss.AddComponent<SaveSystem>();
        }
    }

    public void SpawnPersistentPlayer()
    {
        // 1. Destroy existing persistent one
        if (PersistentPlayer != null) Destroy(PersistentPlayer);
        
        // 2. Find and destroy ANY player in the current scene to avoid duplicates/conflicts
        var scenePlayers = Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in scenePlayers) {
            Debug.Log($"GameManager: Removing old scene player '{p.gameObject.name}' before spawning fresh one.");
            Destroy(p.gameObject);
        }

        PersistentPlayer = null;
        player = null;

        if (playerPrefab == null)
        {
            playerPrefab = Resources.Load<GameObject>("Player");
    #if UNITY_EDITOR
            if (playerPrefab == null) playerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player/Player.prefab");
    #endif
        }

        if (playerPrefab != null)
        {
            PersistentPlayer = Instantiate(playerPrefab);
            player = PersistentPlayer;
            PersistentPlayer.name = "Player";
            PersistentPlayer.tag = "Player";
            PersistentPlayer.transform.SetParent(null);
            DontDestroyOnLoad(PersistentPlayer);
            Debug.Log($"GameManager: Spawned fresh persistent Player (ID: {PersistentPlayer.GetInstanceID()}).");
        }
        else
        {
            Debug.LogError("GameManager: Cannot spawn player, prefab missing!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;

        try {
            Debug.Log($"GameManager: Processing Scene Loaded [{scene.name}].");
            isSceneLoading = false;
            
            string sName = scene.name.ToLower();
            bool isGameplayScene = !sName.Contains("menu") && !sName.Contains("splash");

            // 1. AudioListener Fix - Ensure exactly one active listener
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AudioListener best = null;
            foreach (var l in listeners) {
                // If it's the singleton instance's listener or an active one
                if (best == null && l.gameObject.activeInHierarchy) {
                    l.enabled = true;
                    best = l;
                } else {
                    l.enabled = false;
                }
            }
            if (best == null && listeners.Length > 0) {
                listeners[0].gameObject.SetActive(true);
                listeners[0].enabled = true;
            }

            // 2. EventSystem Cleanup: Ensure only one persistent EventSystem exists
            var allES = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var es in allES)
            {
                if (eventSystem == null || eventSystem.Equals(null))
                {
                    eventSystem = es.gameObject;
                    if (eventSystem.scene.name != "DontDestroyOnLoad") 
                    { 
                        eventSystem.transform.SetParent(null); 
                        DontDestroyOnLoad(eventSystem); 
                    }
                }
                else if (es.gameObject != eventSystem) 
                { 
                    // Destroy immediately to prevent "Multiple EventSystems" warnings
                    DestroyImmediate(es.gameObject); 
                }
            }

            // 3. Player Management
            if (isGameplayScene)
            {
                // Verify persistent player
                if (PersistentPlayer == null || PersistentPlayer.Equals(null))
                {
                    Debug.Log("GameManager: PersistentPlayer is null. Searching for any PlayerMovement in scene...");
                    var allP = Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var p in allP)
                    {
                        if (p.gameObject.scene.name == "DontDestroyOnLoad") { PersistentPlayer = p.gameObject; break; }
                        if (PersistentPlayer == null) { 
                            PersistentPlayer = p.gameObject; 
                            PersistentPlayer.transform.SetParent(null); 
                            DontDestroyOnLoad(PersistentPlayer); 
                        }
                    }
                    player = PersistentPlayer;
                }

                // If still null, try to spawn from prefab
                if ((PersistentPlayer == null || PersistentPlayer.Equals(null)) && playerPrefab != null)
                {
                    Debug.Log("GameManager: No player found. Spawning from prefab...");
                    PersistentPlayer = Instantiate(playerPrefab);
                    player = PersistentPlayer;
                    PersistentPlayer.name = "Player";
                    PersistentPlayer.tag = "Player";
                    PersistentPlayer.transform.SetParent(null);
                    DontDestroyOnLoad(PersistentPlayer);
                }

                if (PersistentPlayer != null)
                {
                    // Do NOT enable visuals if entering a battle scene
                    bool isBattle = scene.name.ToLower().Contains("battle") || scene.name.ToLower().Contains("kampf");
                    
                    PersistentPlayer.SetActive(true); // Keep GameObject active for logic/stats
                    
                    var renderers = PersistentPlayer.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers) r.enabled = !isBattle;

                    PersistentPlayer.transform.SetParent(null); // Ensure it's root
                    // PersistentPlayer.transform.localScale = new Vector3(0.1f, 0.1f, 1f); // Removed to maintain consistent scale
                    
                    if (!isBattle)
{
                        var sr = PersistentPlayer.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) 
                        {
                            sr.enabled = true;
                            sr.color = Color.white;
                            sr.sortingOrder = 100;
                        }

                        var rb = PersistentPlayer.GetComponent<Rigidbody2D>();
                        if (rb != null) { rb.simulated = true; rb.linearVelocity = Vector2.zero; }

                        if (!isLoadingSave) MovePlayerToSpawn();
                        }
                        else
                        {
                        // Move far away in battle
                        PersistentPlayer.transform.position = new Vector3(-1000, -1000, 0);
                    }

                    Debug.Log($"GameManager: Player '{PersistentPlayer.name}' processed for scene {scene.name}. Visible: {!isBattle}");

                    // Safety: Unlock movement and UI on every scene load to prevent stuck states from cutscenes
                    var pm = PersistentPlayer.GetComponent<PlayerMovement>();
                    if (pm != null) pm.canMove = true;
                    if (MyUIManager.Instance != null) MyUIManager.Instance.isLocked = false;
                    }
                    else
                {
                    Debug.LogError("GameManager: FAILED to find or spawn Player!");
                }
                
                // Cleanup duplicate players (that are NOT the persistent one)
                var allPlayers = Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var p in allPlayers)
                {
                    if (p.gameObject != PersistentPlayer) 
                    {
                        Debug.Log($"GameManager: Destroying duplicate player: {p.gameObject.name} in scene {p.gameObject.scene.name}");
                        Destroy(p.gameObject);
                    }
                }
            }

            ReconnectSystems();
            Invoke(nameof(NotifySystemsReady), 0.1f);
        }
        catch (System.Exception e) {
            Debug.LogError("GameManager: Error in OnSceneLoaded: " + e.Message);
        }
    }

    void ReconnectSystems()
    {
        CameraFollow follow = Object.FindAnyObjectByType<CameraFollow>();
        if (follow != null && PersistentPlayer != null) {
            follow.player = PersistentPlayer.transform;
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
        if (PersistentPlayer == null) return;
        
        if (spawnPointName == "ReturnFromBattle")
        {
            PersistentPlayer.transform.position = lastGameplayPosition;
            Debug.Log($"GameManager: Player returned to battle position: {lastGameplayPosition}");
            return;
        }

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
            PersistentPlayer.transform.position = spawn.transform.position;
            Debug.Log($"GameManager: Player moved to {spawn.name} at {spawn.transform.position}");
        }
    }

    public void LoadScene(string sceneName, string spawnPoint = "")
    {
        bool isNextBattle = sceneName.ToLower().Contains("battle") || sceneName.ToLower().Contains("kampf");
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool isCurrentBattle = currentSceneName.ToLower().Contains("battle") || currentSceneName.ToLower().Contains("kampf");

        if (isNextBattle)
        {
            lastGameplayScene = currentSceneName;
            if (PersistentPlayer != null)
            {
                lastGameplayPosition = PersistentPlayer.transform.position;
            }
        }
        else
        {
            // Transitioning to a gameplay scene.
            if (!isCurrentBattle)
            {
                // Simple transition between gameplay scenes -> Reset visit list
                defeatedEnemiesInCurrentScene.Clear();
                Debug.Log("GameManager: Cleared defeated enemies list (transition between gameplay scenes).");
            }
            else
            {
                // Returning from battle.
                // Clear ONLY if we are NOT returning to the scene we came from.
                if (sceneName != lastGameplayScene)
                {
                    defeatedEnemiesInCurrentScene.Clear();
                    Debug.Log($"GameManager: Returned from battle to DIFFERENT scene ({sceneName} != {lastGameplayScene}). Cleared defeated enemies.");
                }
                else
                {
                    Debug.Log($"GameManager: Returned from battle to SAME scene ({sceneName}). Defeated enemies count: {defeatedEnemiesInCurrentScene.Count}");
                }
                }
        }

        spawnPointName = spawnPoint;
        isSceneLoading = true;
        SceneManager.LoadScene(sceneName);
    }

    private void NotifySystemsReady()
    {
        OnSystemsReady?.Invoke();
        Debug.Log("GameManager: Systems are ready.");
    }
    }