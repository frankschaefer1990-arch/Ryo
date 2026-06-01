using UnityEngine;

public class FountainInteraction : MonoBehaviour
{
    public float interactionDistance = 2.5f;
    public Sprite portrait;
    public AudioClip healSFX;

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.R))
        {
            HealPlayer();
        }
    }

    private void HealPlayer()
    {
        if (PlayerStats.Instance != null)
        {
            // Restore HP and Mana
            PlayerStats.Instance.Heal(PlayerStats.Instance.maxHealth);
            PlayerStats.Instance.RestoreMana(PlayerStats.Instance.maxMana);
            
            if (DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
            {
                // Use the provided portrait if available
                DialogueUI.Instance.ShowMessage("Ryo", "Ich fühle mich erfrischt", portrait, 2.5f);
            }

            if (healSFX != null)
            {
                AudioSource.PlayClipAtPoint(healSFX, transform.position);
            }
            
            Debug.Log("Fountain: Player healed!");
        }
    }
}