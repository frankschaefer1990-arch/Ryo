using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public LayerMask wallLayer;

    [Header("Movement Lock")]
    public bool canMove = true;
    public bool isCutsceneMoving = false; // New flag for cutscenes

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 lastMovement;

    private Vector3 originalScale;

    [Header("Up Scale Fix")]
    public float upScaleMultiplier = 1.1f;

    [Header("Speed Scaling")]
    public float baseMoveSpeed = 5f;

    private PlayerStats playerStats;
    private bool wasLockedLastFrame = false;

    void Start()
    {
        // Hide ALL Labyrinth Colliders if present
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("ColliderPainter"))
            {
                // Disable all renderers
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers) r.enabled = false;

                // Also set Tilemap color alpha to 0 just in case
                var tilemap = obj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                if (tilemap != null)
                {
                    Color c = tilemap.color;
                    c.a = 0f;
                    tilemap.color = c;
                }
            }
        }

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // PlayerStats holen
        playerStats = GetComponent<PlayerStats>();

        originalScale = spriteRenderer.transform.localScale;

        lastMovement = Vector2.down;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        canMove = true; // FORCE UNLOCK
        ResetMovementState();
    }

        public void ResetMovementState()
        {
        movement = Vector2.zero;
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("MoveX", lastMovement.x);
            animator.SetFloat("MoveY", lastMovement.y);
        }
        }

        void Update()
    {
        // =========================
        // SPEED SYSTEM
        // =========================
        speed = baseMoveSpeed;

        // =========================
        // INPUT
        // =========================
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // Strict cardinal movement (no diagonals)
        // Priority to vertical movement if both are pressed
        if (Mathf.Abs(inputY) > 0.1f)
        {
            movement = new Vector2(0, inputY).normalized;
        }
        else if (Mathf.Abs(inputX) > 0.1f)
        {
            movement = new Vector2(inputX, 0).normalized;
        }
        else
        {
            movement = Vector2.zero;
        }

        // =========================
        // UI & CURSOR SYSTEM
        // =========================
        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        bool uiPanelOpen = MyUIManager.Instance != null && MyUIManager.Instance.IsAnyPanelOpen();
        bool isCutscene = !canMove; 

        // PRIORITÄT: Wenn UI offen ist, muss die Maus immer frei sein!
        if (uiPanelOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (dialogueActive || isCutscene)
        {
            // Only lock if we are NOT in a UI panel that isn't registered yet
            // But we should register panels.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // =========================
        // MOVEMENT LOCK
        // =========================
        if (!canMove || dialogueActive || uiPanelOpen)
        {
            movement = Vector2.zero;
            if (animator != null && !isCutsceneMoving)
            {
                animator.SetBool("isMoving", false);
                animator.SetFloat("MoveX", lastMovement.x);
                animator.SetFloat("MoveY", lastMovement.y);
            }
        }
        else
        {
            bool isMoving = movement.sqrMagnitude > 0.01f;
            if (isMoving)
            {
                lastMovement = movement.normalized;
            }

            if (animator != null)
            {
                animator.SetFloat("MoveX", lastMovement.x);
                animator.SetFloat("MoveY", lastMovement.y);
                animator.SetBool("isMoving", isMoving);
            }
        }

        // =========================
        // SCALE FIX (Always apply based on lastMovement)
        // =========================
        float scaleMultiplier = lastMovement == Vector2.up ? upScaleMultiplier : 1f;
        float xDirection = lastMovement.x < 0 ? -1f : 1f;

        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x) * scaleMultiplier * xDirection,
                Mathf.Abs(originalScale.y) * scaleMultiplier,
                originalScale.z
            );
        }
}

    void FixedUpdate()
    {
        if (!canMove || movement == Vector2.zero)
            return;

        float moveDistance = speed * Time.fixedDeltaTime;

        // Mask setup (ignore self)
        int mask = wallLayer.value & ~(1 << gameObject.layer);
        
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(mask);
        filter.useLayerMask = true;
        filter.useTriggers = false; 

        // Use BoxCast for superior solid collision detection
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        Vector2 worldSize = Vector2.one;
        Vector2 worldOffset = Vector2.zero;
        if (capsule != null)
        {
            // IMPORTANT: Use absolute values for size, as scale might be negative when flipped
            worldSize = new Vector2(Mathf.Abs(capsule.size.x * transform.lossyScale.x), Mathf.Abs(capsule.size.y * transform.lossyScale.y));
            worldOffset = new Vector2(capsule.offset.x * transform.lossyScale.x, capsule.offset.y * transform.lossyScale.y);
        }
        
        Vector2 castOrigin = rb.position + worldOffset;
        
        // Safety buffer (skin)
        float skinWidth = 0.05f; 
        RaycastHit2D[] hits = new RaycastHit2D[5];
        
        // Box size slightly smaller to avoid "grazing" side walls, but cast distance includes skin
        int hitCount = Physics2D.BoxCast(castOrigin, worldSize * 0.9f, 0, movement, filter, hits, moveDistance + skinWidth);

        bool isBlocked = false;
        float finalMoveDist = moveDistance;

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                // Ignore overlaps (distance near 0) - this allows moving OUT of a wall
                if (hit.distance > 0.0001f)
                {
                    if (hit.distance < finalMoveDist + skinWidth)
                    {
                        finalMoveDist = Mathf.Max(0, hit.distance - skinWidth);
                        isBlocked = true;
                    }
                }
                else
                {
                    // If we are already deep inside, block any movement that doesn't fix it
                    isBlocked = true;
                    finalMoveDist = 0;
                }
            }
        }

        if (!isBlocked)
        {
            rb.MovePosition(rb.position + movement * moveDistance);
        }
        else if (finalMoveDist > 0.001f)
        {
            rb.MovePosition(rb.position + movement * finalMoveDist);
        }
    }
}