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
    
    public bool IsActive => panel != null && panel.activeInHierarchy && gameObject.activeInHierarchy;
    private StoneIdol activeIdol;
    private float openTime;

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
        
        SetupButtons();
        if (panel != null) panel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (directionButtons == null) return;
        for (int i = 0; i < directionButtons.Length; i++)
        {
            int index = i;
            if (directionButtons[i] != null)
            {
                directionButtons[i].onClick.RemoveAllListeners();
                directionButtons[i].onClick.AddListener(() => OnDirectionSelected(index));
                
                var img = directionButtons[i].GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }
        }
    }

    public void Open(StoneIdol idol)
    {
        Debug.Log($"RadialMenu: Open called for {idol.name}. Parent: {transform.parent?.name ?? "ROOT"}");
        activeIdol = idol;
        openTime = Time.time;
        
        // Activate the whole object and the panel
        gameObject.SetActive(true);
        if (panel != null) panel.SetActive(true);
        
        // Ensure buttons are wired
        SetupButtons();
        
        // Ensure Canvas is on top
        Canvas c = GetComponentInParent<Canvas>();
        if (c != null)
        {
            c.sortingOrder = 10000;
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            Debug.Log($"RadialMenu: Canvas {c.name} sortingOrder forced to 10000");
        }

        SetPlayerMovement(false);
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        gameObject.SetActive(false);
        SetPlayerMovement(true);
    }

    private void OnDirectionSelected(int directionIndex)
    {
        Debug.Log($"RadialMenu: Direction {directionIndex} selected.");
        if (activeIdol != null)
        {
            activeIdol.SetDirection((StoneIdol.Direction)directionIndex);
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
                    
                    Animator anim = player.GetComponentInChildren<Animator>();
                    if (anim != null) anim.SetBool("isMoving", false);
                }
            }
        }
    }
    
    void LateUpdate()
    {
        if (IsActive && Time.time > openTime + 0.2f && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.R)))
        {
            Close();
        }

        // FORCE CURSOR while active to prevent other systems (like PlayerMovement or MyUIManager) from locking it
        if (IsActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
