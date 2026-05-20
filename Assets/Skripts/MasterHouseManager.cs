using UnityEngine;
using System.Collections;

public class MasterHouseManager : MonoBehaviour
{
    [Header("Door & Blockers")]
    public GameObject lockedBlocker; // Solid wall
    public GameObject houseInterior; // The interior group/layer

    [Header("Messages")]
    [TextArea] public string lockedMessage = "Das Haus des Meisters scheint abgeschlossen zu sein";
    [TextArea] public string unlockOnceMessage = "Meister, ich werde deine Erinnerung in ehren halten";
    
    private bool playerInDoorway = false;

    private void Start()
    {
        if (houseInterior != null) houseInterior.SetActive(false);
        UpdateHouseState();
    }

    private float messageCooldown = 0f;

    private void Update()
    {
        UpdateHouseState();
        if (messageCooldown > 0) messageCooldown -= Time.deltaTime;
    }

    private bool IsUnlocked()
    {
        if (QuestManager.Instance == null) return false;
        return QuestManager.Instance.finishedTempleSequence || QuestManager.Instance.defeatedTempleBoss;
    }

    private void UpdateHouseState()
    {
        bool isUnlocked = IsUnlocked();

        if (lockedBlocker != null)
        {
            lockedBlocker.SetActive(!isUnlocked);
        }
    }

    public void OnPlayerEnterHouse()
    {
        if (playerInDoorway) return; // Already inside
        playerInDoorway = true;
        Debug.Log("MasterHouseManager: Player entered proximity trigger.");
        
        bool isUnlocked = IsUnlocked();
        if (isUnlocked)
        {
            if (QuestManager.Instance != null && !QuestManager.Instance.masterHouseMessageSeen)
            {
                if (DialogueUI.Instance != null)
                {
                    Debug.Log("MasterHouseManager: Showing HONOR message.");
                    DialogueUI.Instance.ShowMessage("Ryo", unlockOnceMessage, 3.5f);
                    QuestManager.Instance.masterHouseMessageSeen = true;
                }
            }
        }
        else
        {
            if (DialogueUI.Instance != null)
            {
                Debug.Log("MasterHouseManager: Showing LOCKED message.");
                DialogueUI.Instance.ShowMessage("Ryo", lockedMessage, 2.5f);
            }
        }
    }

    public void OnPlayerExitHouse()
    {
        playerInDoorway = false;
        Debug.Log("MasterHouseManager: Player left proximity trigger.");
    }

    public void SetInteriorVisible(bool visible)
    {
        if (houseInterior != null)
        {
            houseInterior.SetActive(visible);
            
            // Auto-reconnect furniture when interior becomes visible
            if (visible && FurnitureUIConnector.Instance != null)
            {
                FurnitureUIConnector.Instance.UpdateFurnitureReferences();
            }
        }
    }
}

