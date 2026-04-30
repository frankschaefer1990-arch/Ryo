using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Map Bounds")]
    public float minX = -17f;
    public float maxX = 17f;
    public float minY = -17f;
    public float maxY = 17f;

    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        // Hälfte der sichtbaren Kamera berechnen
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        // Falls kein Player zugewiesen ist -> nichts tun
        if (player == null) return;

        // Kamera X innerhalb der Map halten
        float clampedX = Mathf.Clamp(
            player.position.x,
            minX + camHalfWidth,
            maxX - camHalfWidth
        );

        // Kamera Y innerhalb der Map halten
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