using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private SpriteRenderer spriteRenderer;
    private bool playerInside = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (playerInside && Input.GetKeyDown(interactKey) && !isPermanentlyEmpty)
        {
            Debug.Log($"Chest: Opening {gameObject.name}");
            if (!isOpened)
            {
                StartCoroutine(OpenChestRoutine());
            }
            else
            {
                OpenChestUI();
            }
        }
    }

    private IEnumerator OpenChestRoutine()
    {
        isOpened = true;
        spriteRenderer.sprite = halfOpenSprite;
        yield return new WaitForSeconds(animationDelay);
        spriteRenderer.sprite = openSprite;
        OpenChestUI();
    }

    private void OpenChestUI()
    {
        if (ChestUI.Instance != null)
        {
            ChestUI.Instance.Open(this);
        }
        else
        {
            Debug.LogError("ChestUI Instance not found!");
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
