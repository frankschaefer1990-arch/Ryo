using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public static RadialMenu Instance;

    public GameObject panel;
    public Button[] directionButtons; // 0: South, 1: SW, 2: W, 3: NW, 4: N, 5: NE, 6: E, 7: SE
    
    public bool IsActive => panel != null && panel.activeInHierarchy;
    private StoneIdol activeIdol;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
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
            }
        }
    }

    public void Open(StoneIdol idol)
    {
        activeIdol = idol;
        if (panel != null) panel.SetActive(true);
        SetPlayerMovement(false);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        SetPlayerMovement(true);
    }

    private void OnDirectionSelected(int directionIndex)
    {
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
