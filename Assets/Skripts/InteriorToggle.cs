using UnityEngine;

public class InteriorToggle : MonoBehaviour
{
    public GameObject interiorObject;

    private void Start()
    {
        if (interiorObject != null) interiorObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interiorObject != null)
        {
            interiorObject.SetActive(true);
            
            // Notify MasterHouseManager for the message
            var mhm = interiorObject.GetComponent<MasterHouseManager>();
            if (mhm != null) mhm.OnPlayerEnterHouse();

            // Connect furniture UI since interior was inactive
            if (FurnitureUIConnector.Instance != null)
            {
                FurnitureUIConnector.Instance.UpdateFurnitureReferences();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interiorObject != null)
        {
            interiorObject.SetActive(false);
        }
    }
}
