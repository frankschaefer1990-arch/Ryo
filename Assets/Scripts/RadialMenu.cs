using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    private static RadialMenu _instance;
    public static RadialMenu Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<RadialMenu>(FindObjectsInactive.Include);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public GameObject panel;
    public Button[] directionButtons; // 0: South, 1: SW, 2: W, 3: NW, 4: N, 5: NE, 6: E, 7: SE
    
    public bool IsActive => panel != null && panel.activeInHierarchy;
    private StoneIdol activeIdol;

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            Debug.Log($"RadialMenu: Instance set on {gameObject.name}");
        }
        else if (Instance != this)
        {
            Debug.Log($"RadialMenu: Duplicate on {gameObject.name} destroyed.");
            Destroy(gameObject);
            return;
        }
        
        if (panel != null) panel.SetActive(false);

        for (int i = 0; i < directionButtons.Length; i++)
        {
            int index = i;
            if (directionButtons[i] != null)
            {
                directionButtons[i].onClick.AddListener(() => OnDirectionSelected(index));
                
                // Ensure the button's image is a raycast target
                var img = directionButtons[i].GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.raycastTarget = true;
                    // Ensure alpha is enough to detect clicks (handled by opaque color now)
                }
            }
        }
    }

    public void Open(StoneIdol idol)
    {
        activeIdol = idol;
        if (panel != null) panel.SetActive(true);
        SetPlayerMovement(false);
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        SetPlayerMovement(true);
        
        // Cursor will be reset by MyUIManager or manually if needed
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDirectionSelected(int directionIndex)
    {
        Debug.Log($"RadialMenu: Direction {directionIndex} selected.");
        if (activeIdol != null)
        {
            Debug.Log($"RadialMenu: Commanding {activeIdol.name} to turn to {(StoneIdol.Direction)directionIndex}");
            activeIdol.SetDirection((StoneIdol.Direction)directionIndex);
        }
        else
        {
            Debug.LogError("RadialMenu: No activeIdol assigned!");
        }
        Close();
    }

    private void SetPlayerMovement(bool canMove)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player") ?? GameObject.Find("Ryo");
        
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.canMove = canMove;
                if (!canMove)
                {
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    
                    Animator anim = player.GetComponent<Animator>();
                    if (anim != null) anim.SetFloat("Speed", 0);
                }
            }
        }
    }
    
    void Update()
    {
        if (IsActive && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.R)))
        {
            Close();
        }
    }
}
