using UnityEngine;

public class EchoStatue : MonoBehaviour
{
    public string bossName;
    public EnemyData bossData;
    public int slotIndex; // 0-8

    [Header("Interaction Settings")]
    public float interactionRadius = 2.0f;
    public Vector2 interactionOffset = new Vector2(0, -2.5f);

    private void Start()
    {
        CheckUnlock();
    }

    private void Update()
    {
        // Calculate the actual center of interaction
        Vector2 interactionCenter = (Vector2)transform.position + interactionOffset;

        // Check for player using overlap circle
        Collider2D[] hits = Physics2D.OverlapCircleAll(interactionCenter, interactionRadius);
        
        Transform playerT = null;
        bool playerInRangeLocal = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.GetComponentInParent<PlayerMovement>() != null)
            {
                playerInRangeLocal = true;
                playerT = hit.transform;
                break;
            }
        }

        if (playerInRangeLocal && Input.GetKeyDown(KeyCode.R))
        {
            // PROXIMITY CHECK: Ensure we are the closest EchoStatue center to the player
            if (IsClosestStatue(playerT.position))
            {
                Debug.Log($"EchoStatue: Interacting with {bossName}");
                if (DialogueUI.Instance == null || !DialogueUI.Instance.IsDialogueActive())
                {
                    Interact();
                }
            }
        }
    }

    private bool IsClosestStatue(Vector3 playerPos)
    {
        Vector2 myCenter = (Vector2)transform.position + interactionOffset;
        float myDist = Vector2.Distance(myCenter, playerPos);
        EchoStatue[] allStatues = Object.FindObjectsByType<EchoStatue>(FindObjectsSortMode.None);
        
        foreach (var s in allStatues)
        {
            if (s == this || !s.gameObject.activeInHierarchy) continue;
            
            Vector2 otherCenter = (Vector2)s.transform.position + s.interactionOffset;
            if (Vector2.Distance(otherCenter, playerPos) < myDist)
            {
                return false; // Another statue's interaction center is closer
            }
        }
        return true;
    }

    public void CheckUnlock()
    {
        bool unlocked = false;
        if (QuestManager.Instance != null)
        {
            if (bossName == "Skelettkrieger") unlocked = QuestManager.Instance.defeatedTempleBoss;
            else if (bossName == "Skelett Magier") unlocked = QuestManager.Instance.kryptaBossDefeated;
            else if (bossName == "Wassergeist") unlocked = QuestManager.Instance.defeatedWassergeist;
        }
        
        gameObject.SetActive(unlocked);
    }

    public void Interact()
    {
        EchoPanelUI panelUI = EchoPanelUI.Instance;
        
        if (panelUI == null && MyUIManager.Instance != null)
        {
            panelUI = MyUIManager.Instance.echoPanelUI;
        }

        // Final desperation search
        if (panelUI == null)
        {
            panelUI = Object.FindAnyObjectByType<EchoPanelUI>(FindObjectsInactive.Include);
        }

        if (panelUI != null)
        {
            panelUI.Open(this);
        }
        else
        {
            Debug.LogError("EchoStatue: Could not find EchoPanelUI instance even with a full search!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + (Vector3)interactionOffset;
        Gizmos.DrawWireSphere(center, interactionRadius);
        
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawLine(transform.position, center);
    }
}
