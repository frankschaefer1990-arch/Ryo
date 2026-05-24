using UnityEngine;
using System;

public class KryptaInteractable : MonoBehaviour
{
    public Vector2 interactionSize = new Vector2(2.5f, 2.5f);
    public string promptMessage = "Drücke 'R' zum Interagieren";
    public Action OnInteract;
    
    private bool isPlayerNearby = false;
    private GameObject player;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player;
        }

        if (player == null) return;

        // Use Box check instead of distance for "Cube" feel
        Vector2 diff = transform.position - player.transform.position;
        bool nearby = Mathf.Abs(diff.x) <= interactionSize.x / 2f && Mathf.Abs(diff.y) <= interactionSize.y / 2f;

        if (nearby && !isPlayerNearby)
        {
            isPlayerNearby = true;
            Debug.Log($"Player nearby {gameObject.name}");
        }
        else if (!nearby && isPlayerNearby)
        {
            isPlayerNearby = false;
        }

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
        {
            OnInteract?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(interactionSize.x, interactionSize.y, 0.1f));
    }
}
