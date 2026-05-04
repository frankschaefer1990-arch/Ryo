using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerGold : MonoBehaviour
{
    public static PlayerGold Instance;

    [Header("Gold System")]
    public int currentGold = 100;

    // =========================
    // SINGLETON + PERSISTENT
    // =========================
    private void Awake()
    {
        // Bereits vorhanden?
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Erste echte Instanz
        Instance = this;

        // Zwischen Szenen behalten
        DontDestroyOnLoad(gameObject);

        // Optional für spätere UI / Save Erweiterung
        SceneManager.sceneLoaded += OnSceneLoaded;
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

        Debug.Log("Gold erhalten: +" + amount + " | Gesamt: " + currentGold);
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