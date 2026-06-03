using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite rKeySprite;
    public float yOffset = 12.0f; // High offset because player scale is 0.1
    public float indicatorScale = 8.0f; 
    public float transparency = 0.55f;

    [Header("Detection")]
    public Vector2 detectionOffset = Vector2.zero;
    public Vector2 capsuleSize = new Vector2(2f, 4f);
    public CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;
    public Color gizmoColor = Color.cyan;
    public bool showGizmo = true;

    private GameObject indicatorObj;
    private SpriteRenderer indicatorSR;

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        Gizmos.color = gizmoColor;
        
        Vector3 worldPos = transform.TransformPoint(detectionOffset);
        DrawWireCapsule(worldPos, capsuleSize, capsuleDirection, transform.eulerAngles.z);
    }

    private void DrawWireCapsule(Vector3 center, Vector2 size, CapsuleDirection2D direction, float angle)
    {
        float radius = (direction == CapsuleDirection2D.Vertical ? size.x : size.y) / 2f;
        float height = (direction == CapsuleDirection2D.Vertical ? size.y : size.x);
        
        if (height < radius * 2) radius = height / 2f;

        float offsetDist = height / 2f - radius;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector3 dirVec = (direction == CapsuleDirection2D.Vertical ? Vector3.up : Vector3.right);
        Vector3 rotatedOffset = rotation * dirVec * offsetDist;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.color = gizmoColor;
        UnityEditor.Handles.DrawWireDisc(center + rotatedOffset, Vector3.forward, radius);
        UnityEditor.Handles.DrawWireDisc(center - rotatedOffset, Vector3.forward, radius);
        #endif

        Vector3 sideVec = (direction == CapsuleDirection2D.Vertical ? Vector3.right : Vector3.up);
        Vector3 rotatedSide = rotation * sideVec * radius;
        
        Gizmos.DrawLine(center + rotatedOffset + rotatedSide, center - rotatedOffset + rotatedSide);
        Gizmos.DrawLine(center + rotatedOffset - rotatedSide, center - rotatedOffset - rotatedSide);
    }

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

        Vector2 worldPos = transform.TransformPoint(detectionOffset);
        Collider2D[] hits = Physics2D.OverlapCapsuleAll(worldPos, capsuleSize, capsuleDirection, transform.eulerAngles.z);
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

            // Elara Rest Interaction
            if (hit.GetComponentInParent<ElaraRestInteraction>() != null)
            {
                return true;
            }

            // Stone Idols
            if (hit.GetComponentInParent<StoneIdol>() != null)
            {
                return true;
            }

            // Echo Statues
            if (hit.GetComponentInParent<EchoStatue>() != null)
            {
                return true;
            }

            // Guardian Interaction
            if (hit.GetComponentInParent<GuardianInteraction>() != null)
            {
                return true;
            }
            }
        return false;
        }
        }