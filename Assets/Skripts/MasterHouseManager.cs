using UnityEngine;

public class MasterHouseManager : MonoBehaviour
{
    [Header("Door Unlocking")]
    public GameObject lockedDoorTrigger;
    public GameObject wallToDeactivate;
    public GameObject entranceTrigger;

    [Header("Message")]
    public string ryoMessage = "Ich werde dich in ehren halten Meister";
    private bool messageShown = false;

    private void Start()
    {
        if (entranceTrigger != null) entranceTrigger.SetActive(false);
    }

    private void Update()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.finishedTempleSequence)
        {
            if (lockedDoorTrigger != null && lockedDoorTrigger.activeSelf)
            {
                lockedDoorTrigger.SetActive(false);
                if (wallToDeactivate != null) wallToDeactivate.SetActive(false);
                if (entranceTrigger != null) entranceTrigger.SetActive(true);
                Debug.Log("MasterHouseManager: House unlocked after Temple sequence.");
            }
        }
    }

    public void OnPlayerEnterHouse()
    {
        if (!messageShown)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Ryo", ryoMessage, 2f);
                messageShown = true;
            }
        }
    }
}
