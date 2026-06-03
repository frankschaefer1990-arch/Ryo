using UnityEngine;

public class BossraumBlocker : MonoBehaviour
{
    public bool blockAfterDefeat = false; // Default to false to allow re-entry unless specified

    private void Start()
    {
        if (blockAfterDefeat && QuestManager.Instance != null && QuestManager.Instance.defeatedWassergeist)
        {
            // Disable all portal related components
            foreach (var col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }
            
            ScenePortal portal = GetComponent<ScenePortal>();
            if (portal != null) portal.enabled = false;

            Debug.Log("BossraumBlocker: Permanently blocked Bossraum exit.");
        }
    }
}
