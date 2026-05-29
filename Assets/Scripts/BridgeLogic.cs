using UnityEngine;

public class BridgeLogic : MonoBehaviour
{
    [Header("Requirements")]
    public StoneIdol triggerIdol;
    public StoneIdol.Direction requiredDirection = StoneIdol.Direction.North;
    
    [Header("References")]
    public GameObject bridgeVisual;
    public Collider2D pathBlocker; // Collider that BLOCKS when bridge is gone
    
    [Header("Audio")]
    public AudioClip showSound;
    private AudioSource audioSource;
    
    private bool isShown = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initial state
        if (triggerIdol != null)
        {
            isShown = triggerIdol.currentDirection == requiredDirection;
            if (bridgeVisual != null) bridgeVisual.SetActive(isShown);
            if (pathBlocker != null) pathBlocker.enabled = !isShown;
            Debug.Log("[Bridge] " + gameObject.name + " initialized. isShown=" + isShown);
        }
    }

    void Update()
    {
        if (triggerIdol == null) {
            Debug.LogWarning("[Bridge] " + gameObject.name + " triggerIdol is missing!");
            return;
        }

        bool shouldBeOn = triggerIdol.currentDirection == requiredDirection;

        if (shouldBeOn != isShown)
        {
            Debug.Log("[Bridge] " + gameObject.name + " Trigger detected! shouldBeOn=" + shouldBeOn);
            isShown = shouldBeOn;
            
            if (bridgeVisual != null) {
                bridgeVisual.SetActive(isShown);
                Debug.Log("[Bridge] Set visual " + bridgeVisual.name + " to " + isShown);
            }
            
            if (pathBlocker != null) {
                pathBlocker.enabled = !isShown;
                Debug.Log("[Bridge] Set blocker " + pathBlocker.name + " to " + (!isShown));
            }

            if (showSound != null)
            {
                if (audioSource != null) audioSource.PlayOneShot(showSound);
            }

            if (isShown && DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowMessage("Ryo", "Die Statuen haben die Brücke freigegeben... Interessant.", 3.0f);
            }
        }
    }
}