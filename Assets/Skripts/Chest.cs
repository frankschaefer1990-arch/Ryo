using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Chest : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite halfOpenSprite;
    public Sprite openSprite;
    public Sprite emptyGreyedSprite;

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.R;
    public float animationDelay = 0.2f;
    
    [Header("Inventory")]
    public List<string> items = new List<string> { "Heiltrank" };
    public bool isOpened = false;
    public bool isPermanentlyEmpty = false;

    [Header("Trap")]
    public EnemyData trapEnemy;

    private SpriteRenderer spriteRenderer;
    private bool playerInside = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Check if trap was already triggered and defeated
        if (trapEnemy != null && GameManager.Instance != null)
        {
            string trapId = gameObject.scene.name + "_" + gameObject.name + "_Trap";
            if (GameManager.Instance.defeatedEnemiesInCurrentScene.Contains(trapId))
            {
                isOpened = true;
                isPermanentlyEmpty = true;
            }
        }

        if (isPermanentlyEmpty)
        {
            spriteRenderer.sprite = emptyGreyedSprite;
            spriteRenderer.color = Color.gray;
        }
        else if (isOpened)
        {
            spriteRenderer.sprite = openSprite;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log($"Chest: Player entered range of {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log($"Chest: Player left range of {gameObject.name}");
        }
    }

    private void Update()
    {
        // Debug key detection
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log($"Chest: '{interactKey}' pressed. playerInside={playerInside}, isOpened={isOpened}, isPermanentlyEmpty={isPermanentlyEmpty}");
            
            // Interaction logic
            if (playerInside && !isPermanentlyEmpty)
            {
                if (!isOpened) StartCoroutine(OpenChestRoutine());
                else OpenChestUI();
            }
            else if (!playerInside && !isPermanentlyEmpty)
            {
                // Distance-based fallback in case Trigger fails
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && Vector2.Distance(transform.position, player.transform.position) < 1.2f)
                {
                    // Wall check
                    int wallLayerMask = LayerMask.GetMask("Wall");
                    RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.transform.position, wallLayerMask);
                    
                    if (wallHit.collider == null)
                    {
                        Debug.Log("Chest: Trigger failed but distance check succeeded and no walls between. Opening...");
                        playerInside = true; 
                        if (!isOpened) StartCoroutine(OpenChestRoutine());
                        else OpenChestUI();
                    }
                    else
                    {
                        Debug.Log("Chest: Distance check succeeded but wall is in the way.");
                    }
                }
            }
        }
    }

    private IEnumerator OpenChestRoutine()
    {
        Debug.Log($"Chest: Starting opening animation for {gameObject.name}");
        isOpened = true;
        if (spriteRenderer != null) spriteRenderer.sprite = halfOpenSprite;
        yield return new WaitForSeconds(animationDelay);
        if (spriteRenderer != null) spriteRenderer.sprite = openSprite;
        
        if (trapEnemy != null)
        {
            TriggerTrap();
        }
        else
        {
            OpenChestUI();
        }
    }

    private void TriggerTrap()
    {
        if (QuestManager.Instance != null && GameManager.Instance != null && trapEnemy != null)
        {
            GameManager.Instance.lastEnemyTriggerID = gameObject.scene.name + "_" + gameObject.name + "_Trap";
            QuestManager.Instance.nextBattleEnemy = trapEnemy;
            GameManager.Instance.LoadScene("BattleScene");
        }
    }

    private void OpenChestUI()
    {
        Debug.Log($"Chest: Attempting to open UI for {gameObject.name}. Item count: {items.Count}");
        ChestUI ui = FindChestUI();

        if (ui != null)
        {
            Debug.Log($"Chest: Found UI on '{ui.gameObject.name}'. Calling Open().");
            ui.Open(this);
        }
        else
        {
            Debug.LogError("Chest: ChestUI could NOT be found in scene or via MyUIManager! Retrying...");
            StartCoroutine(OpenChestRetry());
        }
    }

    private ChestUI FindChestUI()
    {
        // 1. Try MyUIManager link
        if (MyUIManager.Instance != null && MyUIManager.Instance.chestUI != null) 
        {
            Debug.Log("Chest: Found UI via MyUIManager link.");
            return MyUIManager.Instance.chestUI;
        }

        // 2. Try static Instance
        var ui = ChestUI.Instance;
        if (ui != null && ui.gameObject != null) 
        {
            Debug.Log("Chest: Found UI via ChestUI.Instance.");
            return ui;
        }

        // 3. Last resort: Deep scene search
        var allUIs = Object.FindObjectsByType<ChestUI>(FindObjectsInactive.Include);
        var found = allUIs.OrderByDescending(x => x.gameObject.scene.name == "DontDestroyOnLoad").FirstOrDefault();
        if (found != null) Debug.Log($"Chest: Found UI via deep scene search on '{found.gameObject.name}'.");
        
        return found;
    }

    private IEnumerator OpenChestRetry()
    {
        yield return new WaitForSeconds(0.1f);
        var ui = FindChestUI();
        if (ui != null)
        {
            ui.Open(this);
        }
        else
        {
            Debug.LogError("ChestUI Instance not found even after retry! Please check if the 'ChestUI' component exists on your MasterCanvas and that the MasterCanvas is active.");
        }
    }

    public void OnChestEmptied()
    {
        isPermanentlyEmpty = true;
        spriteRenderer.sprite = closedSprite; // Close it
        spriteRenderer.color = Color.gray; // Grey it out
        isOpened = false;
    }
}
