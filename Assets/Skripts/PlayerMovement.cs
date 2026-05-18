using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public LayerMask wallLayer;

    [Header("Movement Lock")]
    public bool canMove = true;

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
        // MOVEMENT LOCK (z.B. Shop offen oder Dialog)
        // =========================
        bool dialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
        bool uiPanelOpen = MyUIManager.Instance != null && MyUIManager.Instance.IsAnyPanelOpen();
        
        if (!canMove || dialogueActive || uiPanelOpen)
        {
            movement = Vector2.zero;
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                // Also ensure the animator knows which way we were last facing
                animator.SetFloat("MoveX", lastMovement.x);
                animator.SetFloat("MoveY", lastMovement.y);
            }
            
            if (!wasLockedLastFrame)
            {
                wasLockedLastFrame = true;
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
        // Kein Bewegen wenn gelockt
        if (!canMove)
            return;

        if (movement == Vector2.zero)
            return;

        float moveDistance = speed * Time.fixedDeltaTime;
        Vector2 targetPosition = rb.position + movement * moveDistance;

        // Directional check: Raycast/CircleCast in movement direction
        // Radius 0.12f matches the small player scale better, distance 0.1f checks slightly ahead
        RaycastHit2D hit = Physics2D.CircleCast(rb.position, 0.12f, movement, 0.1f, wallLayer);

        if (hit.collider == null)
        {
            rb.MovePosition(targetPosition);
        }
    }
}