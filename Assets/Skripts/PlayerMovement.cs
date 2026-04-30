using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 lastMovement;

    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        originalScale = spriteRenderer.transform.localScale;

        lastMovement = Vector2.down;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Kein diagonales Laufen
        if (movement.y != 0)
        {
            movement.x = 0;
        }

        movement = movement.normalized;

        bool isMoving = movement.magnitude > 0;

        if (isMoving)
        {
            lastMovement = movement;
        }

        // Animator
        if (animator != null)
        {
            animator.SetFloat("MoveX", lastMovement.x);
            animator.SetFloat("MoveY", lastMovement.y);
            animator.SetBool("isMoving", isMoving);
        }

        // Scale Fix
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

    [Header("Up Scale Fix")]
    public float upScaleMultiplier = 1.1f;

    void FixedUpdate()
    {
        if (movement == Vector2.zero)
            return;

        Vector2 targetPosition = rb.position + movement * speed * Time.fixedDeltaTime;

        // Prüfen ob Wand im Weg
        Collider2D hit = Physics2D.OverlapCircle(targetPosition, 0.1f, wallLayer);

        if (hit == null)
        {
            rb.MovePosition(targetPosition);
        }
    }
}