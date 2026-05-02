using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera Bounds")]
    public BoxCollider2D boundsCollider;

    [Header("Current Bounds (Auto)")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("Keine Camera Komponente gefunden!");
            return;
        }

        // Sichtbare Kamerahälfte berechnen
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // Bounds initial laden
        UpdateBounds();
    }

    // =========================
    // BOUNDS AUTOMATISCH AKTUALISIEREN
    // =========================
    public void UpdateBounds()
    {
        if (boundsCollider == null)
        {
            Debug.LogError("CameraBounds Collider fehlt!");
            return;
        }

        Bounds bounds = boundsCollider.bounds;

        minX = bounds.min.x;
        maxX = bounds.max.x;
        minY = bounds.min.y;
        maxY = bounds.max.y;

        Debug.Log("Camera Bounds aktualisiert: " +
                  "X(" + minX + " / " + maxX + ") " +
                  "Y(" + minY + " / " + maxY + ")");
    }

    // =========================
    // FOLLOW
    // =========================
    void LateUpdate()
    {
        if (player == null)
            return;

        if (boundsCollider == null)
            return;

        // Falls Bounds im Editor geändert wurden
        UpdateBounds();

        // X clamp
        float clampedX = Mathf.Clamp(
            player.position.x,
            minX + camHalfWidth,
            maxX - camHalfWidth
        );

        // Y clamp
        float clampedY = Mathf.Clamp(
            player.position.y,
            minY + camHalfHeight,
            maxY - camHalfHeight
        );

        // Kamera bewegen
        transform.position = new Vector3(
            clampedX,
            clampedY,
            -10f
        );
    }
}