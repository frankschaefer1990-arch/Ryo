using UnityEngine;
using UnityEngine.SceneManagement;

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

    private float sceneLoadLockTimer = 0f;

    public void TriggerSceneLoadLock(float duration = 1.0f)
    {
        sceneLoadLockTimer = duration;
        Debug.Log($"PlayerMovement: Scene load lock triggered for {duration}s");
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            lastMovement = direction.normalized;
            if (animator != null)
            {
                animator.SetFloat("MoveX", lastMovement.x);
                animator.SetFloat("MoveY", lastMovement.y);
            }
        }
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoadedInternal;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedInternal;
    }

    private void OnSceneLoadedInternal(Scene scene, LoadSceneMode mode)
    {
        // Don't lock in battle or menus
        string sName = scene.name.ToLower();
        if (sName.Contains("battle") || sName.Contains("kampf") || sName.Contains("menu") || sName.Contains("splash"))
            return;

        Debug.Log($"PlayerMovement: Scene {scene.name} loaded. Triggering 1.0s movement lock.");
        TriggerSceneLoadLock(1.0f); 
    }

    void OnEnable()
    {
        // Still keep this for first spawn
        if (sceneLoadLockTimer <= 0) sceneLoadLockTimer = 1.0f;
    }

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

        // Apply global spawn facing if set
        if (GameManager.Instance != null && GameManager.NextSpawnFacing != Vector2.zero)
        {
            lastMovement = GameManager.NextSpawnFacing;
            GameManager.NextSpawnFacing = Vector2.zero; // Reset
            Debug.Log($"PlayerMovement: Applied spawn facing {lastMovement}");
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // REMOVED: canMove = true; // FORCE UNLOCK - This was overwriting cutscene locks
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
        // MOVEMENT LOCK
        // =========================
        if (sceneLoadLockTimer > 0)
        {
            sceneLoadLockTimer -= Time.deltaTime;
        }

        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        bool uiPanelOpen = MyUIManager.Instance != null && MyUIManager.Instance.IsAnyPanelOpen();
        bool radialMenuActive = RadialMenu.Instance != null && RadialMenu.Instance.IsActive;
        bool isCutscene = !canMove; 

        // PRIORITÄT: Wenn UI offen ist, muss die Maus immer frei sein!
        if (uiPanelOpen || radialMenuActive)
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
        if (!canMove || dialogueActive || uiPanelOpen || sceneLoadLockTimer > 0)
        {
            movement = Vector2.zero;
            if (animator != null)
            {
                if (isCutsceneMoving)
                {
                    // Update lastMovement from Animator during cutscenes to fix flipping/direction
                    float ax = animator.GetFloat("MoveX");
                    float ay = animator.GetFloat("MoveY");
                    if (Mathf.Abs(ax) > 0.01f || Mathf.Abs(ay) > 0.01f)
                    {
                        lastMovement = new Vector2(ax, ay).normalized;
                    }
                    animator.SetBool("isMoving", true); // Ensure animation plays!
                }
                else
                {
                    animator.SetBool("isMoving", false);
                    animator.SetFloat("MoveX", lastMovement.x);
                    animator.SetFloat("MoveY", lastMovement.y);
                }
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
        // Determine xDirection: If moving left, flip. If moving right, normal.
        // If moving vertically, keep last horizontal direction.
        float xDirection = 1f;
        if (lastMovement.x < -0.01f) xDirection = -1f;
        else if (lastMovement.x > 0.01f) xDirection = 1f;
        else
        {
            // If strictly vertical, we usually want to face the last horizontal direction
            // But lastMovement already stores the last direction.
            // However, lastMovement.x is 0 if strictly vertical.
            // Let's use the actual current scale to determine "stickiness" or just default.
            // Actually, the current logic is: if x is 0, xDirection = 1 (Right).
            // We'll keep it simple: only flip on actual horizontal movement.
            // To prevent flipping back to right when looking up/down, we only update xDirection when x != 0.
        }

        float scaleMultiplier = lastMovement.y > 0.8f ? upScaleMultiplier : 1f;

        if (spriteRenderer != null)
        {
            // Only update xDirection when there's horizontal movement to avoid flipping back to right on Up/Down
            float currentX = spriteRenderer.transform.localScale.x;
            float finalXDir = (lastMovement.x < -0.01f) ? -1f : (lastMovement.x > 0.01f ? 1f : Mathf.Sign(currentX));

            spriteRenderer.transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x) * scaleMultiplier * finalXDir,
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