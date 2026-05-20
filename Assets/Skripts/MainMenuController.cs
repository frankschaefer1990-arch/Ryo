using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loadPanel;
    public TextMeshProUGUI saveInfoText;
    public Button loadButton;

    [Header("Visuals (Optional)")]
    public Image background;

    [Header("Audio")]
    public AudioClip menuMusic;
    private AudioSource musicSource;

    private void Start()
    {
        if (loadPanel != null) loadPanel.SetActive(false);

        // Ensure SaveSystem exists for standalone tests
        if (SaveSystem.Instance == null)
        {
            GameObject ss = new GameObject("SaveSystem");
            ss.AddComponent<SaveSystem>();
        }

        UpdateMenuState();
        
        // Use AudioManager if available
        if (AudioManager.Instance != null && menuMusic != null)
        {
            // Only play if not already playing this clip to avoid restarts
            // AudioManager.UpdateMusicForScene will also trigger via OnSystemsReady
            // so we don't strictly need to call it here, but it's safe if it has guards.
            AudioManager.Instance.PlayOverrideMusic(menuMusic);
        }
        else if (menuMusic != null)
        {
            // Fallback for when no AudioManager exists
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.5f;
            if (!musicSource.isPlaying) musicSource.Play();
        }

        // Make sure cursor is visible in menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        // Give systems a frame to initialize
        Invoke(nameof(StartMusicDelayed), 0.1f);
    }

    private void StartMusicDelayed()
    {
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayOverrideMusic(menuMusic);
        }
    }

    public void NewGame()
    {
        Debug.Log("MainMenu: Starting New Game.");
        
        // Ensure GameManager exists (especially if starting directly from MainMenu scene)
        if (GameManager.Instance == null)
        {
            GameObject gmPrefab = null;
    #if UNITY_EDITOR
            gmPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefix/GameManager.prefab");
    #endif
            if (gmPrefab != null)
            {
                Instantiate(gmPrefab);
            }
            else
            {
                Debug.LogWarning("MainMenuController: GameManager prefab not found. Player spawning might fail.");
            }
        }

        // Reset everything before starting
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.ResetForNewGame();
        }

        // Spawn a fresh player via GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnPersistentPlayer();
            GameManager.Instance.LoadScene("Legend of Ryo");
        }
        else
        {
            SceneManager.LoadScene("Legend of Ryo");
        }
    }

    public void OpenLoadPanel()
    {
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.Open(true);
        }
        else if (loadPanel != null)
        {
            loadPanel.SetActive(true);
            var ssm = loadPanel.GetComponent<SaveSlotManager>();
            if (ssm != null)
            {
                ssm.Open(true);
            }
            else
            {
                loadPanel.transform.SetAsLastSibling(); 
                if (SaveSystem.Instance != null && saveInfoText != null)
                    saveInfoText.text = SaveSystem.Instance.GetSaveInfo();
            }
        }
    }

    public void CloseLoadPanel()
    {
        if (SaveSlotManager.Instance != null) SaveSlotManager.Instance.Close();
        if (loadPanel != null) loadPanel.SetActive(false);
    }

    public void LoadGame()
    {
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.Open(true);
        }
        else if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            SaveSystem.Instance.Load();
        }
        else
        {
            Debug.Log("Laden nicht möglich: Kein Spielstand.");
        }
    }

    public void Options()
    {
        Debug.Log("Optionen: Noch nicht implementiert.");
    }

    public void QuitGame()
    {
        Debug.Log("Spiel beenden.");
        Application.Quit();
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

    private void UpdateMenuState()
    {
        // Wir lassen den Button immer aktiv, damit man das Panel öffnen kann
        if (loadButton != null)
        {
            loadButton.interactable = true;
        }
    }
}