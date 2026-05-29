using UnityEngine;

public class StoneIdol : MonoBehaviour
{
    public enum Direction
    {
        South = 0,
        SouthWest = 1,
        West = 2,
        NorthWest = 3,
        North = 4,
        NorthEast = 5,
        East = 6,
        SouthEast = 7
    }

    [Header("State")]
    public Direction currentDirection = Direction.South;
    
    [Header("Sprites for Directions")]
    public Sprite spriteSouth;
    public Sprite spriteSouthWest;
    public Sprite spriteWest;
    public Sprite spriteNorthWest;
    public Sprite spriteNorth;
    public Sprite spriteNorthEast;
    public Sprite spriteEast;
    public Sprite spriteSouthEast;

    [Header("Interaction")]
    public float interactionDistance = 1.0f;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    [Header("Audio")]
    public AudioClip slideSound;
    private AudioSource audioSource;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        UpdateAppearance();
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player") ?? GameObject.Find("Ryo");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // Automatically find player if reference is lost
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player") ?? GameObject.Find("Ryo");
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            playerInRange = dist < (interactionDistance + 0.3f);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.R))
        {
            if (RadialMenu.Instance != null && !RadialMenu.Instance.IsActive)
            {
                RadialMenu.Instance.Open(this);
            }
        }
    }

    private void OnValidate()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateAppearance();
    }

    public void SetDirection(Direction newDir)
    {
        Debug.Log("[Statue] " + gameObject.name + " SetDirection to " + newDir);
        if (currentDirection != newDir)
        {
            currentDirection = newDir;
            UpdateAppearance();
            PlaySlideSound();
            
            IdolPuzzleManager manager = Object.FindAnyObjectByType<IdolPuzzleManager>();
            if (manager != null)
            {
                manager.CheckPuzzle();
            }
        }
    }

    void UpdateAppearance()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        switch (currentDirection)
        {
            case Direction.South: spriteRenderer.sprite = spriteSouth; break;
            case Direction.SouthWest: spriteRenderer.sprite = spriteSouthWest; break;
            case Direction.West: spriteRenderer.sprite = spriteWest; break;
            case Direction.NorthWest: spriteRenderer.sprite = spriteNorthWest; break;
            case Direction.North: spriteRenderer.sprite = spriteNorth; break;
            case Direction.NorthEast: spriteRenderer.sprite = spriteNorthEast; break;
            case Direction.East: spriteRenderer.sprite = spriteEast; break;
            case Direction.SouthEast: spriteRenderer.sprite = spriteSouthEast; break;
        }
    }

    private void PlaySlideSound()
    {
        if (audioSource != null && slideSound != null)
        {
            audioSource.PlayOneShot(slideSound, 0.4f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}
