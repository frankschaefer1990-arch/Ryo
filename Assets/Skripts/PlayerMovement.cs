using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Up Scale Fix")]
    public float upScaleMultiplier = 1.1f;

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

        // SpriteRenderer direkt suchen
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Falls SpriteRenderer auf Child sitzt
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        originalScale = spriteRenderer.transform.localScale;

        lastMovement = Vector2.down;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.y != 0)
            movement.x = 0;

        movement = movement.normalized;

        bool isMoving = movement.magnitude > 0;

        if (isMoving)
        {
            if (movement.y > 0)
                lastMovement = Vector2.up;
            else if (movement.y < 0)
                lastMovement = Vector2.down;
            else if (movement.x > 0)
                lastMovement = Vector2.right;
            else if (movement.x < 0)
                lastMovement = Vector2.left;
        }

        animator.SetFloat("MoveX", lastMovement.x);
        animator.SetFloat("MoveY", lastMovement.y);
        animator.SetBool("isMoving", isMoving);

        // Standardgröße
        float scaleMultiplier = 1f;

        // Nach oben kleiner/größer
        if (lastMovement == Vector2.up)
        {
            scaleMultiplier = upScaleMultiplier;
        }

        // Links/Rechts Flip
        float xDirection = lastMovement.x < 0 ? -1f : 1f;

        spriteRenderer.transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * scaleMultiplier * xDirection,
            Mathf.Abs(originalScale.y) * scaleMultiplier,
            originalScale.z
        );
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }
}