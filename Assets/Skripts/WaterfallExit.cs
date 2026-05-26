using UnityEngine;

public class WaterfallExit : MonoBehaviour
{
    public string targetScene = "Wasserfälle von Chlorix LvL 2";
    public string spawnPointName = "SpawnFromWasserfaelle1"; // Correct spawn point name

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(targetScene, spawnPointName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
        }
    }
}
