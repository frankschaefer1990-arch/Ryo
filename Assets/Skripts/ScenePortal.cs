using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    public string targetSceneName;
    public string targetSpawnPoint;

    private bool isSwitching = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Mehrfachtrigger verhindern
        if (isSwitching)
            return;

        // Nur Player darf triggern
        if (!other.CompareTag("Player"))
            return;

        isSwitching = true;

        // Szene wechseln mit Spawnpoint
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(targetSceneName, targetSpawnPoint);
        }
        else
        {
            Debug.LogError("GameManager Instance fehlt!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset wenn Player Trigger verlässt
        if (other.CompareTag("Player"))
        {
            isSwitching = false;
        }
    }
}