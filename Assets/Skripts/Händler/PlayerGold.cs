using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGold : MonoBehaviour
{
    public static PlayerGold Instance;
    public static event System.Action OnGoldChanged;

    [Header("Gold System")]
    public int currentGold = 10;

    // =========================
    // SINGLETON + PERSISTENT
    // =========================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"PlayerGold: Duplicate component on {gameObject.name} removed.");
            Destroy(this); // Destroy only the script component
            return;
        }

        Instance = this;
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static PlayerGold GetInstance()
    {
        if (Instance == null) Instance = FindAnyObjectByType<PlayerGold>();
        return Instance;
    }

    // =========================
    // CLEANUP
    // =========================
    private void OnDestroy()
    {
        // Nur echte Instanz abmelden
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null; // CLEAR INSTANCE
        }
    }

    // =========================
    // SCENE LOAD FIX
    // =========================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sicherheitslog
        Debug.Log("Szene geladen: " + scene.name + " | Gold Stand: " + currentGold);
    }

    // =========================
    // GOLD AUSGEBEN
    // =========================
    public bool SpendGold(int amount)
    {
        // Ungültige Werte blockieren
        if (amount <= 0)
        {
            Debug.LogWarning("Ungültiger Goldbetrag!");
            return false;
        }

        // Nicht genug Gold
        if (currentGold < amount)
        {
            Debug.Log("Nicht genug Gold!");
            return false;
        }

        currentGold -= amount;

        Debug.Log("Gold ausgegeben: -" + amount + " | Rest: " + currentGold);
        OnGoldChanged?.Invoke();

        return true;
        }

        // =========================
        // GOLD HINZUFÜGEN
        // =========================
        public void AddGold(int amount)
        {
        if (amount <= 0)
        {
            Debug.LogWarning("PlayerGold: Ungültiger Goldbetrag: " + amount);
            return;
        }

        int oldGold = currentGold;
        currentGold += amount;
        Debug.Log($"PlayerGold: {amount} Gold hinzugefügt. {oldGold} -> {currentGold}");
        OnGoldChanged?.Invoke();
        }

        public void SetGold(int amount)
        {
        int oldGold = currentGold;
        currentGold = Mathf.Max(0, amount);
        Debug.Log($"PlayerGold: Gold direkt gesetzt. {oldGold} -> {currentGold}");
        OnGoldChanged?.Invoke();
        }

    // =========================
    // CHECK
    // =========================
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }
}