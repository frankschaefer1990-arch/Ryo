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
    public Direction currentDirection = Direction.East;
    
    [Header("References")]
    private Animator animator;
    private bool playerInRange = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        UpdateAppearance();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.R))
        {
            if (RadialMenu.Instance != null && !RadialMenu.Instance.IsActive)
            {
                RadialMenu.Instance.Open(this);
            }
        }
    }

    // Dummy comment
    public void SetDirection(Direction newDir)
    {
        currentDirection = newDir;
        UpdateAppearance();
        
        IdolPuzzleManager manager = Object.FindAnyObjectByType<IdolPuzzleManager>();
        if (manager != null)
        {
            manager.CheckPuzzle();
        }
    }

    void UpdateAppearance()
    {
        if (animator != null)
        {
            animator.SetInteger("Direction", (int)currentDirection);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
