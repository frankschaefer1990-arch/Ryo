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
    }

    // =========================
    // PLAYER SUCHEN
    // =========================
    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("Player NICHT gefunden! Prüfe Tag = Player");
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
        // Falls Player nach Szenenwechsel neu gespawnt wurde
        if (player == null)
        {
            FindPlayer();

            if (player == null)
                return;
        }

        float clampedX = Mathf.Clamp(
            player.position.x,
            minX + camHalfWidth,
            maxX - camHalfWidth
        );

        float clampedY = Mathf.Clamp(
            player.position.y,
            minY + camHalfHeight,
            maxY - camHalfHeight
        );

        transform.position = new Vector3(
            clampedX,
            clampedY,
            -10f
        );
    }
}