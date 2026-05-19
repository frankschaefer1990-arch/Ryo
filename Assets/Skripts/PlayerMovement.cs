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
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Kein diagonales Laufen
        if (movement.y != 0)
        {
            movement.x = 0;
        }

        movement = movement.normalized;

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
            return;
        }

        bool isMoving = movement.magnitude > 0;
        if (isMoving)
        {
            lastMovement = movement;
        }

        // =========================
        // ANIMATOR
        // =========================
        if (animator != null)
        {
            animator.SetFloat("MoveX", lastMovement.x);
            animator.SetFloat("MoveY", lastMovement.y);
            animator.SetBool("isMoving", isMoving);
        }

        // =========================
        // SCALE FIX
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
        filter.useTriggers = true; 

        // Use rb.Cast to detect collisions in movement direction
        // This is more robust than CircleCast because it uses the actual collider shape
        System.Collections.Generic.List<RaycastHit2D> hits = new System.Collections.Generic.List<RaycastHit2D>();
        int hitCount = rb.Cast(movement, filter, hits, moveDistance + 0.05f);

        bool isBlocked = false;
        float minHitDistance = moveDistance;

        if (hitCount > 0)
        {
            foreach (var hit in hits)
            {
                // We block if there's any collider in our way
                // We use a small threshold to avoid blocking on self/inner edges if any
                if (hit.distance >= 0)
                {
                    minHitDistance = Mathf.Min(minHitDistance, hit.distance);
                    isBlocked = true;
                }
            }
        }

        if (!isBlocked)
        {
            rb.MovePosition(rb.position + movement * moveDistance);
        }
        else
        {
            // Move as close as possible without overlapping
            if (minHitDistance > 0.01f)
            {
                rb.MovePosition(rb.position + movement * (minHitDistance - 0.01f));
            }
            // If distance is nearly 0, we stay where we are (blocked)
        }
    }
}