using UnityEngine;

public class BossraumBlocker : MonoBehaviour
{
    private void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.defeatedWassergeist)
        {
            // Disable the portal but keep object active for spawn points
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            ScenePortal portal = GetComponent<ScenePortal>();
            if (portal != null) portal.enabled = false;

            Debug.Log("BossraumBlocker: Disabled portal components because Wassergeist is defeated.");
        }
    }
}
