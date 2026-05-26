using UnityEngine;

public class WaterfallWallBlocker : MonoBehaviour
{
    public GameObject wallObject;
    public string messageIfBlocked = "Der Weg zu den Wasserfällen ist versperrt... Ich spüre eine dunkle Magie aus der Krypta.";
    public string speakerName = "Ryo";

    private bool popupShown = false;

    private void Start()
    {
        if (wallObject == null) wallObject = gameObject;
        CheckBossStatus();
    }

    private void Update()
    {
        CheckBossStatus();
    }

    private void CheckBossStatus()
    {
        if (QuestManager.Instance != null)
        {
            // We check if the Krypta Boss (Skeleton Mage) is defeated
            // Either the flag after the cutscene or the return flag from battle
            bool isDefeated = QuestManager.Instance.kryptaBossDefeated || QuestManager.Instance.defeatedKryptaBossReturn;
            
            if (isDefeated)
            {
                if (wallObject.activeSelf)
                {
                    wallObject.SetActive(false);
                    Debug.Log("WaterfallWallBlocker: Skeleton Mage defeated. Wall is now passable.");
                }
                this.enabled = false;
            }
            else
            {
                if (!wallObject.activeSelf)
                {
                    wallObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && QuestManager.Instance != null)
        {
            bool isDefeated = QuestManager.Instance.kryptaBossDefeated || QuestManager.Instance.defeatedKryptaBossReturn;
            if (!isDefeated)
            {
                ShowMessage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            popupShown = false;
        }
    }

    private void ShowMessage()
    {
        if (popupShown) return;
        
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowMessage(speakerName, messageIfBlocked);
            popupShown = true;
        }
    }
}