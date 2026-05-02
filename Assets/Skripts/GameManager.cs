using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Core")]
    public GameObject player;
    public GameObject canvas;
    public GameObject eventSystem;

    [Header("Spawn System")]
    public string spawnPointName = "";

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Core Objekte automatisch finden
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (canvas == null)
        {
            Canvas foundCanvas = FindFirstObjectByType<Canvas>();

            if (foundCanvas != null)
                canvas = foundCanvas.gameObject;
        }

        if (eventSystem == null)
        {
            UnityEngine.EventSystems.EventSystem foundEvent =
                FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();

            if (foundEvent != null)
                eventSystem = foundEvent.gameObject;
        }

        // Persistent machen
        if (player != null)
            DontDestroyOnLoad(player);

        if (canvas != null)
            DontDestroyOnLoad(canvas);

        if (eventSystem != null)
            DontDestroyOnLoad(eventSystem);
    }

    // =========================
    // SCENE LOADED
    // =========================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReconnectSystems();
        MovePlayerToSpawn();
    }

    // =========================
    // SYSTEME NEU VERBINDEN
    // =========================
    void ReconnectSystems()
    {
        // UI
        MyUIManager uiManager = FindFirstObjectByType<MyUIManager>();

        if (uiManager != null)
        {
            uiManager.SendMessage("ReconnectUI");
        }

        // Camera
        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null && player != null)
        {
            cameraFollow.player = player.transform;
        }

        // Temple Intro
        TempleCameraIntro templeIntro = FindFirstObjectByType<TempleCameraIntro>();

        if (templeIntro != null && player != null)
        {
            templeIntro.player = player.transform;
        }
    }

    // =========================
    // PLAYER ZUM SPAWN
    // =========================
    void MovePlayerToSpawn()
    {
        if (string.IsNullOrEmpty(spawnPointName) || player == null)
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
    // SCENE LOAD MIT SPAWN
    // =========================
    public void LoadScene(string sceneName, string newSpawnPoint)
    {
        spawnPointName = newSpawnPoint;

        SceneManager.LoadScene(sceneName);
    }
}