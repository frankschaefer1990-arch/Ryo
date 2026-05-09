using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleStateController : MonoBehaviour
{
    public static BattleStateController Instance;

    private Vector3 lastOverworldPosition;
    private string lastOverworldScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        if (transform.parent == null || transform.parent.name != "PersistentSystems")
        {
            DontDestroyOnLoad(gameObject);
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isBattleScene = scene.name.Contains("Battle");
        // Use a very short delay to ensure GameManager has updated the player reference
        StartCoroutine(ToggleDelayed(!isBattleScene));
        
        if (!isBattleScene)
        {
            lastOverworldScene = scene.name;
        }
    }

    private IEnumerator ToggleDelayed(bool active)
    {
        // Wait for end of frame instead of 0.2s for cleaner transitions
        yield return new WaitForEndOfFrame();
        TogglePlayerState(active);
    }

    public void TogglePlayerState(bool active)
    {
        GameObject playerObj = null;
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            playerObj = GameManager.Instance.player;
        }
        
        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
        }

        if (playerObj == null) {
            // Only log warning if we are NOT in a battle scene (where player might be disabled/gone)
            if (!SceneManager.GetActiveScene().name.Contains("Battle"))
                Debug.LogWarning($"BattleStateController: Player nicht gefunden in Szene {SceneManager.GetActiveScene().name}.");
            return;
        }

        try {
            // Renderer
            SpriteRenderer sr = playerObj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.enabled = active;

            // Movement
            MonoBehaviour movement = playerObj.GetComponent("PlayerMovement") as MonoBehaviour;
            if (movement != null) movement.enabled = active;

            // Physics
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (!active)
                {
                    lastOverworldPosition = playerObj.transform.position;
                    rb.simulated = false;
                }
                else
                {
                    rb.simulated = true;
                }
            }

            // Collider
            Collider2D col = playerObj.GetComponent<Collider2D>();
            if (col != null) col.enabled = active;

            Debug.Log($"BattleStateController: Player state set to {active} in {SceneManager.GetActiveScene().name}");
        }
        catch (System.Exception e) {
            Debug.LogError("BattleStateController: Error in TogglePlayerState: " + e.Message);
        }
    }
}