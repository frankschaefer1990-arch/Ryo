using UnityEngine;

public class WallBlocker : MonoBehaviour
{
    private Vector3 lastSafePosition;
    private bool touchingWall = false;

    void Start()
    {
        lastSafePosition = transform.position;
    }

    void Update()
    {
        if (!touchingWall)
        {
            lastSafePosition = transform.position;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingWall = true;
            transform.position = lastSafePosition;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingWall = false;
        }
    }
}