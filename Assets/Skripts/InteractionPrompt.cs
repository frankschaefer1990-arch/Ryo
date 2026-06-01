using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite rKeySprite;
    public float yOffset = 12.0f; // High offset because player scale is 0.1
    public float indicatorScale = 8.0f; 
    public float transparency = 0.55f;

    [Header("Detection")]
    public float detectionRadius = 0.2f; 

    private GameObject indicatorObj;
    private SpriteRenderer indicatorSR;

    private void Start()
    {
        // Try to load the sprite if not assigned
        if (rKeySprite == null)
        {
            rKeySprite = Resources.Load<Sprite>("Hintergrund/pngegg");
            #if UNITY_EDITOR
            if (rKeySprite == null)
                rKeySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Hintergrund/pngegg.png");
            #endif
        }

        indicatorObj = new GameObject("RKeyIndicator");
        indicatorObj.transform.SetParent(transform);
        indicatorObj.transform.localPosition = new Vector3(0, yOffset, 0);
        indicatorObj.transform.localScale = Vector3.one * indicatorScale;

        indicatorSR = indicatorObj.AddComponent<SpriteRenderer>();
        indicatorSR.sprite = rKeySprite;
        indicatorSR.color = new Color(1, 1, 1, transparency);
        indicatorSR.sortingOrder = 200; 
        indicatorSR.enabled = false;
    }

    private void Update()
    {
        if (indicatorSR == null) return;

        bool show = CheckForInteractables();
        indicatorSR.enabled = show;
        
        // Ensure indicator stays upright if player flips
        if (transform.lossyScale.x < 0 && indicatorObj.transform.localScale.x > 0)
        {
            Vector3 ls = indicatorObj.transform.localScale;
            ls.x = -Mathf.Abs(ls.x);
            indicatorObj.transform.localScale = ls;
        }
        else if (transform.lossyScale.x > 0 && indicatorObj.transform.localScale.x < 0)
        {
            Vector3 ls = indicatorObj.transform.localScale;
            ls.x = Mathf.Abs(ls.x);
            indicatorObj.transform.localScale = ls;
        }
    }

    private bool CheckForInteractables()
    {
        // Hide if dialogue, UI is open, or movement is locked (Cutscenes)
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive()) return false;
        if (MyUIManager.Instance != null && MyUIManager.Instance.IsAnyPanelOpen()) return false;
        
        // Hide during camera blends or explicit cutscenes
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null && !pm.canMove) return false;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        int wallLayerMask = LayerMask.GetMask("Wall");

        foreach (var hit in hits)
        {
            // Skip the player itself and its children
            if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) continue;

            // 1. WHITELIST: Only show for specific manual interaction scripts
            // Use GetComponentInParent in case the collider is on a child object
            
            // Chests
            Chest chest = hit.GetComponentInParent<Chest>();
            if (chest != null)
            {
                if (!chest.isOpened && !chest.isPermanentlyEmpty) return true;
                continue;
            }

            // NPC / Merchant / Blacksmith
            if (hit.GetComponentInParent<MerchantInteraction>() != null || 
                hit.GetComponentInParent<ShopManager>() != null ||
                hit.GetComponentInParent<NPCInteraction>() != null ||
                hit.GetComponentInParent<BlacksmithInteraction>() != null)
            {
                return true;
            }

            // Tablets
            if (hit.GetComponentInParent<TabletDialogue>() != null)
            {
                return true;
            }

            // Fountain
            if (hit.GetComponentInParent<FountainInteraction>() != null)
            {
                return true;
            }

            // Furniture (Bed, Desk)
            if (hit.GetComponentInParent<HouseMasterFurniture>() != null)
            {
                return true;
            }

            // Krypta Interactables
            if (hit.GetComponentInParent<KryptaInteractable>() != null)
            {
                return true;
            }

            // Stone Idols
            if (hit.GetComponentInParent<StoneIdol>() != null)
            {
                return true;
            }
        }
        return false;
        }
        }