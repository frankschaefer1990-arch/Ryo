using UnityEngine;

public class ProximityNotifier : MonoBehaviour 
{
    public MasterHouseManager manager;
    
    private void OnTriggerEnter2D(Collider2D other) 
    { 
        if (other.CompareTag("Player"))
        {
            Debug.Log($"ProximityNotifier: Player entered doorway on {gameObject.name}");
            if (manager != null) manager.OnPlayerEnterHouse(); 
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) 
    { 
        if (other.CompareTag("Player"))
        {
            Debug.Log($"ProximityNotifier: Player left doorway on {gameObject.name}");
            if (manager != null) manager.OnPlayerExitHouse(); 
        }
    }
}