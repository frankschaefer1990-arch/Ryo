using UnityEngine;

public class PersistentSystems : MonoBehaviour
{
    public static PersistentSystems Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ensure all core managers are present
        ValidateManagers();
    }

    private void ValidateManagers()
    {
        // Add logic to check for missing children if needed
    }
}