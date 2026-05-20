using UnityEngine;

public class EntranceTrigger : MonoBehaviour
{
    public MasterHouseManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && manager != null)
        {
            manager.OnPlayerEnterHouse();
        }
    }
}
