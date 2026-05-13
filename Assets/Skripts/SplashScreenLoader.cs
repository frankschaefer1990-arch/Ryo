using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenLoader : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        Invoke(nameof(LoadNextScene), delay);
    }

    void LoadNextScene()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("Legend of Ryo");
        }
        else
        {
            SceneManager.LoadScene("Legend of Ryo");
        }
    }
}