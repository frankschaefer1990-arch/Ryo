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
        // Komponenten holen
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // SpriteRenderer direkt suchen
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Falls SpriteRenderer auf Child sitzt
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Originalgröße speichern
        originalScale = spriteRenderer.transform.localScale;

        // Standard Blickrichtung nach unten
        lastMovement = Vector2.down;

        // Sicherheitseinstellungen für Top Down
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Input lesen
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Kein diagonales Laufen
        if (movement.y != 0)
        {
            movement.x = 0;
        }

        // Normieren
        movement = movement.normalized;

        bool isMoving = movement.magnitude > 0;

        // Letzte Bewegungsrichtung speichern
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

        // Animator Werte setzen
        if (animator != null)
        {
            animator.SetFloat("MoveX", lastMovement.x);
            animator.SetFloat("MoveY", lastMovement.y);
            animator.SetBool("isMoving", isMoving);
        }

        // Standardgröße
        float scaleMultiplier = 1f;

        // Nach oben optisch anpassen
        if (lastMovement == Vector2.up)
        {
            scaleMultiplier = upScaleMultiplier;
        }

        // Links/Rechts Flip
        float xDirection = lastMovement.x < 0 ? -1f : 1f;

        // Sprite skalieren
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
        // Stabilere Bewegung für Top Down
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}