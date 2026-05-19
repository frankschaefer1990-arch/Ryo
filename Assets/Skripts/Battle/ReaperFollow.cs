using UnityEngine;

public class ReaperFollow : MonoBehaviour
{
    public float detectionRange = 5.0f; 
    public float moveSpeed = 1.5f; 
    public float floatAmplitude = 0.15f;
    public float floatFrequency = 1.5f;
    public LayerMask wallLayer;

    private Transform player;
    private Rigidbody2D rb;
    private float floatOffset;
    private SpriteRenderer spriteRenderer;

    private Transform visualChild;

        void Start()
    {
        floatOffset = Random.Range(0f, 2f * Mathf.PI);
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.simulated = true;
        }

        // Handle visual bobbing safely
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (spriteRenderer.transform == transform)
            {
                Transform existing = transform.Find("VisualRoot");
                if (existing != null)
                {
                    visualChild = existing;
                    spriteRenderer = existing.GetComponent<SpriteRenderer>();
                }
                else
                {
                    Vector3 originalScale = transform.localScale;
                    GameObject visualObj = new GameObject("VisualRoot");
                    visualObj.transform.SetParent(transform);
                    visualObj.transform.localPosition = Vector3.zero;
                    visualObj.transform.localRotation = Quaternion.identity;
                    visualObj.transform.localScale = Vector3.one; 
                    
                    SpriteRenderer newSr = visualObj.AddComponent<SpriteRenderer>();
                    newSr.sprite = spriteRenderer.sprite;
                    newSr.color = spriteRenderer.color;
                    newSr.sortingOrder = spriteRenderer.sortingOrder;
                    newSr.sortingLayerID = spriteRenderer.sortingLayerID;
                    newSr.material = spriteRenderer.material;
                    newSr.flipX = spriteRenderer.flipX;
                    newSr.flipY = spriteRenderer.flipY;
                    
                    Destroy(spriteRenderer);
                    spriteRenderer = newSr;
                    visualChild = visualObj.transform;
                }
            }
            else
            {
                visualChild = spriteRenderer.transform;
                visualChild.localScale = Vector3.one;
            }
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player.transform;
        }
        
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null)
            {
                PlayerMovement pm = Object.FindAnyObjectByType<PlayerMovement>();
                if (pm != null) p = pm.gameObject;
            }
            if (p != null) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (rb == null) return;

        Vector2 currentPos = rb.position;
        float distance = Vector2.Distance(currentPos, (Vector2)player.position);
        
        if (distance <= detectionRange)
        {
            Vector2 moveDirection = ((Vector2)player.position - currentPos).normalized;
            float moveDistance = moveSpeed * Time.fixedDeltaTime;
            
            // Use rb.Cast which is more reliable than CircleCast
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(wallLayer);
            filter.useLayerMask = true;
            filter.useTriggers = true; // Respect all colliders on the layer

            System.Collections.Generic.List<RaycastHit2D> hits = new System.Collections.Generic.List<RaycastHit2D>();
            // Cast slightly further to detect walls before hitting them
            int hitCount = rb.Cast(moveDirection, filter, hits, moveDistance + 0.1f);

            bool pathBlocked = false;
            if (hitCount > 0)
            {
                foreach(var hit in hits) {
                    if (hit.distance < moveDistance + 0.02f) {
                        pathBlocked = true;
                        break;
                    }
                }
            }

            if (!pathBlocked)
            {
                rb.MovePosition(currentPos + moveDirection * moveDistance);
            }
        }
    }

    void Update()
    {
        // Visual bobbing applied to child to avoid physics interference
        if (visualChild != null)
        {
            float verticalOffset = Mathf.Sin(Time.time * floatFrequency + floatOffset) * floatAmplitude;
            visualChild.localPosition = new Vector3(0, verticalOffset, 0);
        }
    }
}