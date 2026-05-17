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
        bool isMenu = scene.name.Contains("Menu") || scene.name.Contains("Splash");
        
        if (isMenu) return;

        // Ensure we handle player state correctly for the current scene
        StartCoroutine(ToggleDelayed(!isBattleScene));
        
        if (!isBattleScene) lastOverworldScene = scene.name;
    }

    private IEnumerator ToggleDelayed(bool active)
    {
        // Wait for GameManager systems to be fully ready
        yield return new WaitForSeconds(0.2f);
        TogglePlayerState(active);
    }

    public void TogglePlayerState(bool active)
    {
        GameObject playerObj = (GameManager.Instance != null) ? GameManager.Instance.player : null;
        
        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
        }

        if (playerObj == null) return;

        try {
            var sr = playerObj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.enabled = active;

            var movement = playerObj.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = active;

            var rb = playerObj.GetComponent<Rigidbody2D>();
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

            var col = playerObj.GetComponent<Collider2D>();
            if (col != null) col.enabled = active;
        }
        catch (System.Exception e) {
            Debug.LogError("BattleStateController: Error in TogglePlayerState: " + e.Message);
        }
    }
}