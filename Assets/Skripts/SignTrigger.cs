using UnityEngine;
using TMPro;

public class SignTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TMP_Text textContent;

    [Header("Sign Text")]
    [TextArea]
    public string message = "Pfeil nach oben = Temple\nPfeil Links = Friedhof\nPfeil rechts = Dorf Canhill\nPfeil runter = Wasserfälle von Chlorius";

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (panel != null) panel.SetActive(true);
            if (textContent != null) textContent.text = message;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
