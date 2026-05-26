using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterfallInteraction : MonoBehaviour
{
    public SpriteRenderer leverRenderer;
    public Sprite spriteLeft;  // Unactive
    public Sprite spriteRight; // Active
    
    public WaterfallMaster master; 
    public int leverIndex = 0; // 0-3 for persistence
    public TilemapRenderer assignedWater; // Individual water surface
    public float fadeSpeed = 2f;
    public AudioClip drainSound;

    private bool isActive = false; 
    private KryptaInteractable interactable;
    private float targetAlpha = 1f;
    private AudioSource audioSource;
    private Collider2D waterCollider;

    void Start()
    {
        interactable = GetComponent<KryptaInteractable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (interactable != null)
        {
            interactable.OnInteract += OnLeverInteract;
        }

        if (assignedWater != null)
        {
            waterCollider = assignedWater.GetComponent<Collider2D>();
        }

        // Load state from QuestManager
        if (QuestManager.Instance != null && QuestManager.Instance.waterfallLevers != null && leverIndex < QuestManager.Instance.waterfallLevers.Length)
        {
            isActive = QuestManager.Instance.waterfallLevers[leverIndex];
        }

        // Set initial alpha and collider state immediately
        if (assignedWater != null)
        {
            Color c = assignedWater.sharedMaterial.color;
            c.a = isActive ? 0f : 1f;
            assignedWater.sharedMaterial.color = c;
            targetAlpha = c.a;
            
            if (waterCollider != null) waterCollider.enabled = !isActive;
        }

        UpdateVisual();
    }

    void Update()
    {
        if (assignedWater != null)
        {
            Color c = assignedWater.sharedMaterial.color;
            if (!Mathf.Approximately(c.a, targetAlpha))
            {
                c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
                assignedWater.sharedMaterial.color = c;
                
                // Disable collider if we are fading out and reached halfway or so
                if (isActive && c.a < 0.1f && waterCollider != null && waterCollider.enabled)
                {
                    waterCollider.enabled = false;
                }
                // Enable collider immediately if we are fading in
                else if (!isActive && c.a > 0.1f && waterCollider != null && !waterCollider.enabled)
                {
                    waterCollider.enabled = true;
                }
            }
        }
    }

    public void OnLeverInteract()
    {
        isActive = !isActive;
        targetAlpha = isActive ? 0f : 1f;
        
        if (isActive && drainSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(drainSound);
        }

        // Save state to QuestManager
        if (QuestManager.Instance != null && QuestManager.Instance.waterfallLevers != null && leverIndex < QuestManager.Instance.waterfallLevers.Length)
        {
            QuestManager.Instance.waterfallLevers[leverIndex] = isActive;
        }

        UpdateVisual();

        if (master != null)
        {
            master.CheckLevers();
        }

        Debug.Log(gameObject.name + (isActive ? ": Switched Right (Active) - Water Fading Out" : ": Switched Left (Inactive) - Water Fading In"));
    }

    private void UpdateVisual()
    {
        if (leverRenderer != null)
        {
            leverRenderer.sprite = isActive ? spriteRight : spriteLeft;
        }
    }

    public bool IsActive()
    {
        return isActive;
    }
}