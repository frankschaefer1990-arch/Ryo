using UnityEngine;

public class HouseInteriorTrigger : MonoBehaviour
{
    public GameObject interior;

    private void Start()
    {
        if (interior != null)
        {
            interior.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interior != null)
        {
            interior.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interior != null)
        {
            interior.SetActive(false);
        }
    }
}
