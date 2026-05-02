using UnityEngine;
using UnityEngine.SceneManagement;

public class TempleEntrance : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Etwas hat TempleEntrance betreten: " + other.name);

        if (!other.CompareTag("Player")) return;

        Debug.Log("PLAYER betritt Tempel!");

        SceneManager.LoadScene("Temple");
    }
}