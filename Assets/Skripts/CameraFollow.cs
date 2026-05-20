using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera Bounds Collider (optional)")]
    public BoxCollider2D boundsCollider;

    [Header("Fallback Manual Bounds")]
    public float minX = -17f;
    public float maxX = 17f;
    public float minY = -17f;
    public float maxY = 17f;

    private float camHalfHeight;
    private float camHalfWidth;

    // =========================
    // START
    // =========================
    private void Start()
    {
        SetupCamera();
        FindPlayer();
        UpdateBounds();

        // Ensure CinemachineBrain is disabled so this script has control
        var brain = GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null) brain.enabled = false;
    }

    // =========================
    // PLAYER SUCHEN
    // =========================
    private void FindPlayer()
    {
        // Prioritize scene instance with "Player" tag
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
            return;
        }

        // Fallback to GameManager only if it's a scene object
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            if (GameManager.Instance.player.scene.name != null) // Not a prefab
            {
                player = GameManager.Instance.player.transform;
                return;
            }
        }
    }

    // =========================
    // CAMERA SETUP
    // =========================
    private void SetupCamera()
    {
        Camera cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("Camera Component fehlt!");
            return;
        }

        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    // =========================
    // BOUNDS
    // =========================
    public void UpdateBounds()
    {
        // Nur einmal suchen
        if (boundsCollider == null)
        {
            GameObject boundsObject = GameObject.Find("CameraBounds");

            if (boundsObject != null)
            {
                boundsCollider = boundsObject.GetComponent<BoxCollider2D>();
            }
        }

        // Collider Bounds
        if (boundsCollider != null)
        {
            Bounds bounds = boundsCollider.bounds;

            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;
        }
    }

    // =========================
    // LATE UPDATE
    // =========================
    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
            
            // Snap to player on first find
            transform.position = new Vector3(player.position.x, player.position.y, -10f);
        }

        camHalfHeight = GetComponent<Camera>().orthographicSize;
        camHalfWidth = camHalfHeight * GetComponent<Camera>().aspect;

        float targetX = player.position.x;
        float targetY = player.position.y;

        float clampedX = targetX;
        float clampedY = targetY;

        // Only clamp if the bounds are larger than the camera view
        if (maxX - minX > camHalfWidth * 2)
        {
            clampedX = Mathf.Clamp(targetX, minX + camHalfWidth, maxX - camHalfWidth);
        }
        
        if (maxY - minY > camHalfHeight * 2)
        {
            clampedY = Mathf.Clamp(targetY, minY + camHalfHeight, maxY - camHalfHeight);
        }

        transform.position = new Vector3(clampedX, clampedY, -10f);
    }
}