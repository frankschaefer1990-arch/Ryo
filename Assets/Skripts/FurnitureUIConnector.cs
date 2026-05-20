using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureUIConnector : MonoBehaviour
{
    public static FurnitureUIConnector Instance;

    public GameObject panel;
    public TextMeshProUGUI textDisplay;
    public GameObject choiceButtons;
    public Button sleepButton;
    public Button cancelButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        GameManager.OnSystemsReady += UpdateFurnitureReferences;
    }

    private void OnDisable()
    {
        GameManager.OnSystemsReady -= UpdateFurnitureReferences;
    }

    public void UpdateFurnitureReferences()
    {
        // Find all furniture in current scene and assign references
        var allFurniture = Object.FindObjectsByType<HouseMasterFurniture>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var f in allFurniture)
        {
            f.interactionPanel = panel;
            f.textDisplay = textDisplay;
            f.choiceButtons = choiceButtons;
            f.sleepButton = sleepButton;
            f.cancelButton = cancelButton;
            Debug.Log($"FurnitureUIConnector: Connected to {f.gameObject.name}");
        }
    }
}
