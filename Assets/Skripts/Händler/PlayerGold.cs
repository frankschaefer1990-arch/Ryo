using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGold : MonoBehaviour
{
    public static PlayerGold Instance;

    [Header("Gold System")]
    public int currentGold = 10;

    // =========================
    // SINGLETON + PERSISTENT
    // =========================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"PlayerGold: Duplicate script on {gameObject.name} removed.");
            Destroy(this); // Only destroy this script instance
            return;
        }

        Instance = this;
        // Only persist if we are a root object or specifically part of persistent systems
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

        return true;
    }

    // =========================
    // GOLD HINZUFÜGEN
    // =========================
    public void AddGold(int amount)
    {
        // Ungültige Werte blockieren
        if (amount <= 0)
        {
            Debug.LogWarning("Ungültiger Goldbetrag!");
            return;
        }

        currentGold += amount;
        Debug.Log($"PlayerGold: {amount} Gold hinzugefügt. Neuer Stand: {currentGold}");
    }

    // =========================
    // DIREKT SETZEN (SaveSystem später)
    // =========================
    public void SetGold(int amount)
    {
        currentGold = Mathf.Max(0, amount);

        Debug.Log("Gold gesetzt auf: " + currentGold);
    }

    // =========================
    // CHECK
    // =========================
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }
}